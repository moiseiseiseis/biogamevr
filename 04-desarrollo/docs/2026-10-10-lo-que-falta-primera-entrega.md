# Equipo de Desarrollo: lo que falta para la primera entrega

Oct 10, 2026 · @Moisés

## Dónde estamos

En `main` ya está integrado el trabajo de todos, con correcciones. Se puede jugar la secuencia completa en el simulador:

**Hangar → Abordaje → Vasos → Capilar → Citoplasma → Llegada**

Cada paso se avanza con el botón de la consola (o con Espacio en el editor). Los saltos de escala tienen su efecto: túnel de partículas, destello y cambio de entorno. Los entornos son placeholders hechos con primitivas.

Lo que falta está abajo, por tarea. Cada tarea dice **quién, dónde, cómo y cuándo se considera terminada**, y tiene su issue en GitHub.

**Ustedes solo llegan, programan y mandan su PR.** El rol 1 (Moisés) se encarga de lo demás: revisar y fusionar los PR, mantener `main` al día e integrar los prefabs en la escena principal.

> Las ramas viejas (`rol2/jugador-sentado`, `rol3/dev-interaccion-cabina`, `rol4/Secuencia_y_flujo`, `rol5/script-datos-escala`) **ya se integraron y se borraron de GitHub**. Empieza siempre desde `main` actualizado. Lo que se corrigió de tu código está en la bitácora del 10 de octubre (`minutas/desarrollo`); léela para poder explicar tu script.

---

## Antes de empezar (todos)

1. Actualiza `main`: en GitHub Desktop, *Fetch* → *Pull*. Por terminal, `git checkout main` y `git pull`.
2. Crea tu rama desde `main`, con el nombre que dice tu tarea (por ejemplo `rol3/pantalla-escala`).
3. Abre `04-desarrollo/BioVR` con **Unity 6000.3.26f1**.
4. Para probar, abre `Assets/_BioVR/Escenas/00-EscenaInicial.unity` y dale Play. Con el XR Device Simulator:
   - **Espacio** avanza la secuencia.
   - Las teclas **1 a 4** saltan a Hangar, Abordaje, Viaje y Llegada.
5. **No guardes cambios en `00-EscenaInicial`** (la escena principal es del rol 1). Trabaja en **tu escena de prueba** (`Escenas/Pruebas/RolN_...`) y entrega un **prefab**. En la descripción del PR, di dónde va y con qué se conecta, y el rol 1 lo integra.
6. Sube siempre los `.meta`. Nombres sin acentos ni espacios. PR a `main`; nadie sube directo.

### No corras estos menús (salvo que tu tarea lo diga)

| Menú | Qué hace | Por qué no |
| --- | --- | --- |
| BioVR → Armar escena inicial | Arma la escena principal desde las escenas de prueba | Ya se usó; si la escena ya está armada, no hace nada |
| BioVR → Generar entornos placeholder | Rehace los 4 entornos con primitivas | **Sobrescribe los prefabs** de `Entornos/`; si ya hay modelos de Diseño, se pierden |
| BioVR → Agregar efecto de salto | Rehace el objeto `EfectoSalto` | Solo hace falta si se borró |
| BioVR → Agregar cabina holográfica | Conecta `CabinaHolografica` en la cabina | Ya está hecho |
| BioVR → Build APK | Genera el APK | Solo quien haga el build (tarea 8) |

### Datos útiles del proyecto

**La cabina** está en el origen:
- Mide 4 × 4 m. El piso está en y = 0 y el techo en 2 m.
- El jugador va sentado en el centro y **mira hacia +Z**.
- El parabrisas es todo el frente, entre 0.4 m y 2 m de altura.
- La consola con el botón está en (0, 0.7, 0.45), al alcance de la mano.
- Todo está en `Cabina (provisional)`.

**Índice de escala:** lo usan `GameManager.Salto` y `GestorEscala`.

| Índice | Nivel | Asset | Zona |
| --- | --- | --- | --- |
| 0 | Hangar | `Escala/Datos/Escala0_Hangar` | Hangar, Abordaje |
| 1 | Vasos sanguíneos | `Escala1_Vasos` | Vasos |
| 2 | Capilar | `Escala2_Capilar` | Capilar |
| 3 | Citoplasma | `Escala3_Citoplasma` | Citoplasma, Llegada |

