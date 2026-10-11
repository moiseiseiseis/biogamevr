using System.Collections.Generic;
using BioVR.Secuencia;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BioVR.Escala
{
    // Protocolo Matrioska: desde la escala celular la cabina ya no es física, es una reconstrucción.
    // Al llegar al salto indicado, las paredes, el techo y el cristal desaparecen (para ver el escenario sin
    // estorbos) y lo demás (piso, consola, asiento) cambia a un material holográfico semitransparente.
    // Al volver a una escala mayor todo regresa como estaba.
    // Los botones y palancas (todo lo que sea interactuable) no se tocan: conservan sus colores de estado.
    // Va en el objeto "Cabina (provisional)".
    public class CabinaHolografica : MonoBehaviour
    {
        [SerializeField] Material holograma;

        [Tooltip("Desde qué salto la cabina se ve holográfica (3 = Citoplasma).")]
        [SerializeField] int saltoHolograma = 3;

        [Tooltip("Piezas que desaparecen en modo holograma (paredes, techo y cristal). Se compara el inicio del nombre.")]
        [SerializeField] string[] ocultar = { "CAB_Estructura", "CAB_Cristal", "CAB_Mobiliario" };

        readonly Dictionary<Renderer, Material[]> originales = new();
        readonly List<Renderer> ocultos = new();
        GameManager gm;
        bool activa;

        void Start()
        {
            gm = GameManager.Instance;
            if (gm == null || holograma == null)
            {
                Debug.LogError("[BioVR - Rol 5] CabinaHolografica necesita el GameManager en la escena y el material holograma.", this);
                enabled = false;
                return;
            }

            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                if (r.GetComponentInParent<XRBaseInteractable>(true) != null) continue; // botones y palanca no cambian
                if (SeOculta(r.name)) ocultos.Add(r);
                else originales[r] = r.sharedMaterials;
            }

            gm.OnSaltoCambiado += AlCambiarSalto;
            AlCambiarSalto(gm.Salto);
        }

        void OnDestroy()
        {
            if (gm != null) gm.OnSaltoCambiado -= AlCambiarSalto;
        }

        bool SeOculta(string nombre)
        {
            foreach (var e in ocultar)
                if (!string.IsNullOrEmpty(e) && nombre.StartsWith(e)) return true;
            return false;
        }

        void AlCambiarSalto(int salto)
        {
            bool holografica = salto >= saltoHolograma;
            if (holografica == activa) return;
            activa = holografica;

            foreach (var (r, mats) in originales)
            {
                if (r == null) continue;
                if (!holografica) { r.sharedMaterials = mats; continue; }
                var holo = new Material[mats.Length];
                for (int i = 0; i < holo.Length; i++) holo[i] = holograma;
                r.sharedMaterials = holo;
            }
            foreach (var r in ocultos)
                if (r != null) r.enabled = !holografica;
            Debug.Log($"[BioVR - Rol 5] Cabina {(holografica ? "holográfica (reconstrucción)" : "física")}.");
        }
    }
}
