using System.Collections;
using BioVR.Secuencia;
using UnityEngine;

namespace BioVR.Escala
{
    // Túnel de partículas del salto Matrioska.
    // Cuando el GameManager avisa OnSaltoIniciado, las partículas (rayas que pasan alrededor de la cabina)
    // aceleran durante el efecto, con el color del nivel al que se salta; luego viene el destello blanco
    // del GameManager y, ya con la pantalla tapada, se apagan.
    // La cabina nunca se mueve: solo el exterior, para no marear.
    public class EfectoSalto : MonoBehaviour
    {
        [SerializeField] ParticleSystem particulas;
        [SerializeField] GestorEscala gestorEscala;

        [Tooltip("Partículas por segundo al final del efecto.")]
        [SerializeField] float emisionMaxima = 400f;

        [Tooltip("Velocidad de simulación al inicio y al final del efecto (acelera).")]
        [SerializeField] float velocidadInicial = 0.4f, velocidadFinal = 2.5f;

        [Tooltip("Debe coincidir con 'Duracion Efecto Salto' + 'Duracion Destello' del GameManager.")]
        [SerializeField] float duracion = 1.45f;

        GameManager gm;

        void Start()
        {
            gm = GameManager.Instance;
            if (gm == null || particulas == null)
            {
                Debug.LogError("[BioVR - Rol 5] EfectoSalto necesita el GameManager en la escena y su ParticleSystem.", this);
                enabled = false;
                return;
            }
            gm.OnSaltoIniciado += AlIniciarSalto;
            gm.OnSaltoCambiado += AlCambiarSalto;
            Apagar();
        }

        void OnDestroy()
        {
            if (gm == null) return;
            gm.OnSaltoIniciado -= AlIniciarSalto;
            gm.OnSaltoCambiado -= AlCambiarSalto;
        }

        void AlIniciarSalto(int saltoNuevo)
        {
            StopAllCoroutines();
            StartCoroutine(Tunel(ColorDelNivel(saltoNuevo)));
        }

        // El cambio de zona ocurre con la pantalla en blanco: ahí se limpian las partículas
        void AlCambiarSalto(int _)
        {
            StopAllCoroutines();
            Apagar();
        }

        IEnumerator Tunel(Color color)
        {
            var main = particulas.main;
            main.startColor = color;
            var emision = particulas.emission;
            particulas.Play();

            for (float t = 0; t < duracion; t += Time.deltaTime)
            {
                float k = t / duracion;
                emision.rateOverTime = emisionMaxima * k;
                main.simulationSpeed = Mathf.Lerp(velocidadInicial, velocidadFinal, k * k);
                yield return null;
            }
        }

        void Apagar()
        {
            var emision = particulas.emission;
            emision.rateOverTime = 0f;
            particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        Color ColorDelNivel(int indice)
        {
            var niveles = gestorEscala != null ? gestorEscala.nivelesEscala : null;
            if (niveles == null || indice < 0 || indice >= niveles.Length || niveles[indice] == null)
                return Color.white;
            var c = niveles[indice].colorFluido;
            // Aclarar el color del fluido para que las rayas brillen sobre el fondo
            return c.a <= 0f ? Color.white : Color.Lerp(c, Color.white, 0.35f);
        }
    }
}
