using System;
using UnityEngine;

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

    void Start()
    {
        var gm = GameManager.Instance;
        gm.OnEstadoCambiado += _ => Aplicar();
        gm.OnSaltoCambiado += _ => Aplicar();
        Aplicar(); // por si el primer evento ya pasó
    }

    void Aplicar()
    {
        var gm = GameManager.Instance;
        foreach (var z in zonas)
            if (z.objeto != null)
                z.objeto.SetActive(z.estado == gm.Estado && (z.salto < 0 || z.salto == gm.Salto));
    }
}