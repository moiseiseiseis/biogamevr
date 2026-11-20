using System;
using System.Collections.Generic;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    [Serializable]
    public class EntradaSonido
    {
        public string clave;
        public AudioClip clip;
        [Range(0f, 1f)] public float volumen = 1f;
    }

    /// <summary>
    /// ScriptableObject que vincula cada clave sonora de la Cocina con su AudioClip.
    /// </summary>
    [CreateAssetMenu(fileName = "SonidosCocina", menuName = "BioVR/Cocina/Sonidos Cocina")]
    public class SonidosCocinaData : ScriptableObject
    {
        public List<EntradaSonido> sonidos = new List<EntradaSonido>()
        {
            new EntradaSonido { clave = "acierto" },
            new EntradaSonido { clave = "error" },
            new EntradaSonido { clave = "clic" },
            new EntradaSonido { clave = "tic" },
            new EntradaSonido { clave = "llave" },
            new EntradaSonido { clave = "cargado" },
            new EntradaSonido { clave = "malo" },
            new EntradaSonido { clave = "sacude" },
            new EntradaSonido { clave = "lanzar" },
            new EntradaSonido { clave = "cae" },
            new EntradaSonido { clave = "recicla" },
            new EntradaSonido { clave = "golpe" },
            new EntradaSonido { clave = "pop" },
            new EntradaSonido { clave = "alarma" },
            new EntradaSonido { clave = "pliega" },
            new EntradaSonido { clave = "torcida" },
            new EntradaSonido { clave = "escala" },
            new EntradaSonido { clave = "chaperona" },
            new EntradaSonido { clave = "tritura" },
            new EntradaSonido { clave = "estrella" }
        };
    }
}