using BioVR.Minijuegos.ExpresionGenetica;
using TMPro;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // M6 · La horquilla del ARNm (especificación 7.2, HTML: golpear). Va en el prefab Cocina_Horquilla.
    //
    //   - Aparece con EventosCocina.OnHorquilla(true), en el ARNm del ribosoma de mesa, del lado 3′
    //     (posición local 0.19, 0.012, 0.02 del ribosoma, la que da M4).
    //   - Se mece en Y (±0.15 rad) con el letrero "horquilla".
    //   - Se golpea con el rayo y el gatillo, o tocándola con la pinza (Senalable acepta los dos).
    //     Cada golpe se lo avisa a ControlCocina.GolpearHorquilla, que cuenta, suma y pone los sonidos;
    //     aquí solo se abre el blend shape "abierta" a golpes/3 y se inclina.
    //   - Al tercero ControlCocina lanza OnHorquilla(false) y se desvanece en 1.3 s.
    //
    // Si cambian el modelo, basta con asignar otra malla y el nombre de su blend shape en el Inspector.
    [RequireComponent(typeof(Senalable))]
    public class HorquillaARNm : MonoBehaviour
    {
        [Header("Dónde aparece")]
        [Tooltip("El ribosoma de mesa (o su marca en Cocina_Base). La horquilla se pone en su ARNm.")]
        public Transform ribosoma;
        // En el HTML es (0.19, 0.012, 0.02). En los dos el +Z del ribosoma apunta al jugador, pero three.js usa
        // ejes de mano derecha (+X = derecha del jugador) y Unity de mano izquierda (+X = izquierda del jugador).
        // Por eso aquí la x lleva el signo contrario: −X = derecha del jugador, lado 3′ del ARNm.
        [Tooltip("Local al ribosoma. −X = derecha del jugador (lado 3′ del ARNm), +Z = hacia el jugador")]
        public Vector3 posicionEnRibosoma = new Vector3(-0.19f, 0.012f, 0.02f);

        [Tooltip("Cuánto más a la derecha del jugador que posicionEnRibosoma (m)")]
        [SerializeField] float corrimientoDerecha = 0.06f;
        // Giro extra para que el modelo quede de pie: viene acostado sobre el ARNm (con el lazo hacia el jugador)
        static readonly Vector3 giroVertical = new Vector3(-90f, 0f, 0f);

        [Header("Modelo")]
        [Tooltip("Lo que se mece, se inclina y se desvanece (el modelo y su letrero)")]
        [SerializeField] Transform visual;
        [SerializeField] SkinnedMeshRenderer malla;
        [SerializeField] string blendShape = "abierta";

        [Header("Letrero")]
        [SerializeField] TextMeshPro letrero;
        [SerializeField] string claveLetrero = "letrero_horquilla";
        [Tooltip("Se usa mientras la clave no exista en TextosCocina")]
        [SerializeField] string letreroRespaldo = "horquilla";

        [Header("Movimiento")]
        [SerializeField] float meceRadianes = 0.15f;
        [SerializeField] float meceVelocidad = 2.2f;
        [Tooltip("Grados que se inclina cuando ya está abierta del todo")]
        [SerializeField] float inclinacionAbierta = 35f;
        [SerializeField] float segundosDesvanecer = 1.3f;

        Senalable senal;
        int indiceBlend = -1;
        Vector3 escalaVisual = Vector3.one;
        float abierta;          // 0 = cerrada, 1 = abierta (golpes / golpesHorquilla)
        float desvanecer = -1f; // < 0 = no se está desvaneciendo
        float reloj;
        Camera camara;

        void Awake()
        {
            senal = GetComponent<Senalable>();
            if (visual == null) visual = transform;
            escalaVisual = visual.localScale;
            if (malla != null && malla.sharedMesh != null) indiceBlend = malla.sharedMesh.GetBlendShapeIndex(blendShape);
            if (malla != null && indiceBlend < 0)
                Debug.LogWarning($"[Cocina] La malla {malla.name} no tiene el blend shape \"{blendShape}\": la horquilla no se va a abrir.", this);
            Esconder();
        }

        void OnEnable()
        {
            senal.OnSenalado += AlGolpear;
            EventosCocina.OnHorquilla += AlCambiarHorquilla;
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
        }

        void OnDisable()
        {
            senal.OnSenalado -= AlGolpear;
            EventosCocina.OnHorquilla -= AlCambiarHorquilla;
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
        }

        void AlIniciarTurno(TurnoData t) => Esconder();

        void AlCambiarHorquilla(bool aparece)
        {
            if (aparece) Mostrar();
            else if (visual.gameObject.activeSelf) { desvanecer = 0f; senal.Habilitado = false; }
        }

        void Mostrar()
        {
            if (ribosoma != null)
            {
                // Prueba del 23-nov: 6 cm más a la derecha del jugador (−X local) y de pie (sin la inclinación del
                // ribosoma, solo su giro horizontal)
                transform.position = ribosoma.TransformPoint(posicionEnRibosoma + Vector3.left * corrimientoDerecha);
                var frente = Vector3.ProjectOnPlane(ribosoma.forward, Vector3.up);
                transform.rotation = frente.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(frente, Vector3.up) * Quaternion.Euler(giroVertical) : ribosoma.rotation;
            }
            abierta = 0f;
            desvanecer = -1f;
            reloj = 0f;
            visual.localScale = escalaVisual;
            visual.gameObject.SetActive(true);
            PonerAbierta(0f);
            if (letrero != null)
            {
                var textos = ControlCocina.Instancia != null ? ControlCocina.Instancia.Textos : null;
                letrero.text = textos != null && textos.Tiene(claveLetrero) ? textos.Texto(claveLetrero) : letreroRespaldo;
            }
            senal.Habilitado = true;
        }

        void Esconder()
        {
            desvanecer = -1f;
            senal.Habilitado = false;
            visual.gameObject.SetActive(false);
        }

        void AlGolpear(Senalable s)
        {
            var c = ControlCocina.Instancia;
            if (c == null || !c.HorquillaActiva || c.Pausado) return;
            PonerAbierta(c.GolpearHorquilla());
        }

        void PonerAbierta(float valor)
        {
            abierta = Mathf.Clamp01(valor);
            if (malla != null && indiceBlend >= 0) malla.SetBlendShapeWeight(indiceBlend, abierta * 100f);
        }

        void Update()
        {
            if (!visual.gameObject.activeSelf) return;
            var c = ControlCocina.Instancia;
            bool pausa = c != null && c.Pausado;

            if (!pausa) reloj += Time.deltaTime;
            float mece = Mathf.Sin(reloj * meceVelocidad) * meceRadianes * Mathf.Rad2Deg;
            visual.localRotation = Quaternion.Euler(abierta * inclinacionAbierta, mece, 0f);

            if (desvanecer >= 0f && !pausa)
            {
                desvanecer += Time.deltaTime / Mathf.Max(segundosDesvanecer, 0.01f);
                visual.localScale = escalaVisual * Mathf.Clamp01(1f - desvanecer);
                if (desvanecer >= 1f) Esconder();
            }
        }

        void LateUpdate()
        {
            if (letrero == null || !visual.gameObject.activeSelf) return;
            if (camara == null) camara = Camera.main;
            if (camara == null) return;
            var t = letrero.transform;
            t.rotation = Quaternion.LookRotation(t.position - camara.transform.position, Vector3.up);
        }
    }
}
