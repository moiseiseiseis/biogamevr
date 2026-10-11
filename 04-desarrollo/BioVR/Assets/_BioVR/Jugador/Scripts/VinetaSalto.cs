using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using BioVR.Secuencia;

namespace BioVR.Jugador
{
    // Viñeta de comodidad durante el salto Matrioska: se cierra mientras pasan las rayas del túnel
    // y se abre al llegar al nuevo entorno.
    // Se registra como "proveedor" del TunnelingVignetteController (el de Starter Assets, hijo de la Main Camera):
    // sin un proveedor activo el controlador mantiene la viñeta abierta, por eso no basta con prender el objeto.
    public class VinetaSalto : MonoBehaviour, ITunnelingVignetteProvider
    {
        [Tooltip("El TunnelingVignetteController del prefab TunnelingVignette bajo la Main Camera.")]
        public TunnelingVignetteController controlador;

        [SerializeField] VignetteParameters parametros = new VignetteParameters
        {
            apertureSize = 0.55f,     // qué tanto se cierra (1 = nada, 0 = todo)
            featheringEffect = 0.25f, // borde suave
            easeInTime = 0.4f,
            easeOutTime = 0.4f,
            vignetteColor = Color.black,
            vignetteColorBlend = Color.black,
        };

        public VignetteParameters vignetteParameters => parametros;

        GameManager gm;

        void Start()
        {
            gm = GameManager.Instance;
            if (gm == null || controlador == null)
            {
                Debug.LogError("[Jugador] VinetaSalto necesita el GameManager en la escena y el TunnelingVignetteController.", this);
                enabled = false;
                return;
            }
            gm.OnSaltoIniciado += ActivarVineta;
            gm.OnSaltoCambiado += DesactivarVineta;
        }

        void OnDestroy()
        {
            if (gm == null) return;
            gm.OnSaltoIniciado -= ActivarVineta;
            gm.OnSaltoCambiado -= DesactivarVineta;
        }

        // Se cierra en cuanto inicia el túnel de partículas
        void ActivarVineta(int saltoNuevo) => controlador.BeginTunnelingVignette(this);

        // Se abre al llegar al nuevo entorno
        void DesactivarVineta(int salto) => controlador.EndTunnelingVignette(this);
    }
}
