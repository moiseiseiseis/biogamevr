using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BioVR.Minijuegos.Cocina
{
    /// <summary>
    /// Pantalla de resultados al terminar cada turno: muestra título, 3 estrellas,
    /// las 7 cifras de desempeño, frase final y botones Siguiente / Repetir / Menú.
    /// </summary>
    public class PantallaFinal : MonoBehaviour
    {
        [Header("Contenedor")]
        [SerializeField] private GameObject panelFinal;

        [Header("Encabezado y Estrellas")]
        [SerializeField] private TMP_Text textoTitulo;
        [SerializeField] private Image[] estrellas = new Image[3];
        [SerializeField] private Color colorEstrellaActiva = new Color(1f, 0.78f, 0.25f, 1f);
        [SerializeField] private Color colorEstrellaApagada = new Color(0.3f, 0.25f, 0.4f, 0.45f);

        [Header("Las 7 Cifras")]
        [SerializeField] private TMP_Text textoPuntos;
        [SerializeField] private TMP_Text textoTiempo;
        [SerializeField] private TMP_Text textoAciertos;
        [SerializeField] private TMP_Text textoErrores;
        [SerializeField] private TMP_Text textoReciclados;
        [SerializeField] private TMP_Text textoArreglos;
        [SerializeField] private TMP_Text textoMalosEntraron;

        [Header("Frase y Botones")]
        [SerializeField] private TMP_Text textoFrase;
        [SerializeField] private Button botonSiguienteTurno;
        [SerializeField] private Button botonRepetir;
        [SerializeField] private Button botonMenu;

        [Header("Escena de Menú / Hangar")]
        [SerializeField] private string nombreEscenaHangar = "Hangar";

        private void OnEnable()
        {
            EventosCocina.OnTerminado += MostrarResultados;
            EventosCocina.OnTurnoIniciado += OcultarAlIniciar;

            if (botonSiguienteTurno != null) botonSiguienteTurno.onClick.AddListener(AlClicSiguiente);
            if (botonRepetir != null) botonRepetir.onClick.AddListener(AlClicRepetir);
            if (botonMenu != null) botonMenu.onClick.AddListener(AlClicMenu);
        }

        private void OnDisable()
        {
            EventosCocina.OnTerminado -= MostrarResultados;
            EventosCocina.OnTurnoIniciado -= OcultarAlIniciar;

            if (botonSiguienteTurno != null) botonSiguienteTurno.onClick.RemoveListener(AlClicSiguiente);
            if (botonRepetir != null) botonRepetir.onClick.RemoveListener(AlClicRepetir);
            if (botonMenu != null) botonMenu.onClick.RemoveListener(AlClicMenu);
        }

        private void Start()
        {
            if (panelFinal != null) panelFinal.SetActive(false);
        }

        private void OcultarAlIniciar(TurnoData turno)
        {
            if (panelFinal != null) panelFinal.SetActive(false);
        }

        public void MostrarResultados(MarcadorCocina m)
        {
            if (panelFinal != null) panelFinal.SetActive(true);

            var cocina = ControlCocina.Instancia;
            int indiceTurno = cocina != null ? cocina.NumeroTurno : 0;
            bool haySiguiente = cocina != null && cocina.HaySiguienteTurno;

            if (textoTitulo != null)
            {
                string claveTitulo = !haySiguiente ? "final_juego_titulo" : "final_turno_titulo";
                textoTitulo.text = cocina != null ? cocina.Texto(claveTitulo) : "TURNO COMPLETADO";
            }

            if (textoFrase != null)
            {
                string claveFrase = indiceTurno switch
                {
                    0 => "final_frase_t1",
                    1 => "final_frase_t2",
                    _ => "final_frase_t3"
                };
                textoFrase.text = cocina != null ? cocina.Texto(claveFrase) : string.Empty;
            }

            int cantidadEstrellas = cocina != null ? cocina.Estrellas : 1;
            for (int i = 0; i < estrellas.Length; i++)
            {
                if (estrellas[i] != null)
                    estrellas[i].color = i < cantidadEstrellas ? colorEstrellaActiva : colorEstrellaApagada;
            }

            if (textoPuntos != null) textoPuntos.text = m.puntos.ToString();
            if (textoTiempo != null) textoTiempo.text = m.Reloj;
            if (textoAciertos != null) textoAciertos.text = m.aciertos.ToString();
            if (textoErrores != null) textoErrores.text = m.errores.ToString();
            if (textoReciclados != null) textoReciclados.text = m.reciclados.ToString();
            if (textoArreglos != null) textoArreglos.text = m.arreglos.ToString();
            if (textoMalosEntraron != null) textoMalosEntraron.text = m.aminoacidosMalos.ToString();

            if (botonSiguienteTurno != null)
                botonSiguienteTurno.gameObject.SetActive(haySiguiente);
        }

        private void AlClicSiguiente()
        {
            if (panelFinal != null) panelFinal.SetActive(false);
            ControlCocina.Instancia?.Sonar("clic");
            ControlCocina.Instancia?.SiguienteTurno();
        }

        private void AlClicRepetir()
        {
            if (panelFinal != null) panelFinal.SetActive(false);
            ControlCocina.Instancia?.Sonar("clic");
            ControlCocina.Instancia?.RepetirTurno();
        }

        private void AlClicMenu()
        {
            ControlCocina.Instancia?.Sonar("clic");
            if (!string.IsNullOrEmpty(nombreEscenaHangar) && Application.CanStreamedLevelBeLoaded(nombreEscenaHangar))
                SceneManager.LoadScene(nombreEscenaHangar);
            else
                SceneManager.LoadScene(0);
        }
    }
}