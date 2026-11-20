using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

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

        readonly Dictionary<ARNtCocina, Vector3> ultimaPos = new Dictionary<ARNtCocina, Vector3>();
        readonly Dictionary<ARNtCocina, Vector3> velocidadCalculada = new Dictionary<ARNtCocina, Vector3>();

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

        public void RegistrarARNt(ARNtCocina arnt)
        {
            if (arnt == null) return;
            var grab = arnt.GetComponent<XRGrabInteractable>();
            if (grab == null) return;

            grab.activated.AddListener(args => AlApretarGatilloSosteniendo(arnt, args));
        }

        public void ActualizarRastreoMano(ARNtCocina arnt)
        {
            if (arnt == null) return;
            Vector3 actual = arnt.transform.position;
            if (ultimaPos.TryGetValue(arnt, out Vector3 prev) && Time.deltaTime > 0.0001f)
            {
                Vector3 velInstantanea = (actual - prev) / Time.deltaTime;
                velocidadCalculada[arnt] = velocidadCalculada.TryGetValue(arnt, out Vector3 vAnt)
                    ? Vector3.Lerp(vAnt, velInstantanea, 0.5f)
                    : velInstantanea;
            }
            ultimaPos[arnt] = actual;
        }

        public void LimpiarRastreo(ARNtCocina arnt)
        {
            if (arnt == null) return;
            ultimaPos.Remove(arnt);
            velocidadCalculada.Remove(arnt);
        }

        // Opción A VR: Apretar el gatillo mientras sostienes el ARNt apuntando con el rayo
        void AlApretarGatilloSosteniendo(ARNtCocina arnt, ActivateEventArgs args)
        {
            if (arnt == null || arnt.Lugar != LugarARNt.Mano) return;
            Transform interactorTr = args.interactorObject?.transform;
            if (interactorTr == null) return;

            if (IntentarLanzarPorDireccion(arnt, interactorTr.forward))
            {
                var grab = arnt.GetComponent<XRGrabInteractable>();
                if (grab != null && grab.isSelected && grab.interactionManager != null && grab.firstInteractorSelecting != null)
                    grab.interactionManager.SelectExit(grab.firstInteractorSelecting, grab);
            }
        }


        // Opción B VR: Soltar el grip moviendo la mano rápido (> 1.2 m/s)
        public bool IntentarLanzarVR(ARNtCocina arnt, Vector3 velocidadFallback)
        {
            Vector3 vel = velocidadCalculada.TryGetValue(arnt, out Vector3 vCalc) ? vCalc : velocidadFallback;
            LimpiarRastreo(arnt);

            if (vel.magnitude <= ControlesCocina.VelocidadLanzar) return false;
            return IntentarLanzarPorDireccion(arnt, vel.normalized);
        }

        bool IntentarLanzarPorDireccion(ARNtCocina arnt, Vector3 dirLanzamiento)
        {
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


