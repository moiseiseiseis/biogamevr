# Cómo subir un avance hecho en otro proyecto de Unity

Esta guía es para cuando hiciste algo (una escena, un minijuego, una mecánica) en **tu propio proyecto de Unity** y quieres pasarlo al proyecto del juego, **BioVR**, sin romper nada.

La idea es simple: **no se sube tu proyecto completo**, solo tus archivos (escena, scripts, prefabs, materiales, modelos). Se meten en una carpeta propia dentro de BioVR, en una rama, y se manda un Pull Request.

> Rama ya creada para el prototipo de minijuego: **`game-design/minijuego-prototipo`**

---

## Antes de empezar

| Necesitas | Detalle |
|---|---|
| Unity **6000.3.26f1** | Es la versión del equipo. Instálala desde Unity Hub si no la tienes. No abras BioVR con otra versión porque cambia archivos del proyecto. |
| Git + Git LFS | Los modelos (`.fbx`, `.blend`, `.obj`…) y las imágenes se suben con LFS. |
| Acceso al repo | Si no puedes hacer `push`, pídele a Moisés que te agregue como colaborador. |

---

## Paso 1. Exportar tu avance desde TU proyecto

1. Abre tu proyecto de Unity, el tuyo, no BioVR.
2. En la ventana **Project**, selecciona **solo** las carpetas de tu avance: escena, scripts, prefabs, materiales, modelos, sonidos.
3. Clic derecho → **Export Package…**
4. Deja marcado **Include dependencies**, revisa la lista y **desmarca** lo que no sea tuyo: paquetes de ejemplo, `TextMesh Pro`, `XR`, `Samples`, etc.
5. Guarda el archivo como `minijuego-prototipo.unitypackage` **fuera** de la carpeta del repo (por ejemplo, en Descargas).

Anota también:
- En qué **versión de Unity** lo hiciste.
- Si instalaste **paquetes extra** (Window → Package Manager), como ProBuilder, Shader Graph, Cinemachine o algo de la Asset Store.

---

## Paso 2. Descargar el repo y cambiarte a la rama

En una terminal (Git Bash):

```bash
git lfs install
git clone https://github.com/moiseiseiseis/biogamevr.git   # solo la primera vez
cd biogamevr
git fetch
git checkout game-design/minijuego-prototipo
```

---

## Paso 3. Importar en BioVR

1. En Unity Hub → **Add** → selecciona la carpeta `biogamevr/04-desarrollo/BioVR` y ábrela con **6000.3.26f1**. La primera vez tarda varios minutos.
2. **Assets → Import Package → Custom Package…** y elige tu `.unitypackage`.
3. Cuando termine, mueve **todo** lo que importaste a tu carpeta propia:

   ```
   Assets/_BioVR/Minijuegos/Prototipos/<NombreDelMinijuego>/
       Escenas/
       Scripts/
       Prefabs/
       Materiales/
       Modelos/
   ```

   Muévelo **dentro de Unity**, arrastrando en la ventana Project, no desde el explorador de Windows, para que no se rompan las referencias.

---

## Paso 4. Revisar que funcione

Abre tu escena y dale Play. Revisa estos problemas típicos:

| Si ves… | Qué pasa y qué hacer |
|---|---|
| Objetos **rosas/magenta** | El proyecto usa **URP**. Selecciona tus materiales → **Edit → Rendering → Materials → Convert Selected Built-in Materials to URP**. |
| Errores con `Input.GetKey`, `Input.mousePosition`, `OnMouseDrag`, `OnMouseDown`… | BioVR usa **solo el Input System nuevo** (es VR, no hay mouse). **No cambies la configuración del proyecto.** Déjalo así y avísale a Desarrollo; nosotros lo adaptamos a los controles del visor. |
| Errores de que falta un paquete | Avísanos qué paquete usaste antes de instalar nada. |
| Textos que no se ven | Importa **TMP Essentials** si Unity lo pide (Window → TextMeshPro → Import TMP Essential Resources). |

Si sigue habiendo errores rojos en la consola y no sabes por qué, **súbelo igual** y menciónalo en el PR. Lo revisamos juntos.

---

## Paso 5. Subir los cambios

```bash
git status                     # revisa qué archivos se van a subir
git add 04-desarrollo/BioVR/Assets/_BioVR/Minijuegos
git commit -m "Game Design: prototipo de minijuego <nombre>"
git push
```

Después, en GitHub: **Compare & pull request** → base `main` ← `game-design/minijuego-prototipo`. En la descripción explica:
- Qué hace el minijuego y cómo se juega.
- Qué escena hay que abrir.
- Qué no funciona todavía o qué errores quedaron.

---

## ⚠️ No subas ni modifiques

- `ProjectSettings/`, `Packages/` ni `00-EscenaInicial.unity`. Si `git status` los muestra como modificados, **no los agregues**: `git restore <archivo>`.
- Carpetas `Library/`, `Temp/`, `Logs/`, `UserSettings/` (ya están ignoradas).
- El archivo `.unitypackage`.
- Tu proyecto de Unity completo.

Por eso en el paso 5 se usa `git add` **solo** con la carpeta `Minijuegos` y no con `git add .`.

---

## ¿No te sientes cómodo con git?

No pasa nada. Haz solo el **Paso 1** y mándale a Moisés el `.unitypackage` (por Drive o WhatsApp), junto con la versión de Unity y los paquetes que usaste. Desarrollo lo importa en la rama y abre el PR a tu nombre.
