using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Minijuegos.ExpresionGenetica
{
    // I4 · Secuencia de traducción (Iker, rol 3). Mo7 filas 3, 4, 5 y 5b.
    //
    // Ciclo de juego (acordado con Moisés): GUIADO → PACIENTE.
    //   Guiado (ARNm sano AUG–GGC–UUU–UAA), los 6 pasos del guion M4:
    //     1. señalar el ribosoma y la cinta de ARNm · 2. señalar AUG · 3. inspeccionar los 4 ARNt
    //     4. AUG con guía (el ARNt de Met parpadea) · 5. GGC sin ayuda · 6. UUU automático (D7) y señalar el paro UAA
    //   Paciente (AUG–GGC–UAG, decisión D2): sin ayuda; el paro llega antes y la cadena sale más corta.
    //
    // Reglas de rigor que respeta (informe sec. 04, fichas G2):
    //   - Los ARNt ya traen su aminoácido; el jugador nunca lo une (ficha 4).
    //   - Un ARNt que no encaja se pone rojo y regresa (P4, lo hace RanuraSeleccion de Mo2). El ribosoma no "escoge".
    //   - El enlace lo forma el ribosoma: el aminoácido pasa solo del ARNt a la cadena (ficha 7).
    //   - En el paro ningún ARNt encaja; el jugador lo señala y el factor de liberación entra solo (D1, ficha 8).
    //
    // Teclas de depuración (solo editor y builds de desarrollo): N = completar el paso actual, R = reiniciar,
    // P = saltar al ciclo del paciente.
    // Lo arma el menú BioVR → Armar prueba Traducción (Rol 3).
    public class SecuenciaTraduccion : MonoBehaviour
    {
        public enum Fase { Preparacion, Inspeccion, Inicio, Elongacion, Terminacion, Fin }
        public enum Ciclo { Guiado, Paciente }
        public enum Modo { Guiado, Libre, Automatico }

        [Header("Datos")]
        [SerializeField] SecuenciaARNm secuenciaSana;
        [SerializeField] SecuenciaARNm secuenciaPaciente;
        [SerializeField] TextosTraduccion textos;

        [Header("Escena")]
        [SerializeField] TextMeshPro letrero;
        [SerializeField] RanuraSeleccion sitioLectura;
        [SerializeField] TiraARNm tira;
        [SerializeField] Senalable ribosoma;
        [SerializeField] Transform subunidadMayor;
        [SerializeField] Transform subunidadMenor;
        [SerializeField] CadenaPolipeptidica cadena;
        [SerializeField] Transform factorLiberacion;
        [SerializeField] ARNt[] arnts = new ARNt[0];
        [SerializeField] GameObject botonContinuar;

        [Header("Ritmo")]
        [Tooltip("Segundos que se lee un texto de confirmación antes de seguir.")]
        [SerializeField] float pausa = 2.5f;
        [Tooltip("Pista: el ARNt correcto parpadea después de N errores en un paso sin ayuda. 0 = sin pistas (M4, paso 5).")]
        [SerializeField] int pistaTrasErrores = 0;

        // ---- Eventos (para los textos y el feedback de Genaro, G4, y para el tutorial) ----
        public event Action<Ciclo, Fase> OnFaseCambiada;
        public event Action<CodonData> OnCodonActivo;
        public event Action<ARNt> OnARNtCorrecto;
        public event Action<ARNt> OnARNtIncorrecto;
        public event Action<int> OnCadenaLiberada;      // cuántos aminoácidos tiene
        public event Action<Ciclo> OnCicloTerminado;

        public Ciclo CicloActual { get; private set; }
        public Fase FaseActual { get; private set; }
        public int LargoCadenaSana { get; private set; } = 3;

        SecuenciaARNm secuencia;
        XRSocketInteractor socket;
        Pose poseFactor, poseMayor, poseMenor;
        readonly HashSet<ARNt> inspeccionados = new HashSet<ARNt>();
        ARNt acoplado, ultimoSujetado;
        int errores;
        bool forzar, continuar;
        Coroutine flujo;

        // ---------------------------------------------------------------------------------------------
        void Awake()
        {
            socket = sitioLectura != null ? sitioLectura.GetComponent<XRSocketInteractor>() : null;
            if (factorLiberacion != null) poseFactor = new Pose(factorLiberacion.position, factorLiberacion.rotation);
            if (subunidadMayor != null) poseMayor = new Pose(subunidadMayor.position, subunidadMayor.rotation);
            if (subunidadMenor != null) poseMenor = new Pose(subunidadMenor.position, subunidadMenor.rotation);
        }

        void OnEnable()
        {
            if (sitioLectura != null)
            {
                sitioLectura.OnAcierto += AlEncajar;
                sitioLectura.OnError += AlRechazar;
            }
            foreach (var a in arnts) if (a != null) a.OnSujetado += AlSujetar;
        }

        void OnDisable()
        {
            if (sitioLectura != null)
            {
                sitioLectura.OnAcierto -= AlEncajar;
                sitioLectura.OnError -= AlRechazar;
            }
            foreach (var a in arnts) if (a != null) a.OnSujetado -= AlSujetar;
        }

        void Start()
        {
            if (secuenciaSana == null || secuenciaPaciente == null || textos == null || letrero == null ||
                sitioLectura == null || socket == null || tira == null || cadena == null || arnts.Length == 0)
            {
                Debug.LogError("[Traduccion] Faltan referencias. Vuelve a correr BioVR → Armar prueba Traducción (Rol 3).", this);
                enabled = false;
                return;
            }
            Reiniciar();
        }

        // ---- Botones ----
        public void Reiniciar() => Empezar(Guiado());
        public void IrAlPaciente() => Empezar(Paciente());
        public void Continuar() => continuar = true;

        void Empezar(IEnumerator ciclo)
        {
            if (flujo != null) StopCoroutine(flujo);
            StopAllCoroutines();
            flujo = StartCoroutine(ciclo);
        }

        // =============================================================================================
        // Ciclo guiado: guion M4
        // =============================================================================================
        IEnumerator Guiado()
        {
            CicloActual = Ciclo.Guiado;
            Preparar(secuenciaSana, mostrarARNt: false);

            // Paso 1 · Ribosoma y ARNm
            CambiarFase(Fase.Preparacion);
            Escribir(textos.paso1Llegar);
            if (ribosoma != null) yield return Senalar(ribosoma);
            Escribir(textos.paso1Ribosoma);
            yield return Senalar(tira.Cinta);
            Escribir(textos.paso1Acertar);
            yield return Pausa();

            // Paso 2 · Codón AUG
            Escribir(textos.paso2Llegar);
            yield return Senalar(tira.Codon(0), parpadear: true);
            Escribir(textos.paso2Acertar);
            yield return Pausa();

            // Paso 3 · ARNt y anticodón: aparecen los 4 y se inspeccionan (sin errores posibles)
            CambiarFase(Fase.Inspeccion);
            MostrarARNt(true);
            inspeccionados.Clear();
            Escribir(textos.paso3Llegar);
            bool avisado = false;
            while (inspeccionados.Count < arnts.Length && !Consumir())
            {
                if (!avisado && inspeccionados.Count > 0) { Escribir(textos.paso3Inspeccionar); avisado = true; }
                yield return null;
            }
            Escribir(textos.paso3Terminar1);
            yield return Pausa();
            Escribir(textos.paso3Terminar2);
            yield return Pausa(0.6f);

            // Paso 4 · Inicio, con guía
            yield return Acoplar(0, Modo.Guiado, textos.paso4Llegar, textos.paso4Acertar);
            // Paso 5 · Elongación, sin ayuda
            yield return Acoplar(1, Modo.Libre, textos.paso5Llegar, textos.paso5Acertar);
            // Paso 6 · UUU automático (D7) y paro
            yield return Acoplar(2, Modo.Automatico, textos.paso6Automatico, null);
            yield return Paro(3, textos.paso6Paro);

            LargoCadenaSana = cadena.Cantidad;
            CambiarFase(Fase.Fin);
            Escribir(textos.cierreGuiado);
            OnCicloTerminado?.Invoke(Ciclo.Guiado);
            yield return Pausa();

            // Pasar al paciente con el botón Continuar (o la tecla N)
            Escribir(textos.continuar);
            continuar = false;
            if (botonContinuar != null) botonContinuar.SetActive(true);
            while (!continuar && !Consumir()) yield return null;
            if (botonContinuar != null) botonContinuar.SetActive(false);

            flujo = StartCoroutine(Paciente());
        }

        // =============================================================================================
        // Ciclo del paciente: AUG–GGC–UAG, sin ayuda (Mo6, D2)
        // =============================================================================================
        IEnumerator Paciente()
        {
            CicloActual = Ciclo.Paciente;
            Preparar(secuenciaPaciente, mostrarARNt: true);
            CambiarFase(Fase.Preparacion);
            Escribir(textos.alertaPaciente);
            yield return Pausa(pausa * 1.5f);

            for (int i = 0; i < secuencia.Largo; i++)
            {
                var c = secuencia[i];
                if (c.esParo)
                {
                    yield return Paro(i, string.Format(textos.pacienteParo, c.codon));
                    break;
                }
                yield return Acoplar(i, Modo.Libre, string.Format(textos.pacienteBuscar, c.codon),
                    string.Format(textos.pacienteAcierto, c.aminoacido));
            }

            CambiarFase(Fase.Fin);
            Escribir(string.Format(textos.cierrePaciente, cadena.Cantidad, LargoCadenaSana));
            OnCicloTerminado?.Invoke(Ciclo.Paciente);
            yield return Pausa(pausa * 1.5f);
            Escribir(textos.diagnostico);
            flujo = null;
        }

        // =============================================================================================
        // Pasos
        // =============================================================================================

        // Lleva el codón i al sitio de lectura y espera el ARNt que encaja.
        IEnumerator Acoplar(int i, Modo modo, string llegar, string acertar)
        {
            var codon = secuencia[i];
            CambiarFase(i == 0 ? Fase.Inicio : Fase.Elongacion);
            yield return tira.IrA(i);
            OnCodonActivo?.Invoke(codon);
            Escribir(llegar);

            var esperado = ARNtPara(codon);
            if (esperado == null) { Debug.LogError($"[Traduccion] No hay ARNt para {codon.codon}.", this); yield break; }

            bool forzado = false;
            if (modo == Modo.Automatico)
            {
                yield return Pausa(1f);
                forzado = true;
            }
            else
            {
                if (modo == Modo.Guiado) Parpadeo.Iniciar(esperado.Pieza.modelo.gameObject);
                errores = 0;
                acoplado = null;
                sitioLectura.clave = codon.codon; // solo encaja el ARNt cuyo anticodón le corresponde
                while (acoplado == null)
                {
                    if (Consumir()) { forzado = true; break; }
                    if (modo == Modo.Libre && pistaTrasErrores > 0 && errores >= pistaTrasErrores)
                        Parpadeo.Iniciar(esperado.Pieza.modelo.gameObject);
                    yield return null;
                }
                sitioLectura.clave = "";
                Parpadeo.Detener(esperado.Pieza.modelo.gameObject);
            }

            var arnt = forzado ? esperado : acoplado;
            yield return Transferir(arnt, llevarAlSitio: forzado);
            OnARNtCorrecto?.Invoke(arnt);
            Debug.Log($"[Traduccion] {codon.codon} → {codon.aminoacido} (cadena: {cadena.Cantidad}).");
            if (!string.IsNullOrEmpty(acertar))
            {
                Escribir(acertar);
                yield return Pausa();
            }
        }

        // Codón de paro: ningún ARNt encaja; el jugador lo señala y entra solo el factor de liberación (D1).
        IEnumerator Paro(int i, string llegar)
        {
            var codon = secuencia[i];
            CambiarFase(Fase.Terminacion);
            yield return tira.IrA(i);
            OnCodonActivo?.Invoke(codon);
            sitioLectura.clave = ""; // ningún ARNt encaja en un paro
            Escribir(llegar);
            yield return Senalar(tira.Codon(i), parpadear: CicloActual == Ciclo.Guiado);

            Escribir(textos.liberacion);
            if (factorLiberacion != null)
                yield return Mover(factorLiberacion, sitioLectura.transform.position, sitioLectura.transform.rotation, 0.9f);
            yield return Pausa(0.4f);
            StartCoroutine(SepararSubunidades());
            yield return cadena.Liberar();
            OnCadenaLiberada?.Invoke(cadena.Cantidad);
            Debug.Log($"[Traduccion] Paro en {codon.codon}: la cadena se suelta con {cadena.Cantidad} aminoácidos.");
            yield return Pausa();
        }

        // El aminoácido pasa del ARNt a la cadena y el ARNt vacío se va.
        IEnumerator Transferir(ARNt arnt, bool llevarAlSitio)
        {
            if (llevarAlSitio)
            {
                arnt.gameObject.SetActive(true);
                // Si el jugador lo tiene en la mano (o está en otra ranura), se suelta primero
                if (arnt.Agarre.isSelected && arnt.Agarre.interactionManager != null)
                    arnt.Agarre.interactionManager.CancelInteractableSelection((IXRSelectInteractable)arnt.Agarre);
                yield return Mover(arnt.transform, sitioLectura.transform.position, sitioLectura.transform.rotation, 0.8f);
            }
            yield return Pausa(0.3f);

            if (arnt.aminoacido != null) yield return cadena.Agregar(arnt.aminoacido);

            if (socket.hasSelection) sitioLectura.Vaciar();
            var salida = arnt.transform.position + new Vector3(-0.25f, 0.2f, 0.1f);
            yield return Mover(arnt.transform, salida, arnt.transform.rotation, 0.6f);
            arnt.gameObject.SetActive(false);
        }

        // ---- Respuestas de la ranura (Mo2) ----
        void AlEncajar(RanuraSeleccion ranura)
        {
            var pieza = socket.firstInteractableSelected;
            acoplado = pieza != null ? pieza.transform.GetComponent<ARNt>() : null;
        }

        void AlRechazar(RanuraSeleccion ranura, string motivo)
        {
            errores++;
            if (ultimoSujetado != null) OnARNtIncorrecto?.Invoke(ultimoSujetado);

            if (FaseActual == Fase.Inspeccion || FaseActual == Fase.Preparacion) Escribir(textos.esperaInspeccion);
            else if (FaseActual == Fase.Terminacion) Escribir(textos.paroSinARNt);
            else if (FaseActual == Fase.Inicio || FaseActual == Fase.Elongacion) Escribir(textos.noEncaja);
        }

        // Cada vez que el jugador sujeta un ARNt: cuenta para la inspección (paso 3) y, si lo suelta
        // en el sitio y no encaja, es el que se rechazó (RanuraSeleccion no dice cuál fue)
        void AlSujetar(ARNt arnt)
        {
            ultimoSujetado = arnt;
            inspeccionados.Add(arnt);
        }

        // ---- Utilidades ----
        void Preparar(SecuenciaARNm sec, bool mostrarARNt)
        {
            secuencia = sec;
            sitioLectura.clave = "";
            sitioLectura.Vaciar();
            cadena.Limpiar();
            foreach (var a in arnts)
            {
                if (a == null) continue;
                Parpadeo.Detener(a.Pieza.modelo.gameObject);
                a.Recargar();
                a.gameObject.SetActive(mostrarARNt);
            }
            if (factorLiberacion != null) factorLiberacion.SetPositionAndRotation(poseFactor.position, poseFactor.rotation);
            if (subunidadMayor != null) subunidadMayor.SetPositionAndRotation(poseMayor.position, poseMayor.rotation);
            if (subunidadMenor != null) subunidadMenor.SetPositionAndRotation(poseMenor.position, poseMenor.rotation);
            if (botonContinuar != null) botonContinuar.SetActive(false);
            if (ribosoma != null) ribosoma.Habilitado = false;
            tira.Construir(sec);
            forzar = false;
            Debug.Log($"[Traduccion] Ciclo {CicloActual}: ARNm {sec.nombre}.");
        }

        void MostrarARNt(bool mostrar)
        {
            foreach (var a in arnts) if (a != null) a.gameObject.SetActive(mostrar);
        }

        ARNt ARNtPara(CodonData codon)
        {
            foreach (var a in arnts)
                if (a != null && a.codon != null && a.codon.codon == codon.codon) return a;
            return null;
        }

        IEnumerator Senalar(Senalable objetivo, bool parpadear = false)
        {
            if (objetivo == null) yield break;
            bool senalado = false;
            void Al(Senalable s) => senalado = true;
            objetivo.OnSenalado += Al;
            objetivo.Habilitado = true;
            if (parpadear) Parpadeo.Iniciar(objetivo.gameObject);
            while (!senalado && !Consumir()) yield return null;
            objetivo.OnSenalado -= Al;
            objetivo.Habilitado = false;
            Parpadeo.Detener(objetivo.gameObject);
        }

        IEnumerator SepararSubunidades()
        {
            if (subunidadMayor == null || subunidadMenor == null) yield break;
            var mayor = subunidadMayor.position + Vector3.up * 0.12f;
            var menor = subunidadMenor.position + Vector3.down * 0.08f;
            StartCoroutine(Mover(subunidadMenor, menor, subunidadMenor.rotation, 0.9f));
            yield return Mover(subunidadMayor, mayor, subunidadMayor.rotation, 0.9f);
        }

        static IEnumerator Mover(Transform t, Vector3 destino, Quaternion rotacion, float duracion)
        {
            var desde = t.position;
            var giro = t.rotation;
            for (float k = 0f; k < 1f; k += Time.deltaTime / duracion)
            {
                float s = Mathf.SmoothStep(0f, 1f, k);
                t.SetPositionAndRotation(Vector3.Lerp(desde, destino, s), Quaternion.Slerp(giro, rotacion, s));
                yield return null;
            }
            t.SetPositionAndRotation(destino, rotacion);
        }

        IEnumerator Pausa(float segundos = -1f)
        {
            float fin = Time.time + (segundos < 0f ? pausa : segundos);
            while (Time.time < fin && !Consumir()) yield return null;
        }

        void CambiarFase(Fase fase)
        {
            FaseActual = fase;
            OnFaseCambiada?.Invoke(CicloActual, fase);
        }

        void Escribir(string texto)
        {
            if (letrero != null) letrero.text = texto;
        }

        // La tecla N "completa" lo que se esté esperando (solo para probar)
        bool Consumir()
        {
            if (!forzar) return false;
            forzar = false;
            return true;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void Update()
        {
            var k = Keyboard.current;
            if (k == null) return;
            if (k.nKey.wasPressedThisFrame) forzar = true;
            if (k.rKey.wasPressedThisFrame) Reiniciar();
            if (k.pKey.wasPressedThisFrame) IrAlPaciente();
        }
#endif
    }
}
