using System.Collections.Generic;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // Todos los textos de la Cocina, por clave. El código solo usa claves; el texto vive aquí
    // para que Investigación o Narrativa lo cambien sin tocar scripts.
    // M1 llena las claves de mensajes y rechazos; M8 agrega las del tutorial y la pantalla final.
    [CreateAssetMenu(menuName = "BioVR/Cocina/Textos", fileName = "TextosCocina")]
    public class TextosCocina : ScriptableObject
    {
        [System.Serializable]
        public class Entrada
        {
            public string clave;
            [TextArea(2, 6)] public string texto;
        }

        public List<Entrada> textos = new List<Entrada>();

        Dictionary<string, string> indice;

        void OnEnable() => indice = null;

        public bool Tiene(string clave)
        {
            if (indice == null) Indexar();
            return clave != null && indice.ContainsKey(clave);
        }

        // El texto de la clave. Si no existe, devuelve "[clave]" para que se note en el visor que falta.
        public string Texto(string clave)
        {
            if (indice == null) Indexar();
            return clave != null && indice.TryGetValue(clave, out var t) ? t : $"[{clave}]";
        }

        void Indexar()
        {
            indice = new Dictionary<string, string>();
            foreach (var e in textos)
                if (!string.IsNullOrEmpty(e.clave)) indice[e.clave] = e.texto;
        }

        // Solo para el editor (el armador llena las claves que falten sin borrar las que alguien ya cambió).
        public bool AgregarSiFalta(string clave, string texto)
        {
            if (Tiene(clave)) return false;
            textos.Add(new Entrada { clave = clave, texto = texto });
            indice = null;
            return true;
        }
    }
}
