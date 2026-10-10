using UnityEngine;
using UnityEngine.Events;

public class DetectorPalanca : MonoBehaviour
{
    private HingeJoint bisagra;
    public UnityEvent alLlegarAlTope;
    private bool eventoDisparado = false;

    void Start()
    {
        bisagra = GetComponent<HingeJoint>();
    }

    void Update()
    {
        // Comprueba si la palanca está a menos de 2 grados de cualquiera de sus límites
        if (Mathf.Abs(bisagra.angle - bisagra.limits.max) < 2f || Mathf.Abs(bisagra.angle - bisagra.limits.min) < 2f)
        {
            if (!eventoDisparado)
            {
                alLlegarAlTope.Invoke();
                eventoDisparado = true;
                Debug.Log("¡La palanca llegó al tope!");
            }
        }
        else
        {
            // Se resetea para que pueda volver a dispararse si la regresan al centro
            eventoDisparado = false;
        }
    }
}