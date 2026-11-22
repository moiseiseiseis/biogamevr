using UnityEditor;
using UnityEngine;

namespace BioVR.Comun.Editor
{
    // Configuración de importación para el Quest de todo audio en Comun/Audio/ (la escena inicial).
    // Se aplica sola al importar o reimportar. Igual que AjustesAudioCocina, pero aquí suenan muchos loops a la vez
    // (zumbido, sistemas, flujo, corriente, partículas, desplazamiento y música), así que solo la música va en streaming:
    //   - MUS/ y VOZ/: Vorbis en streaming (largos, uno a la vez).
    //   - Loops de AMB/ y SFX/ (nombre con "-loop"): Vorbis comprimido en memoria (no abren un stream cada uno).
    //   - Lo demás (efectos cortos e interfaz): Vorbis descomprimido al cargar, para que suene sin retraso.
    // Los que suenan en 3D (nombre con "-3d-" y los esporádicos de SonidosEscena) se pasan a mono: un sonido
    // con posición en el espacio no necesita dos canales.
    public class AjustesAudioEscena : AssetPostprocessor
    {
        const string Carpeta = "Assets/_BioVR/Comun/Audio/";

        static readonly string[] Mono3D = { "-3d-", "SFX-INS-05", "SFX-ENT-01", "SFX-MEC-07" };

        void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(Carpeta)) return;
            var importador = (AudioImporter)assetImporter;
            string nombre = System.IO.Path.GetFileName(assetPath);
            // Música y voz (el prólogo: 13 partes, 4 min) van en streaming, una a la vez
            bool musica = assetPath.StartsWith(Carpeta + "MUS/") || assetPath.StartsWith(Carpeta + "VOZ/");
            bool loop = nombre.Contains("-loop");

            var ajustes = importador.defaultSampleSettings;
            ajustes.compressionFormat = AudioCompressionFormat.Vorbis;
            ajustes.quality = musica || loop ? 0.5f : 0.7f;
            ajustes.loadType = musica ? AudioClipLoadType.Streaming
                : loop ? AudioClipLoadType.CompressedInMemory
                : AudioClipLoadType.DecompressOnLoad;
            ajustes.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            importador.defaultSampleSettings = ajustes;
            importador.forceToMono = System.Array.Exists(Mono3D, nombre.Contains);
            importador.loadInBackground = musica || loop;
        }
    }
}
