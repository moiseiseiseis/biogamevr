using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BioVR.Minijuegos.ExpresionGenetica
{
    // Cinta de ARNm provisional: un bloque por codón con su texto, de 5' (izquierda) a 3' (derecha).
    // El sitio de lectura del ribosoma queda fijo; lo que se corre es la cinta, un codón por paso.
    // (Es el mismo movimiento relativo que el ribosoma avanzando sobre el ARNm, y así el sitio no se mueve.)
    // Se arma en el juego desde una SecuenciaARNm, porque la sana y la del paciente tienen codones distintos.
    // Cuando esté el ARNm de Ascencio (S2) se puede cambiar el aspecto sin cambiar la lógica.
    public class TiraARNm : MonoBehaviour
    {
        [SerializeField] Material materialCodon;
        [SerializeField] Material materialCinta;
        [SerializeField] Vector3 tamanoCodon = new Vector3(0.09f, 0.04f, 0.04f);
        [SerializeField] float espaciado = 0.1f;
        [SerializeField] float tamanoTexto = 0.22f;
        [SerializeField] Color colorTexto = new Color(0.08f, 0.1f, 0.15f);
        [SerializeField] float duracionAvance = 0.6f;

        Transform contenedor;
        readonly List<Senalable> codones = new List<Senalable>();
        readonly List<Renderer> bloques = new List<Renderer>();
        readonly List<Material> materiales = new List<Material>();

        public Senalable Cinta { get; private set; }

        void OnDestroy()
        {
            foreach (var m in materiales) Destroy(m);
        }
        public int Activo { get; private set; }

        public Senalable Codon(int i) => codones[i];
        public Renderer Bloque(int i) => bloques[i];

        public void Construir(SecuenciaARNm secuencia)
        {
            if (contenedor != null) Destroy(contenedor.gameObject);
            foreach (var m in materiales) Destroy(m);
            materiales.Clear();
            codones.Clear();
            bloques.Clear();

            contenedor = new GameObject("Cinta " + secuencia.nombre).transform;
            contenedor.SetParent(transform, false);

            // La cinta completa (para el paso 1: "señala el ARNm"), detrás de los codones
            float largo = espaciado * (secuencia.Largo + 1);
            var cinta = Bloque("ARNm", contenedor, new Vector3(espaciado * (secuencia.Largo - 1) / 2f, 0f, tamanoCodon.z * 0.75f),
                new Vector3(largo, tamanoCodon.y * 0.5f, tamanoCodon.z * 0.5f), materialCinta);
            Cinta = cinta.gameObject.AddComponent<Senalable>();
            Cinta.Habilitado = false;

            for (int i = 0; i < secuencia.Largo; i++)
            {
                var c = secuencia[i];
                var r = Bloque("Codon " + c.codon, contenedor, new Vector3(espaciado * i, 0f, 0f), tamanoCodon, materialCodon);
                // Material propio por codón (no un PropertyBlock), para que el Parpadeo respete su color
                var mat = new Material(r.sharedMaterial) { name = "Codon " + c.codon };
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c.color);
                r.sharedMaterial = mat;
                materiales.Add(mat);
                bloques.Add(r);

                var texto = Texto(r.transform.parent, c.codon, new Vector3(espaciado * i, 0f, -tamanoCodon.z / 2f - 0.002f));
                texto.name = "Texto " + c.codon;

                var s = r.gameObject.AddComponent<Senalable>();
                s.Habilitado = false;
                codones.Add(s);
            }

            // Extremos 5' y 3'
            Texto(contenedor, "5'", new Vector3(-espaciado * 0.8f, 0f, -tamanoCodon.z / 2f - 0.002f));
            Texto(contenedor, "3'", new Vector3(espaciado * (secuencia.Largo - 0.2f), 0f, -tamanoCodon.z / 2f - 0.002f));

            IrAInstantaneo(0);
        }

        // Deja el codón i en el sitio de lectura
        public IEnumerator IrA(int i)
        {
            Activo = i;
            var desde = contenedor.localPosition;
            var hasta = new Vector3(-espaciado * i, 0f, 0f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / duracionAvance)
            {
                contenedor.localPosition = Vector3.Lerp(desde, hasta, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            contenedor.localPosition = hasta;
        }

        public void IrAInstantaneo(int i)
        {
            Activo = i;
            contenedor.localPosition = new Vector3(-espaciado * i, 0f, 0f);
        }

        Renderer Bloque(string nombre, Transform padre, Vector3 posicion, Vector3 escala, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = nombre;
            go.transform.SetParent(padre, false);
            go.transform.localPosition = posicion;
            go.transform.localScale = escala;
            var r = go.GetComponent<Renderer>();
            if (material != null) r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return r;
        }

        TextMeshPro Texto(Transform padre, string texto, Vector3 posicion)
        {
            var go = new GameObject("Texto " + texto);
            go.transform.SetParent(padre, false);
            go.transform.localPosition = posicion;
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = texto;
            tmp.fontSize = tamanoTexto;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = colorTexto;
            tmp.rectTransform.sizeDelta = new Vector2(espaciado, tamanoCodon.y);
            return tmp;
        }
    }
}
