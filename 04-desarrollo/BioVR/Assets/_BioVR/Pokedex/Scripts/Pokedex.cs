using System;
using System.Collections.Generic;
using BioVR.Secuencia;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Pokedex
{
    // Pokédex biológico. Durante el viaje, el jugador apunta con el mando a una célula de la sangre o a un organelo
    // y aprieta el gatillo: aparece su ficha corta en el panel de la cabina y queda registrada ("7 de 15").
    //
    //   - El rayo es propio (Physics.RaycastAll de hasta 2 km), porque los organelos están a decenas o cientos
    //     de metros y el rayo de XRI no llega tan lejos. Sale del mismo origen que el rayo de cada mano.
    //   - Si la mano está apuntando a algo de la cabina (un botón), el gatillo es para eso y no registra.
    //   - Mientras se apunta a algo registrable, una etiqueta sobre el rayo dice "¿?" (o su nombre si ya se registró).
    //   - El registro se borra al volver al hangar: cada jugador de la feria empieza de cero.
    //
    // Lo arma el menú BioVR → Agregar pokédex biológico. Las fichas son assets (FichaPokedex) en Pokedex/Fichas.
    public class Pokedex : MonoBehaviour
    {
        [Tooltip("Todas las fichas que se pueden encontrar en el viaje (el total del contador).")]
        [SerializeField] FichaPokedex[] fichas = new FichaPokedex[0];

        [Header("Panel de la cabina")]
        [SerializeField] GameObject panel;
        [SerializeField] TextMeshProUGUI titulo;
        [SerializeField] TextMeshProUGUI tamano;
        [SerializeField] TextMeshProUGUI descripcion;
        [SerializeField] TextMeshProUGUI curiosidad;
        [SerializeField] TextMeshProUGUI contador;

        [Header("Etiqueta sobre el rayo")]
        [SerializeField] TextMeshPro etiqueta;
        [Tooltip("A qué distancia de la mano, sobre el rayo, se dibuja la etiqueta (m).")]
        [SerializeField] float distanciaEtiqueta = 2.5f;

        [SerializeField] float alcance = 2000f;

        public static Pokedex Instancia { get; private set; }

        // La ficha y si es nueva (para el sonido)
        public static event Action<FichaPokedex, bool> OnRegistrada;

        public int Total => fichas.Length;
        public int Registradas => registro.Count;
        public bool EstaRegistrada(FichaPokedex f) => f != null && registro.Contains(f);

        class Mano
        {
            public XRBaseInteractor interactor;
            public Transform origen;
            public InputAction gatillo;
        }

        readonly HashSet<FichaPokedex> registro = new HashSet<FichaPokedex>();
        readonly List<Mano> manos = new List<Mano>();
        readonly RaycastHit[] golpes = new RaycastHit[32];
        GameManager gm;
        Transform camara;

        void Awake() => Instancia = this;

        void Start()
        {
            gm = GameManager.Instance;
            camara = Camera.main != null ? Camera.main.transform : null;

            // Origen del rayo de cada mano: el de XRI (Near-Far del rig de inicio, o un Ray Interactor)
            foreach (var nf in FindObjectsByType<NearFarInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                AgregarMano(nf, nf.curveOrigin != null ? nf.curveOrigin : nf.transform);
            foreach (var ri in FindObjectsByType<XRRayInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                AgregarMano(ri, ri.rayOriginTransform != null ? ri.rayOriginTransform : ri.transform);
            if (manos.Count == 0) Debug.LogWarning("[Pokedex] No encontré los rayos de las manos (Near-Far o Ray Interactor).", this);

            if (gm != null) gm.OnEstadoCambiado += AlCambiarEstado;
            if (etiqueta != null) etiqueta.gameObject.SetActive(false);
            Mostrar(null);
            AlCambiarEstado(gm != null ? gm.Estado : GameState.Viaje);
        }

        void OnDestroy()
        {
            if (Instancia == this) Instancia = null;
            if (gm != null) gm.OnEstadoCambiado -= AlCambiarEstado;
            foreach (var m in manos) m.gatillo?.Dispose();
        }

        void AgregarMano(XRBaseInteractor interactor, Transform origen)
        {
            string mano = interactor.handedness == InteractorHandedness.Left ? "LeftHand" : "RightHand";
            var gatillo = new InputAction("Pokedex gatillo " + mano, InputActionType.Button);
            gatillo.AddBinding($"<XRController>{{{mano}}}/trigger");
            gatillo.Enable();
            manos.Add(new Mano { interactor = interactor, origen = origen, gatillo = gatillo });
        }

        // El panel solo se ve en el viaje y la llegada; al volver al hangar se borra el registro
        void AlCambiarEstado(GameState estado)
        {
            bool viajando = estado == GameState.Viaje || estado == GameState.Llegada;
            if (panel != null) panel.SetActive(viajando);
            if (estado == GameState.Hangar && registro.Count > 0)
            {
                registro.Clear();
                Mostrar(null);
            }
        }

        void Update()
        {
            if (gm != null && (gm.Pausado || gm.EnTransicion || (gm.Estado != GameState.Viaje && gm.Estado != GameState.Llegada)))
            {
                if (etiqueta != null) etiqueta.gameObject.SetActive(false);
                return;
            }

            Descubrible apuntado = null;
            Mano manoApuntando = null;
            foreach (var m in manos)
            {
                if (m.origen == null || !m.interactor.isActiveAndEnabled) continue;
                // Si la mano está sobre algo de XRI (un botón de la cabina), el gatillo es para eso
                if (m.interactor.hasHover) continue;
                var d = Buscar(m.origen.position, m.origen.forward);
                if (d == null) continue;
                apuntado = d;
                manoApuntando = m;
                if (m.gatillo.WasPressedThisFrame()) Registrar(d.ficha);
                break;
            }
            Etiqueta(apuntado, manoApuntando);
        }

        // El Descubrible más cercano sobre el rayo (las paredes de la cabina y lo demás no lo tapan)
        Descubrible Buscar(Vector3 origen, Vector3 direccion)
        {
            int n = Physics.RaycastNonAlloc(origen, direccion, golpes, alcance, ~0, QueryTriggerInteraction.Collide);
            Descubrible mejor = null;
            float distancia = float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                if (golpes[i].distance >= distancia) continue;
                var d = golpes[i].collider.GetComponentInParent<Descubrible>();
                if (d == null || d.ficha == null) continue;
                mejor = d;
                distancia = golpes[i].distance;
            }
            return mejor;
        }

        public void Registrar(FichaPokedex ficha)
        {
            if (ficha == null) return;
            bool nueva = registro.Add(ficha);
            Mostrar(ficha);
            Debug.Log($"[Pokedex] {(nueva ? "Registrada" : "Ya estaba")}: {ficha.numero} {ficha.nombre} ({registro.Count} de {Total}).");
            OnRegistrada?.Invoke(ficha, nueva);
        }

        void Mostrar(FichaPokedex f)
        {
            if (contador != null) contador.text = $"Registradas: {registro.Count} de {Total}";
            if (f == null)
            {
                if (titulo != null) titulo.text = "Pokédex biológico";
                if (tamano != null) tamano.text = "";
                if (descripcion != null) descripcion.text = "Apunta con el mando a una célula o a un organelo y aprieta el gatillo para registrarlo.";
                if (curiosidad != null) curiosidad.text = "";
                return;
            }
            if (titulo != null) titulo.text = $"{f.numero}. {f.nombre}";
            if (tamano != null) tamano.text = string.IsNullOrEmpty(f.tamano) ? f.zona : $"{f.zona} · {f.tamano}";
            if (descripcion != null) descripcion.text = f.descripcion;
            if (curiosidad != null) curiosidad.text = f.curiosidad;
        }

        // Sobre el rayo, a unos metros de la mano, mirando a la cámara: "¿?" o el nombre si ya se registró
        void Etiqueta(Descubrible d, Mano m)
        {
            if (etiqueta == null) return;
            if (d == null || m == null) { etiqueta.gameObject.SetActive(false); return; }
            etiqueta.gameObject.SetActive(true);
            etiqueta.text = registro.Contains(d.ficha) ? $"{d.ficha.nombre}\n<size=60%>registrada</size>" : "¿ ? \n<size=60%>gatillo: registrar</size>";
            var p = m.origen.position + m.origen.forward * distanciaEtiqueta;
            etiqueta.transform.position = p;
            if (camara != null) etiqueta.transform.rotation = Quaternion.LookRotation(p - camara.position);
        }
    }
}
