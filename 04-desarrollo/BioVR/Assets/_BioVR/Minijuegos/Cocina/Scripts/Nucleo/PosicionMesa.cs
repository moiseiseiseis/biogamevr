using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // Coordenadas de la mesa (especificación 1.2). Todo se mide desde AnclaCocina: el piso debajo del
    // asiento, con +Z hacia el frente del jugador.
    //
    //   ángulo 0° = al frente; positivo = a la IZQUIERDA del jugador; negativo = a la derecha
    //   radio     = distancia horizontal desde el jugador (m)
    //   altura    = desde el piso (m)
    //
    // El HTML usa three.js, donde el frente es −Z; por eso aquí z lleva el signo contrario.
    public static class PosicionMesa
    {
        public const float AlturaMesa = 0.78f; // la mesa curva de Uli

        // Posición LOCAL respecto a AnclaCocina. Úsala con transform.localPosition.
        public static Vector3 En(float angulo, float radio, float altura = AlturaMesa)
        {
            float a = angulo * Mathf.Deg2Rad;
            return new Vector3(-radio * Mathf.Sin(a), altura, radio * Mathf.Cos(a));
        }

        // Gira el objeto (solo en Y) para que su +Z apunte al jugador, como el "mirar" del HTML.
        // Para un panel de texto (Canvas) usa alReves = true: un Canvas se lee cuando su +Z se aleja del que lo mira.
        public static void MirarAlJugador(Transform t, bool alReves = false)
        {
            var local = t.localPosition;
            var haciaJugador = new Vector3(-local.x, 0f, -local.z);
            if (haciaJugador.sqrMagnitude < 1e-6f) return;
            if (alReves) haciaJugador = -haciaJugador;
            var rot = Quaternion.LookRotation(haciaJugador, Vector3.up);
            t.localRotation = rot;
        }

        // Coloca y orienta en un paso.
        public static void Colocar(Transform t, float angulo, float radio, float altura = AlturaMesa, bool alReves = false)
        {
            t.localPosition = En(angulo, radio, altura);
            MirarAlJugador(t, alReves);
        }
    }
}
