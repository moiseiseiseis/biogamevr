using System;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace BioVR.Jugador
{
    // Altura de los ojos del piloto, igual para todos (niño o adulto, sentado o de pie):
    // sube o baja el XR Origin para que la cámara quede a "alturaOjos" metros del piso.
    // Se aplica al iniciar (cuando el visor ya reporta la cabeza), al ajustarla desde el menú y al recentrar,
    // porque Recentrar lleva la cámara a "puntoOjos" y aquí se mantiene su altura igual a alturaOjos.
    // El valor se guarda en el visor (PlayerPrefs) para no ajustarlo cada vez.
    public class AlturaAsiento : MonoBehaviour
    {
        const string ClaveGuardado = "BioVR.AlturaOjos";

        [SerializeField] XROrigin origen;

        [Tooltip("El mismo Punto ojos que usa Recentrar.")]
        [SerializeField] Transform puntoOjos;

        [Tooltip("Altura de los ojos por defecto, en metros. La consola llega a 1.22 m.")]
        [SerializeField] float alturaPorDefecto = 1.4f;

        [SerializeField] float paso = 0.05f;
        [SerializeField] float minima = 1f;
        [SerializeField] float maxima = 1.9f;

        [Tooltip("Texto del menú que muestra la altura actual (opcional).")]
        [SerializeField] TextMeshProUGUI etiqueta;

        // Avisa cada vez que el jugador cambia la altura; lo usa el tutorial
        public static event Action<float> OnAjuste;

        public float AlturaOjos { get; private set; }

        bool aplicadaAlInicio;

        void Start()
        {
            if (origen == null || puntoOjos == null)
            {
                Debug.LogError("[Jugador] AlturaAsiento necesita el XR Origin y el Punto ojos.", this);
                enabled = false;
                return;
            }
            AlturaOjos = Mathf.Clamp(PlayerPrefs.GetFloat(ClaveGuardado, alturaPorDefecto), minima, maxima);
            PonerPuntoOjos();
            ActualizarEtiqueta();
        }

        void Update()
        {
            // El visor tarda unos cuadros en reportar la cabeza; antes de eso la cámara está en el piso
            if (aplicadaAlInicio || origen.Camera.transform.localPosition.y < 0.2f) return;
            aplicadaAlInicio = true;
            Aplicar();
        }

        // Botones "Subir" y "Bajar" del menú
        public void Subir() => Cambiar(paso);
        public void Bajar() => Cambiar(-paso);

        void Cambiar(float delta)
        {
            AlturaOjos = Mathf.Clamp(Mathf.Round((AlturaOjos + delta) * 100f) / 100f, minima, maxima);
            PlayerPrefs.SetFloat(ClaveGuardado, AlturaOjos);
            PlayerPrefs.Save();
            PonerPuntoOjos();
            Aplicar();
            ActualizarEtiqueta();
            Debug.Log($"[Jugador] Altura de los ojos: {AlturaOjos:0.00} m");
            OnAjuste?.Invoke(AlturaOjos);
        }

        // Solo mueve en vertical: la posición dentro de la cabina y hacia dónde mira no cambian
        void Aplicar()
        {
            float diferencia = AlturaOjos - origen.Camera.transform.position.y;
            origen.transform.position += Vector3.up * diferencia;
            Physics.SyncTransforms(); // el CharacterController del XR Origin debe enterarse del cambio
        }

        void PonerPuntoOjos()
        {
            var p = puntoOjos.position;
            puntoOjos.position = new Vector3(p.x, AlturaOjos, p.z);
        }

        void ActualizarEtiqueta()
        {
            if (etiqueta != null) etiqueta.text = $"Altura de los ojos: {AlturaOjos:0.00} m";
        }
    }
}
