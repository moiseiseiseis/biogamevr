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
            PonerManivelaDeFrente();
        }

        // Prueba del 24-nov: "la manivela sería mejor ponerla de frente a nosotros". Estaba en el costado izquierdo
        // de la plancha con el plano de giro de canto. Se cuelga de un soporte nuevo que la gira para que su eje
        // (con la perilla) apunte al jugador y la pone frente a la cara de enfrente de la plancha, en su orilla
        // izquierda (donde está el letrero "Gira la manivela"), con el eje a la altura de la superficie: el brazo gira
        // en el aire frente a esa cara, sin tapar las ranuras. A la derecha quedaba pegada al ribosoma de mesa y
        // encima de la plancha, a la altura de los ojos (capturas del 24-nov). Manivela calcula el giro en el espacio
        // de su padre, así que el soporte no le cambia nada.
        const float Holgura = 0.03f; // m entre la cara de la plancha y la manivela

        void PonerManivelaDeFrente()
        {
            var m = GetComponentInChildren<Manivela>(true);
            if (m == null || m.transform.parent == null) return;

            // La plancha (malla base-sintetasa), medida en el espacio de la estación con las esquinas de su malla
            // (la caja de Renderer.bounds sale inflada porque la estación está girada)
            MeshFilter plancha = null;
            foreach (var mf in GetComponentsInChildren<MeshFilter>(true))
                if (mf.name.StartsWith("base-sintetasa") && mf.sharedMesh != null) { plancha = mf; break; }
            if (plancha == null) return;
            var caja = CajaEn(transform, plancha.transform, plancha.sharedMesh.bounds);

            // Hacia dónde está el jugador: el origen de AnclaCocina es donde se para (PosicionMesa); si no, la cámara
            var ancla = GameObject.Find("AnclaCocina");
            Vector3 jugador = ancla != null ? ancla.transform.position : (Camera.main != null ? Camera.main.transform.position : transform.position - transform.forward);
            var haciaJugador = transform.InverseTransformPoint(jugador) - caja.center;
            haciaJugador.y = 0f;
            // La cara de la plancha que da al jugador (eje dominante) y su derecha vista por él
            Vector3 frente = Mathf.Abs(haciaJugador.x) > Mathf.Abs(haciaJugador.z)
                ? new Vector3(Mathf.Sign(haciaJugador.x), 0f, 0f) : new Vector3(0f, 0f, Mathf.Sign(haciaJugador.z));
            var derecha = Vector3.Cross(Vector3.up, -frente); // el jugador mira hacia −frente

            Transform perilla = null;
            foreach (var t in m.GetComponentsInChildren<Transform>(true)) if (t.name == "Manivela_perilla") { perilla = t; break; }

            var soporte = new GameObject("Soporte manivela (de frente)").transform;
            soporte.SetParent(m.transform.parent, false);
            soporte.localPosition = m.transform.localPosition;
            soporte.localRotation = Quaternion.identity;
            soporte.localScale = Vector3.one;
            m.transform.SetParent(soporte, true);

            // Eje de giro: X del padre, del lado donde sale la perilla. Se gira (sobre la vertical) hasta el frente.
            var eje = transform.InverseTransformDirection(soporte.right);
            if (perilla != null && Vector3.Dot(transform.InverseTransformDirection(perilla.position - m.transform.position), eje) < 0f) eje = -eje;
            eje.y = 0f;
            float giro = Vector3.SignedAngle(eje, frente, Vector3.up);
            soporte.rotation = transform.rotation * Quaternion.Euler(0f, giro, 0f) * Quaternion.Inverse(transform.rotation) * soporte.rotation;

            // Lo que mide el brazo desde el eje (radio del círculo que barre), en el espacio de la estación
            var ejeLocal = transform.InverseTransformDirection(m.transform.right);
            var pivote = transform.InverseTransformPoint(m.transform.position);
            float radio = 0f;
            foreach (var mf in m.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null) continue;
                var bb = mf.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    var esquina = transform.InverseTransformPoint(mf.transform.TransformPoint(Esquina(bb, i)));
                    radio = Mathf.Max(radio, Vector3.ProjectOnPlane(esquina - pivote, ejeLocal).magnitude);
                }
            }

            float mitad = Mathf.Abs(derecha.x) * caja.extents.x + Mathf.Abs(derecha.z) * caja.extents.z;
            float fondo = Mathf.Abs(frente.x) * caja.extents.x + Mathf.Abs(frente.z) * caja.extents.z;
            var destino = caja.center + frente * (fondo + Holgura) - derecha * Mathf.Max(0f, mitad - radio - Holgura);
            destino.y = caja.max.y;
            soporte.position += transform.TransformPoint(destino) - m.transform.position;
            m.RecordarPosicion();
        }

        static Vector3 Esquina(Bounds b, int i) =>
            new Vector3((i & 1) == 0 ? b.min.x : b.max.x, (i & 2) == 0 ? b.min.y : b.max.y, (i & 4) == 0 ? b.min.z : b.max.z);

        static Bounds CajaEn(Transform espacio, Transform t, Bounds local)
        {
            var caja = new Bounds(espacio.InverseTransformPoint(t.TransformPoint(Esquina(local, 0))), Vector3.zero);
            for (int i = 1; i < 8; i++) caja.Encapsulate(espacio.InverseTransformPoint(t.TransformPoint(Esquina(local, i))));
            return caja;
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