**Eventos a los que te puedes suscribir:** siempre en `Start` y desuscribirte en `OnDestroy`. Hay ejemplos en `ZonasManager.cs` y `EfectoSalto.cs`.

```csharp
using BioVR.Secuencia;   // GameManager, GameState
using BioVR.Escala;      // GestorEscala, NivelEscalaData

GameManager.Instance.OnEstadoCambiado += (GameState estado) => { };  // Hangar, Abordaje, Viaje, Llegada
GameManager.Instance.OnSaltoCambiado  += (int salto) => { };         // 0 a 3, ya con la zona nueva
GameManager.Instance.OnSaltoIniciado  += (int saltoNuevo) => { };    // empieza el efecto, antes del destello
GestorEscala.OnCambioEscala           += (NivelEscalaData datos) => { }; // evento estático con los datos del nivel
GameManager.Instance.Avanzar();                                      // lo que hace el botón de la consola
```

---

## Tareas

### 1. Pantalla de escala en la cabina: rol 3 (Iker) · issue #11

**Qué:** un letrero en la cabina que diga en qué nivel estamos. Por ejemplo, **"Vasos sanguíneos · mecha ~1-5 mm"**.

**Dónde:**
- Prefab `Assets/_BioVR/Cabina/PantallaEscala.prefab`
- Script `Assets/_BioVR/Cabina/Scripts/PantallaEscala.cs` (`namespace BioVR.Cabina`)
- Escena de prueba `Escenas/Pruebas/Rol3_BotonCabina.unity`
- Rama: `rol3/pantalla-escala`

**Cómo:**
1. *GameObject → UI → Canvas*. En el Canvas, pon **Render Mode: World Space**, escala 0.001 (así 1 px = 1 mm) y tamaño 600 × 200. El texto va con *UI → Text - TextMeshPro*; TMP Essentials ya está importado.
2. Agrégale **Tracked Device Graphic Raycaster** en lugar de *Graphic Raycaster*. Aunque no tenga botones, así no estorba a los rayos de los mandos.
3. En el script, suscríbete a `GestorEscala.OnCambioEscala` y escribe `$"{datos.nombreNivel} · mecha {datos.tamanoMecha}"`. En `Start`, lee también `FindFirstObjectByType<GestorEscala>().EscalaActual` para mostrar el nivel inicial.
4. Posición sugerida dentro de la cabina: (0, 1.1, 1.6), un poco inclinada hacia el jugador, arriba del tablero y sin tapar el parabrisas.

**Terminado cuando:** en Play, el letrero cambia en cada salto y se lee bien en el simulador.

> La proporción ("1 : 1,000") se puede agregar después. Si la quieren, el rol 5 agrega un campo `textoEscala` a `NivelEscalaData`. Investigación tiene que confirmar los números, porque la tabla dice "~1000 veces por nivel", pero de Capilar (~1 µm) a Citoplasma (~200-500 nm) no son 1000.

### 2. Botones con nombre: rol 3 (Iker) · issue #11

**Qué:** los botones provisionales **"Iniciar inmersión"** y **"Salto Matrioska"**. La vibración ya funciona: viene en los *Poke Interactor* del XR Rig.

**Dónde:** `Assets/_BioVR/Cabina/`, en la misma rama que la tarea 1 o en `rol3/botones-cabina`.

**Cómo:**
1. Duplica `Button1.prefab` en dos prefabs con una etiqueta TMP encima (un Canvas world space chiquito, como en la tarea 1).
2. **No cambies sus componentes.** Ya es `XRSimpleInteractable` + `XRPokeFilter`, se presiona desde arriba y regresa a su color al soltarlo. Ya se probó con los mandos.
3. La conexión con `GameManager.Avanzar()` se hace en la escena principal (la hace el rol 1). En el PR, di qué botón hace qué.

**Terminado cuando:** los dos botones se ven con su nombre y se presionan con el dedo.

### 3. Palanca (opcional): rol 3 · issue #25

**Qué:** una palanca que se agarra, se mueve en un solo eje con tope y dispara un evento al llegar al final.

**Dónde:** `Cabina/Palanca.prefab` y `Cabina/Scripts/Palanca.cs`. Rama `rol3/palanca`.

