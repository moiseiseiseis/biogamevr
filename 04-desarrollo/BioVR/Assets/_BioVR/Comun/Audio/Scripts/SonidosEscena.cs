using System;
using System.Collections;
using System.Collections.Generic;
using BioVR.Cabina;
using BioVR.Jugador;
using BioVR.Secuencia;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Random = UnityEngine.Random;

namespace BioVR.Comun
{
    // Sonido de la escena inicial (hangar → abordaje → viaje → llegada), según la plantilla de banda sonora.
    // No modifica ningún script de otro rol: solo escucha los eventos que ya existen (GameManager, escotilla,
    // palanca, botones de la cabina, altura del asiento, menú de pausa y tutorial).
    //
    //   - Capas: loops 2D (ambientes y música) con un volumen por estado; al cambiar de estado o de escala
    //     se cruzan con un fundido. Volumen 0 = no suena (y se detiene para no gastar).
    //   - Momentos: sonidos de una vez (entrar a la cabina, saltos de escala, transiciones, controles).
    //   - Fijos 3D: loops pegados a un objeto de una zona (p. ej. la membrana de la célula); se oyen más al acercarse.
    //   - Esporádicos 3D: sonidos sueltos al azar cada tantos segundos (herramientas del hangar, glóbulos que pasan).
    //   - Interfaz: sonidos 2D que suenan aunque el juego esté en pausa (menú, botones, tutorial).
    //
    // Lo llena el menú BioVR → Agregar sonidos a la escena inicial. Los audios están en Comun/Audio/.
    public class SonidosEscena : MonoBehaviour
    {
        [Serializable]
        public class Capa
        {
            public string nombre;
            public AudioClip clip;
            [Tooltip("Volumen en cada estado: Hangar, Abordaje, Viaje, Llegada. 0 = no suena.")]
            public float[] volumenPorEstado = new float[4];
            [Tooltip("Solo en estas escalas (0 Hangar, 1 Vasos, 2 Capilar, 3 Citoplasma). Vacío = en todas.")]
            public int[] saltos = new int[0];
            [HideInInspector] public AudioSource fuente;
        }

        [Serializable]
        public class Fijo
        {
            public string nombre;
            public AudioClip clip;
            [Tooltip("Zonas (hijos de \"Zonas\") donde se busca el objeto.")]
            public string[] zonas = new string[0];
            [Tooltip("Se pega al primer objeto de la zona cuyo nombre contenga alguno de estos textos.")]
            public string[] objetos = new string[0];
            public float volumen = 0.6f;
            public float distanciaMaxima = 25f;
        }

        [Serializable]
        public class Esporadico
        {
            public string nombre;
            public AudioClip[] clips = new AudioClip[0];
            [Tooltip("Estados en que suena: Hangar, Abordaje, Viaje, Llegada.")]
            public bool[] estados = new bool[4];
            [Tooltip("Solo en estas escalas. Vacío = en todas.")]
            public int[] saltos = new int[0];
            [Tooltip("Segundos entre uno y otro (mínimo y máximo).")]
            public Vector2 intervalo = new Vector2(5f, 12f);
            [Tooltip("Si tiene nombre, suena en un punto al azar dentro de esa zona; si no, alrededor del jugador.")]
            public string zona;
            [Tooltip("Distancia al jugador (mínima y máxima) cuando suena alrededor de él.")]
            public Vector2 distancia = new Vector2(2f, 4f);
            public float volumen = 0.6f;
            [HideInInspector] public AudioSource fuente;
            [HideInInspector] public float reloj;
        }

        [Header("Capas (loops 2D por estado)")]
        [SerializeField] List<Capa> capas = new List<Capa>();
        [SerializeField] float fundido = 1.5f;

        [Header("Fijos y esporádicos (3D)")]
        [SerializeField] List<Fijo> fijos = new List<Fijo>();
        [SerializeField] List<Esporadico> esporadicos = new List<Esporadico>();

        [Header("Hangar y abordaje")]
        [SerializeField] AudioClip revelacionMecha;     // MUS-HAN-03, al empezar (y al volver al hangar)
        [SerializeField] AudioClip entradaCabina;       // MUS-ABO-01
        [SerializeField] AudioClip cinturon;            // SFX-CAB-01
        [SerializeField] AudioClip encenderPantallas;   // SFX-CAB-02
        [SerializeField] AudioClip compuerta;           // SFX-MEC-01, escotilla

