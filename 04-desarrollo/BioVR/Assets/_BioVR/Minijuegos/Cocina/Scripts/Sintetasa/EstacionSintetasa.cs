using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    public class EstacionSintetasa : MonoBehaviour
    {
        [Header("Ranuras (Punto_ARNt_1 y Punto_ARNt_2)")]
        [SerializeField] Transform[] puntosRanura = new Transform[2];

        [Header("Barras de progreso (Barra_carga_1 y Barra_carga_2)")]
        [SerializeField] Transform[] barrasCarga = new Transform[2];

        class Ranura
        {
            public ARNtCocina arnt;
            public float progreso;
            public int ultimoOctavo;
        }

        [Header("Ajustes de la prueba del 23-nov")]
        [Tooltip("Distancia (m) a la estación o a una ranura a la que se acepta un ARNt soltado")]
        [SerializeField] float radioSoltar = 0.35f;
        [Tooltip("Cuánto arriba de la ranura queda el ARNt (m), para que se vea fuera de la enzima")]
        [SerializeField] float alturaSobreRanura = 0.07f;

        readonly Ranura[] ranuras = new Ranura[] { new Ranura(), new Ranura() };
        CorrienteARNt corriente;

        public bool TieneARNtVacio
        {
            get
            {
                for (int i = 0; i < ranuras.Length; i++)
                    if (ranuras[i].arnt != null && !ranuras[i].arnt.Cargado)
                        return true;
                return false;
            }
        }

        void Awake()
        {
            corriente = FindFirstObjectByType<CorrienteARNt>();
            for (int i = 0; i < barrasCarga.Length; i++)
                ActualizarBarraVisual(i, 0f);
            // Escucha desde Awake (no OnEnable): la estación se apaga al terminar el turno y tiene que oír el
            // siguiente para volver a encenderse. Antes se quedaba apagada y el turno 2 no se podía jugar
            // (prueba del 23-nov: "al reiniciar el turno desaparece la mesa de la sintetasa").
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
            EventosCocina.OnFaseCambiada += AlCambiarFase;
            QuitarManivelaDecorativa();
            ResaltarManivela();
        }

        // La manivela que funciona medía unos 4 cm y era color olivo: casi no se veía. Se agranda y se pinta
        // del ámbar que tenía la de adorno.
        void ResaltarManivela()
        {
            var m = GetComponentInChildren<Manivela>(true);
            if (m == null) return;
            m.transform.localScale *= 1.6f;
            // La zona de agarre (esfera de 10 cm a escala del modelo) creció con la manivela y alcanzaba las ranuras:
            // el mando se "anclaba" a la manivela al querer agarrar un ARNt de la sintetasa (prueba del 23-nov).
            // Se deja una esfera de 5 cm centrada en la perilla, del lado de afuera.
            var esfera = m.GetComponent<SphereCollider>();
            Transform perilla = null;
            foreach (var t in m.GetComponentsInChildren<Transform>(true)) if (t.name == "Manivela_perilla") { perilla = t; break; }
            if (esfera != null)
            {
                var rp = perilla != null ? perilla.GetComponent<Renderer>() : null;
                var centro = rp != null ? rp.bounds.center : m.transform.position;
                esfera.center = m.transform.InverseTransformPoint(centro);
                esfera.radius = 0.05f / Mathf.Max(0.0001f, m.transform.lossyScale.x);
            }
            var bloque = new MaterialPropertyBlock();
            foreach (var r in m.GetComponentsInChildren<Renderer>(true))
            {
                r.GetPropertyBlock(bloque);
                bloque.SetColor("_BaseColor", new Color(1f, 0.8f, 0.2f));
                bloque.SetColor("_EmissionColor", new Color(0.6f, 0.4f, 0.05f));
                r.SetPropertyBlock(bloque);
            }
        }

        // base-sintetasa.fbx (Uli) trae una manivela de adorno (aro ámbar y brazos oscuros) a la izquierda que no hace
        // nada; la que funciona es la de estacion-sintetasa-1qtq (componente Manivela). En la prueba del 23-nov había
        // "dos manivelas" y se decidió dejar solo la que funciona: aquí se quitan esas dos partes de la malla de la base.
        static readonly string[] MaterialesManivelaDecorativa = { "metal_oscuro", "neon_ambar" };

        void QuitarManivelaDecorativa()
        {
            foreach (var mf in GetComponentsInChildren<MeshFilter>(true))
            {
                if (!mf.name.StartsWith("base-sintetasa") || mf.sharedMesh == null) continue;
                var r = mf.GetComponent<MeshRenderer>();
                if (r == null) continue;
                var original = mf.sharedMesh;
                var mats = r.sharedMaterials;
                var quedan = new System.Collections.Generic.List<int>();
                for (int i = 0; i < original.subMeshCount; i++)
                {
                    string nombre = i < mats.Length && mats[i] != null ? mats[i].name : "";
                    if (System.Array.IndexOf(MaterialesManivelaDecorativa, nombre) < 0) quedan.Add(i);
                }
                if (quedan.Count == original.subMeshCount || quedan.Count == 0) continue;

                // La cruceta de la manivela de adorno está en la malla principal: se quitan también los triángulos
                // que caen dentro del aro (su caja, un poco más gruesa hacia los lados)
                var caja = new Bounds();
                bool hayCaja = false;
                for (int i = 0; i < original.subMeshCount; i++)
                    if (!quedan.Contains(i))
                    {
                        var b = original.GetSubMesh(i).bounds;
                        if (!hayCaja) { caja = b; hayCaja = true; } else caja.Encapsulate(b);
                    }
                caja.Expand(new Vector3(0.02f, 0f, 0f));
                var v = original.vertices;

                var malla = Instantiate(original);
                malla.name = original.name + " (sin manivela de adorno)";
                malla.subMeshCount = quedan.Count;
                var nuevos = new Material[quedan.Count];
                for (int k = 0; k < quedan.Count; k++)
                {
                    var tri = original.GetTriangles(quedan[k]);
                    var filtrados = new System.Collections.Generic.List<int>(tri.Length);
                    for (int t = 0; t + 2 < tri.Length; t += 3)
                    {
                        var centro = (v[tri[t]] + v[tri[t + 1]] + v[tri[t + 2]]) / 3f;
                        // Solo en el cuerpo morado (ahí está la cruceta); el neón del borde no se toca
                        bool cuerpo = mats[quedan[k]] != null && mats[quedan[k]].name == "metal_morado";
                        if (cuerpo && hayCaja && caja.Contains(centro)) continue;
                        filtrados.Add(tri[t]); filtrados.Add(tri[t + 1]); filtrados.Add(tri[t + 2]);
                    }
                    malla.SetTriangles(filtrados, k);
                    nuevos[k] = mats[quedan[k]];
                }
                mf.sharedMesh = malla;
                r.sharedMaterials = nuevos;
            }
        }

        void OnDestroy()
        {
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
            EventosCocina.OnFaseCambiada -= AlCambiarFase;
        }

        void AlIniciarTurno(TurnoData turno)
        {
            for (int i = 0; i < ranuras.Length; i++)
            {
                ranuras[i].arnt = null;
                ranuras[i].progreso = 0f;
                ranuras[i].ultimoOctavo = 0;
                ActualizarBarraVisual(i, 0f);
            }
            gameObject.SetActive(true);
        }

        void AlCambiarFase(FaseCocina fase)
        {
            if (fase == FaseCocina.Plegando || fase == FaseCocina.Fin)
                gameObject.SetActive(false);
        }

        void Update()
        {
            for (int i = 0; i < ranuras.Length; i++)
            {
                var r = ranuras[i];
                if (r.arnt == null) continue;

                // Si el jugador agarró y sacó el ARNt de la ranura, liberarla
                if (r.arnt.Lugar != LugarARNt.Sintetasa)
                {
                    r.arnt = null;
                    r.progreso = 0f;
                    r.ultimoOctavo = 0;
                    ActualizarBarraVisual(i, 0f);
                    continue;
                }

                if (i < puntosRanura.Length && puntosRanura[i] != null)
                {
                    // Un poco arriba de la ranura: en el punto exacto el ARNt quedaba metido en la enzima y no se veía
                    r.arnt.transform.SetPositionAndRotation(puntosRanura[i].position + Vector3.up * alturaSobreRanura, puntosRanura[i].rotation);
                }
            }
        }

        // Llamado desde CorrienteARNt.AlSoltar cuando el jugador suelta un ARNt
        public bool IntentarColocar(ARNtCocina arnt)
        {
            if (arnt == null) return false;
            if (arnt.Usado) return false; // el usado va al embudo; si se suelta aquí, regresa a la bandeja

            // Cerca de la estación o de cualquiera de sus ranuras, con margen amplio: en el visor costaba atinarle
            float dist = Vector3.Distance(arnt.transform.position, transform.position);
            foreach (var p in puntosRanura)
                if (p != null) dist = Mathf.Min(dist, Vector3.Distance(arnt.transform.position, p.position));
            if (dist > radioSoltar) return false;

            var control = ControlCocina.Instancia;
            if (control == null) return false;

            // 1. Verificar si hay ranura libre antes de llamar a M1
            int libre = -1;
            for (int i = 0; i < ranuras.Length; i++)
            {
                if (ranuras[i].arnt == null)
                {
                    libre = i;
                    break;
                }
            }

            if (libre == -1)
            {
                EventosCocina.Rechazo("sint_llena");
                EventosCocina.Mensaje("sint_llena");
                control.Sonar("error");
                DevolverACorriente(arnt);
                return true;
            }

            // 2. M1 decide si puede entrar (revisa fase, si ya está cargado o si trae error)
            if (!control.PuedeEntrarASintetasa(arnt, out _))
            {
                DevolverACorriente(arnt);
                return true;
            }

            // 3. Entra en la primera ranura libre con progreso 0
            ranuras[libre].arnt = arnt;
            ranuras[libre].progreso = 0f;
            ranuras[libre].ultimoOctavo = 0;
            ActualizarBarraVisual(libre, 0f);
            return true;
        }

        public void AvanzarProgreso(float deltaProgreso)
        {
            var control = ControlCocina.Instancia;
            if (control == null || control.Pausado) return;

            for (int i = 0; i < ranuras.Length; i++)
            {
                var r = ranuras[i];
                if (r.arnt == null || r.arnt.Cargado) continue;

                r.progreso = Mathf.Clamp01(r.progreso + deltaProgreso);
                ActualizarBarraVisual(i, r.progreso);

                int octavo = Mathf.FloorToInt(r.progreso * 8f);
                if (octavo > r.ultimoOctavo && r.progreso < 1f)
                {
                    r.ultimoOctavo = octavo;
                    control.Sonar("tic");
                }

                if (r.progreso >= 1f)
                {
                    // M1 calcula el errorSintetasa (una vez por ARNt), asigna carga y lanza OnARNtCargado
                    control.TerminarCarga(r.arnt);
                }
            }
        }

        void ActualizarBarraVisual(int indice, float valor)
        {
            if (indice < 0 || indice >= barrasCarga.Length || barrasCarga[indice] == null) return;
            Transform barra = barrasCarga[indice];
            float v = Mathf.Clamp01(valor);
            barra.gameObject.SetActive(v > 0.001f);
            barra.localScale = new Vector3(Mathf.Lerp(0.001f, 0.11f, v), 0.018f, 0.015f);
        }
        void DevolverACorriente(ARNtCocina arnt)
        {
            if (corriente == null) corriente = FindFirstObjectByType<CorrienteARNt>();
            if (corriente != null) corriente.DevolverAlTubo(arnt);
            else if (ControlCocina.Instancia != null) ControlCocina.Instancia.DevolverACorriente(arnt);
        }


    }
}