**Cómo:** `XRGrabInteractable` con *Track Position* apagado, en un pivote. En `Palanca.cs`, limita el ángulo (por ejemplo de 0 a 45°) y, al pasar de 40°, dispara un `UnityEvent alLlegarAlTope` una sola vez hasta que regrese.

**Terminado cuando:** en el simulador se agarra, no pasa del tope y el evento se dispara.

### 4. Viñeta, asiento y recentrar: rol 2 (Genaro) · issue #24

**Qué:** las tres cosas que faltan del jugador sentado.

**Dónde:** `Assets/_BioVR/Jugador/`, escena `Escenas/Pruebas/Rol2_JugadorSentado.unity`. Rama `rol2/vineta-recentrar`.

**Cómo:**
- **Viñeta durante el salto:**
  1. Pon el prefab `Samples/XR Interaction Toolkit/3.6.1/Starter Assets/TunnelingVignette/TunnelingVignette.prefab` como hijo de la Main Camera.
  2. Escribe `Jugador/Scripts/VinetaSalto.cs`, que implemente `ITunnelingVignetteProvider` (`using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;`).
  3. En `GameManager.Instance.OnSaltoIniciado`, llama `controller.BeginTunnelingVignette(this)`. En `OnSaltoCambiado`, llama `controller.EndTunnelingVignette(this)`.
  4. Así la viñeta solo aparece mientras pasan las rayas del túnel.
- **Asiento:** un cubo provisional en (0, 0.25, −0.3) de 0.5 × 0.5 × 0.5, para que el jugador sepa dónde está sentado.
- **Recentrar:** `Jugador/Scripts/Recentrar.cs`.
  - Con un botón del mando (por ejemplo **B / Y**, con una `InputActionReference`) llama `xrOrigin.MoveCameraToWorldLocation(puntoOjos)` y `xrOrigin.MatchOriginUpCameraForward(Vector3.up, Vector3.forward)`.
  - Usa como `puntoOjos` un objeto vacío en (0, 1.2, 0).
- Hoy el XR Origin usa el modo de tracking por defecto. Con recentrar no hace falta cambiarlo; si propones otro, explícalo en el PR.

**Entrega:** los scripts y un prefab, o la lista de qué agregar al XR Origin. En el PR, di exactamente qué componente va en qué objeto del XR Origin para que el rol 1 lo integre.

**Terminado cuando:** en el simulador aparece la viñeta durante cada salto y, al presionar el botón, la vista vuelve al asiento mirando al frente.

### 5. `AbordajeTrigger`: rol 4 (7744marcods) · issue #26

En el PR #18 aparece "AbordajeTrigger: la zona junto a la nave en el hangar que lleva al abordaje", pero **el archivo nunca se subió** y la escena no lo usa.
- Si lo tienes, súbelo en la rama `rol4/abordaje-trigger` (en `Assets/_BioVR/Secuencia/`, con `namespace BioVR.Secuencia` y su `.meta`). Explica en el PR qué lo activa, porque el jugador va sentado y no camina.
- Si ya no aplica, avísale al rol 1 para quitarlo de la lista.

### 6. Efectos por escala y cabina holográfica: rol 5 · ✅ HECHO (PR #23)

Lo hizo el rol 1, porque Ascenci no tiene computadora:
- `Escala/Scripts/EmpujonesMoleculares.cs`: las moléculas del citoplasma tiemblan según `intensidadEmpujones`.
- `Escala/Scripts/CabinaHolografica.cs`: desde el salto 3 la cabina se ve como una reconstrucción holográfica.

