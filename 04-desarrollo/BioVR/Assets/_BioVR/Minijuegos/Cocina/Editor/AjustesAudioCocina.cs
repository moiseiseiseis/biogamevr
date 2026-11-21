using UnityEditor;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina.Herramientas
{
    // Configuración de importación para el Quest de todo audio en Minijuegos/Cocina/Audio/ (se aplica sola al
    // importar o reimportar). Sigue la guía de diseño sonoro (sección 3.2): se entrega WAV y Unity comprime.
    //   - MUS/ y AMB/ (música y ambientes en loop, largos): Vorbis en streaming, para no ocupar memoria.
    //   - SFX/ y UI/ (cortos): Vorbis descomprimido al cargar, para que suenen sin retraso.
    // Se dejan en estéreo: en la Cocina suenan en 2D (sin posición), como la interfaz.
    public class AjustesAudioCocina : AssetPostprocessor
    {
        const string Carpeta = CrearDatosCocina.Raiz + "/Audio/";

        void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(Carpeta)) return;
            var importador = (AudioImporter)assetImporter;
            bool largo = assetPath.StartsWith(Carpeta + "MUS/") || assetPath.StartsWith(Carpeta + "AMB/");

            var ajustes = importador.defaultSampleSettings;
            ajustes.compressionFormat = AudioCompressionFormat.Vorbis;
            ajustes.quality = largo ? 0.5f : 0.7f;
            ajustes.loadType = largo ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            ajustes.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            importador.defaultSampleSettings = ajustes;
            importador.forceToMono = false;
            importador.loadInBackground = largo;
        }
    }
}
