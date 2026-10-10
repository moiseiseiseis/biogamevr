using System;
using UnityEngine;

namespace BioVR.Secuencia
{
    public class ZonasManager : MonoBehaviour
    {
        [Serializable]
        public class Zona
        {
            public GameObject objeto;
            public GameState estado;
            public int salto = -1; // -1 = cualquier salto
        }

        [SerializeField] Zona[] zonas;

        GameManager gm;

        void Start()
        {
            gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogError("[Secuencia] ZonasManager no encontró un GameManager en la escena.", this);
                enabled = false;
                return;
            }
            gm.OnEstadoCambiado += AlCambiarEstado;
            gm.OnSaltoCambiado += AlCambiarSalto;
            Aplicar(); // por si el primer evento ya pasó
        }

        void OnDestroy()
        {
            if (gm == null) return;
            gm.OnEstadoCambiado -= AlCambiarEstado;
            gm.OnSaltoCambiado -= AlCambiarSalto;
        }

        void AlCambiarEstado(GameState _) => Aplicar();
        void AlCambiarSalto(int _) => Aplicar();

        void Aplicar()
        {
            foreach (var z in zonas)
                if (z.objeto != null)
                    z.objeto.SetActive(z.estado == gm.Estado && (z.salto < 0 || z.salto == gm.Salto));
        }
    }
}
