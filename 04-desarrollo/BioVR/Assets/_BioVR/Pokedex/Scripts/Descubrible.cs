using UnityEngine;

namespace BioVR.Pokedex
{
    // Marca un objeto del viaje (una célula de la sangre, un organelo) como algo que se puede registrar en el pokédex.
    // El rayo del mando choca con los colliders de este objeto o de sus hijos; Pokedex busca este componente
    // hacia arriba desde el collider que tocó. Lo pone el menú BioVR → Agregar pokédex biológico.
    public class Descubrible : MonoBehaviour
    {
        public FichaPokedex ficha;
    }
}
