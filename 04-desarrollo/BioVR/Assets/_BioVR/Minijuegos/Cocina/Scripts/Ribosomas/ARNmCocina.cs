using System.Collections.Generic;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // M4 · El ARNm que pasa por el ribosoma (especificación 5.2 y 5.5).
    // Cinta de tramos de 0.125 m (uno por codón) de −2 a +3 posiciones alrededor del sitio A, y encima
    // 4 codones (del anterior al índice +2) hechos con piezas Base_* de Uli, hacia arriba. El codón actual
    // a escala 0.75 y los demás a 0.62; el de paro es un bloque rojo.
    //
    // Este objeto queda en el sitio A con su +X hacia el extremo 3′ (a la derecha del jugador): así 5′
    // queda a la izquierda. Al avanzar, la cinta se desliza un codón hacia 5′; si el ribosoma avanza más
    // rápido que la animación (avance automático cada 0.13 s), se pone al corriente sin quedarse atrás.
    public class ARNmCocina : MonoBehaviour
    {
        [Header("Piezas (prefabs que saca el armador de arnm-bases.fbx)")]
        [SerializeField] GameObject tramo;
        [SerializeField] GameObject baseA, baseU, baseG, baseC;
        [SerializeField] Material materialParo;

        [Header("Medidas")]
        [SerializeField] float paso = 0.125f;
        [Tooltip("Dónde va cada una de las 3 bases sobre un tramo (Punto_base_1..3 del FBX), relativo al centro del tramo")]
        [SerializeField] Vector3[] puntosBase = { new Vector3(-0.034f, 0.012f, 0f), new Vector3(0f, 0.012f, 0f), new Vector3(0.034f, 0.012f, 0f) };
        [SerializeField] float escalaActual = 0.75f;
        [SerializeField] float escalaOtros = 0.62f;
        [SerializeField] Vector3 tamanoParo = new Vector3(0.1f, 0.03f, 0.04f);
        [SerializeField] float duracionAvance = 0.3f;

        Transform cinta;
        IReadOnlyList<Codon> codones;
        float mostrado;     // índice que se ve (con decimales mientras se desliza)
        int objetivo;
        int dibujado = int.MinValue;

        public void Iniciar(IReadOnlyList<Codon> lista, int indice)
        {
            codones = lista;
            objetivo = indice;
            mostrado = indice;
            dibujado = int.MinValue;
            Dibujar(indice);
            Colocar();
        }

        public void IrA(int indice) => objetivo = indice;

        void Update()
        {
            if (codones == null || Mathf.Approximately(mostrado, objetivo)) return;
            // Si va atrasado varios codones, acelera para no quedarse atrás
            float velocidad = Mathf.Max(1f, Mathf.Abs(objetivo - mostrado)) / duracionAvance;
            mostrado = Mathf.MoveTowards(mostrado, objetivo, velocidad * Time.deltaTime);
            int b = Mathf.FloorToInt(mostrado + 0.0001f);
            if (b != dibujado) Dibujar(b);
            Colocar();
        }

        // La cinta se recorre hacia 5′ (−X) conforme avanza
        void Colocar()
        {
            if (cinta == null) return;
            float fraccion = mostrado - dibujado;
            cinta.localPosition = new Vector3(-fraccion * paso, 0f, 0f);
        }

        void Dibujar(int b)
        {
            dibujado = b;
            if (cinta != null) Destroy(cinta.gameObject);
            cinta = new GameObject("Cinta").transform;
            cinta.SetParent(transform, false);
            if (codones == null) return;

            // Tramos de −2 a +3 alrededor del sitio A
            for (int k = b - 2; k <= b + 3; k++)
                if (tramo != null) Poner(tramo, new Vector3((k - b) * paso, 0f, 0f), 1f);

            // 4 codones: del anterior al +2
            for (int k = b - 1; k <= b + 2; k++)
            {
                if (k < 0 || k >= codones.Count) continue;
                float x = (k - b) * paso;
                float escala = k == b ? escalaActual : escalaOtros;
                var c = codones[k];
                if (c.esParo)
                {
                    var paro = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    paro.name = $"Paro {c.tripleta}";
                    Destroy(paro.GetComponent<Collider>());
                    paro.transform.SetParent(cinta, false);
                    paro.transform.localPosition = new Vector3(x, tamanoParo.y * escala / 2f, 0f);
                    paro.transform.localScale = tamanoParo * escala;
                    if (materialParo != null) paro.GetComponent<Renderer>().sharedMaterial = materialParo;
                    continue;
                }
                for (int i = 0; i < 3 && i < c.tripleta.Length; i++)
                {
                    var pieza = Pieza(c.tripleta[i]);
                    if (pieza == null) continue;
                    var p = i < puntosBase.Length ? puntosBase[i] : new Vector3((i - 1) * 0.034f, 0f, 0f);
                    Poner(pieza, new Vector3(x + p.x * escala, p.y, p.z * escala), escala);
                }
            }
        }

        // Para la tarjeta del codón (TarjetaCodon): las mismas piezas que la cinta
        public GameObject PiezaDe(char letra) => Pieza(letra);

        GameObject Pieza(char letra) => letra switch
        {
            'A' => baseA,
            'U' => baseU,
            'G' => baseG,
            'C' => baseC,
            _ => null
        };

        void Poner(GameObject plantilla, Vector3 posicion, float escala)
        {
            var go = Instantiate(plantilla, cinta);
            go.transform.localPosition = posicion;
            go.transform.localScale = plantilla.transform.localScale * escala;
            foreach (var col in go.GetComponentsInChildren<Collider>()) Destroy(col);
        }
    }
}
