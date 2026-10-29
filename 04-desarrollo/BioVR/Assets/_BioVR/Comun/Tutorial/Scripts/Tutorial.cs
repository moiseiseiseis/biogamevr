using System.Collections;
using BioVR.Cabina;
using BioVR.Jugador;
using BioVR.Secuencia;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace BioVR.Comun
{
    // Tutorial por pasos en un panel junto a la consola. Cada paso (PasoTutorial) dice qué hacer y qué "clave"
    // lo completa. Al recibir esa clave muestra "¡Bien!" y pasa al siguiente. Las acciones de práctica (recentrar,
    // caminar, girar…) solo cuentan en su propio paso: girar en el paso 1 no debe brincar al 5. Solo los avances del
    // juego (estado:… y salto:…), que no tienen vuelta atrás, pueden adelantar el tutorial hasta su paso.
    //
    // Claves que ya existen (este script escucha los eventos del juego y las traduce):
    //   recentrar         B / Y (Recentrar)
    //   caminar, girar    stick izquierdo / derecho (proveedores de locomoción del XR Origin)
    //   menu              abrir el menú de pausa (MenuPausa)
    //   asiento           cambiar la altura del asiento (AlturaAsiento)
    //   salir, entrar     salir al hangar por la escotilla / volver a la cabina (Escotilla)
    //   estado:<Estado>   llegar a un estado del GameManager: estado:Abordaje, estado:Viaje, estado:Llegada…
    //   salto:<n>         llegar al salto n: salto:2 = Capilar, salto:3 = Citoplasma
    // Para una clave nueva (p. ej. de un minijuego), desde cualquier script: Tutorial.Notificar("atp_listo");
    // Lo arma el menú BioVR → Crear tutorial.
    public class Tutorial : MonoBehaviour
    {
        [SerializeField] PasoTutorial[] pasos = new PasoTutorial[0];
        [SerializeField] GameObject panel;
        [SerializeField] TextMeshProUGUI progreso;
        [SerializeField] TextMeshProUGUI titulo;
        [SerializeField] TextMeshProUGUI instruccion;
        [SerializeField] GameObject botonSiguiente;
        [SerializeField] TextMeshProUGUI textoSiguiente;

        [Tooltip("Segundos que se ve \"¡Bien!\" antes del siguiente paso (no se detiene con la pausa).")]
        [SerializeField] float pausaAlCompletar = 1f;

        static Tutorial instancia;

        // Avisa qué paso se está mostrando; null = ninguno (completándose, terminado o saltado). Lo usa ResaltadorTutorial.
        public static event System.Action<PasoTutorial> OnPasoCambiado;

        // El paso que se está mostrando ahora, o null
        public static PasoTutorial PasoActual =>
            instancia != null && instancia.enabled && !instancia.completando && !instancia.Terminado
                ? instancia.pasos[instancia.indice] : null;

        public bool Terminado => indice >= pasos.Length;

        int indice;
        bool completando;
        GameManager gm;
        LocomotionProvider mover, girar;

        // Para avisar al tutorial desde cualquier script, aunque no haya tutorial en la escena
        public static void Notificar(string clave)
        {
            if (instancia != null) instancia.AlNotificar(clave);
        }

        void Awake() => instancia = this;

        void Start()
        {
            int total = pasos.Length;
            pasos = System.Array.FindAll(pasos, p => p != null);
            if (pasos.Length < total)
                Debug.LogWarning($"[Tutorial] {total - pasos.Length} paso(s) vacío(s) en la lista; vuelve a correr BioVR → Crear tutorial.", this);

            if (panel == null || titulo == null || instruccion == null || pasos.Length == 0)
            {
                Debug.LogError("[Tutorial] Faltan el panel, los textos o los pasos.", this);
                enabled = false;
                return;
            }

            Recentrar.OnRecentrado += AlRecentrar;
            MenuPausa.OnMenu += AlMenu;
            AlturaAsiento.OnAjuste += AlAjustarAsiento;
            Escotilla.OnAfuera += AlCambiarLugar;
            gm = GameManager.Instance;
            if (gm != null)
            {
                gm.OnEstadoCambiado += AlCambiarEstado;
                gm.OnSaltoCambiado += AlCambiarSalto;
            }
            mover = FindFirstObjectByType<ContinuousMoveProvider>(FindObjectsInactive.Include);
            girar = FindFirstObjectByType<SnapTurnProvider>(FindObjectsInactive.Include);
            if (mover != null) mover.locomotionStarted += AlCaminar;
            if (girar != null) girar.locomotionStarted += AlGirar;

            Mostrar(0);
        }

        void OnDestroy()
        {
            if (instancia == this) instancia = null;
            Recentrar.OnRecentrado -= AlRecentrar;
            MenuPausa.OnMenu -= AlMenu;
            AlturaAsiento.OnAjuste -= AlAjustarAsiento;
            Escotilla.OnAfuera -= AlCambiarLugar;
            if (gm != null)
            {
                gm.OnEstadoCambiado -= AlCambiarEstado;
                gm.OnSaltoCambiado -= AlCambiarSalto;
            }
            if (mover != null) mover.locomotionStarted -= AlCaminar;
            if (girar != null) girar.locomotionStarted -= AlGirar;
        }

        // ---- Eventos del juego → claves ----
        void AlRecentrar() => AlNotificar("recentrar");
        void AlCaminar(LocomotionProvider p) => AlNotificar("caminar");
        void AlGirar(LocomotionProvider p) => AlNotificar("girar");
        void AlMenu(bool abierto) { if (abierto) AlNotificar("menu"); }
        void AlAjustarAsiento(float altura) => AlNotificar("asiento");
        void AlCambiarLugar(bool afuera) => AlNotificar(afuera ? "salir" : "entrar");
        void AlCambiarEstado(GameState estado) => AlNotificar($"estado:{estado}");
        void AlCambiarSalto(int salto) => AlNotificar($"salto:{salto}");

        void AlNotificar(string clave)
        {
            if (Terminado || completando || string.IsNullOrEmpty(clave)) return;
            bool esAvance = clave.StartsWith("estado:") || clave.StartsWith("salto:");
            int hasta = esAvance ? pasos.Length : indice + 1;
            for (int i = indice; i < hasta; i++)
            {
                if (pasos[i] == null || pasos[i].clave != clave) continue;
                StartCoroutine(Completar(i));
                return;
            }
        }

        IEnumerator Completar(int paso)
        {
            completando = true;
            OnPasoCambiado?.Invoke(null);
            titulo.text = "¡Bien!";
            botonSiguiente.SetActive(false);
            yield return new WaitForSecondsRealtime(pausaAlCompletar);
            completando = false;
            Mostrar(paso + 1);
        }

        void Mostrar(int i)
        {
            indice = i;
            if (Terminado) { Terminar(); return; }
            var paso = pasos[i];
            if (progreso != null) progreso.text = $"Tutorial · paso {i + 1} de {pasos.Length}";
            titulo.text = paso.titulo;
            instruccion.text = paso.instruccion;
            bool conBoton = string.IsNullOrEmpty(paso.clave) || paso.opcional;
            botonSiguiente.SetActive(conBoton);
            if (textoSiguiente != null) textoSiguiente.text = i == pasos.Length - 1 ? "Terminar" : "Siguiente";
            panel.SetActive(true);
            OnPasoCambiado?.Invoke(paso);
        }

        // Botón "Siguiente" (o "Terminar" en el último paso)
        public void Siguiente()
        {
            if (!completando && !Terminado) Mostrar(indice + 1);
        }

        // Botón "Saltar tutorial"
        public void Terminar()
        {
            StopAllCoroutines();
            completando = false;
            indice = pasos.Length;
            panel.SetActive(false);
            OnPasoCambiado?.Invoke(null);
            Debug.Log("[Tutorial] Terminado.");
        }
    }
}
