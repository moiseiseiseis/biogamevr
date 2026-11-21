using BioVR.Secuencia;
using UnityEngine;

namespace BioVR.Comun
{
    // ASTRA, la IA de navegación (astra-ia-de-viaje de Diego), como holograma pequeño en la consola de la sala de enlace.
    // Flota y gira despacio; cuando "habla" (cambia un paso del tutorial, empieza un salto, cambia el estado o se
    // registra una ficha del pokédex) brilla y crece un poco durante unos segundos. Sus materiales emisivos de Diseño
    // (ASTRA_Emisivo_*) se suben de intensidad al hablar.
    public class AstraHolograma : MonoBehaviour
    {
        [SerializeField] Transform modelo;
        [SerializeField] float flotar = 0.015f;       // metros arriba y abajo
        [SerializeField] float giro = 12f;            // grados por segundo
        [SerializeField] float duracionHablar = 2.5f;
        [SerializeField] float brilloHablando = 2.5f;

        Vector3 posicionBase, escalaBase;
        float hablando;
        Material[] emisivos = new Material[0];
        Color[] emisionBase = new Color[0];
        GameManager gm;

        void Start()
        {
            if (modelo == null) modelo = transform;
            posicionBase = modelo.localPosition;
            escalaBase = modelo.localScale;

            // Copias de los materiales emisivos para poder subirles el brillo sin tocar los del FBX
            var lista = new System.Collections.Generic.List<Material>();
            foreach (var r in modelo.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.materials; // instancia
                foreach (var m in mats)
                    if (m != null && m.name.Contains("Emisivo") && m.HasProperty("_EmissionColor"))
                    {
                        m.EnableKeyword("_EMISSION");
                        lista.Add(m);
                    }
            }
            emisivos = lista.ToArray();
            emisionBase = new Color[emisivos.Length];
            for (int i = 0; i < emisivos.Length; i++)
            {
                var c = emisivos[i].GetColor("_EmissionColor");
                if (c.maxColorComponent < 0.05f && emisivos[i].HasProperty("_BaseColor")) c = emisivos[i].GetColor("_BaseColor");
                emisionBase[i] = c;
            }

            gm = GameManager.Instance;
            if (gm != null)
            {
                gm.OnEstadoCambiado += AlEstado;
                gm.OnSaltoIniciado += AlSalto;
            }
            Tutorial.OnPasoCambiado += AlPaso;
            BioVR.Pokedex.Pokedex.OnRegistrada += AlRegistrar;
        }

        void OnDestroy()
        {
            if (gm != null)
            {
                gm.OnEstadoCambiado -= AlEstado;
                gm.OnSaltoIniciado -= AlSalto;
            }
            Tutorial.OnPasoCambiado -= AlPaso;
            BioVR.Pokedex.Pokedex.OnRegistrada -= AlRegistrar;
        }

        void AlEstado(GameState _) => Hablar();
        void AlSalto(int _) => Hablar();
        void AlPaso(PasoTutorial p) { if (p != null) Hablar(); }
        void AlRegistrar(BioVR.Pokedex.FichaPokedex _, bool nueva) { if (nueva) Hablar(); }

        public void Hablar() => hablando = duracionHablar;

        void Update()
        {
            hablando = Mathf.Max(0f, hablando - Time.deltaTime);
            float k = hablando > 0f ? 0.5f + 0.5f * Mathf.Sin(Time.time * 10f) : 0f; // pulso mientras habla
            modelo.localPosition = posicionBase + Vector3.up * Mathf.Sin(Time.time * 1.6f) * flotar;
            modelo.Rotate(Vector3.up, giro * Time.deltaTime, Space.World);
            modelo.localScale = escalaBase * (1f + 0.06f * k);
            for (int i = 0; i < emisivos.Length; i++)
                emisivos[i].SetColor("_EmissionColor", emisionBase[i] * (1f + (brilloHablando - 1f) * k));
        }
    }
}
