using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // M4 · Un ribosoma de la Cocina (Iker, rol 3). El mismo componente sirve para los dos:
    //   - De mesa (lejano = false): el ARNt se suelta en el sitio A y se gira como llave (LlaveARNt).
    //   - Lejano (lejano = true): recibe ARNt aventados; entran directo, sin llave.
    //
    // No decide nada: le pregunta a ControlCocina (M1) y escucha sus eventos. Lo que hace es mostrar:
    // el ARNm que avanza (ARNmCocina), la cadena que sale (CadenaSaliente), el botón FIN, los letreros,
    // el sitio A que pulsa y la transparencia del ribosoma cuando hay un ARNt adentro.
    //
    // El prefab lo arma el menú BioVR → Cocina → Armar prueba M4. Su +Z mira al jugador (PosicionMesa).
    public class RibosomaCocina : MonoBehaviour
    {
        [Tooltip("Falso = ribosoma de mesa (llave). Verdadero = ribosoma lejano (recibe ARNt aventados).")]
        public bool lejano;

        [Header("Partes (las asigna el armador)")]
        [SerializeField] GameObject visual;            // todo lo que se ve; se apaga si el turno no usa este ribosoma
        [SerializeField] Transform sitioA;             // donde se para el ARNt: el anticodón queda sobre el codón
        [SerializeField] ARNmCocina arnm;
        [SerializeField] CadenaSaliente cadena;
        [SerializeField] BotonFin botonFin;
        [SerializeField] LlaveARNt llave;              // solo el de mesa
        [SerializeField] Renderer cajaSitioA;          // caja cian que pulsa
        [SerializeField] Renderer[] renderersRibosoma; // las subunidades (transparentes)

        [Header("Letreros")]
        [SerializeField] TextMeshPro titulo;
        [SerializeField] TextMeshPro subtitulo;
        [SerializeField] TextMeshPro pide;
        [SerializeField] TextMeshPro codonGrande;      // en la placa bajo el sitio A
        [SerializeField] TextMeshPro letreroSitio;

        [Header("Textos (claves de TextosCocina; si faltan, se usa el texto de respaldo)")]
        [SerializeField] string claveTitulo = "ribosoma_mesa";
        [SerializeField] string respaldoTitulo = "RIBOSOMA";
        [SerializeField] string claveSubtitulo = "ribosoma_rer";
        [SerializeField] string respaldoSubtitulo = "pegado al RER: la amilina se hace aquí y entra al retículo";
        [SerializeField] string clavePide = "ribosoma_pide";
        [SerializeField] string respaldoPide = "pide: {0}";
        [SerializeField] string claveSitio = "ribosoma_sitio_a";
        [SerializeField] string respaldoSitio = "codón en el sitio A";

        [Header("Transparencia (especificación 5.2)")]
        [SerializeField, Range(0f, 1f)] float opacidadNormal = 0.45f;
        [SerializeField, Range(0f, 1f)] float opacidadConARNt = 0.22f;
        [SerializeField] float velocidadOpacidad = 3f;

        [Header("Sitio A que pulsa")]
        [SerializeField, Range(0f, 1f)] float pulsoMinimo = 0.35f;
        [SerializeField, Range(0f, 1f)] float pulsoMaximo = 0.70f;

        // Todos los ribosomas de la escena, para que la corriente (M2) los encuentre al aventar
        public static readonly List<RibosomaCocina> Todos = new List<RibosomaCocina>();

        public RibosomaEstado Estado { get; private set; }
        public Transform SitioA => sitioA;
        public LlaveARNt Llave => llave;
        public bool Activo => Estado != null;

        // ARNt que entraron y siguen en este ribosoma (hasta que M1 los manda a la salida)
        readonly List<ARNtCocina> dentro = new List<ARNtCocina>();
        MaterialPropertyBlock bloque;
        float opacidad;

        static ControlCocina Control => ControlCocina.Instancia;

        [Header("Tarjeta del codón (prueba del 23-nov)")]
        [Tooltip("Cambia la placa 'codón en el sitio A' por una tarjeta con las piezas del codón y del ARNt que encaja")]
        [SerializeField] bool usarTarjeta = true;
        [SerializeField] float escalaTarjeta = 0.8f;
        [Tooltip("Dónde queda la tarjeta respecto a la placa: x a la derecha, y arriba, z hacia el jugador (m). " +
                 "En la placa misma la mitad de abajo quedaba dentro de la mesa.")]
        // 24-nov: más baja y más cerca (antes 0, 0.13, 0.1): se encimaba con el ribosoma. Fijo en el código (no
        // serializado): la copia importada del prefab se quedaba con el valor viejo.
        static readonly Vector3 desplazamientoTarjeta = new Vector3(0f, 0.07f, 0.17f);
        TarjetaCodon tarjeta;

        void Awake()
        {
            bloque = new MaterialPropertyBlock();
            opacidad = opacidadNormal;
            // El recuadro cian que pulsaba tapaba las piezas del codón: se quita (el codón se ve en la tarjeta)
            if (usarTarjeta && cajaSitioA != null) cajaSitioA.enabled = false;
            if (usarTarjeta && codonGrande != null && arnm != null)
            {
                var placa = codonGrande.transform;
                tarjeta = TarjetaCodon.Crear(placa, arnm, codonGrande.font, escalaTarjeta);
                // TextMeshPro mira hacia su −Z: "hacia el jugador" es −forward, pero solo en horizontal (la placa está
                // inclinada como atril y su −forward también baja: subir y acercar se cancelaban)
                var haciaJugador = -Vector3.ProjectOnPlane(placa.forward, Vector3.up).normalized;
                var derecha = Vector3.ProjectOnPlane(placa.right, Vector3.up).normalized;
                tarjeta.transform.position += derecha * desplazamientoTarjeta.x + Vector3.up * desplazamientoTarjeta.y
                                              + haciaJugador * desplazamientoTarjeta.z;
                // De frente a los ojos (la placa estaba acostada como atril y la fila de abajo no se leía)
                var cam = Camera.main;
                var dir = cam != null ? tarjeta.transform.position - cam.transform.position : -haciaJugador;
                if (dir.sqrMagnitude > 1e-6f) tarjeta.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                codonGrande.gameObject.SetActive(false);
                if (letreroSitio != null) letreroSitio.gameObject.SetActive(false);
                // La placa cian vieja atravesaba la tarjeta
                foreach (var r in GetComponentsInChildren<Renderer>(true))
                    if (r.name.StartsWith("Placa sitio")) r.enabled = false;
            }
        }

        void OnEnable()
        {
            Todos.Add(this);
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
            EventosCocina.OnCodonCambiado += AlCambiarCodon;
            EventosCocina.OnAminoacidoAceptado += AlAceptar;
            EventosCocina.OnARNtASalida += AlSalir;
            EventosCocina.OnFaseCambiada += AlCambiarFase;
            // Si el turno ya empezó antes de que este objeto se encendiera (AnclaCocina se enciende tarde)
            if (Control != null && Control.Turno != null) AlIniciarTurno(Control.Turno);
        }

        void OnDisable()
        {
            Todos.Remove(this);
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
            EventosCocina.OnCodonCambiado -= AlCambiarCodon;
            EventosCocina.OnAminoacidoAceptado -= AlAceptar;
            EventosCocina.OnARNtASalida -= AlSalir;
            EventosCocina.OnFaseCambiada -= AlCambiarFase;
        }

        // ---------------------------------------------------------------------------------------------
        // Lo que llaman los demás
        // ---------------------------------------------------------------------------------------------

        // Llega un ARNt: soltado en el sitio A (mesa) o aventado (lejano, lo llama la corriente de M2).
        // Devuelve el resultado de M1. Si se rechaza, el ARNt regresa a la corriente.
        public ResultadoColocacion Recibir(ARNtCocina t)
        {
            if (Control == null || Estado == null || t == null) return ResultadoColocacion.Rechazado;
            var resultado = Control.IntentarColocarEnRibosoma(t, Estado, out _);
            switch (resultado)
            {
                case ResultadoColocacion.Rechazado:
                    PuenteCorriente.Devolver(t);
                    break;
                case ResultadoColocacion.Llave:
                    dentro.Add(t);
                    if (llave != null) llave.Empezar(t, this);
                    else Control.Aceptar(t, Estado); // por si alguien olvidó la llave en un ribosoma de mesa
                    break;
                case ResultadoColocacion.Aceptado:
                    dentro.Add(t);
                    Acomodar(t);
                    break;
            }
            return resultado;
        }

        // Deja el ARNt quieto en el sitio A, derecho (después de la llave o al entrar al lejano)
        public void Acomodar(ARNtCocina t)
        {
            PuenteCorriente.Soltar(t);
            t.transform.SetPositionAndRotation(sitioA.position, sitioA.rotation);
            PuenteCorriente.PermitirAgarre(t, false);
        }

        // La llama LlaveARNt cuando el ARNt hizo clic
        public void LlaveEncajo(ARNtCocina t)
        {
            Acomodar(t);
            Control.Aceptar(t, Estado);
        }

        // ---------------------------------------------------------------------------------------------
        // Eventos de M1
        // ---------------------------------------------------------------------------------------------

        void AlIniciarTurno(TurnoData turno)
        {
            Estado = null;
            if (Control != null)
                foreach (var r in Control.Ribosomas)
                    if (r.Lejano == lejano) { Estado = r; break; }

            dentro.Clear();
            if (llave != null) llave.Cancelar();
            if (visual != null) visual.SetActive(Estado != null);
            if (cadena != null) cadena.Limpiar();
            if (Estado == null) return;

            if (titulo != null) titulo.text = Texto(claveTitulo, respaldoTitulo);
            if (subtitulo != null) subtitulo.text = Texto(claveSubtitulo, respaldoSubtitulo);
            ApilarLetreros();
            if (letreroSitio != null) letreroSitio.text = Texto(claveSitio, respaldoSitio);
            if (arnm != null) arnm.Iniciar(Control.Codones, Estado.indice);
            Refrescar();
        }

        // "RIBOSOMA LEJANO" no cabía en el ancho del letrero (0.4 m): se partía en dos renglones y el
        // segundo caía encima del subtítulo, que también ocupa dos (prueba del 24-nov). El título va en
        // un solo renglón y se pone justo arriba del alto real del subtítulo. Ambos van centrados en
        // vertical, así que el borde de arriba del subtítulo es su centro + la mitad de su alto.
        const float SeparacionLetreros = 0.008f;

        void ApilarLetreros()
        {
            if (titulo == null || subtitulo == null) return;
            titulo.textWrappingMode = TextWrappingModes.NoWrap;
            var rs = subtitulo.rectTransform;
            var rt = titulo.rectTransform;
            float altoSub = subtitulo.GetPreferredValues(subtitulo.text, rs.sizeDelta.x, 0f).y;
            float altoTit = titulo.GetPreferredValues(titulo.text).y;
            float y = rs.anchoredPosition.y + altoSub * 0.5f + SeparacionLetreros + altoTit * 0.5f;
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
        }

        void AlCambiarCodon(RibosomaEstado r)
        {
            if (r != Estado) return;
            if (arnm != null) arnm.IrA(r.indice);
            Refrescar();
        }

        void AlAceptar(RibosomaEstado r, bool correcto)
        {
            if (r != Estado || cadena == null || r.cadena.Count == 0) return;
            cadena.Agregar(r.cadena[r.cadena.Count - 1], correcto);
        }

        void AlSalir(ARNtCocina t) => dentro.Remove(t);

        void AlCambiarFase(FaseCocina fase)
        {
            if (fase != FaseCocina.Armando) { dentro.Clear(); if (llave != null) llave.Cancelar(); }
            Refrescar();
        }

        // Letreros y botón FIN según el codón actual
        void Refrescar()
        {
            if (Estado == null || Control == null) return;
            var codones = Control.Codones;
            bool hayCodon = Estado.indice < codones.Count && !Estado.listo;

            if (codonGrande != null) codonGrande.text = hayCodon ? FormatoCodon(codones[Estado.indice]) : "";

            var pideAhora = Control.PideAhora(Estado);
            if (pide != null)
            {
                pide.gameObject.SetActive(pideAhora.HasValue);
                if (pideAhora.HasValue && pideAhora.Value.aminoacido != null)
                {
                    var aa = pideAhora.Value.aminoacido;
                    pide.text = string.Format(Texto(clavePide, respaldoPide), aa.abreviatura);
                    pide.color = aa.color;
                }
            }
            if (botonFin != null) botonFin.Mostrar(Control.EnParo(Estado));
            if (tarjeta != null)
                tarjeta.Mostrar(hayCodon ? codones[Estado.indice] : (Codon?)null, pideAhora.HasValue, Control.EnParo(Estado));
        }

        static string FormatoCodon(Codon c) => c.esParo ? $"{c.tripleta}\n<size=60%>paro</size>" : c.tripleta;

        // ---------------------------------------------------------------------------------------------
        // Cada cuadro: transparencia y pulso del sitio A
        // ---------------------------------------------------------------------------------------------

        void Update()
        {
            if (Estado == null) return;
            bool conARNt = dentro.Count > 0 || (llave != null && llave.Ocupada);
            opacidad = Mathf.MoveTowards(opacidad, conARNt ? opacidadConARNt : opacidadNormal, velocidadOpacidad * Time.deltaTime);
            PintarAlfa(renderersRibosoma, opacidad);

            if (cajaSitioA != null)
            {
                float k = Mathf.Sin(Time.time * 4f) * 0.5f + 0.5f;
                PintarAlfa(new[] { cajaSitioA }, Mathf.Lerp(pulsoMinimo, pulsoMaximo, k));
            }
        }

        void PintarAlfa(Renderer[] renderers, float alfa)
        {
            if (renderers == null) return;
            foreach (var r in renderers)
            {
                if (r == null) continue;
                var mats = r.sharedMaterials;
                for (int m = 0; m < mats.Length; m++)
                {
                    if (mats[m] == null || !mats[m].HasProperty("_BaseColor")) continue;
                    var c = mats[m].GetColor("_BaseColor");
                    c.a = alfa;
                    r.GetPropertyBlock(bloque, m);
                    bloque.SetColor("_BaseColor", c);
                    r.SetPropertyBlock(bloque, m);
                }
            }
        }

        // Texto de TextosCocina; si la clave aún no existe (la agrega M8), el de respaldo
        static string Texto(string clave, string respaldo)
        {
            var textos = Control != null ? Control.Textos : null;
            return textos != null && textos.Tiene(clave) ? textos.Texto(clave) : respaldo;
        }
    }
}
