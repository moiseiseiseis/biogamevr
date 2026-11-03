using System.Linq;
using TMPro;
using UnityEngine;

namespace BioVR.Minijuegos.ExpresionGenetica
{
    // Mesa de prueba de controles (Mo2, para la prueba T1 de Marcos): cuenta las ranuras llenas y
    // muestra en el letrero qué hacer, los errores y cuándo se terminó. El botón Reiniciar regresa las piezas a la mesa.
    // Lo arma el menú BioVR → Armar escena Expresión Genética.
    public class MesaPrueba : MonoBehaviour
    {
        [SerializeField] RanuraSeleccion[] ranuras = new RanuraSeleccion[0];
        [SerializeField] PiezaManipulable[] piezas = new PiezaManipulable[0];
        [SerializeField] TextMeshPro letrero;

        [TextArea(2, 4)]
        [SerializeField] string instruccion =
            "Apunta a una pieza y aprieta el grip para atraerla.\nGírala y suéltala en su ranura con la cara blanca hacia ti.";

        void OnEnable()
        {
            foreach (var r in ranuras)
            {
                r.OnAcierto += AlAcertar;
                r.OnError += AlFallar;
            }
        }

        void OnDisable()
        {
            foreach (var r in ranuras)
            {
                r.OnAcierto -= AlAcertar;
                r.OnError -= AlFallar;
            }
        }

        void Start() => Escribir(instruccion);

        void AlAcertar(RanuraSeleccion ranura)
        {
            int llenas = ranuras.Count(r => r.Llena);
            Escribir(llenas == ranuras.Length
                ? "¡Listo! Todas las piezas están en su lugar.\nToca Reiniciar para probar otra vez."
                : $"¡Bien! {llenas} de {ranuras.Length}.");
            Debug.Log($"[MesaPrueba] Acierto en '{ranura.name}' ({llenas}/{ranuras.Length}).");
        }

        void AlFallar(RanuraSeleccion ranura, string motivo)
        {
            Escribir(motivo);
            Debug.Log($"[MesaPrueba] Error en '{ranura.name}': {motivo}");
        }

        void Escribir(string texto)
        {
            if (letrero != null) letrero.text = texto;
        }

        // Botón Reiniciar: saca las piezas de las ranuras y las regresa a la mesa
        public void Reiniciar()
        {
            foreach (var r in ranuras) r.Vaciar();
            foreach (var p in piezas) p.Regresar();
            Escribir(instruccion);
        }
    }
}
