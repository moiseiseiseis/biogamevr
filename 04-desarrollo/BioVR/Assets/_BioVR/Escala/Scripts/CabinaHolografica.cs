using System.Collections.Generic;
using BioVR.Secuencia;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BioVR.Escala
{
    // Protocolo Matrioska: desde la escala celular la cabina ya no es física, es una reconstrucción.
    // Al llegar al salto indicado cambia el material de las piezas de la cabina por uno holográfico
    // (semitransparente) y al volver a una escala mayor regresa los materiales originales.
    // Los botones y palancas (todo lo que sea interactuable) no se tocan: conservan sus colores de estado.
    // Va en el objeto "Cabina (provisional)".
    public class CabinaHolografica : MonoBehaviour
    {
        [SerializeField] Material holograma;

        [Tooltip("Desde qué salto la cabina se ve holográfica (3 = Citoplasma).")]
        [SerializeField] int saltoHolograma = 3;

        readonly Dictionary<Renderer, Material[]> originales = new();
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
                if (r.GetComponentInParent<XRBaseInteractable>(true) == null)
                    originales[r] = r.sharedMaterials;

            gm.OnSaltoCambiado += AlCambiarSalto;
            AlCambiarSalto(gm.Salto);
        }

        void OnDestroy()
        {
            if (gm != null) gm.OnSaltoCambiado -= AlCambiarSalto;
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
            Debug.Log($"[BioVR - Rol 5] Cabina {(holografica ? "holográfica (reconstrucción)" : "física")}.");
        }
    }
}
