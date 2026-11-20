using System.Collections.Generic;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    public class HologramaCocina : MonoBehaviour
    {
        [Header("Referencias")]
        public LectorCoordenadas lector;
        public GameObject cuentaPrefab;

        [Tooltip("Opcional: material para la línea lila que une las cuentas")]
        public Material materialLinea;

        [Header("Medidas y Tiempos (§8.2 - §8.5)")]
        [Tooltip("Altura sobre el proyector donde flota el centro de la cadena (0.36 m)")]
        public float alturaSobreProyector = 0.36f;

        [Tooltip("Escala base de cada esfera (0.94 según §8.4)")]
        public float escalaCuenta = 0.94f;

        [Tooltip("Velocidad de aparición en cuentas por segundo (22 cuentas/s)")]
        public float cuentasPorSegundo = 22f;

        [Tooltip("Retraso en cuentas para el suavizado smoothstep (~5 cuentas)")]
        public float cuentasSuavizado = 5f;

        [Tooltip("Duración en segundos para encoger y cortar la Met inicial (~1.4 s)")]
        public float duracionCorteMet = 1.4f;

        [Tooltip("Punto local de salida de las cuentas respecto al centro del holograma")]
        public Vector3 puntoSalida = new Vector3(0.2f, -0.24f, 0.05f);

        [Header("Rotación (rad/s)")]
        public float velocidadRotacionNormal = 0.35f;
        public float velocidadRotacionBusca = 0.15f;

        [Header("Colores")]
        public Color colorError = new Color(0.95f, 0.18f, 0.18f, 1f);
        public Color colorLineaLila = new Color(0.72f, 0.45f, 1f, 0.6f);
        public Color colorPorDefecto = new Color(0.25f, 0.8f, 1f, 1f);

        // Acceso público para M10 (Depósitos, BiP y Proteasoma 26S)
        public Transform ContenedorCadena => contenedorCadena;
        public IReadOnlyList<CuentaHolograma> Cuentas => cuentas;
        public bool UsandoFormaMutante { get; private set; }

        private Transform contenedorCadena;
        private LineRenderer lineaColumna;
        private readonly List<CuentaHolograma> cuentas = new List<CuentaHolograma>();
        private readonly List<Vector3> destinos = new List<Vector3>();

        private float tiempoAnimacion;
        private float tiempoFinDoblez;
        private float tiempoCorteMet;
        private bool animandoDoblez;
        private bool cortandoMet;
        private bool metCortada;
        private bool enFaseBusca;
        private int erroresPrueba;

        void Awake()
        {
            CrearContenedorYLinea();
        }

        void OnDestroy()
        {
            if (contenedorCadena != null) Destroy(contenedorCadena.gameObject);
        }

        void OnEnable()
        {
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
            EventosCocina.OnCadenaTerminada += AlTerminarCadena;
            EventosCocina.OnFaseCambiada += AlCambiarFase;
            EventosCocina.OnErrorBusca += AlFallarBusca;
            EventosCocina.OnCuentaEncontrada += AlEncontrarCuenta;
        }

        void OnDisable()
        {
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
            EventosCocina.OnCadenaTerminada -= AlTerminarCadena;
            EventosCocina.OnFaseCambiada -= AlCambiarFase;
            EventosCocina.OnErrorBusca -= AlFallarBusca;
            EventosCocina.OnCuentaEncontrada -= AlEncontrarCuenta;
        }

        private void CrearContenedorYLinea()
        {
            if (contenedorCadena == null)
            {
                GameObject go = new GameObject("Contenedor_Cadena_Holograma");
                // Se coloca al mismo nivel que el proyector con escala (1,1,1) para no deformar la proteína
                go.transform.SetParent(transform.parent, false);
                go.transform.localScale = Vector3.one;
                contenedorCadena = go.transform;
            }

            contenedorCadena.position = transform.position + Vector3.up * alturaSobreProyector;

            if (lineaColumna == null)
            {
                lineaColumna = contenedorCadena.gameObject.AddComponent<LineRenderer>();
                lineaColumna.useWorldSpace = false;
                lineaColumna.startWidth = 0.004f;
                lineaColumna.endWidth = 0.004f;
                lineaColumna.numCapVertices = 4;
                lineaColumna.numCornerVertices = 4;

                if (materialLinea != null)
                {
                    lineaColumna.sharedMaterial = materialLinea;
                }
                else
                {
                    Shader shader = Shader.Find("Sprites/Default");
                    if (shader != null) lineaColumna.material = new Material(shader);
                }

                lineaColumna.startColor = colorLineaLila;
                lineaColumna.endColor = colorLineaLila;
                lineaColumna.positionCount = 0;
            }
        }

        private void AlIniciarTurno(TurnoData turno)
        {
            LimpiarHolograma();
            enFaseBusca = false;
            UsandoFormaMutante = false;
            erroresPrueba = 0;
        }

        private void AlTerminarCadena(RibosomaEstado ribosoma)
        {
            if (ribosoma == null || ribosoma.cadena == null || ribosoma.cadena.Count == 0) return;

            bool esMutante = ControlCocina.Instancia != null &&
                             ControlCocina.Instancia.Turno != null &&
                             ControlCocina.Instancia.Turno.TieneMutacion;

            int indiceMutacion = (ControlCocina.Instancia != null && ControlCocina.Instancia.Turno != null)
                ? ControlCocina.Instancia.Turno.CodonMutacion
                : 20;

            MostrarCadena(ribosoma.cadena, ribosoma.malos, esMutante, indiceMutacion);
        }

        private void AlCambiarFase(FaseCocina fase)
        {
            enFaseBusca = (fase == FaseCocina.Busca);

            if (enFaseBusca)
            {
                if (!UsandoFormaMutante && lector != null && cuentas.Count > 5)
                {
                    List<Vector3> posMutante = lector.ObtenerPosicionesMutante();
                    if (posMutante.Count >= cuentas.Count)
                    {
                        destinos.Clear();
                        for (int i = 0; i < cuentas.Count; i++) destinos.Add(posMutante[i]);
                        UsandoFormaMutante = true;
                    }
                }

                foreach (var c in cuentas)
                {
                    if (c != null) c.PonerInteractuable(true);
                }
            }
            else if (fase == FaseCocina.Armando || fase == FaseCocina.Fin)
            {
                foreach (var c in cuentas)
                {
                    if (c != null) c.PonerInteractuable(false);
                }
            }
        }

        private void AlFallarBusca(int errores)
        {
            bool activarPista = (ControlCocina.Instancia != null && ControlCocina.Instancia.PistaCuenta) || errores >= 2;
            if (!activarPista) return;

            foreach (var c in cuentas)
            {
                if (c != null && c.EsMutacion)
                {
                    c.ActivarParpadeoPista(true);
                }
            }
        }

        private void AlEncontrarCuenta()
        {
            enFaseBusca = false;
            foreach (var c in cuentas)
            {
                if (c == null) continue;
                c.PonerInteractuable(false);
                if (c.EsMutacion)
                {
                    c.MarcarComoEncontrada();
                }
            }
        }

        public void MostrarCadena(IReadOnlyList<AminoacidoData> cadena, HashSet<int> indicesMalos, bool usarFormaMutante, int codonMutacion = 20)
        {
            if (lector == null || cuentaPrefab == null)
            {
                Debug.LogError("[M7] Falta asignar LectorCoordenadas o CuentaPrefab en HologramaCocina.");
                return;
            }

            CrearContenedorYLinea();
            LimpiarHolograma();

            int total = cadena.Count;
            List<Vector3> posiciones;

            if (total <= 5)
            {
                posiciones = lector.ObtenerPosicionesLineales(total);
                UsandoFormaMutante = false;
            }
            else if (usarFormaMutante)
            {
                posiciones = lector.ObtenerPosicionesMutante();
                UsandoFormaMutante = true;
            }
            else
            {
                posiciones = lector.ObtenerPosicionesNormal();
                UsandoFormaMutante = false;
            }

            int cantidad = Mathf.Min(total, posiciones.Count);
            float radio = Mathf.Max(0.01f, lector.radioMaximo);
            float escalaFinal = escalaCuenta * (radio / 0.19f);

            for (int i = 0; i < cantidad; i++)
            {
                GameObject obj = Instantiate(cuentaPrefab, contenedorCadena);
                obj.name = i == 0 ? "Cuenta_00_Met" : $"Cuenta_{i:00}_{(cadena[i] != null ? cadena[i].abreviatura : "AA")}";
                obj.transform.localPosition = puntoSalida;
                obj.transform.localScale = Vector3.zero;

                CuentaHolograma cuenta = obj.GetComponent<CuentaHolograma>();
                if (cuenta == null) cuenta = obj.AddComponent<CuentaHolograma>();

                bool esMalo = indicesMalos != null && indicesMalos.Contains(i);
                Color col = esMalo
                    ? colorError
                    : (cadena[i] != null ? cadena[i].color : colorPorDefecto);

                bool esMutacion = usarFormaMutante && (i == codonMutacion);
                cuenta.Inicializar(i, col, escalaFinal, esMutacion);

                cuentas.Add(cuenta);
                destinos.Add(posiciones[i]);
            }

            tiempoAnimacion = 0f;
            tiempoCorteMet = 0f;
            float intervalo = 1f / Mathf.Max(1f, cuentasPorSegundo);
            float duracionViaje = cuentasSuavizado * intervalo;
            tiempoFinDoblez = ((cantidad - 1) * intervalo) + duracionViaje;

            animandoDoblez = true;
            cortandoMet = false;
            metCortada = false;
        }

        void Update()
        {
            if (contenedorCadena != null)
            {
                contenedorCadena.position = transform.position + Vector3.up * alturaSobreProyector;
            }

            if (cuentas.Count == 0) return;
            if (ControlCocina.Instancia != null && ControlCocina.Instancia.Pausado) return;

            float dt = Time.deltaTime;

            float velRad = enFaseBusca ? velocidadRotacionBusca : velocidadRotacionNormal;
            contenedorCadena.Rotate(Vector3.up, velRad * Mathf.Rad2Deg * dt, Space.Self);

            ActualizarAnimacionCuentas(dt);
            ActualizarLineaLila();
        }

        private void ActualizarAnimacionCuentas(float dt)
        {
            float intervalo = 1f / Mathf.Max(1f, cuentasPorSegundo);
            float duracionViaje = Mathf.Max(0.01f, cuentasSuavizado * intervalo);
            float radio = lector != null ? Mathf.Max(0.01f, lector.radioMaximo) : 0.19f;
            float escalaFinal = escalaCuenta * (radio / 0.19f);

            if (animandoDoblez)
            {
                tiempoAnimacion += dt;

                for (int i = 0; i < cuentas.Count; i++)
                {
                    float tInicio = i * intervalo;
                    float u = Mathf.Clamp01((tiempoAnimacion - tInicio) / duracionViaje);
                    float suave = Mathf.SmoothStep(0f, 1f, u);

                    cuentas[i].transform.localPosition = Vector3.LerpUnclamped(puntoSalida, destinos[i], suave);

                    if (!(i == 0 && (cortandoMet || metCortada)))
                    {
                        float factorAparicion = Mathf.Clamp01(u * 4f);
                        cuentas[i].transform.localScale = Vector3.one * (escalaFinal * factorAparicion);
                    }
                }

                if (tiempoAnimacion >= tiempoFinDoblez)
                {
                    animandoDoblez = false;
                    if (cuentas.Count > 5 && !metCortada)
                    {
                        cortandoMet = true;
                        tiempoCorteMet = 0f;
                    }
                }
            }
            else
            {
                for (int i = 0; i < cuentas.Count; i++)
                {
                    cuentas[i].transform.localPosition = Vector3.Lerp(
                        cuentas[i].transform.localPosition,
                        destinos[i],
                        dt * 12f
                    );
                }
            }

            if (cortandoMet && cuentas.Count > 0)
            {
                tiempoCorteMet += dt;
                float k = Mathf.Clamp01(tiempoCorteMet / Mathf.Max(0.05f, duracionCorteMet));
                cuentas[0].transform.localScale = Vector3.one * (escalaFinal * (1f - k));

                if (k >= 1f)
                {
                    cortandoMet = false;
                    metCortada = true;
                    cuentas[0].gameObject.SetActive(false);
                }
            }
        }

        private void ActualizarLineaLila()
        {
            if (lineaColumna == null) return;

            float intervalo = 1f / Mathf.Max(1f, cuentasPorSegundo);
            int visibles = animandoDoblez
                ? Mathf.Clamp(Mathf.FloorToInt(tiempoAnimacion / intervalo) + 1, 0, cuentas.Count)
                : cuentas.Count;

            int inicio = (cortandoMet || metCortada) && visibles > 1 ? 1 : 0;
            int totalPuntos = Mathf.Max(0, visibles - inicio);

            lineaColumna.positionCount = totalPuntos;
            for (int i = 0; i < totalPuntos; i++)
            {
                lineaColumna.SetPosition(i, cuentas[inicio + i].transform.localPosition);
            }
        }

        public void LimpiarHolograma()
        {
            for (int i = 0; i < cuentas.Count; i++)
            {
                if (cuentas[i] != null) Destroy(cuentas[i].gameObject);
            }
            cuentas.Clear();
            destinos.Clear();

            if (lineaColumna != null) lineaColumna.positionCount = 0;
            animandoDoblez = false;
            cortandoMet = false;
            metCortada = false;
        }

        // M10 (proteasoma): el holograma deja de mover, girar y unir con la línea a sus cuentas y se las entrega
        // a quien las va a destruir. Sin la Met si ya se cortó. Devuelve la lista vacía si no hay cadena.
        public List<CuentaHolograma> EntregarCuentas()
        {
            var entregadas = new List<CuentaHolograma>();
            foreach (var c in cuentas)
                if (c != null && c.gameObject.activeSelf) entregadas.Add(c);
            cuentas.Clear();
            destinos.Clear();
            if (lineaColumna != null) lineaColumna.positionCount = 0;
            animandoDoblez = false;
            cortandoMet = false;
            return entregadas;
        }

        // ================= Utilidades de prueba rápida en Cocina_M7 =================

        public void NotificarToquePrueba(int indiceResiduo)
        {
            if (indiceResiduo == 20)
            {
                Debug.Log("[M7 Prueba] ¡Acertaste la cuenta 20 (Ser -> Gly)!");
                AlEncontrarCuenta();
            }
            else
            {
                erroresPrueba++;
                Debug.Log($"[M7 Prueba] Cuenta incorrecta ({indiceResiduo}). Errores: {erroresPrueba}");
                AlFallarBusca(erroresPrueba);
            }
        }

        [ContextMenu("Probar Turno 1 (Lineal 4 cuentas)")]
        public void ProbarTurno1()
        {
            var falsa = CrearCadenaFalsa(4);
            enFaseBusca = false;
            MostrarCadena(falsa, null, false);
        }

        [ContextMenu("Probar Turno 2 (Amilina Normal + Corte Met)")]
        public void ProbarTurno2()
        {
            var falsa = CrearCadenaFalsa(38);
            var malos = new HashSet<int> { 7 };
            enFaseBusca = false;
            MostrarCadena(falsa, malos, false);
        }

        [ContextMenu("Probar Turno 3 (Horquilla S20G + Fase Busca)")]
        public void ProbarTurno3Busca()
        {
            var falsa = CrearCadenaFalsa(38);
            erroresPrueba = 0;
            MostrarCadena(falsa, null, true, 20);
            AlCambiarFase(FaseCocina.Busca);
        }

        private List<AminoacidoData> CrearCadenaFalsa(int cantidad)
        {
            var lista = new List<AminoacidoData>(cantidad);
            for (int i = 0; i < cantidad; i++)
            {
                var aa = ScriptableObject.CreateInstance<AminoacidoData>();
                aa.abreviatura = i == 0 ? "Met" : (i == 20 ? "Gly" : "Ala");
                aa.color = Color.HSVToRGB((i * 0.07f) % 1f, 0.75f, 0.95f);
                lista.Add(aa);
            }
            return lista;
        }
    }
}