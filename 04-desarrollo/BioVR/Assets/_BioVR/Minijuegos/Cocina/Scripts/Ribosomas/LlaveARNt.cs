using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Minijuegos.Cocina
{
    // M4 · Girar el ARNt como llave en el ribosoma de mesa (especificación 5.3; HTML: colocar con riboA, girarLlave).
    //
    //   1. M1 aceptó el ARNt en modo Llave: entra "chueco" (giro inicial al azar de ±57° a 97° e inclinado) y
    //      aparece el factor eEF1A en la punta.
    //   2. El jugador lo agarra y gira la muñeca: el ARNt NO se mueve con la mano (se apaga el seguimiento del
    //      XRGrabInteractable); solo se lee cuánto giró la muñeca alrededor del eje del ARNt.
    //   3. Encaja si queda a menos de 17° del derecho o si al girar rápido pasa por el cero. Entonces vibra,
    //      se va eEF1A y el ribosoma llama a ControlCocina.Aceptar.
    //
    // Si lo suelta sin encajar, se queda en la llave (como en el HTML) hasta que lo vuelva a agarrar.
    public class LlaveARNt : MonoBehaviour
    {
        [SerializeField] GameObject eEF1A;                 // esfera verde azulado; la mueve a la punta del ARNt
        [SerializeField] float alturaEEF1A = 0.17f;        // sobre el sitio A (la punta del ARNt)
        [SerializeField] Vector2 giroInicial = new Vector2(57f, 97f);
        [SerializeField] float inclinacionInicial = 31.5f; // −0.55 rad en Z; se endereza conforme se acerca al cero
        [SerializeField] float pasoTic = 10f;              // vibración corta cada 10°

        public bool Ocupada => arnt != null;

        RibosomaCocina ribosoma;
        ARNtCocina arnt;
        XRGrabInteractable agarre;
        bool seguiaPosicion, seguiaRotacion;

        float giro;             // grados; 0 = derecho
        float giroInicio;       // giro al momento de agarrarlo
        float maximo;           // giro con el que entró (para enderezar la inclinación)
        IXRSelectInteractor mano;
        Quaternion manoInicio;
        Vector3 eje;            // eje del ARNt (el "arriba" del sitio A)

        public void Empezar(ARNtCocina t, RibosomaCocina r)
        {
            Cancelar();
            ribosoma = r;
            arnt = t;
            agarre = t.GetComponent<XRGrabInteractable>();
            PuenteCorriente.Soltar(t);
            PuenteCorriente.PermitirAgarre(t, true);

            if (agarre != null)
            {
                seguiaPosicion = agarre.trackPosition;
                seguiaRotacion = agarre.trackRotation;
                agarre.trackPosition = false;   // agarrarlo no lo saca del sitio A
                agarre.trackRotation = false;   // el giro lo calcula este script
                agarre.selectEntered.AddListener(AlAgarrar);
                agarre.selectExited.AddListener(AlSoltar);
            }

            float signo = Random.value < 0.5f ? -1f : 1f;
            giro = signo * Random.Range(giroInicial.x, giroInicial.y);
            maximo = Mathf.Abs(giro);
            mano = null;
            Colocar();

            if (eEF1A != null) eEF1A.SetActive(true);
        }

        // Se va sin encajar (cambio de turno o de fase)
        public void Cancelar()
        {
            if (agarre != null)
            {
                agarre.selectEntered.RemoveListener(AlAgarrar);
                agarre.selectExited.RemoveListener(AlSoltar);
                agarre.trackPosition = seguiaPosicion;
                agarre.trackRotation = seguiaRotacion;
            }
            agarre = null;
            arnt = null;
            mano = null;
            if (eEF1A != null) eEF1A.SetActive(false);
        }

        void AlAgarrar(SelectEnterEventArgs args)
        {
            if (args.interactorObject is XRSocketInteractor) return;
            mano = args.interactorObject;
            manoInicio = mano.GetAttachTransform(agarre).rotation;
            giroInicio = giro;
            eje = ribosoma.SitioA.up;
        }

        void AlSoltar(SelectExitEventArgs args)
        {
            if (args.interactorObject == mano) mano = null;
        }

        void Update()
        {
            if (arnt == null) return;
            if (!arnt.gameObject.activeInHierarchy) { Cancelar(); return; }

            if (mano != null)
            {
                // Cuánto giró la muñeca alrededor del eje del ARNt desde que lo agarró (descomposición swing-twist)
                var delta = mano.GetAttachTransform(agarre).rotation * Quaternion.Inverse(manoInicio);
                float torsion = Torsion(delta, eje);
                float antes = giro;
                giro = Mathf.Clamp(giroInicio + torsion, -ControlesCocina.GiroMaximoLlave, ControlesCocina.GiroMaximoLlave);

                if (Mathf.FloorToInt(antes / pasoTic) != Mathf.FloorToInt(giro / pasoTic)) Vibrar(0.15f, 0.02f);

                bool cruzo = (antes > 0f && giro <= 0f) || (antes < 0f && giro >= 0f);
                if (Mathf.Abs(giro) < ControlesCocina.ToleranciaLlave || cruzo)
                {
                    Encajar();
                    return;
                }
            }
            Colocar();
        }

        // Solo para probar en el editor sin girar la muñeca: endereza el ARNt estos grados (encaja al llegar)
        public void GirarPrueba(float grados)
        {
            if (arnt == null || ribosoma == null) return;
            float antes = giro;
            giro = Mathf.MoveTowards(giro, 0f, grados);
            if (Mathf.FloorToInt(antes / pasoTic) != Mathf.FloorToInt(giro / pasoTic)) Vibrar(0.15f, 0.02f);
            if (Mathf.Abs(giro) < ControlesCocina.ToleranciaLlave) Encajar();
            else Colocar();
        }

        void Encajar()
        {
            var t = arnt;
            Vibrar(0.6f, 0.12f);
            Cancelar();               // restaura el agarre y apaga eEF1A
            PuenteCorriente.Soltar(t);
            ribosoma.LlaveEncajo(t);  // lo deja derecho en el sitio A y llama a ControlCocina.Aceptar
        }

        void Colocar()
        {
            if (arnt == null || ribosoma == null) return;
            var sitio = ribosoma.SitioA;
            float inclinacion = maximo > 0f ? inclinacionInicial * Mathf.Abs(giro) / maximo : 0f;
            arnt.transform.SetPositionAndRotation(sitio.position,
                sitio.rotation * Quaternion.Euler(0f, 0f, -inclinacion) * Quaternion.Euler(0f, giro, 0f));
            if (eEF1A != null)
                eEF1A.transform.position = arnt.transform.position + arnt.transform.up * alturaEEF1A;
        }

        // Ángulo (grados, con signo) de la rotación q alrededor del eje
        static float Torsion(Quaternion q, Vector3 eje)
        {
            var v = new Vector3(q.x, q.y, q.z);
            float proyeccion = Vector3.Dot(v, eje.normalized);
            float angulo = 2f * Mathf.Atan2(proyeccion, q.w) * Mathf.Rad2Deg;
            if (angulo > 180f) angulo -= 360f;
            if (angulo < -180f) angulo += 360f;
            return angulo;
        }

        void Vibrar(float fuerza, float duracion)
        {
            if (mano is XRBaseInputInteractor entrada) entrada.SendHapticImpulse(fuerza, duracion);
        }
    }
}
