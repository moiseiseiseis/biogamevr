using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BioVR.Entornos;
using BioVR.Secuencia;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BioVR.Prologo
{
    // Prólogo: pasarela de modelos de Diseño (#91) con la voz de Emi (13 audios) y música de fondo.
    //
    // El jugador está quieto; cada modelo entra por la derecha, se detiene al centro y gira despacio mientras suena
    // su audio (con subtítulos), y sale por la izquierda cuando entra el siguiente (máximo 2 a la vez).
    // "Reemplaza" (la Tierra seca) aparece en el centro mientras el anterior se encoge, como un cambio de textura.
    // Versión de la feria: solo las entradas marcadas "en feria" (01, 05, 09 y 11, ~1.7 min). Botón para saltar.
    // Al terminar: fundido a negro y la escena siguiente (el hangar).
    // Lo arma el menú BioVR → Prólogo → Armar prólogo; los textos vienen de la transcripción de Sonido.
    public class Pasarela : MonoBehaviour
    {
        public enum Tipo { Pasa, Reemplaza }

        [Serializable]
        public class Entrada
        {
            public int numero;
            public GameObject modelo;
            public AudioClip voz;
            [TextArea(3, 8)] public string texto;
            public Tipo tipo = Tipo.Pasa;
            public bool enFeria;
        }

        [SerializeField] List<Entrada> entradas = new List<Entrada>();
        [Tooltip("Solo las entradas \"en feria\" (versión corta). Apagado: las 13.")]
        [SerializeField] bool versionFeria = true;

        [Header("Recorrido")]
        [SerializeField] Transform centro;
        [SerializeField] Vector3 desdeCentroEntrada = new Vector3(2.8f, 0f, 0.6f);
        [SerializeField] Vector3 desdeCentroSalida = new Vector3(-2.8f, 0f, 0.6f);
        [SerializeField] float duracionTraslado = 1.6f;
        [SerializeField] float giro = 18f;      // grados por segundo mientras está a la vista
        [SerializeField] float pausaEntreAudios = 0.5f;

        [Header("Sonido y texto")]
        [SerializeField] AudioSource voz;
        [SerializeField] AudioSource musica;
        [SerializeField] TextMeshPro subtitulo;
        [SerializeField] int largoSubtitulo = 120; // caracteres por frase en pantalla

        [Header("Final")]
        [SerializeField] ScreenFader fader;
        [SerializeField] string escenaSiguiente = "00-EscenaInicial";
        [SerializeField] float fundido = 1.2f;
        [Tooltip("Segundos que tarda la música en apagarse al final; empieza ese tiempo antes de que termine el prólogo.")]
        [SerializeField] float fundidoMusica = 5f;

        readonly List<Transform> aLaVista = new List<Transform>();
        bool terminando;
        Coroutine fundiendoMusica;

        IEnumerator Start()
        {
            foreach (var e in entradas)
                if (e.modelo != null) e.modelo.SetActive(false);
            if (subtitulo != null) subtitulo.text = "";
            if (fader != null)
            {
                yield return fader.FadeA(1f, 0f, Color.black); // empieza en negro
                yield return MirarAlCentro();
                yield return fader.FadeA(0f, fundido);
            }
            if (musica != null) musica.Play();

            // La última entrada que se va a ver: al empezar su voz ya se sabe cuánto falta para terminar
            Entrada ultima = null;
            foreach (var e in entradas)
                if (e.modelo != null && (!versionFeria || e.enFeria)) ultima = e;

            Transform anterior = null;
            foreach (var e in entradas)
            {
                if (terminando) yield break;
                if (e.modelo == null || (versionFeria && !e.enFeria)) continue;
                var actual = e.modelo.transform;

                // El anterior sale (o se encoge si el nuevo lo reemplaza) mientras el nuevo entra
                if (anterior != null)
                {
                    if (e.tipo == Tipo.Reemplaza) StartCoroutine(Encoger(anterior));
                    else StartCoroutine(Mover(anterior, centro.position, centro.position + desdeCentroSalida, ocultarAlFinal: true));
                }
                actual.gameObject.SetActive(true);
                aLaVista.Add(actual);
                if (e.tipo == Tipo.Reemplaza) yield return Crecer(actual);
                else yield return Mover(actual, centro.position + desdeCentroEntrada, centro.position, ocultarAlFinal: false);

                var nave = actual.GetComponentInChildren<NaveDespliegue>();
                if (nave != null) nave.Desplegar();

                if (e == ultima)
                {
                    // Lo que falta: la voz, la pausa, la salida del modelo y el fundido a negro
                    float falta = (e.voz != null ? e.voz.length : 0f) + pausaEntreAudios + duracionTraslado + (fader != null ? fundido : 0f);
                    fundiendoMusica = StartCoroutine(FundirMusica(Mathf.Max(0f, falta - fundidoMusica), Mathf.Min(fundidoMusica, falta)));
                }
                if (e.voz != null) yield return Narrar(e);
                yield return new WaitForSeconds(pausaEntreAudios);
                anterior = actual;
            }

            if (anterior != null) yield return Mover(anterior, centro.position, centro.position + desdeCentroSalida, ocultarAlFinal: true);
            yield return Terminar();
        }

        // Mientras está en negro, gira al jugador (sobre su cabeza) hacia donde pasan los modelos: el visor arranca
        // mirando hacia donde quedó el límite del Quest, que puede ser cualquier lado.
        IEnumerator MirarAlCentro()
        {
            var origen = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (origen == null || centro == null) yield break;
            float limite = Time.time + 2f;
            while (origen.CameraInOriginSpaceHeight < 0.2f && Time.time < limite) yield return null;
            yield return null;
            var hacia = Vector3.ProjectOnPlane(centro.position - origen.Camera.transform.position, Vector3.up);
            if (hacia.sqrMagnitude > 1e-4f) origen.MatchOriginUpCameraForward(Vector3.up, hacia.normalized);
        }

        void Update()
        {
            foreach (var t in aLaVista)
                if (t != null && t.gameObject.activeSelf) t.Rotate(Vector3.up, giro * Time.deltaTime, Space.World);
        }

        // La voz y sus frases: cada frase dura lo que le toca según su largo
        IEnumerator Narrar(Entrada e)
        {
            voz.clip = e.voz;
            voz.Play();
            var frases = Frases(e.texto, largoSubtitulo);
            int total = 0;
            foreach (var f in frases) total += f.Length;
            foreach (var f in frases)
            {
                if (subtitulo != null) subtitulo.text = f;
                float t = e.voz.length * f.Length / Mathf.Max(1, total);
                for (float s = 0f; s < t && voz.isPlaying; s += Time.deltaTime) yield return null;
            }
            while (voz.isPlaying) yield return null;
            if (subtitulo != null) subtitulo.text = "";
        }

        // Parte el párrafo en frases (por punto, signo o dos puntos) y junta las cortas hasta "largo" caracteres
        static List<string> Frases(string texto, int largo)
        {
            var r = new List<string>();
            if (string.IsNullOrWhiteSpace(texto)) return r;
            var partes = Regex.Split(texto.Trim(), @"(?<=[\.\?\!:;])\s+");
            string actual = "";
            foreach (var p in partes)
            {
                if (actual.Length > 0 && actual.Length + 1 + p.Length > largo) { r.Add(actual); actual = p; }
                else actual = actual.Length == 0 ? p : actual + " " + p;
            }
            if (actual.Length > 0) r.Add(actual);
            return r;
        }

        IEnumerator Mover(Transform t, Vector3 desde, Vector3 hasta, bool ocultarAlFinal)
        {
            for (float s = 0f; s < duracionTraslado; s += Time.deltaTime)
            {
                t.position = Vector3.Lerp(desde, hasta, Mathf.SmoothStep(0f, 1f, s / duracionTraslado));
                yield return null;
            }
            t.position = hasta;
            if (ocultarAlFinal) { t.gameObject.SetActive(false); aLaVista.Remove(t); }
        }

        IEnumerator Crecer(Transform t)
        {
            var escala = t.localScale;
            t.position = centro.position;
            for (float s = 0f; s < duracionTraslado; s += Time.deltaTime)
            {
                t.localScale = escala * Mathf.SmoothStep(0f, 1f, s / duracionTraslado);
                yield return null;
            }
            t.localScale = escala;
        }

        IEnumerator Encoger(Transform t)
        {
            var escala = t.localScale;
            for (float s = 0f; s < duracionTraslado; s += Time.deltaTime)
            {
                t.localScale = escala * (1f - Mathf.SmoothStep(0f, 1f, s / duracionTraslado));
                yield return null;
            }
            t.gameObject.SetActive(false);
            t.localScale = escala;
            aLaVista.Remove(t);
        }

        IEnumerator FundirMusica(float espera, float duracion)
        {
            if (musica == null) yield break;
            if (espera > 0f) yield return new WaitForSeconds(espera);
            float inicial = musica.volume;
            for (float s = 0f; s < duracion; s += Time.deltaTime)
            {
                musica.volume = inicial * (1f - s / duracion);
                yield return null;
            }
            musica.volume = 0f;
        }

        // Botón "Saltar prólogo"
        public void Saltar()
        {
            if (terminando) return;
            StopAllCoroutines();
            fundiendoMusica = null;
            StartCoroutine(Terminar());
        }

        IEnumerator Terminar()
        {
            terminando = true;
            if (voz != null) voz.Stop();
            if (subtitulo != null) subtitulo.text = "";
            // Si se saltó el prólogo, la música se apaga junto con el fundido a negro
            if (fundiendoMusica == null) fundiendoMusica = StartCoroutine(FundirMusica(0f, fader != null ? fundido : 0.3f));
            if (fader != null) yield return fader.FadeA(1f, fundido, Color.black);
            if (musica != null) musica.Stop();
            SceneManager.LoadScene(escenaSiguiente);
        }
    }
}
