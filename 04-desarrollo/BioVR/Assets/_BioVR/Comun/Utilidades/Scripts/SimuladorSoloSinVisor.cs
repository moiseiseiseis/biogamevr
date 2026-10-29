#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace BioVR.Comun
{
    // El XR Device Simulator aparece solo al dar Play en el editor, y al aparecer BORRA el visor real
    // (removeOtherHMDDevices): con Quest Link se ve "la vista de PC" y los mandos no responden.
    // Esto lo apaga si al dar Play ya hay un visor de verdad funcionando (Link / Air Link).
    // Sin visor (la laptop) no cambia nada: el simulador aparece como siempre.
    static class SimuladorSoloSinVisor
    {
        static bool restaurar;

        // BeforeSceneLoad corre después de que XR arranca y antes de que XRI cree el simulador (AfterSceneLoad)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Revisar()
        {
            if (!HayVisor()) return;
            if (!CambiarAutoInstanciar(false)) return;
            restaurar = true;
            // La opción está en un asset del proyecto: se regresa a como estaba al salir de Play
            Application.quitting += Restaurar;
            Debug.Log("[BioVR] Hay un visor conectado: no se usa el XR Device Simulator.");
        }

        static bool HayVisor()
        {
            var pantallas = new List<XRDisplaySubsystem>();
            SubsystemManager.GetSubsystems(pantallas);
            return pantallas.Exists(p => p.running);
        }

        static void Restaurar()
        {
            Application.quitting -= Restaurar;
            if (restaurar) CambiarAutoInstanciar(true);
            restaurar = false;
        }

        // Los ajustes del simulador son internal en XRI, por eso se usa reflexión
        static bool CambiarAutoInstanciar(bool valor)
        {
            const BindingFlags banderas = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.FlattenHierarchy; // Instance está en la clase base (ScriptableSettings)
            var tipo = typeof(XRInteractionSimulatorLoader).Assembly.GetType(
                "UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRDeviceSimulatorSettings");
            var ajustes = tipo?.GetProperty("Instance", banderas)?.GetValue(null);
            var propiedad = tipo?.GetProperty("automaticallyInstantiateSimulatorPrefab", banderas);
            if (ajustes == null || propiedad == null)
            {
                Debug.LogWarning("[BioVR] No se encontraron los ajustes del XR Device Simulator (¿cambió la versión de XRI?).");
                return false;
            }
            if (!valor && !(bool)propiedad.GetValue(ajustes)) return false; // ya estaba apagado
            propiedad.SetValue(ajustes, valor);
            return true;
        }
    }
}
#endif
