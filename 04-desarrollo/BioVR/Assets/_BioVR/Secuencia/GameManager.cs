using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { Hangar, Abordaje, Viaje, Llegada }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] ScreenFader fader;
    [SerializeField] float fadeTime = 0.6f;
    [SerializeField] GameState estadoInicial = GameState.Hangar;

    public GameState Estado { get; private set; }
    public int Salto { get; private set; } // 0..3 (solo dentro de Viaje)

    // Otros roles se suscriben a estos eventos para activar/desactivar su zona
    public event Action<GameState> OnEstadoCambiado;
    public event Action<int> OnSaltoCambiado;

    bool enTransicion;

    void Awake() => Instance = this;
    void Start() => IrA(estadoInicial, instantaneo: true);

    // ---- API pública (para los botones del rol 3) ----
    public void Avanzar()
    {
        if (enTransicion) return;
        switch (Estado)
        {
            case GameState.Hangar: IrA(GameState.Abordaje); break;
            case GameState.Abordaje: IrA(GameState.Viaje); break;
            case GameState.Viaje:
                if (Salto < 3) StartCoroutine(HacerSalto(Salto + 1));
                else IrA(GameState.Llegada);
                break;
        }
    }

    public void IrA(GameState nuevo, bool instantaneo = false)
    {
        if (enTransicion) return;
        StartCoroutine(Transicion(nuevo, instantaneo));
    }

    // ---- Internos ----
    IEnumerator Transicion(GameState nuevo, bool instantaneo)
    {
        enTransicion = true;
        if (!instantaneo) yield return fader.FadeA(1f, fadeTime);

        Estado = nuevo;
        Salto = (nuevo == GameState.Llegada) ? 3 : 0;
        OnEstadoCambiado?.Invoke(Estado);
        Debug.Log($"Estado: {Estado} | Salto: {Salto}");
        OnSaltoCambiado?.Invoke(Salto);

        if (!instantaneo) yield return fader.FadeA(0f, fadeTime);
        enTransicion = false;
    }

    IEnumerator HacerSalto(int nuevoSalto)
    {
        enTransicion = true;
        yield return fader.FadeA(1f, fadeTime);
        Salto = nuevoSalto;
        OnSaltoCambiado?.Invoke(Salto);
        yield return fader.FadeA(0f, fadeTime);
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