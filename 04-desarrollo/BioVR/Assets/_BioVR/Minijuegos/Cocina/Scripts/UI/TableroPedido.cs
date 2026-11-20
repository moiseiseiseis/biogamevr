using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BioVR.Minijuegos.Cocina
{
    /// <summary>
    /// Controla el tablero superior de pedido (80x26 cm sobre tablero-pedido.fbx):
    /// muestra cadenas hechas, reloj mm:ss, puntos, 3 estrellas y alerta de codón raro.
    /// </summary>
    public class TableroPedido : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private TMP_Text textoPedido;
        [SerializeField] private TMP_Text textoReloj;
        [SerializeField] private TMP_Text textoPuntos;
        [SerializeField] private TMP_Text textoCodonRaro;

        [Header("Estrellas (3 iconos)")]
        [SerializeField] private Image[] iconosEstrellas = new Image[3];
        [SerializeField] private Color colorEstrellaActiva = new Color(1f, 0.78f, 0.25f, 1f);
        [SerializeField] private Color colorEstrellaApagada = new Color(0.3f, 0.25f, 0.4f, 0.45f);

        [Header("Alerta Codón Raro")]
        [SerializeField] private GameObject contenedorCodonRaro;

        private void Start()
        {
            if (contenedorCodonRaro != null)
                contenedorCodonRaro.SetActive(false);
        }

        private void Update()
        {
            var cocina = ControlCocina.Instancia;
            if (cocina == null || cocina.Turno == null) return;

            var m = cocina.Marcador;
            var turno = cocina.Turno;

            if (textoPedido != null)
            {
                int totalCadenas = turno.ribosomas != null ? turno.ribosomas.Length : 1;
                textoPedido.text = $"PEDIDO: {m.cadenasTerminadas} / {totalCadenas}";
            }

            if (textoReloj != null)
                textoReloj.text = m.Reloj;

            if (textoPuntos != null)
                textoPuntos.text = $"{m.puntos} PTS";

            ActualizarEstrellas(cocina.Estrellas);
            ActualizarCodonRaro(cocina);
        }

        private void ActualizarEstrellas(int activas)
        {
            for (int i = 0; i < iconosEstrellas.Length; i++)
            {
                if (iconosEstrellas[i] != null)
                    iconosEstrellas[i].color = i < activas ? colorEstrellaActiva : colorEstrellaApagada;
            }
        }

        private void ActualizarCodonRaro(ControlCocina cocina)
        {
            bool activo = cocina.CodonRaroActivo;
            if (contenedorCodonRaro != null)
                contenedorCodonRaro.SetActive(activo);

            if (activo && textoCodonRaro != null)
            {
                int segundos = Mathf.CeilToInt(cocina.SegundosRaro);
                textoCodonRaro.text = $"{cocina.Texto("tablero_codon_raro")} ({segundos}s)";
            }
        }
    }
}