using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BioVR.Secuencia
{
    public enum GameState { Hangar, Abordaje, Viaje, Llegada }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] ScreenFader fader;
        [SerializeField] float fadeTime = 0.6f;
        [SerializeField] GameState estadoInicial = GameState.Hangar;

        public GameState Estado { get; private set; }

        // Índice de escala: 0 = Hangar (tamaño normal), 1 = Vasos, 2 = Capilar, 3 = Citoplasma.
        // Es el mismo índice que usa GestorEscala (rol 5) para elegir su NivelEscalaData.
        public int Salto { get; private set; }
        public const int SaltoMaximo = 3;

        // Otros roles se suscriben a estos eventos para activar/desactivar su zona
        public event Action<GameState> OnEstadoCambiado;
        public event Action<int> OnSaltoCambiado;

        bool enTransicion;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError($"[Secuencia] Hay más de un GameManager en la escena; se ignora el de '{name}'.", this);
                enabled = false;
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start() => IrA(estadoInicial, instantaneo: true);

        // ---- API pública (para los botones del rol 3) ----
        public void Avanzar()
        {
            if (enTransicion) { Debug.Log("[Secuencia] Avanzar ignorado: hay una transición en curso."); return; }
            switch (Estado)
            {
                case GameState.Hangar: IrA(GameState.Abordaje); break;
                case GameState.Abordaje: IrA(GameState.Viaje); break;
                case GameState.Viaje:
                    if (Salto < SaltoMaximo) StartCoroutine(HacerSalto(Salto + 1));
                    else IrA(GameState.Llegada);
                    break;
            }
        }

        public void IrA(GameState nuevo, bool instantaneo = false)
        {
            if (enTransicion) { Debug.Log($"[Secuencia] IrA({nuevo}) ignorado: hay una transición en curso."); return; }
            StartCoroutine(Transicion(nuevo, instantaneo));
        }

        // ---- Internos ----
        IEnumerator Transicion(GameState nuevo, bool instantaneo)
        {
            enTransicion = true;
            if (!instantaneo && fader != null) yield return fader.FadeA(1f, fadeTime);

            Estado = nuevo;
            Salto = SaltoDeEstado(nuevo);
            OnEstadoCambiado?.Invoke(Estado);
            Debug.Log($"[Secuencia] Estado: {Estado} | Salto: {Salto}");
            OnSaltoCambiado?.Invoke(Salto);

            if (!instantaneo && fader != null) yield return fader.FadeA(0f, fadeTime);
            enTransicion = false;
        }

        // Al entrar a Viaje ya se hizo el primer salto Matrioska (Hangar → Vasos).
        static int SaltoDeEstado(GameState estado) => estado switch
        {
            GameState.Viaje => 1,
            GameState.Llegada => SaltoMaximo,
            _ => 0,
        };

        IEnumerator HacerSalto(int nuevoSalto)
        {
            enTransicion = true;
            if (fader != null) yield return fader.FadeA(1f, fadeTime);
            Salto = nuevoSalto;
            Debug.Log($"[Secuencia] Salto: {Salto}");
            OnSaltoCambiado?.Invoke(Salto);
            if (fader != null) yield return fader.FadeA(0f, fadeTime);
            enTransicion = false;
        }

        // ---- Atajo de depuración: 1-4 = estados, Espacio = avanzar ----
        void Update()
        {
            var k = Keyboard.current;
            if (k == null) return;
            if (k.digit1Key.wasPressedThisFrame) IrA(GameState.Hangar);
            if (k.digit2Key.wasPressedThisFrame) IrA(GameState.Abordaje);
            if (k.digit3Key.wasPressedThisFrame) IrA(GameState.Viaje);
            if (k.digit4Key.wasPressedThisFrame) IrA(GameState.Llegada);
            if (k.spaceKey.wasPressedThisFrame) Avanzar();
        }
    }
}