        [Header("Saltos de escala")]
        [SerializeField] AudioClip repliegue;           // SFX-MEC-04, antes del primer salto
        [SerializeField] AudioClip primeraReduccion;    // SFX-ESC-02
        [Tooltip("Salto 2 (1 µm), 3 (50 nm) y 4 (5 nm).")]
        [SerializeField] AudioClip[] reducciones = new AudioClip[0]; // SFX-ESC-04
        [SerializeField] AudioClip reduccionCompletada; // SFX-ESC-03
        [SerializeField] AudioClip entradaCelula;       // SFX-ENT-07, al llegar al citoplasma
        [SerializeField] AudioClip[] transiciones = new AudioClip[0]; // MUS-GEN-07, cambios de estado sin salto
        [SerializeField] AudioClip musicaCarga;         // MUS-GEN-01, mientras carga la Cocina

        [Header("Controles de la cabina")]
        [SerializeField] AudioClip boton;               // SFX-CAB-03
        [SerializeField] AudioClip noDisponible;        // UI-FBK-04, botón que no aplica en ese momento
        [SerializeField] AudioClip palanca;             // SFX-CAB-04
        [SerializeField] AudioClip ajusteAsiento;       // SFX-CAB-06

        [Header("Interfaz (suena aunque esté en pausa)")]
        [SerializeField] AudioClip musicaPausa;         // MUS-GEN-02
        [SerializeField] AudioClip abrirPanel;          // UI-NAV-04
        [SerializeField] AudioClip cerrarPanel;         // UI-NAV-03
        [SerializeField] AudioClip hover;               // UI-NAV-01
        [SerializeField] AudioClip tocarPantalla;       // UI-INT-04
        [SerializeField] AudioClip pasoTutorial;        // UI-FBK-01

        [Header("Volúmenes")]
        [SerializeField] float volumenMomentos = 0.8f;
        [SerializeField] float volumenInterfaz = 0.6f;
        [SerializeField] float volumenPausa = 0.4f;

        public static SonidosEscena Instancia { get; private set; }

        GameManager gm;
        Transform zonas;
        Transform oyente;
        AudioSource momentos, interfaz, pausa;
        GameState estadoAnterior;
        bool enSalto;
        bool cargando;
        int transicion;
        PasoTutorial pasoAnterior;

        void Awake() => Instancia = this;

        void Start()
        {
            gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogError("[Sonido] SonidosEscena necesita un GameManager en la escena.", this);
                enabled = false;
                return;
            }
            var raizZonas = GameObject.Find("Zonas");
            zonas = raizZonas != null ? raizZonas.transform : null;
            var listener = FindFirstObjectByType<AudioListener>();
            oyente = listener != null ? listener.transform : Camera.main != null ? Camera.main.transform : transform;

            momentos = Fuente2D("Momentos", false);
            interfaz = Fuente2D("Interfaz", true);
            pausa = Fuente2D("Musica de pausa", true);
            pausa.loop = true;
            pausa.clip = musicaPausa;
            pausa.volume = volumenPausa;

            foreach (var c in capas)
            {
                if (c.clip == null) continue;
                c.fuente = Fuente2D("Capa " + c.nombre, false);
                c.fuente.clip = c.clip;
                c.fuente.loop = true;
                c.fuente.volume = 0f;
            }
            foreach (var e in esporadicos)
            {
                e.fuente = Fuente3D("Esporadico " + e.nombre, transform, 30f);
                e.reloj = Random.Range(e.intervalo.x, e.intervalo.y);
            }
            PonerFijos();
            Conectar();

            estadoAnterior = gm.Estado;
            AplicarCapas(instantaneo: true);
            if (gm.Estado == GameState.Hangar) StartCoroutine(Tocar(revelacionMecha, 1.5f));
        }

        void OnDestroy()
        {
            if (Instancia == this) Instancia = null;
            if (gm != null)
            {
                gm.OnEstadoCambiado -= AlCambiarEstado;
                gm.OnSaltoIniciado -= AlIniciarSalto;
                gm.OnSaltoCambiado -= AlCambiarSalto;
                gm.OnCargandoCocina -= AlCargarCocina;
            }
            Escotilla.OnAfuera -= AlUsarEscotilla;
            AlturaAsiento.OnAjuste -= AlAjustarAsiento;
            MenuPausa.OnMenu -= AlAbrirMenu;
            Tutorial.OnPasoCambiado -= AlCambiarPaso;
        }

        // ---------------- Conexiones con lo que ya existe ----------------

