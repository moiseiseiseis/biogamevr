using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // M10 · Proteasoma 26S (especificación 9.2, punto 3). Al recibir OnProteasoma:
    //   1. A la cadena del holograma (solo la original: los depósitos son de DepositosAmiloides y se quedan)
    //      se le pega la marca de ubiquitina: 4 esferas verde lima en cadena, en la primera cuenta.
    //   2. Las cuentas viajan una por una a la entrada del proteasoma (0.03 s entre cada una), empezando
    //      por la marcada, y al llegar se encogen a 0.9 por segundo hasta desaparecer.
    // Los puntos, el sonido "tritura", el mensaje y Terminar() los pone M1 (ControlCocina.UsarProteasoma).
    public class Proteasoma26S : MonoBehaviour
    {
        [Tooltip("Marcador Proteasoma_Entrada_1 del modelo de Diego (arriba)")]
        [SerializeField] Transform entrada;
        [SerializeField] Material materialUbiquitina;

        [Header("Ubiquitina")]
        [SerializeField] int esferasUbiquitina = 4;
        [SerializeField] float diametroUbiquitina = 0.012f;
        [Tooltip("Cuánto se ve la marca antes de que la cadena empiece a viajar (s)")]
        [SerializeField] float esperaMarca = 0.35f;

        [Header("Viaje y degradación")]
        [SerializeField] float retrasoEntreCuentas = 0.03f;
        [SerializeField] float duracionViaje = 0.5f;
        [Tooltip("Escala que pierde cada cuenta por segundo al entrar (1 = desaparece en 1 s)")]
        [SerializeField] float velocidadEncoger = 0.9f;

        GameObject cadenaEnViaje;

        void OnEnable()
        {
            EventosCocina.OnProteasoma += AlUsarProteasoma;
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
        }

        void OnDisable()
        {
            EventosCocina.OnProteasoma -= AlUsarProteasoma;
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
        }

        void AlIniciarTurno(TurnoData _)
        {
            StopAllCoroutines();
            if (cadenaEnViaje != null) Destroy(cadenaEnViaje);
        }

        void AlUsarProteasoma()
        {
            var holograma = FindFirstObjectByType<HologramaCocina>();
            if (holograma == null || entrada == null)
            {
                Debug.LogWarning("[Cocina] Proteasoma26S: falta el holograma en la escena o el marcador de entrada.", this);
                return;
            }
            var cuentas = holograma.EntregarCuentas();
            if (cuentas.Count == 0) return;
            StartCoroutine(Degradar(cuentas));
        }

        IEnumerator Degradar(List<CuentaHolograma> cuentas)
        {
            // Las cuentas dejan el contenedor del holograma (que gira) y se quedan donde están
            cadenaEnViaje = new GameObject("Cadena hacia el proteasoma");
            var piezas = new List<Transform>(cuentas.Count);
            var inicio = new List<Vector3>(cuentas.Count);
            var escala = new List<Vector3>(cuentas.Count);
            foreach (var c in cuentas)
            {
                c.PonerInteractuable(false);
                c.enabled = false; // la cuenta 20 (encontrada) fija su escala en cada frame y no dejaría encogerla
                c.transform.SetParent(cadenaEnViaje.transform, true);
                piezas.Add(c.transform);
                inicio.Add(c.transform.position);
                escala.Add(c.transform.localScale);
            }
            MarcarConUbiquitina(piezas[0], piezas.Count > 1 ? piezas[1] : null);

            float t = -esperaMarca;
            int quedan = piezas.Count;
            while (quedan > 0)
            {
                if (!Pausado()) t += Time.deltaTime;
                quedan = 0;
                for (int i = 0; i < piezas.Count; i++)
                {
                    if (piezas[i] == null) continue;
                    float ti = t - i * retrasoEntreCuentas;
                    if (ti > 0f)
                    {
                        float u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(ti / duracionViaje));
                        piezas[i].position = Vector3.Lerp(inicio[i], entrada.position, u);
                        float queda = 1f - Mathf.Max(0f, ti - duracionViaje) * velocidadEncoger;
                        if (queda <= 0f)
                        {
                            Destroy(piezas[i].gameObject);
                            piezas[i] = null;
                            continue;
                        }
                        piezas[i].localScale = escala[i] * queda;
                    }
                    quedan++;
                }
                yield return null;
            }
            Destroy(cadenaEnViaje);
        }

        // Cuatro esferas en línea, saliendo de la primera cuenta hacia afuera de la cadena
        void MarcarConUbiquitina(Transform primera, Transform segunda)
        {
            var afuera = segunda != null ? (primera.position - segunda.position).normalized : Vector3.up;
            if (afuera.sqrMagnitude < 1e-6f) afuera = Vector3.up;
            var marca = new GameObject("Ubiquitina").transform;
            marca.SetParent(primera, false);
            float escalaCuenta = Mathf.Max(primera.lossyScale.x, 1e-4f);
            for (int i = 0; i < esferasUbiquitina; i++)
            {
                var esfera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(esfera.GetComponent<Collider>());
                esfera.name = $"Ubiquitina {i + 1}";
                if (materialUbiquitina != null) esfera.GetComponent<Renderer>().sharedMaterial = materialUbiquitina;
                esfera.transform.SetParent(marca, false);
                esfera.transform.position = primera.position + afuera * diametroUbiquitina * (i + 1.2f);
                esfera.transform.localScale = Vector3.one * (diametroUbiquitina / escalaCuenta);
            }
        }

        static bool Pausado() => ControlCocina.Instancia != null && ControlCocina.Instancia.Pausado;
    }
}
