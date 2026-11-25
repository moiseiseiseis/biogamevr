using TMPro;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // Tarjeta bajo el ribosoma con el ejemplo del codón que se pide (prueba del 23-nov: "no se identifica el codón;
    // poner una imagen de ejemplo para saber qué anticodón usar"; en el HTML el letrero va debajo del ribosoma).
    //
    //      ARNt que encaja    U   A   C      ← piezas del anticodón, volteadas, con su letra
    //                         ▼   ▼   ▼
    //      codón del ARNm     A   U   G      ← piezas del codón, con su letra
    //
    // Usa las mismas piezas que el ARNm (ARNmCocina) y los mismos colores, para que el jugador las reconozca en la
    // corriente. La crea RibosomaCocina en tiempo de juego, en el lugar de la placa "codón en el sitio A".
    public class TarjetaCodon : MonoBehaviour
    {
        const float Separacion = 0.055f;
        const float EscalaPieza = 1.5f;

        ARNmCocina arnm;
        TMP_FontAsset fuente;
        Transform piezas;
        TextMeshPro titulo, etiquetaArriba, etiquetaAbajo, nota;
        readonly TextMeshPro[] letrasArriba = new TextMeshPro[3];
        readonly TextMeshPro[] letrasAbajo = new TextMeshPro[3];
        string mostrado;

        public static TarjetaCodon Crear(Transform lugar, ARNmCocina arnm, TMP_FontAsset fuente, float escala)
        {
            var go = new GameObject("Tarjeta del codón");
            go.transform.SetParent(lugar.parent, false);
            go.transform.SetPositionAndRotation(lugar.position, lugar.rotation);
            go.transform.localScale = Vector3.one * escala;
            var t = go.AddComponent<TarjetaCodon>();
            t.arnm = arnm;
            t.fuente = fuente;
            t.Armar();
            return t;
        }

        void Armar()
        {
            // Fondo oscuro detrás (TextMeshPro se lee mirando hacia su +Z: "detrás" es +Z)
            var fondo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fondo.name = "Fondo";
            Destroy(fondo.GetComponent<Collider>());
            fondo.transform.SetParent(transform, false);
            fondo.transform.localPosition = new Vector3(0f, 0f, 0.03f);
            fondo.transform.localScale = new Vector3(0.40f, 0.27f, 1f);
            var r = fondo.GetComponent<Renderer>();
            var sh = Shader.Find("Universal Render Pipeline/Unlit");
            if (sh != null) { r.material = new Material(sh); r.material.SetColor("_BaseColor", new Color(0.05f, 0.07f, 0.16f)); }

            titulo = Texto("Titulo", new Vector3(0f, 0.105f, 0f), 0.24f, Color.white);
            etiquetaArriba = Texto("ARNt", new Vector3(-0.145f, 0.045f, 0f), 0.15f, new Color(0.75f, 0.9f, 1f));
            etiquetaAbajo = Texto("ARNm", new Vector3(-0.145f, -0.04f, 0f), 0.15f, new Color(0.75f, 0.9f, 1f));
            etiquetaArriba.text = "ARNt que\nencaja";
            etiquetaAbajo.text = "codón del\nARNm";
            nota = Texto("Nota", new Vector3(0f, -0.11f, -0.005f), 0.17f, new Color(1f, 0.85f, 0.4f));
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * Separacion + 0.03f;
                letrasArriba[i] = Texto($"Anticodon {i}", new Vector3(x, 0.068f, -0.01f), 0.3f, Color.white);
                letrasAbajo[i] = Texto($"Codon {i}", new Vector3(x, -0.078f, -0.01f), 0.3f, Color.white);
            }
        }

        TextMeshPro Texto(string nombre, Vector3 pos, float tamano, Color color)
        {
            var go = new GameObject(nombre);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;
            var t = go.AddComponent<TextMeshPro>();
            if (fuente != null) t.font = fuente;
            t.fontSize = tamano;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.rectTransform.sizeDelta = new Vector2(0.35f, 0.05f);
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.fontStyle = FontStyles.Bold;
            return t;
        }

        // pide: el codón es del jugador (si no, el ribosoma avanza solo)
        public void Mostrar(Codon? codon, bool pide, bool enParo)
        {
            string clave = codon.HasValue ? codon.Value.tripleta + pide + enParo : "-";
            if (clave == mostrado) return;
            mostrado = clave;

            if (piezas != null) Destroy(piezas.gameObject);
            piezas = new GameObject("Piezas").transform;
            piezas.SetParent(transform, false);

            bool hay = codon.HasValue && !codon.Value.esParo;
            string tripleta = codon.HasValue ? codon.Value.tripleta : "";
            string anti = hay ? Codon.Complemento(tripleta) : "";

            // El aviso importante va en el título (la línea de abajo quedaba fuera de la vista)
            titulo.text = !codon.HasValue ? "" : enParo || codon.Value.esParo ? $"<color=#FF7070>{tripleta} = PARO · toca FIN</color>"
                        : pide ? "El ribosoma pide:" : "Avanza solo…";
            nota.text = "";
            etiquetaArriba.gameObject.SetActive(hay);
            etiquetaAbajo.gameObject.SetActive(codon.HasValue);

            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * Separacion + 0.03f;
                char cb = i < tripleta.Length ? tripleta[i] : ' ';
                char ca = i < anti.Length ? anti[i] : ' ';
                Letra(letrasAbajo[i], cb);
                Letra(letrasArriba[i], hay ? ca : ' ');
                if (codon.HasValue && !codon.Value.esParo) Pieza(cb, new Vector3(x, -0.035f, 0f), false);
                if (hay) Pieza(ca, new Vector3(x, 0.03f, 0f), true);
            }
        }

        void Letra(TextMeshPro t, char c)
        {
            t.text = c == ' ' ? "" : c.ToString();
            t.color = ColorDe(c);
        }

        // Pieza de ARNm (hacia arriba) o de ARNt (volteada hacia abajo, como en el ARNt)
        void Pieza(char letra, Vector3 pos, bool volteada)
        {
            var plantilla = arnm != null ? arnm.PiezaDe(letra) : null;
            if (plantilla == null) return;
            var go = Instantiate(plantilla, piezas);
            go.transform.localPosition = pos;
            // El ARNm se ve desde su +Z; esta tarjeta, desde su −Z: se gira 180° para enseñar la misma cara
            go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f) * (volteada ? Quaternion.Euler(0f, 0f, 180f) : Quaternion.identity);
            go.transform.localScale = plantilla.transform.localScale * EscalaPieza;
            foreach (var col in go.GetComponentsInChildren<Collider>()) Destroy(col);
        }

        Color ColorDe(char letra)
        {
            var plantilla = arnm != null ? arnm.PiezaDe(letra) : null;
            var r = plantilla != null ? plantilla.GetComponentInChildren<Renderer>() : null;
            if (r != null && r.sharedMaterial != null)
            {
                var m = r.sharedMaterial;
                var c = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : m.color;
                c.a = 1f;
                return Color.Lerp(c, Color.white, 0.25f); // un poco más claro para leerse sobre el fondo oscuro
            }
            return Color.white;
        }
    }
}
