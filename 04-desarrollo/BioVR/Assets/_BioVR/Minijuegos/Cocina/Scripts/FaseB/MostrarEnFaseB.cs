using System;
using BioVR.Minijuegos.ExpresionGenetica;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // M10 · La BiP y el proteasoma solo aparecen después de encontrar la cuenta 20 (fases Decide, Final y Fin:
    // en Fin para que el proteasoma termine de deshacer la cadena aunque M1 ya haya llamado a Terminar).
    // El objeto raíz se queda encendido para escuchar los eventos; se prende y apaga "visual" y se
    // habilita el toque (Senalable) solo mientras se ve.
    public class MostrarEnFaseB : MonoBehaviour
    {
        [SerializeField] GameObject visual;
        [SerializeField] FaseCocina[] fases = { FaseCocina.Decide, FaseCocina.Final, FaseCocina.Fin };

        Senalable senalable;

        void Awake() => senalable = GetComponent<Senalable>();

        void OnEnable()
        {
            EventosCocina.OnFaseCambiada += Aplicar;
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
        }

        void OnDisable()
        {
            EventosCocina.OnFaseCambiada -= Aplicar;
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
        }

        void Start()
        {
            // Antes del primer turno ControlCocina está en Fin: no hay que mostrarlos
            var control = ControlCocina.Instancia;
            Aplicar(control != null && control.NumeroTurno >= 0 ? control.Fase : FaseCocina.Armando);
        }

        void AlIniciarTurno(TurnoData _) => Aplicar(FaseCocina.Armando);

        void Aplicar(FaseCocina fase)
        {
            bool ver = Array.IndexOf(fases, fase) >= 0;
            if (visual != null) visual.SetActive(ver);
            if (senalable != null) senalable.Habilitado = ver;
        }
    }
}