**Ascenci:** léelos para poder explicarlos en la presentación. Falta revisar el mareo con Implementación cuando haya APK (issue #28).

### 7. Entornos con los modelos de Diseño: rol 6 (Moisés) · issue #27

**Qué:** sustituir los placeholders por los FBX de Diseño **cuando lleguen**. Mientras tanto, se pueden mejorar los placeholders.

**Dónde:** FBX en `Assets/_BioVR/Modelos/` y prefabs en `Assets/_BioVR/Entornos/{Hangar, VasoSanguineo, Capilar, EspacioCelular}/`. Rama `rol6/modelos-diseno`.

**Cómo:**
1. Importa el FBX. Revisa que venga en la escala correcta (1 unidad = 1 m, Y arriba) y con unos 100 mil triángulos como máximo por entorno.
2. Abre el prefab del entorno (doble clic) y sustituye las primitivas por el modelo. **No muevas la raíz del prefab.** Recuerda que la cabina está en el origen, mira a +Z y no puede atravesarse.
3. Si quieres que algo fluya, déjalo como hijo de `Globulos (flujo)` o `Proteinas (flujo)`, que ya tienen `FlujoPlaceholder`. Para la cinesina, usa `Cinesina con vesicula`, que tiene `RecorridoLineal`.
4. **No vuelvas a correr *Generar entornos placeholder*,** porque sobrescribe los prefabs.
5. Anota cada modelo en `CREDITOS.md`.

**Terminado cuando:** los cuatro entornos usan los modelos de Diseño y se ven por el parabrisas en su zona.

### 8. Primer APK en el visor: rol 1 + Implementación · issue #28

**Qué:** lo más importante que falta, porque nunca se ha probado en un Quest.

**Cómo:**
1. Cierra Unity. Ábrelo y usa el menú **BioVR → Build APK**, o por terminal: `Unity -batchmode -buildTarget Android -executeMethod BioVR.Comun.Editor.BuildAndroid.Construir`. Sale en `04-desarrollo/BioVR/Builds/BioVR.apk`.
2. Activa el modo desarrollador en el visor, conéctalo por USB y corre `adb install -r "04-desarrollo/BioVR/Builds/BioVR.apk"`.
3. En el visor: *Biblioteca → Fuentes desconocidas → BioVR*.

**Qué revisar y anotar en el issue #28:**
- [ ] Abre y se ve la cabina desde el asiento
- [ ] El botón de la consola se presiona con el dedo y avanza la secuencia
- [ ] Los 4 entornos se ven y el efecto de salto se ve bien
- [ ] Va fluido, sin tirones (si se puede, ver los FPS con *OVR Metrics Tool*; la meta es 72)
- [ ] Mareo: nadie se siente mal en los saltos ni en el vaso sanguíneo
- [ ] No aparecen los puntitos blancos (en la laptop del rol 1 sí salen, pero parece ser su GPU)
- [ ] Si algo falla, guardar el log: `adb logcat -s Unity > log.txt`

### 9. Organización: rol 1

- [x] Fusionar los PR #19 a #23 y dejar `main` al día
- [x] Borrar las ramas ya integradas o rotas (en GitHub solo queda `main`)
- [x] Un issue por tarea (#11 y #24 a #28); se cerraron #9 y #10, que ya se cumplieron
- [x] Vibración de los mandos: ya venía en el XR Rig
- [x] Llenar `06-management/equipos.md` (PR #29)
- [x] Rol 6: Moisés (issue #27)
- [ ] Recordar a chrisroiz, JupiterVega y andresflores2076-netizen que acepten la invitación al repo
- [ ] Integrar en la escena principal lo que entreguen las tareas 1 a 5

---

## Prioridad

| Prioridad | Tarea | Por qué |
| --- | --- | --- |
| 🔴 1 | **8. APK en el visor** | Nunca se ha probado; es la única forma de saber si el rendimiento y el mareo están bien |
| 🟠 2 | **1. Pantalla de escala** | Es corta y hace que se entienda qué está pasando |
| 🟠 3 | **4. Viñeta y recentrar** | Comodidad en el visor |
| 🟡 4 | 2 y 7 | Mejoran mucho la presentación |
| ⚪ 5 | 3 y 5 | Opcionales |

## Reglas (las de siempre)

1. Unity **6000.3.26f1**.
2. Una rama por tarea y PR a `main`. Nadie sube directo a `main`.
3. Cada quien trabaja en su carpeta de `_BioVR/` y en su escena de `Escenas/Pruebas/`. La escena principal solo la toca el rol 1.
4. Lo que va a la escena principal se entrega como prefab, o con instrucciones claras en el PR.
5. Siempre suban los `.meta`. Nombres sin acentos ni espacios. **Ojo con los caracteres invisibles al copiar nombres de un chat.**
6. Quien sube un script debe poder explicar qué hace.
7. Cualquier duda o bloqueo: una línea en el grupo con qué terminaste, qué sigue y qué te bloquea.
