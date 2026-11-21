using BioVR.Secuencia;
using UnityEngine;

namespace BioVR.Entornos
{
    // Animación de la Seraphim (seraphim-mecha.fbx de Diseño, cuadros 1 a 60: despliega 6 brazos y 2 patas).
    //
    //   - En la escena inicial: en el hangar está desplegada (parada en sus patas). Al abordar (Iniciar inmersión)
    //     se repliega, con el sonido de los élitros, lista para reducirse. Al volver al hangar aparece desplegada.
    //   - En el prólogo: Desplegar() la abre cuando llega al centro de la pasarela.
    // Usa el componente Animation (el FBX se importa como Legacy). Lo pone BioVR → Poner nave Seraphim.
    public class NaveDespliegue : MonoBehaviour
    {
        [Tooltip("Si escucha al GameManager (escena inicial). En el prólogo va apagado.")]
        [SerializeField] bool seguirAlJuego = true;
        [SerializeField] AudioClip sonidoPliegue;   // SFX-MEC-03 élitros
        [SerializeField] AudioClip sonidoDespliegue; // SFX-MEC-02 patas
        [SerializeField] float volumen = 0.8f;

        Animation anim;
        AudioSource fuente;
        GameManager gm;

        void Awake()
        {
            anim = GetComponentInChildren<Animation>();
            fuente = gameObject.AddComponent<AudioSource>();
            fuente.playOnAwake = false;
            fuente.spatialBlend = 1f;
            fuente.minDistance = 4f;
            fuente.maxDistance = 60f;
            fuente.rolloffMode = AudioRolloffMode.Linear;
        }

        void Start()
        {
            if (anim == null || anim.clip == null) { Debug.LogWarning("[Nave] La Seraphim no trae su animación (¿se importó como Legacy?).", this); enabled = false; return; }
            anim.playAutomatically = false;
            if (!seguirAlJuego) { Pose(0f); return; } // plegada, esperando a la pasarela
            gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnEstadoCambiado += AlCambiarEstado;
            Pose(gm.Estado == GameState.Hangar ? 1f : 0f);
        }

        void OnDestroy()
        {
            if (gm != null) gm.OnEstadoCambiado -= AlCambiarEstado;
        }

        void AlCambiarEstado(GameState estado)
        {
            if (estado == GameState.Hangar) Pose(1f);           // llega un jugador nuevo: desplegada
            else if (estado == GameState.Abordaje) Plegar();     // se prepara para reducirse
        }

        public void Desplegar() => Reproducir(1f, sonidoDespliegue);
        public void Plegar() => Reproducir(-1f, sonidoPliegue);

        void Reproducir(float velocidad, AudioClip sonido)
        {
            if (anim == null || anim.clip == null) return;
            var estado = anim[anim.clip.name];
            estado.wrapMode = WrapMode.ClampForever;
            estado.speed = velocidad;
            estado.normalizedTime = velocidad > 0f ? 0f : 1f;
            anim.Play(anim.clip.name);
            if (sonido != null) fuente.PlayOneShot(sonido, volumen);
        }

        // Pose fija: 0 = plegada (cuadro 1), 1 = desplegada (cuadro 60)
        void Pose(float t)
        {
            var estado = anim[anim.clip.name];
            estado.wrapMode = WrapMode.ClampForever;
            anim.Play(anim.clip.name);
            estado.normalizedTime = t;
            estado.speed = 0f;
            anim.Sample();
        }
    }
}
