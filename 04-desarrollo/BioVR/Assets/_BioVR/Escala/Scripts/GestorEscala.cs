using System;
using UnityEngine;
using BioVR.Secuencia;

namespace BioVR.Escala
{
    public class GestorEscala : MonoBehaviour
    {
        [Header("Configuración de Escalas")]
        [Tooltip("Asigna los 4 ScriptableObjects en orden: Hangar, Vasos, Capilar, Citoplasma (índice = GameManager.Salto).")]
        public NivelEscalaData[] nivelesEscala;

        [SerializeField]
        private int indiceEscalaActual = 0;

        // Evento para avisar a otros sistemas (partículas, UI, viñeta) cuándo cambia la escala
        public static event Action<NivelEscalaData> OnCambioEscala;

        public NivelEscalaData EscalaActual =>
            (nivelesEscala != null && indiceEscalaActual >= 0 && indiceEscalaActual < nivelesEscala.Length)
            ? nivelesEscala[indiceEscalaActual]
            : null;

        GameManager gm;

        // El GameManager (rol 4) lleva la cuenta de los saltos Matrioska;
        // este gestor solo escucha y aplica los datos del nivel que corresponde.
        void Start()
        {
            gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogError("[BioVR - Rol 5] GestorEscala no encontró un GameManager en la escena.", this);
                return;
            }
            gm.OnSaltoCambiado += IrAEscala;
            IrAEscala(gm.Salto); // por si el primer evento ya pasó
        }

        void OnDestroy()
        {
            if (gm != null) gm.OnSaltoCambiado -= IrAEscala;
        }

        /// <summary>
        /// Aplica una escala específica. La llama el GameManager en cada salto;
        /// también sirve como atajo de depuración.
        /// </summary>
        public void IrAEscala(int indice)
        {
            if (nivelesEscala == null || indice < 0 || indice >= nivelesEscala.Length)
            {
                Debug.LogWarning($"[BioVR - Rol 5] No hay NivelEscalaData para el índice {indice}.", this);
                return;
            }
            indiceEscalaActual = indice;
            AplicarEscalaActual();
        }

        private void AplicarEscalaActual()
        {
            NivelEscalaData datos = EscalaActual;
            if (datos == null) return;

            Debug.Log($"[BioVR - Rol 5] Escala actual: {datos.nombreNivel} | Viscosidad: {datos.viscosidad}");

            // Dispara el evento para que los efectos de partículas y UI se actualicen
            OnCambioEscala?.Invoke(datos);
        }
    }
}
