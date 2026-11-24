using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BioVR.Minijuegos.Cocina
{
    /// <summary>
    /// Gestiona las 19 ventanas del tutorial de la Cocina, pausando el juego mientras
    /// están abiertas, señalando la estación con MarcaTutorial y permitiendo cerrar
    /// con el botón Entendido o con el botón primario del control VR (A / X).
    /// Con "modoGuiado" (por defecto, desde la prueba del 21-nov) las ventanas las decide GuiaCocina,
    /// paso por paso según lo que hace el jugador; esta clase solo pone la ventana y la marca.
    /// </summary>
    public class TutorialCocina : MonoBehaviour
    {
        [Serializable]
        public class ObjetivoClave
        {
            public string clave;
            public Transform objetivo;
        }

        [Header("Datos")]
        [SerializeField] private TutorialesCocina baseDatos;

        [Header("UI de la Ventana")]
        [SerializeField] private GameObject panelVentana;
        [SerializeField] private TMP_Text textoTitulo;
        [SerializeField] private TMP_Text textoCuerpo;
        [SerializeField] private TMP_Text textoContador;
        [SerializeField] private Image imagenIlustracion;
        [SerializeField] private Button botonEntendido;

        [Header("Marca 3D en Escena")]
        [SerializeField] private MarcaTutorial marcaTutorial;
        [SerializeField] private List<ObjetivoClave> objetivos = new List<ObjetivoClave>();

        [Header("Input VR (Botón A / X)")]
        [SerializeField] private InputActionProperty accionEntendido;

        [Header("Tutorial guiado")]
        [Tooltip("Encendido: GuiaCocina dice qué hacer paso por paso. Apagado: las 19 ventanas de antes.")]
        [SerializeField] private bool modoGuiado = true;
        private GuiaCocina guia;

        private readonly Queue<EntradaTutorial> cola = new Queue<EntradaTutorial>();
        private static readonly HashSet<string> clavesVistas = new HashSet<string>();
        private bool ventanaAbierta;

        [Header("Ventana que sigue al jugador (prueba del 23-nov)")]
        [SerializeField] private bool seguirAlJugador = true;
        private BioVR.Comun.SeguirAlJugador seguidor;

        private void Awake()
        {
            // El botón decía "Button" (texto por defecto del prefab)
            if (botonEntendido != null)
            {
                var etiqueta = botonEntendido.GetComponentInChildren<TMP_Text>(true);
                if (etiqueta != null)
                {
                    etiqueta.text = "Entendido (" + BioVR.Comun.EtiquetasMandos.Colores.Etiqueta("A", BioVR.Comun.EtiquetasMandos.Colores.A) + " / " +
                                    BioVR.Comun.EtiquetasMandos.Colores.Etiqueta("X", BioVR.Comun.EtiquetasMandos.Colores.X) + ")";
                }
            }
            if (seguirAlJugador)
            {
                seguidor = GetComponent<BioVR.Comun.SeguirAlJugador>();
                if (seguidor == null)
                {
                    seguidor = gameObject.AddComponent<BioVR.Comun.SeguirAlJugador>();
                    // Más cerca que todo lo de la mesa (el letrero RIBOSOMA queda a ~0.65 m y se le encimaba),
                    // a la altura de la mirada, entre el ribosoma (abajo) y la tele del pedido (arriba)
                    seguidor.distancia = 0.55f;
                    // 20 cm arriba de la mirada (inclinada hacia el jugador): a la altura de los ojos tapaba los
                    // componentes (prueba del 23-nov)
                    seguidor.altura = 0.2f;
                    transform.localScale = Vector3.one * 0.62f;
                }
            }
            escalaSiguiendo = transform.localScale;
            EventosCocina.OnTurnoIniciado += AcomodarSegunTurno;
            // La ventana no debe tapar el rayo: solo el botón Entendido recibe clics (si no, al estar frente al
            // jugador, no dejaba apuntar al botón FIN ni a la chaperona)
            foreach (var g in GetComponentsInChildren<Graphic>(true))
                if (botonEntendido == null || !g.transform.IsChildOf(botonEntendido.transform)) g.raycastTarget = false;

            if (!modoGuiado) return;
            guia = GetComponent<GuiaCocina>();
            if (guia == null) guia = gameObject.AddComponent<GuiaCocina>();
            guia.Usar(this);
        }

        // Prueba del 24-nov: desde el turno 2 (dos ribosomas) la ventana que sigue al jugador estorbaba ("son
        // demasiados elementos"). Desde el turno 2 se queda fija a la izquierda, detrás de la corriente de ARNt,
        // con letras más grandes. En el turno 1 sigue al jugador como antes.
        // Constantes, no campos serializados: Unity guarda el valor por defecto de un campo nuevo en su copia
        // importada del prefab y no lo actualiza al cambiar el código (pasó el 23 y el 24-nov).
        private const float AnguloFijo = 75f;    // + = a la izquierda (PosicionMesa)
        private const float RadioFijo = 1.3f;
        private const float AlturaFija = 1.97f;  // más abajo, el botón Entendido quedaba detrás de la fila de ARNt
        // Tamaño en el mundo: a 1.3 m, 2.0 ≈ 1.4 veces más grande a la vista que la ventana que sigue (0.62 a 0.55 m)
        private const float EscalaFija = 2.0f;
        private Vector3 escalaSiguiendo;

        private void OnDestroy() => EventosCocina.OnTurnoIniciado -= AcomodarSegunTurno;

        private void AcomodarSegunTurno(TurnoData turno)
        {
            var c = ControlCocina.Instancia;
            bool fija = c != null && c.NumeroTurno >= 1;
            if (seguidor != null) seguidor.enabled = !fija;
            if (!fija)
            {
                transform.localScale = escalaSiguiendo;
                if (seguidor != null) seguidor.TraerAlFrente();
                return;
            }
            // Colgada de AnclaCocina (el armador la pone ahí); un Canvas se lee con su +Z alejándose del jugador
            transform.localPosition = PosicionMesa.En(AnguloFijo, RadioFijo, AlturaFija);
            var desdeOjos = transform.localPosition - new Vector3(0f, 1.2f, 0f);
            transform.localRotation = Quaternion.LookRotation(desdeOjos, Vector3.up);
            transform.localScale = Vector3.one * EscalaFija;
        }

        private void OnEnable()
        {
            if (botonEntendido != null)
                botonEntendido.onClick.AddListener(CerrarVentanaActual);
            if (accionEntendido.action != null)
            {
                accionEntendido.action.Enable();
                accionEntendido.action.performed += AlPresionarBotonVR;
            }
            if (modoGuiado) return; // la guía escucha sus propios eventos

            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
            EventosCocina.OnCodonCambiado += AlCambiarCodon;
            EventosCocina.OnARNtCargado += AlCargarARNt;
            EventosCocina.OnAminoacidoAceptado += AlAceptarAminoacido;
            EventosCocina.OnReciclado += AlReciclar;
            EventosCocina.OnHorquilla += AlCambiarHorquilla;
            EventosCocina.OnCodonRaro += AlCambiarCodonRaro;
            EventosCocina.OnFaseCambiada += AlCambiarFase;
            EventosCocina.OnCuentaEncontrada += AlEncontrarCuenta;
            EventosCocina.OnChaperona += AlUsarChaperona;
        }

        private void OnDisable()
        {
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
            EventosCocina.OnCodonCambiado -= AlCambiarCodon;
            EventosCocina.OnARNtCargado -= AlCargarARNt;
            EventosCocina.OnAminoacidoAceptado -= AlAceptarAminoacido;
            EventosCocina.OnReciclado -= AlReciclar;
            EventosCocina.OnHorquilla -= AlCambiarHorquilla;
            EventosCocina.OnCodonRaro -= AlCambiarCodonRaro;
            EventosCocina.OnFaseCambiada -= AlCambiarFase;
            EventosCocina.OnCuentaEncontrada -= AlEncontrarCuenta;
            EventosCocina.OnChaperona -= AlUsarChaperona;

            if (botonEntendido != null)
                botonEntendido.onClick.RemoveListener(CerrarVentanaActual);

            if (accionEntendido.action != null)
                accionEntendido.action.performed -= AlPresionarBotonVR;
        }

        private void Start()
        {
            if (modoGuiado) return; // la guía ya pudo haber abierto su primer paso
            if (panelVentana != null) panelVentana.SetActive(false);
            if (marcaTutorial != null) marcaTutorial.Ocultar();
        }

        private void AlPresionarBotonVR(InputAction.CallbackContext ctx)
        {
            if (modoGuiado) { if (guia != null) guia.Entendido(); return; }
            if (ventanaAbierta)
                CerrarVentanaActual();
        }

        // --- Ventana para GuiaCocina ---

        public void MostrarGuia(string titulo, string cuerpo, string pie, bool conBoton, Transform objetivo)
        {
            // Un paso con información (botón Entendido) se pone al frente de inmediato
            if (conBoton && seguidor != null && (panelVentana == null || !panelVentana.activeSelf)) seguidor.TraerAlFrente();
            if (panelVentana != null) panelVentana.SetActive(true);
            if (textoTitulo != null) textoTitulo.text = titulo;
            if (textoCuerpo != null)
            {
                textoCuerpo.text = cuerpo;
            }
            if (textoContador != null) textoContador.text = pie ?? string.Empty;
            if (imagenIlustracion != null) imagenIlustracion.gameObject.SetActive(false);
            if (botonEntendido != null) botonEntendido.gameObject.SetActive(conBoton);
            AcomodarVentana(conBoton);
            if (marcaTutorial != null)
            {
                if (objetivo != null) marcaTutorial.Senalar(objetivo);
                else marcaTutorial.Ocultar();
            }
        }

        // Prueba del 24-nov: "las letras del tutorial deben ser más grandes". Antes el cuerpo se encogía solo
        // (auto-size de 15 a 26) para caber en una ventana fija de 0.62 × 0.44 y los pasos largos quedaban casi
        // en el mínimo. Ahora la letra es fija y grande, y la ventana crece hacia arriba con el texto: el botón se
        // queda donde estaba (hacia abajo tapaba el letrero del ribosoma). Medidas en metros del espacio de PanelVentana (los textos van a escala 0.001, así que
        // 1 unidad de UI = 1 mm); el botón cuelga del Canvas, que mide en mm.
        private const float LetraTitulo = 40f, LetraCuerpo = 32f, LetraPie = 28f, LetraBoton = 28f;
        private const float AnchoVentana = 0.72f, Margen = 0.03f, Hueco = 0.018f, AltoFila = 0.065f;
        private const float BordeAbajo = -0.22f; // borde de abajo de la ventana de antes (0.44 / 2)
        private RectTransform fondo;
        private string ultimoAcomodo;

        private void AcomodarVentana(bool conBoton)
        {
            if (textoTitulo == null || textoCuerpo == null || panelVentana == null) return;
            string clave = textoTitulo.text + " | " + textoCuerpo.text + " | " + conBoton;
            if (clave == ultimoAcomodo) return; // la guía vuelve a poner la ventana 10 veces por segundo
            ultimoAcomodo = clave;
            if (fondo == null)
            {
                var f = panelVentana.transform.Find("Fondo");
                if (f != null) fondo = f as RectTransform;
            }

            float anchoTexto = (AnchoVentana - 2f * Margen) * 1000f;
            foreach (var t in new[] { textoTitulo, textoCuerpo })
            {
                t.enableAutoSizing = false;
                t.textWrappingMode = TextWrappingModes.Normal;
                t.overflowMode = TextOverflowModes.Overflow;
                t.alignment = t == textoTitulo ? TextAlignmentOptions.Center : TextAlignmentOptions.Top;
            }
            textoTitulo.fontSize = LetraTitulo;
            textoCuerpo.fontSize = LetraCuerpo;
            if (textoContador != null) { textoContador.enableAutoSizing = false; textoContador.fontSize = LetraPie; }

            float altoTitulo = textoTitulo.GetPreferredValues(textoTitulo.text, anchoTexto, 0f).y * 0.001f;
            float altoCuerpo = textoCuerpo.GetPreferredValues(textoCuerpo.text, anchoTexto, 0f).y * 0.001f;
            float alto = Margen + altoTitulo + Hueco + altoCuerpo + Hueco + AltoFila + Margen;

            float BordeArriba = BordeAbajo + alto;
            float y = BordeArriba - Margen - altoTitulo * 0.5f;
            Colocar(textoTitulo.rectTransform, 0f, y, anchoTexto, altoTitulo * 1000f);
            y = BordeArriba - Margen - altoTitulo - Hueco - altoCuerpo * 0.5f;
            Colocar(textoCuerpo.rectTransform, 0f, y, anchoTexto, altoCuerpo * 1000f);
            float yFila = BordeArriba - alto + Margen + AltoFila * 0.5f;
            if (textoContador != null)
            {
                textoContador.alignment = conBoton ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
                float anchoPie = conBoton ? anchoTexto * 0.5f : anchoTexto;
                float xPie = conBoton ? -AnchoVentana * 0.5f + Margen + anchoPie * 0.0005f : 0f;
                Colocar(textoContador.rectTransform, xPie, yFila, anchoPie, AltoFila * 1000f);
            }
            if (botonEntendido != null)
            {
                var rb = (RectTransform)botonEntendido.transform;
                rb.sizeDelta = new Vector2(280f, AltoFila * 1000f);
                rb.anchoredPosition = new Vector2((AnchoVentana * 0.5f - Margen) * 1000f - 140f, yFila * 1000f);
                var etiqueta = botonEntendido.GetComponentInChildren<TMP_Text>(true);
                if (etiqueta != null) { etiqueta.enableAutoSizing = false; etiqueta.fontSize = LetraBoton; }
            }
            if (fondo != null)
            {
                fondo.sizeDelta = new Vector2(AnchoVentana * 1000f, alto * 1000f);
                fondo.anchoredPosition = new Vector2(0f, BordeArriba - alto * 0.5f);
            }
        }

        private static void Colocar(RectTransform r, float x, float y, float ancho, float alto)
        {
            r.anchoredPosition = new Vector2(x, y);
            r.sizeDelta = new Vector2(ancho, alto);
        }

        public void OcultarGuia()
        {
            if (panelVentana != null) panelVentana.SetActive(false);
            if (marcaTutorial != null) marcaTutorial.Ocultar();
        }

        // --- Disparadores de las 19 claves (ControlCocina.NumeroTurno es 0, 1, 2) ---

        private void AlIniciarTurno(TurnoData turno)
        {
            cola.Clear();
            var cocina = ControlCocina.Instancia;
            int num = cocina != null ? cocina.NumeroTurno + 1 : 1;

            if (num == 1)
            {
                Encolar("bienvenida");
                Encolar("ribosoma_mesa");
                Encolar("corriente");
                Encolar("sintetasa");
                Encolar("llave");
            }
            else if (num == 2)
            {
                Encolar("t2_intro");
                Encolar("ribosoma_lejano");
            }
            else if (num == 3)
            {
                Encolar("t3_intro");
            }
        }

        private void AlCambiarCodon(RibosomaEstado ribo)
        {
            var cocina = ControlCocina.Instancia;
            if (cocina != null && cocina.EnParo(ribo))
                Encolar("boton_fin");
        }

        private void AlCargarARNt(ARNtCocina arnt, bool equivocado)
        {
            if (equivocado)
                Encolar("sintetasa_error");
        }

        private void AlAceptarAminoacido(RibosomaEstado ribo, bool correcto)
        {
            Encolar("salida");
        }

        private void AlReciclar(ARNtCocina arnt)
        {
            Encolar("reciclaje");
        }

        private void AlCambiarHorquilla(bool activa)
        {
            if (activa) Encolar("horquilla");
        }

        private void AlCambiarCodonRaro(bool activo)
        {
            if (activo) Encolar("codon_raro");
        }

        private void AlCambiarFase(FaseCocina fase)
        {
            if (fase == FaseCocina.Busca)
            {
                Encolar("fase_b");
                Encolar("busca");
            }
        }

        private void AlEncontrarCuenta()
        {
            Encolar("chaperona");
        }

        private void AlUsarChaperona()
        {
            Encolar("decide");
        }

        // --- Gestión de cola y visualización ---

        public void Encolar(string clave)
        {
            if (baseDatos == null || clavesVistas.Contains(clave)) return;

            var entrada = baseDatos.Buscar(clave);
            if (entrada == null) return;

            var cocina = ControlCocina.Instancia;
            int turnoActual = cocina != null ? cocina.NumeroTurno + 1 : 1;
            if (entrada.turno > turnoActual) return;

            clavesVistas.Add(clave);
            cola.Enqueue(entrada);

            if (!ventanaAbierta)
                MostrarSiguiente();
            else
                ActualizarContador();
        }

        private void MostrarSiguiente()
        {
            if (cola.Count == 0)
            {
                ventanaAbierta = false;
                if (panelVentana != null) panelVentana.SetActive(false);
                if (marcaTutorial != null) marcaTutorial.Ocultar();

                if (ControlCocina.Instancia != null)
                    ControlCocina.Instancia.PonerPausa(false);
                return;
            }

            var actual = cola.Dequeue();
            ventanaAbierta = true;

            if (ControlCocina.Instancia != null)
            {
                ControlCocina.Instancia.PonerPausa(true);
                ControlCocina.Instancia.Sonar("pop");
            }

            if (panelVentana != null) panelVentana.SetActive(true);
            if (textoTitulo != null) textoTitulo.text = actual.titulo;
            if (textoCuerpo != null) textoCuerpo.text = actual.cuerpo;

            if (imagenIlustracion != null)
            {
                bool tieneImagen = actual.imagenOpcional != null;
                imagenIlustracion.gameObject.SetActive(tieneImagen);
                if (tieneImagen) imagenIlustracion.sprite = actual.imagenOpcional;
            }

            ActualizarContador();
            ColocarMarcaParaClave(actual.clave);
        }

        public void CerrarVentanaActual()
        {
            if (modoGuiado) { if (guia != null) guia.Entendido(); return; }
            if (!ventanaAbierta) return;
            ControlCocina.Instancia?.Sonar("clic");
            MostrarSiguiente();
        }

        private void ActualizarContador()
        {
            if (textoContador == null) return;
            textoContador.text = cola.Count > 0 ? $"quedan {cola.Count}" : string.Empty;
        }

        private void ColocarMarcaParaClave(string clave)
        {
            if (marcaTutorial == null) return;

            Transform destino = null;
            for (int i = 0; i < objetivos.Count; i++)
            {
                if (objetivos[i].clave == clave)
                {
                    destino = objetivos[i].objetivo;
                    break;
                }
            }

            if (destino != null)
                marcaTutorial.Senalar(destino);
            else
                marcaTutorial.Ocultar();
        }

        /// <summary>
        /// Borra las ventanas vistas para el botón "Reiniciar tutorial" del menú de pausa (A7).
        /// </summary>
        public static void ReiniciarHistorial()
        {
            clavesVistas.Clear();
            GuiaCocina.OlvidarPresentaciones();
        }
    }
}