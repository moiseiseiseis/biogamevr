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

        private BioVR.Comun.SeguirAlJugador seguidor;

        private void Start()
        {
            if (panelFinal != null) panelFinal.SetActive(false);
            // El Canvas del prefab quedó en (499, 203, −11): a 500 m del jugador. La pantalla salía, pero nadie la
            // veía (prueba del 23-nov: "nunca apareció el botón siguiente"). Se centra en su objeto.
            foreach (var c in GetComponentsInChildren<Canvas>(true))
                if (c.transform.parent == transform)
                {
                    c.transform.localPosition = Vector3.zero;
                    c.transform.localRotation = Quaternion.identity;
                }
            Acomodar();
            // Los tres botones decían "Button" (texto por defecto): nadie reconocía el de pasar al siguiente
            // turno (prueba del 23-nov: "nunca apareció el botón siguiente")
            Etiqueta(botonSiguienteTurno, "SIGUIENTE >");
            Etiqueta(botonRepetir, "Repetir");
            Etiqueta(botonMenu, "Salir");
            // El de seguir, verde: es el que importa
            if (botonSiguienteTurno != null && botonSiguienteTurno.targetGraphic != null)
                botonSiguienteTurno.targetGraphic.color = new Color(0.2f, 0.75f, 0.35f);
            // Las etiquetas de Tiempo y Aciertos decían "Puntos" en el prefab
            string[] filas = { "Puntos", "Tiempo", "Aciertos", "Errores", "Reciclados", "Arreglos", "MalosEntraron" };
            string[] textos = { "Puntos:", "Tiempo:", "Aciertos:", "Errores:", "Reciclados:", "Arreglos:", "Malos que entraron:" };
            if (panelFinal != null)
                for (int i = 0; i < filas.Length; i++)
                {
                    var t = panelFinal.transform.Find("Etiqueta" + filas[i]);
                    var tmp = t != null ? t.GetComponent<TMP_Text>() : null;
                    if (tmp != null) tmp.text = textos[i];
                }
            transform.localScale = Vector3.one * 1.15f;
            // Encima de todo y frente al jugador cuando aparece
            BioVR.Comun.SobreTodo.Aplicar(gameObject);
            seguidor = GetComponent<BioVR.Comun.SeguirAlJugador>();
            if (seguidor == null)
            {
                seguidor = gameObject.AddComponent<BioVR.Comun.SeguirAlJugador>();
                seguidor.distancia = 0.85f;
                seguidor.altura = 0.05f;
                seguidor.anguloLibre = 35f;
            }
        }

        // En el prefab varias piezas quedaron encimadas en el centro (título, una estrella, los puntos): se acomodan
        // aquí, en píxeles del panel (750 × 650). Arriba el título y las estrellas, en medio las 7 cifras en dos
        // columnas (etiqueta | valor), abajo la frase y los tres botones.
        private void Acomodar()
        {
            if (panelFinal == null) return;
            var porNombre = new System.Collections.Generic.Dictionary<string, RectTransform>();
            foreach (Transform t in panelFinal.transform)
                if (t is RectTransform rt) porNombre[t.name] = rt;

            void Poner(string nombre, float x, float y, float ancho = -1f, float alto = -1f)
            {
                if (!porNombre.TryGetValue(nombre, out var rt)) return;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                // Cada pieza del prefab está escalada a 0.001 dentro del Canvas: su posición va en unidades del
                // Canvas, no en píxeles. Por eso las que no estaban en (0, 0) quedaban a decenas de metros.
                float k = rt.localScale.x;
                rt.anchoredPosition = new Vector2(x, y) * k;
                rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0f);
                if (ancho > 0f) rt.sizeDelta = new Vector2(ancho, alto);
            }

            Poner("Fondo", 0f, 0f, 750f, 650f);
            Poner("TextoTitulo", 0f, 270f, 680f, 55f);
            Poner("Estrella1", -80f, 205f); Poner("Estrella2", 0f, 205f); Poner("Estrella3", 80f, 205f);
            string[] filas = { "Puntos", "Tiempo", "Aciertos", "Errores", "Reciclados", "Arreglos", "MalosEntraron" };
            for (int i = 0; i < filas.Length; i++)
            {
                float y = 135f - i * 40f;
                Poner("Etiqueta" + filas[i], -120f, y, 260f, 38f);
                Poner("Texto" + filas[i], 140f, y, 220f, 38f);
            }
            Poner("TextoMalosEncontraron", 140f, 135f - 6 * 40f, 220f, 38f); // el valor de "Malos entraron"
            Poner("TextoFrase", 0f, -175f, 680f, 60f);
            Poner("BotonSiguienteTurno", 230f, -262f, 230f, 62f); // a la derecha: "adelante"
            Poner("BotonRepetir", 0f, -262f, 200f, 62f);
            Poner("BotonMenu", -230f, -262f, 200f, 62f);
        }

        private static void Etiqueta(Button b, string texto)
        {
            if (b == null) return;
            var t = b.GetComponentInChildren<TMP_Text>(true);
            if (t != null) { t.text = texto; t.enableAutoSizing = true; t.fontSizeMin = 10f; t.fontSizeMax = 40f; t.fontStyle = FontStyles.Bold; }
        }

        private void OcultarAlIniciar(TurnoData turno)
        {
            if (panelFinal != null) panelFinal.SetActive(false);
        }

        public void MostrarResultados(MarcadorCocina m)
        {
            if (panelFinal != null) panelFinal.SetActive(true);
            if (seguidor != null) seguidor.TraerAlFrente();

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