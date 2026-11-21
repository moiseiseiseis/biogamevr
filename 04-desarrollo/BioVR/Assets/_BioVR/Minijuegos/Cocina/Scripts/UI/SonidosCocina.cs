using System.Collections.Generic;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    /// <summary>
    /// Escucha EventosCocina.OnSonido y reproduce el AudioClip correspondiente si existe.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SonidosCocina : MonoBehaviour
    {
        [SerializeField] private SonidosCocinaData bancoSonidos;

        private AudioSource fuente;
        private readonly Dictionary<string, EntradaSonido> mapa = new Dictionary<string, EntradaSonido>();

        private void Awake()
        {
            fuente = GetComponent<AudioSource>();
            if (bancoSonidos == null) return;

            for (int i = 0; i < bancoSonidos.sonidos.Count; i++)
            {
                EntradaSonido e = bancoSonidos.sonidos[i];
                if (!string.IsNullOrEmpty(e.clave) && !mapa.ContainsKey(e.clave))
                    mapa.Add(e.clave, e);
            }
        }

        private void OnEnable()
        {
            EventosCocina.OnSonido += Reproducir;
        }

        private void OnDisable()
        {
            EventosCocina.OnSonido -= Reproducir;
        }

        private void Reproducir(string clave)
        {
            if (fuente == null || !mapa.TryGetValue(clave, out EntradaSonido entrada)) return;
            if (entrada.clip != null)
            {
                fuente.PlayOneShot(entrada.clip, entrada.volumen);
            }
        }
    }
}