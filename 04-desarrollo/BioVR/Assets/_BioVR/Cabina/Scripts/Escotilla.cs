using System;
using System.Collections;
using BioVR.Jugador;
using BioVR.Secuencia;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BioVR.Cabina
{
    // Escotilla trasera de la cabina: en el Hangar deja salir al jugador para ver la nave desde afuera.
    // El modelo de Diseño no tiene hueco detrás de la escotilla (la pared es sólida), así que es una puerta
    // "de paso": al presionarla hay un fundido a negro y el jugador aparece afuera, en "puntoAfuera".
    // Afuera, el panel "Volver a la cabina" lo regresa al asiento. Solo funciona en el Hangar; en cualquier
    // cambio de estado el jugador vuelve al asiento (Recentrar) y el panel de afuera se apaga.
    // Lo arma el menú BioVR → Agregar movimiento y escotilla.
    public class Escotilla : MonoBehaviour
    {
        [Tooltip("INT_Escotilla del modelo de la cabina, con su XRSimpleInteractable.")]
        [SerializeField] XRSimpleInteractable botonSalir;

        [Tooltip("Panel afuera de la cabina para volver a entrar.")]
        [SerializeField] XRSimpleInteractable botonEntrar;

        [Tooltip("Letrero \"Salir\" sobre la escotilla; solo se ve en el Hangar.")]
        [SerializeField] GameObject letreroSalir;

        [Tooltip("Dónde aparece el jugador al salir (en el piso); su eje Z azul es hacia dónde mira.")]
        [SerializeField] Transform puntoAfuera;

        [SerializeField] Recentrar jugador;
        [SerializeField] ScreenFader fader;
        [SerializeField] float fadeTime = 0.4f;

        // Avisa cuando el jugador sale (true) o vuelve a entrar (false); lo usa el tutorial
        public static event Action<bool> OnAfuera;

        public bool Afuera { get; private set; }

        GameManager gm;
        bool ocupada;

        void Start()
        {
            gm = GameManager.Instance;
            if (botonSalir == null || botonEntrar == null || puntoAfuera == null || jugador == null || gm == null)
            {
                Debug.LogError("[Cabina] Escotilla necesita sus dos botones, el punto de afuera, Recentrar y el GameManager.", this);
                enabled = false;
                return;
            }
            botonSalir.selectEntered.AddListener(AlPresionarSalir);
            botonEntrar.selectEntered.AddListener(AlPresionarEntrar);
            gm.OnEstadoCambiado += AlCambiarEstado;
            AlCambiarEstado(gm.Estado);
        }

        void OnDestroy()
        {
            if (botonSalir != null) botonSalir.selectEntered.RemoveListener(AlPresionarSalir);
            if (botonEntrar != null) botonEntrar.selectEntered.RemoveListener(AlPresionarEntrar);
            if (gm != null) gm.OnEstadoCambiado -= AlCambiarEstado;
        }

        void AlPresionarSalir(SelectEnterEventArgs args) => Salir();
        void AlPresionarEntrar(SelectEnterEventArgs args) => Entrar();

        public void Salir()
        {
            if (gm.Estado != GameState.Hangar) { Debug.Log("[Cabina] La escotilla solo se abre en el Hangar."); return; }
            if (Afuera || ocupada || gm.Pausado || gm.EnTransicion) return;
            StartCoroutine(Pasar(afuera: true));
        }

        public void Entrar()
        {
            if (!Afuera || ocupada || gm.Pausado || gm.EnTransicion) return;
            StartCoroutine(Pasar(afuera: false));
        }

        IEnumerator Pasar(bool afuera)
        {
            ocupada = true;
            if (fader != null) yield return fader.FadeA(1f, fadeTime, Color.black);

            if (afuera) jugador.LlevarA(puntoAfuera.position, puntoAfuera.forward);
            else jugador.Centrar();
            Afuera = afuera;
            botonEntrar.gameObject.SetActive(afuera);
            Debug.Log(afuera ? "[Cabina] El jugador salió al hangar." : "[Cabina] El jugador volvió a la cabina.");

            if (fader != null) yield return fader.FadeA(0f, fadeTime);
            ocupada = false;
            OnAfuera?.Invoke(afuera);
        }

        // Todo cambio de estado ocurre bajo un fundido y Recentrar sienta al jugador: ya está adentro
        void AlCambiarEstado(GameState estado)
        {
            if (Afuera) OnAfuera?.Invoke(false);
            Afuera = false;
            botonEntrar.gameObject.SetActive(false);
            if (letreroSalir != null) letreroSalir.SetActive(estado == GameState.Hangar);
        }
    }
}
