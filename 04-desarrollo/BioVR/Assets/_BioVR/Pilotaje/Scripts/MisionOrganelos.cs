using System.Collections;
using System.Collections.Generic;
using BioVR.Comun;
using BioVR.Pokedex;
using BioVR.Secuencia;
using TMPro;
using UnityEngine;

namespace BioVR.Pilotaje
{
    // Misión del modo investigación: registrar en el pokédex 4 organelos distintos de la célula para desbloquear
    // el salto a 5 nm (la Cocina de proteínas). Mientras falten, Salto Matrioska y la palanca no hacen el salto:
    // el letrero sobre la consola parpadea y dice cuántos faltan.
    //
    // Avisa al tutorial de la misión: "organelo" (registró uno nuevo) y "mision" (completó los 4).
    // Si el jugador ya los tenía cuando el tutorial llega a ese paso, se lo vuelve a avisar.
    // Se reinicia al volver al hangar. Lo arma BioVR → Pilotaje → Armar prueba de pilotaje.
    public class MisionOrganelos : MonoBehaviour
    {
        [SerializeField] int requeridos = 4;
        [Tooltip("Solo cuentan las fichas de esta zona (las de la sangre no).")]
        [SerializeField] string zona = "Interior de la célula";
        [SerializeField] TextMeshPro letrero;
        [SerializeField] Color colorNormal = new Color(1f, 0.85f, 0.35f);
        [SerializeField] Color colorCumplida = new Color(0.45f, 1f, 0.55f);
        [SerializeField] Color colorBloqueo = new Color(1f, 0.3f, 0.3f);

        readonly HashSet<FichaPokedex> organelos = new HashSet<FichaPokedex>();
        GameManager gm;

        public bool Cumplida => organelos.Count >= requeridos;

        void Start()
        {
            gm = GameManager.Instance;
            if (gm == null) { Debug.LogError("[Mision] Necesita el GameManager.", this); enabled = false; return; }
            gm.BloqueoCocina = Motivo;
            gm.OnCocinaBloqueada += AlBloquear;
            gm.OnEstadoCambiado += AlCambiarEstado;
            Pokedex.Pokedex.OnRegistrada += AlRegistrar;
            Tutorial.OnPasoCambiado += AlCambiarPaso;
            AlCambiarEstado(gm.Estado);
        }

        void OnDestroy()
        {
            if (gm != null)
            {
                if (gm.BloqueoCocina == Motivo) gm.BloqueoCocina = null;
                gm.OnCocinaBloqueada -= AlBloquear;
                gm.OnEstadoCambiado -= AlCambiarEstado;
            }
            Pokedex.Pokedex.OnRegistrada -= AlRegistrar;
            Tutorial.OnPasoCambiado -= AlCambiarPaso;
        }

        string Motivo() => Cumplida ? null : $"Registra {requeridos} organelos: llevas {organelos.Count} de {requeridos}";

        void AlRegistrar(FichaPokedex ficha, bool nueva)
        {
            if (ficha == null || ficha.zona != zona || !organelos.Add(ficha)) return;
            Debug.Log($"[Mision] Organelo {organelos.Count} de {requeridos}: {ficha.nombre}.");
            Tutorial.Notificar("organelo");
            if (organelos.Count == requeridos)
            {
                Debug.Log("[Mision] Cumplida: el salto a 5 nm está desbloqueado.");
                Tutorial.Notificar("mision");
            }
            Actualizar();
        }

        // Si ya se cumplió lo que pide el paso que acaba de aparecer, se completa solo
        void AlCambiarPaso(PasoTutorial paso)
        {
            if (paso == null) return;
            if (paso.clave == "organelo" && organelos.Count > 0) Tutorial.Notificar("organelo");
            else if (paso.clave == "mision" && Cumplida) Tutorial.Notificar("mision");
        }

        void AlCambiarEstado(GameState estado)
        {
            if (estado == GameState.Hangar) organelos.Clear();
            if (letrero != null) letrero.gameObject.SetActive(estado == GameState.Llegada);
            Actualizar();
        }

        void AlBloquear(string _)
        {
            StopAllCoroutines();
            StartCoroutine(Parpadear());
        }

        IEnumerator Parpadear()
        {
            if (letrero == null) yield break;
            for (int i = 0; i < 6; i++)
            {
                letrero.color = i % 2 == 0 ? colorBloqueo : colorNormal;
                yield return new WaitForSeconds(0.18f);
            }
            Actualizar();
        }

        void Actualizar()
        {
            if (letrero == null) return;
            letrero.color = Cumplida ? colorCumplida : colorNormal;
            letrero.text = Cumplida
                ? "Misión cumplida\n<size=70%>Salto Matrioska: a 5 nm, al ribosoma</size>"
                : $"Misión: {organelos.Count} de {requeridos} organelos\n<size=70%>regístralos con el pokédex</size>";
        }
    }
}