        void Conectar()
        {
            gm.OnEstadoCambiado += AlCambiarEstado;
            gm.OnSaltoIniciado += AlIniciarSalto;
            gm.OnSaltoCambiado += AlCambiarSalto;
            gm.OnCargandoCocina += AlCargarCocina;
            Escotilla.OnAfuera += AlUsarEscotilla;
            AlturaAsiento.OnAjuste += AlAjustarAsiento;
            MenuPausa.OnMenu += AlAbrirMenu;
            Tutorial.OnPasoCambiado += AlCambiarPaso;

            // Botones de la consola (los crea BioVR → Integrar controles de cabina con estos nombres)
            foreach (var b in FindObjectsByType<XRSimpleInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (b.name == "Boton Iniciar inmersion")
                    b.selectEntered.AddListener(_ => AlPresionarBoton(gm.Estado != GameState.Viaje));
                else if (b.name == "Boton Salto Matrioska")
                    b.selectEntered.AddListener(_ => AlPresionarBoton(gm.Estado == GameState.Viaje || gm.Estado == GameState.Llegada));
            }
            foreach (var p in FindObjectsByType<Palanca>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                p.alLlegarAlTope.AddListener(() => Tocar2D(palanca));

            // Botones de los paneles (menú de pausa, tutorial): hover y clic
            foreach (var b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (b.GetComponent<SonidoBotonUI>() == null) b.gameObject.AddComponent<SonidoBotonUI>();
        }

        void AlCambiarEstado(GameState estado)
        {
            if (estado == estadoAnterior) return;
            estadoAnterior = estado;
            AplicarCapas(instantaneo: false);

            if (estado == GameState.Hangar) StartCoroutine(Tocar(revelacionMecha, 1f));
            else if (estado == GameState.Abordaje)
            {
                StartCoroutine(Tocar(entradaCabina, 0f));
                StartCoroutine(Tocar(cinturon, 0.4f));
                StartCoroutine(Tocar(encenderPantallas, 1.2f));
            }
            else if (!enSalto && transiciones.Length > 0)
            {
                // Viaje → Llegada, Llegada → Hangar: cambios con fundido a negro, sin salto
                StartCoroutine(Tocar(transiciones[transicion % transiciones.Length], 0f));
                transicion++;
            }
        }

        void AlIniciarSalto(int salto)
        {
            enSalto = true;
            if (salto == 1)
            {
                StartCoroutine(Tocar(repliegue, 0f));
                StartCoroutine(Tocar(primeraReduccion, 0.5f));
            }
            else if (salto - 2 < reducciones.Length)
                StartCoroutine(Tocar(reducciones[salto - 2], 0f));
        }

        void AlCambiarSalto(int salto)
        {
            AplicarCapas(instantaneo: false);
            if (!enSalto || salto == 0) return;
            enSalto = false;
            StartCoroutine(Tocar(reduccionCompletada, 0.6f));
            if (salto == GameManager.SaltoMaximo) StartCoroutine(Tocar(entradaCelula, 0.2f));
        }

        // Mientras carga la Cocina: se callan las capas y suena la música de carga hasta que cambia la escena
        void AlCargarCocina()
        {
            cargando = true;
            foreach (var c in capas)
                if (c.fuente != null) c.fuente.Stop();
            if (musicaCarga == null) return;
            pausa.clip = musicaCarga;
            pausa.volume = volumenPausa;
            pausa.Play();
        }

        void AlPresionarBoton(bool aplica)
        {
            Tocar2D(boton);
            if (!aplica || gm.Pausado) Tocar2D(noDisponible, interfaz);
        }

        void AlUsarEscotilla(bool afuera) => Tocar2D(compuerta);
        void AlAjustarAsiento(float _) => Tocar2D(ajusteAsiento, interfaz);

        void AlAbrirMenu(bool abierto)
        {
            Tocar2D(abierto ? abrirPanel : cerrarPanel, interfaz);
            if (pausa.clip == null) return;
            if (abierto) pausa.Play();
            else pausa.Stop();
        }

        // El tutorial avisa null cuando un paso se completa ("¡Bien!"); también al terminar, pero entonces
        // el paso anterior ya era null y no suena dos veces
        void AlCambiarPaso(PasoTutorial paso)
        {
            if (paso == null && pasoAnterior != null) Tocar2D(pasoTutorial, interfaz);
            pasoAnterior = paso;
        }

        // Para SonidoBotonUI
        public void Hover() => Tocar2D(hover, interfaz, 0.5f);
        public void Clic() => Tocar2D(tocarPantalla, interfaz);

        // ---------------- Capas ----------------

        void AplicarCapas(bool instantaneo)
        {
            foreach (var c in capas)
            {
                if (c.fuente == null) continue;
                float objetivo = Objetivo(c);
                if (instantaneo)
                {
                    c.fuente.volume = objetivo;
                    if (objetivo > 0f && !c.fuente.isPlaying) c.fuente.Play();
                }
                else if (objetivo > 0f && !c.fuente.isPlaying)
                {
                    c.fuente.volume = 0f;
                    c.fuente.Play();
                }
            }
        }

        float Objetivo(Capa c)
        {
            int e = (int)gm.Estado;
            float v = e < c.volumenPorEstado.Length ? c.volumenPorEstado[e] : 0f;
            if (c.saltos.Length > 0 && Array.IndexOf(c.saltos, gm.Salto) < 0) v = 0f;
            return v;
        }

        void Update()
        {
            if (cargando) return;
            float paso = Time.deltaTime / Mathf.Max(0.01f, fundido);
            foreach (var c in capas)
            {
                if (c.fuente == null) continue;
                float objetivo = Objetivo(c);
                c.fuente.volume = Mathf.MoveTowards(c.fuente.volume, objetivo, paso);
                if (objetivo <= 0f && c.fuente.volume <= 0f && c.fuente.isPlaying) c.fuente.Stop();
            }

            foreach (var e in esporadicos)
            {
                if (e.clips.Length == 0 || !Activo(e)) continue;
                e.reloj -= Time.deltaTime;
                if (e.reloj > 0f) continue;
                e.reloj = Random.Range(e.intervalo.x, e.intervalo.y);
                var clip = e.clips[Random.Range(0, e.clips.Length)];
                if (clip == null) continue;
                e.fuente.transform.position = PuntoEsporadico(e);
                e.fuente.pitch = Random.Range(0.92f, 1.08f);
                e.fuente.PlayOneShot(clip, e.volumen);
            }
        }

        bool Activo(Esporadico e)
        {
            int i = (int)gm.Estado;
            if (i >= e.estados.Length || !e.estados[i]) return false;
            return e.saltos.Length == 0 || Array.IndexOf(e.saltos, gm.Salto) >= 0;
        }

        Vector3 PuntoEsporadico(Esporadico e)
        {
            var zona = string.IsNullOrEmpty(e.zona) || zonas == null ? null : zonas.Find(e.zona);
            if (zona != null)
            {
                // Un punto al azar dentro de lo que se ve de la zona, lejos del jugador
                var limites = Limites(zona);
                for (int intento = 0; intento < 8; intento++)
                {
                    var p = new Vector3(
                        Random.Range(limites.min.x, limites.max.x),
                        Random.Range(limites.min.y, Mathf.Min(limites.max.y, limites.min.y + 4f)),
                        Random.Range(limites.min.z, limites.max.z));
                    if (Vector3.Distance(p, oyente.position) > 4f) return p;
                }
                return limites.center;
            }
            // Alrededor del jugador, de lado o adelante (no justo encima)
            var dir = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(-150f, 150f), 0f) * Vector3.forward;
            return oyente.position + dir * Random.Range(e.distancia.x, e.distancia.y);
        }

        static Bounds Limites(Transform t)
        {
            var renderers = t.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds(t.position, Vector3.one * 10f);
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            return b;
        }

        // ---------------- Fijos ----------------

        void PonerFijos()
        {
            if (zonas == null) return;
            foreach (var f in fijos)
            {
                if (f.clip == null) continue;
                foreach (var nombreZona in f.zonas)
                {
                    var zona = zonas.Find(nombreZona);
                    var objeto = zona != null ? Buscar(zona, f.objetos) : null;
                    if (objeto == null)
                    {
                        Debug.Log($"[Sonido] {f.nombre}: no hay {string.Join("/", f.objetos)} en la zona {nombreZona}; no suena ahí.");
                        continue;
                    }
                    // Hijo del objeto: se apaga y se prende con su zona y sigue al objeto si se mueve
                    var fuente = Fuente3D(f.nombre, objeto, f.distanciaMaxima);
                    fuente.transform.position = Limites(objeto).center;
                    fuente.clip = f.clip;
                    fuente.loop = true;
                    fuente.volume = f.volumen;
                    fuente.playOnAwake = true;
                    if (fuente.isActiveAndEnabled) fuente.Play();
                }
            }
        }

        static Transform Buscar(Transform raiz, string[] nombres)
        {
            foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
                foreach (var n in nombres)
                    if (t.name.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0) return t;
            return null;
        }

        // ---------------- Fuentes ----------------

        AudioSource Fuente2D(string nombre, bool sonarEnPausa)
        {
            var go = new GameObject(nombre);
            go.transform.SetParent(transform, false);
            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            s.ignoreListenerPause = sonarEnPausa;
            return s;
        }

        static AudioSource Fuente3D(string nombre, Transform padre, float distanciaMaxima)
        {
            var go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 1f;
            s.rolloffMode = AudioRolloffMode.Linear;
            s.minDistance = 1.5f;
            s.maxDistance = distanciaMaxima;
            s.dopplerLevel = 0f;
            return s;
        }

        void Tocar2D(AudioClip clip, AudioSource fuente = null, float volumen = 1f)
        {
            if (clip == null) return;
            fuente = fuente != null ? fuente : momentos;
            fuente.PlayOneShot(clip, volumen * (fuente == interfaz ? volumenInterfaz : volumenMomentos));
        }

        IEnumerator Tocar(AudioClip clip, float retraso)
        {
            if (clip == null) yield break;
            if (retraso > 0f) yield return new WaitForSeconds(retraso);
            Tocar2D(clip);
        }
    }
}
