using TMPro;
using UnityEngine;

namespace BioVR.Pilotaje
{
    // Aviso de mareo que reaparece mientras se pilota, aunque ya se haya cerrado el tutorial (prueba del 24-nov:
    // el pilotaje gustó mucho pero marea; en vez de quitarle movimiento, se avisa).
    // Cuenta solo el tiempo en que la nave se mueve: el primer aviso sale a los PrimerAviso segundos y luego cada
    // Intervalo segundos, durante Duracion segundos, con un fundido. Va en el letrero, que cuelga de la brújula:
    // solo existe mientras se pilota y la cabina holográfica no lo toca.
    // Tiempos en constantes (no campos serializados) para que la escena no los deje viejos.
    [RequireComponent(typeof(TextMeshPro))]
    public class AvisoMareo : MonoBehaviour
    {
        const float PrimerAviso = 25f, Intervalo = 90f, Duracion = 7f, Fundido = 0.6f;

        TextMeshPro texto;
        float enMovimiento, mostrandoDesde = -1f;
        float siguiente = PrimerAviso;

        void Awake()
        {
            texto = GetComponent<TextMeshPro>();
            texto.alpha = 0f;
        }

        void Update()
        {
            if (PilotajeLibre.Activo && PilotajeLibre.Rapidez > 0.05f) enMovimiento += Time.deltaTime;

            if (mostrandoDesde < 0f && enMovimiento >= siguiente)
            {
                mostrandoDesde = Time.time;
                siguiente = enMovimiento + Intervalo;
            }

            float a = 0f;
            if (mostrandoDesde >= 0f)
            {
                float t = Time.time - mostrandoDesde;
                if (t > Duracion) mostrandoDesde = -1f;
                else a = Mathf.Clamp01(Mathf.Min(t, Duracion - t) / Fundido);
            }
            texto.alpha = a;
        }
    }
}
