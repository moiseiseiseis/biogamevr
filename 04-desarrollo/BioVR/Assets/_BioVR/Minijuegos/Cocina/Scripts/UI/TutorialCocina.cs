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

        private readonly Queue<EntradaTutorial> cola = new Queue<EntradaTutorial>();
        private static readonly HashSet<string> clavesVistas = new HashSet<string>();
        private bool ventanaAbierta;

        private void OnEnable()
        {
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

            if (botonEntendido != null)
                botonEntendido.onClick.AddListener(CerrarVentanaActual);

            if (accionEntendido.action != null)
            {
                accionEntendido.action.Enable();
                accionEntendido.action.performed += AlPresionarBotonVR;
            }
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
            if (panelVentana != null) panelVentana.SetActive(false);
            if (marcaTutorial != null) marcaTutorial.Ocultar();
        }

        private void AlPresionarBotonVR(InputAction.CallbackContext ctx)
        {
            if (ventanaAbierta)
                CerrarVentanaActual();
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
        }
    }
}