using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // M9 · Une la corriente (M2) con los ribosomas (M4) en la escena integrada.
    // VueloARNt avisa cuando un ARNt aventado llega al ribosoma lejano, pero ningún módulo recibía ese aviso:
    // el ARNt se quedaba "en vuelo" para siempre. Aquí se le entrega al ribosoma lejano activo; si no hay
    // ninguno, vuelve al tubo. (El rechazo del ribosoma ya lo regresa PuenteCorriente.Devolver.)
    public class ConexionCorrienteRibosomas : MonoBehaviour
    {
        VueloARNt vuelo;

        void Start()
        {
            vuelo = VueloARNt.Instancia;
            if (vuelo == null)
            {
                Debug.LogError("[Cocina] ConexionCorrienteRibosomas: no hay VueloARNt en la escena.", this);
                return;
            }
            vuelo.AlLlegarRibosomaLejano += AlLlegarLejano;
        }

        void OnDestroy()
        {
            if (vuelo != null) vuelo.AlLlegarRibosomaLejano -= AlLlegarLejano;
        }

        void AlLlegarLejano(ARNtCocina t)
        {
            if (t == null) return;
            foreach (var r in RibosomaCocina.Todos)
            {
                if (r.lejano && r.Activo)
                {
                    r.Recibir(t);
                    return;
                }
            }
            PuenteCorriente.Devolver(t);
        }
    }
}
