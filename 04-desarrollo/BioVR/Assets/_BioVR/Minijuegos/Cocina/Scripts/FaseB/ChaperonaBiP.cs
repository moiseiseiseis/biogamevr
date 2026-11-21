using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    public class ChaperonaBiP : MonoBehaviour
    {
        [Header("Estados del Prefab (auto-detectados si se dejan vacíos)")]
        [SerializeField] private GameObject bipAbierta;
        [SerializeField] private GameObject bipCerrada;

        [Header("Referencia opcional")]
        [SerializeField] private HologramaCocina holograma;

        [Header("Tiempos y Temblor (§9.2)")]
        [SerializeField] private float duracionTotal = 3.0f;
        [SerializeField] private float tiempoAperturaInicial = 0.6f;
        [SerializeField] private float tiempoSujecionCerrada = 1.8f;
        [SerializeField] private float amplitudTemblor = 0.012f;

        private bool animando;
        private float tiempoTranscurrido;
        private bool estabaCerrada;

        private Vector3 ultimoOffsetTemblor;
        private Vector3 ultimaPosicionAplicada;
        private bool tieneOffsetActivo;

        void Awake()
        {
            if (bipAbierta == null)
            {
                Transform tAbierta = BuscarHijoRecursivo(transform, "BiP_Abierta");
                if (tAbierta != null) bipAbierta = tAbierta.gameObject;
            }

            if (bipCerrada == null)
            {
                Transform tCerrada = BuscarHijoRecursivo(transform, "BiP_Cerrada");
                if (tCerrada != null) bipCerrada = tCerrada.gameObject;
            }

            PonerEstadoAbierto(true);
        }

        void OnEnable()
        {
            EventosCocina.OnChaperona += IniciarSecuenciaBiP;
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
        }

        void OnDisable()
        {
            EventosCocina.OnChaperona -= IniciarSecuenciaBiP;
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
            LimpiarTemblor();
        }

        private void AlIniciarTurno(TurnoData _)
        {
            animando = false;
            tiempoTranscurrido = 0f;
            estabaCerrada = false;
            LimpiarTemblor();
            PonerEstadoAbierto(true);
        }

        [ContextMenu("Probar Animación BiP")]
        public void IniciarSecuenciaBiP()
        {
            if (holograma == null)
            {
                holograma = FindFirstObjectByType<HologramaCocina>();
            }

            LimpiarTemblor();
            tiempoTranscurrido = 0f;
            estabaCerrada = false;
            animando = true;
            PonerEstadoAbierto(true);
        }

        void Update()
        {
            if (!animando) return;
            if (ControlCocina.Instancia != null && ControlCocina.Instancia.Pausado) return;

            tiempoTranscurrido += Time.deltaTime;

            float finSujecion = tiempoAperturaInicial + tiempoSujecionCerrada;
            bool debeEstarCerrada = tiempoTranscurrido >= tiempoAperturaInicial && tiempoTranscurrido < finSujecion;

            if (debeEstarCerrada != estabaCerrada)
            {
                PonerEstadoAbierto(!debeEstarCerrada);
                estabaCerrada = debeEstarCerrada;
            }

            if (tiempoTranscurrido >= duracionTotal)
            {
                animando = false;
                estabaCerrada = false;
                PonerEstadoAbierto(true);
            }
        }

        void LateUpdate()
        {
            Transform contenedor = holograma != null ? holograma.ContenedorCadena : null;

            if (animando && estabaCerrada && contenedor != null)
            {
                Vector3 basePos = contenedor.position;
                if (tieneOffsetActivo && Vector3.SqrMagnitude(basePos - ultimaPosicionAplicada) < 1e-8f)
                {
                    basePos -= ultimoOffsetTemblor;
                }

                Vector3 nuevoOffset = new Vector3(
                    Random.Range(-amplitudTemblor, amplitudTemblor),
                    Random.Range(-amplitudTemblor, amplitudTemblor),
                    Random.Range(-amplitudTemblor, amplitudTemblor)
                );

                contenedor.position = basePos + nuevoOffset;
                ultimoOffsetTemblor = nuevoOffset;
                ultimaPosicionAplicada = contenedor.position;
                tieneOffsetActivo = true;
            }
            else if (tieneOffsetActivo)
            {
                LimpiarTemblor();
            }
        }

        private void PonerEstadoAbierto(bool abierta)
        {
            if (bipAbierta != null) bipAbierta.SetActive(abierta);
            if (bipCerrada != null) bipCerrada.SetActive(!abierta);
        }

        private void LimpiarTemblor()
        {
            if (!tieneOffsetActivo) return;

            Transform contenedor = holograma != null ? holograma.ContenedorCadena : null;
            if (contenedor != null && Vector3.SqrMagnitude(contenedor.position - ultimaPosicionAplicada) < 1e-8f)
            {
                contenedor.position -= ultimoOffsetTemblor;
            }

            ultimoOffsetTemblor = Vector3.zero;
            tieneOffsetActivo = false;
        }

        private static Transform BuscarHijoRecursivo(Transform padre, string nombre)
        {
            foreach (Transform hijo in padre)
            {
                if (hijo.name == nombre) return hijo;
                Transform encontrado = BuscarHijoRecursivo(hijo, nombre);
                if (encontrado != null) return encontrado;
            }
            return null;
        }
    }
}