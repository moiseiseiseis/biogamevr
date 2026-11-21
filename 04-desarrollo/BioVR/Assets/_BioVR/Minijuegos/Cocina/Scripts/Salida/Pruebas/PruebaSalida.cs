using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Minijuegos.Cocina
{
    // SOLO PARA Cocina_M5 (escena de prueba de Iker). Los ARNt de prueba vienen del estante de PruebaRibosomas,
    // no de la corriente de M2, así que nadie le avisa a VueloARNt cuando se sueltan. Esta prueba hace ese
    // papel con los ARNt USADOS (los que llegan a la bandeja):
    //
    //   - Mientras están en la mano, le pasa su posición a VueloARNt (ActualizarRastreoMano).
    //   - Al soltarlos, le pide a VueloARNt que intente el lanzamiento (IntentarLanzarVR: más de 1.2 m/s y
    //     menos de 25° del destino). Si atina, M2 lo hace volar y al llegar avisa a M5 (AlLlegarRegreso).
    //   - También funciona la otra opción de M2: con el usado en la mano, apuntar con el rayo y apretar el gatillo.
    //   - Tecla C: avienta al regreso el primer ARNt de la bandeja con VueloARNt.Lanzar (para no depender
    //     del simulador).
    //
    // Si la escena no tiene VueloARNt, usa un vuelo propio equivalente (0.75 s, arco y bamboleo).
    public class PruebaSalida : MonoBehaviour
    {
        [SerializeField] BandejaSalida bandeja;

        readonly Dictionary<ARNtCocina, UnityAction<SelectExitEventArgs>> seguidos =
            new Dictionary<ARNtCocina, UnityAction<SelectExitEventArgs>>();
        readonly HashSet<ARNtCocina> registrados = new HashSet<ARNtCocina>(); // gatillo + rayo de VueloARNt

        static ControlCocina Control => ControlCocina.Instancia;
        static VueloARNt Vuelo => VueloARNt.Instancia;

        void OnEnable()
        {
            EventosCocina.OnARNtASalida += Seguir;
            EventosCocina.OnReciclado += Dejar;
            EventosCocina.OnReciclado += Reportar;
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
        }

        void OnDisable()
        {
            EventosCocina.OnARNtASalida -= Seguir;
            EventosCocina.OnReciclado -= Dejar;
            EventosCocina.OnReciclado -= Reportar;
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
            foreach (var t in new List<ARNtCocina>(seguidos.Keys)) Dejar(t);
        }

        // En esta escena no está el tablero de M8: los puntos se ven en la consola
        void Reportar(ARNtCocina t)
        {
            if (Control == null) return;
            var m = Control.Marcador;
            Debug.Log($"[M5 prueba] Llegó al regreso al citoplasma: {t.name}. Puntos: {m.puntos} · reciclados: {m.reciclados}");
        }

        void AlIniciarTurno(TurnoData _)
        {
            foreach (var t in new List<ARNtCocina>(seguidos.Keys)) Dejar(t);
            seguidos.Clear();
        }

        // Solo los usados se avientan al regreso: se siguen desde que llegan a la bandeja
        void Seguir(ARNtCocina t)
        {
            if (t == null || seguidos.ContainsKey(t)) return;
            UnityAction<SelectExitEventArgs> alSoltar = args => AlSoltar(t, args);
            var agarre = t.GetComponent<XRGrabInteractable>();
            if (agarre != null) agarre.selectExited.AddListener(alSoltar);
            seguidos[t] = alSoltar;
            // Opción de M2: con el ARNt en la mano, apuntar con el rayo y apretar el gatillo
            if (Vuelo != null && registrados.Add(t)) Vuelo.RegistrarARNt(t);
        }

        void Dejar(ARNtCocina t)
        {
            if (!seguidos.TryGetValue(t, out var alSoltar)) return;
            seguidos.Remove(t);
            if (t == null) return;
            var agarre = t.GetComponent<XRGrabInteractable>();
            if (agarre != null) agarre.selectExited.RemoveListener(alSoltar);
            if (Vuelo != null) Vuelo.LimpiarRastreo(t);
        }

        void Update()
        {
            if (Vuelo != null)
                foreach (var t in seguidos.Keys)
                    if (t != null && t.Lugar == LugarARNt.Mano) Vuelo.ActualizarRastreoMano(t);

            if (Control == null || Control.Fase != FaseCocina.Armando || Control.Pausado) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var k = Keyboard.current;
            if (k != null && k.cKey.wasPressedThisFrame) AventarPrimero();
#endif
        }

        void AlSoltar(ARNtCocina t, SelectExitEventArgs args)
        {
            if (args.isCanceled || args.interactorObject is XRSocketInteractor) return;
            if (t == null || t.Lugar != LugarARNt.Mano) return;
            if (Vuelo != null) Vuelo.IntentarLanzarVR(t, Vector3.zero);
            // Si no atinó sigue en la mano: PruebaRibosomas lo regresa al estante (como haría la corriente de M2)
        }

        void AventarPrimero()
        {
            var destino = RegresoCitoplasma.Todos.Count > 0 ? RegresoCitoplasma.Todos[0] : null;
            var t = bandeja != null ? bandeja.Primero() : null;
            if (destino == null || t == null) { Debug.Log("[M5 prueba] No hay ARNt quietos en la bandeja o falta el regreso."); return; }
            if (!Control.Agarrar(t)) return;

            if (Vuelo != null)
                Vuelo.Lanzar(t, destino.Boca.position, Control.Config.vueloLejos, true, () => destino.Recibir(t));
            else
                StartCoroutine(VolarSinM2(t, destino));
        }

        IEnumerator VolarSinM2(ARNtCocina t, RegresoCitoplasma destino)
        {
            Control.EnVuelo(t);
            var desde = t.transform.position;
            float duracion = Control.Config.vueloLejos;
            for (float k = 0f; k < 1f; k += Time.deltaTime / duracion)
            {
                if (t == null) yield break;
                var p = Vector3.Lerp(desde, destino.Boca.position, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * Control.Config.alturaArcoVuelo;
                p += t.transform.right * 0.06f * Mathf.Sin(k * Mathf.PI * 6f) * (1f - k);
                t.transform.position = p;
                yield return null;
            }
            if (t == null) yield break;
            t.transform.position = destino.Boca.position;
            destino.Recibir(t);
        }
    }
}
