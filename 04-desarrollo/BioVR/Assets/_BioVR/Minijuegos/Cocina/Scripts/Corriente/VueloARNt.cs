using System;
using System.Collections;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    public class VueloARNt : MonoBehaviour
    {
        public static VueloARNt Instancia { get; private set; }

        [Header("Destinos lejanos para asistencia VR")]
        [SerializeField] Transform destinoRibosomaLejano;
        [SerializeField] Transform destinoRegresoCitoplasma;

        public Action<ARNtCocina> AlLlegarRibosomaLejano;
        public Action<ARNtCocina> AlLlegarRegreso;

        void Awake()
        {
            if (Instancia != null && Instancia != this)
                Debug.LogWarning("[Cocina] Hay dos VueloARNt en la escena.", this);
            Instancia = this;
        }

        void OnDestroy()
        {
            if (Instancia == this) Instancia = null;
        }

        public void ConfigurarDestinos(Transform ribosomaLejano, Transform regresoCitoplasma)
        {
            destinoRibosomaLejano = ribosomaLejano;
            destinoRegresoCitoplasma = regresoCitoplasma;
        }

        // Evalúa si al soltar un ARNt con velocidad > 1.2 m/s le atina a un destino lejano dentro de 25°
        public bool IntentarLanzarVR(ARNtCocina arnt, Vector3 velocidadMano)
        {
            float rapidez = velocidadMano.magnitude;
            if (rapidez <= ControlesCocina.VelocidadLanzar) return false;

            Vector3 dirLanzamiento = velocidadMano.normalized;
            Vector3 origen = arnt.transform.position;

            Transform mejorDestino = null;
            float mejorAngulo = ControlesCocina.AnguloAtinar;
            bool esRegreso = false;

            if (destinoRibosomaLejano != null && destinoRibosomaLejano.gameObject.activeInHierarchy)
            {
                float ang = Vector3.Angle(dirLanzamiento, (destinoRibosomaLejano.position - origen).normalized);
                if (ang <= mejorAngulo)
                {
                    mejorAngulo = ang;
                    mejorDestino = destinoRibosomaLejano;
                    esRegreso = false;
                }
            }

            if (destinoRegresoCitoplasma != null && destinoRegresoCitoplasma.gameObject.activeInHierarchy)
            {
                float ang = Vector3.Angle(dirLanzamiento, (destinoRegresoCitoplasma.position - origen).normalized);
                if (ang <= mejorAngulo)
                {
                    mejorAngulo = ang;
                    mejorDestino = destinoRegresoCitoplasma;
                    esRegreso = true;
                }
            }

            if (mejorDestino == null) return false;

            float duracion = ControlCocina.Instancia != null ? ControlCocina.Instancia.Config.vueloLejos : 0.75f;
            Lanzar(arnt, mejorDestino.position, duracion, esRegreso, () =>
            {
                if (esRegreso) AlLlegarRegreso?.Invoke(arnt);
                else AlLlegarRibosomaLejano?.Invoke(arnt);
            });

            return true;
        }

        public void Lanzar(ARNtCocina arnt, Vector3 destino, float duracion, bool conBamboleo = false, Action alLlegar = null)
        {
            if (arnt == null) return;
            if (ControlCocina.Instancia != null)
                ControlCocina.Instancia.EnVuelo(arnt);

            EventosCocina.Sonido("lanzar");
            StartCoroutine(RutinaVuelo(arnt, destino, duracion, conBamboleo, alLlegar));
        }

        IEnumerator RutinaVuelo(ARNtCocina arnt, Vector3 destino, float duracion, bool conBamboleo, Action alLlegar)
        {
            Transform tr = arnt.transform;
            Vector3 inicio = tr.position;
            Quaternion rotInicio = tr.rotation;
            float alturaArco = ControlCocina.Instancia != null ? ControlCocina.Instancia.Config.alturaArcoVuelo : 0.35f;

            float t = 0f;
            while (t < duracion)
            {
                if (arnt == null) yield break;
                if (ControlCocina.Instancia != null && ControlCocina.Instancia.Pausado)
                {
                    yield return null;
                    continue;
                }

                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duracion);

                Vector3 pos = Vector3.Lerp(inicio, destino, k);
                pos.y += alturaArco * Mathf.Sin(k * Mathf.PI);

                if (conBamboleo)
                {
                    float bamb = 0.06f * Mathf.Sin(k * Mathf.PI * 6f) * (1f - k);
                    pos += tr.right * bamb;
                }

                tr.position = pos;
                tr.rotation = rotInicio * Quaternion.Euler(0f, 0f, k * 360f);
                yield return null;
            }

            if (arnt != null)
            {
                tr.position = destino;
                alLlegar?.Invoke();
            }
        }
    }
}

