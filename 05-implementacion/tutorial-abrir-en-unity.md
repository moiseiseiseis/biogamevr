# Tutorial: abrir BioVR en Unity y darle Play

Para el equipo de Implementación · Windows · ~30 minutos la primera vez (casi todo es esperar)

Con esto ves el juego en la PC sin visor, con el simulador de VR. Para probar en el Quest se sigue usando el APK (ver `guia-visor-vr.md`).

> **Si ya lo abriste y "no carga nada"**, casi siempre es una de estas cuatro cosas. Revisa la tabla del final antes de reinstalar algo:
> 1. Abriste la carpeta `biogamevr` en vez de `04-desarrollo/BioVR`.
> 2. Unity abrió una escena vacía (*Untitled* o *SampleScene*) y falta abrir `00-EscenaInicial`.
> 3. Bajaste el repo como **ZIP** o sin **Git LFS**: los modelos llegan vacíos.
> 4. Lo abriste con **otra versión de Unity**.

## 1. Ten el repo clonado y al día

Si no lo tienes, sigue `tutorial-clonar-repo.md`. **No uses "Download ZIP"** de GitHub: el ZIP no trae los modelos ni las imágenes (van por Git LFS).

Antes de abrir Unity, actualiza:

- **GitHub Desktop:** **Fetch origin** y luego **Pull origin** (en la rama `main`).
- **Terminal** (Git Bash, dentro de la carpeta `biogamevr`):

```
git checkout main
git pull
git lfs pull
```

## 2. Instala la versión exacta de Unity

El proyecto usa **Unity 6000.3.26f1**. Con otra versión no abre bien.

1. Descarga e instala **Unity Hub**: https://unity.com/download
2. Entra con tu cuenta de Unity (la gratuita sirve).
3. En Unity Hub ve a **Installs → Install Editor → Archive**. Eso abre la página de versiones anteriores: busca **6000.3.26f1** y da clic en **Install** (se abre en el Hub).
   - Atajo: pega en el navegador `unityhub://6000.3.26f1/de8167e7440c`
4. En los módulos, para solo abrir y darle Play **no necesitas nada extra**. Si quieres generar APK, marca **Android Build Support** (con *OpenJDK* y *Android SDK & NDK Tools*).
5. Espera a que termine (unos 10–15 GB).

## 3. Agrega el proyecto correcto al Hub

1. Unity Hub → **Projects → Add → Add project from disk**.
2. Entra a tu carpeta `biogamevr` y elige **`04-desarrollo\BioVR`**. Selecciona la carpeta `BioVR`, **no** la carpeta `biogamevr`.

   ```
   biogamevr\
   └── 04-desarrollo\
       └── BioVR\        ← esta
           ├── Assets\
           ├── Packages\
           └── ProjectSettings\
   ```

3. En la lista, la columna **Editor version** debe decir **6000.3.26f1**. Si sale otra o un aviso amarillo, da clic ahí y elige 6000.3.26f1.
4. Si Unity pregunta si quieres **actualizar/cambiar de versión** el proyecto: **No**. Hacerlo cambia archivos de todo el proyecto.

## 4. Ábrelo y espera

Da clic en el proyecto. **La primera vez tarda varios minutos** (10–20 en laptops): Unity descarga paquetes e importa todo. Es normal que parezca congelado. No lo cierres.

- Si sale **"Enter Safe Mode?"**, elige **Ignore** y avísale a Desarrollo con captura de la consola (hay errores de código que corregir de nuestro lado).
- Si pregunta por el **nuevo Input System** o reiniciar el editor, acepta **Yes**.

## 5. Abre la escena del juego

Unity abre una escena vacía por defecto. Por eso "no se ve nada".

1. En la ventana **Project** (abajo) ve a `Assets → _BioVR → Escenas`.
2. Doble clic en **`00-EscenaInicial`**.
3. En la ventana **Hierarchy** (izquierda) ya deben salir el hangar, la cabina y el XR Origin.

Las siguientes veces Unity recuerda la última escena abierta.

## 6. Dale Play con el simulador

1. Da clic en **▶ Play** (arriba al centro).
2. Sin visor conectado, aparece solo el **XR Device Simulator**: en la ventana **Game** se ve un panel con los controles del teclado y el mouse.
3. Da clic dentro de la ventana **Game** para que reciba el teclado.
4. Prueba lo mismo que en el visor: el tutorial en la cabina, el menú, el botón **Iniciar** y el viaje hasta la célula.
5. Para salir, da clic otra vez en **▶ Play**. Lo que cambies durante el Play **no se guarda**.

Si el panel del simulador no aparece: **Edit → Project Settings → XR Plug-in Management → XR Interaction Toolkit** y marca **Use XR Device Simulator in scenes**.

> **Quest Link no funciona con este proyecto en la PC** (solo está configurado para Android). Para probar en el visor usa el APK.

## 7. Minijuego Expresión Genética (cuando esté en `main`)

Es una escena aparte: `Assets → _BioVR → Minijuegos → ExpresionGenetica → Escenas → ExpresionGenetica`. Se abre y se le da Play igual que en el paso 5. Si todavía no aparece, es porque aún no se sube a `main`.

## 8. No subas cambios de Unity sin avisar

Al abrir el proyecto, Unity a veces modifica archivos solo (`ProjectSettings`, escenas, `.meta`). Implementación **no sube esos cambios**:

- **GitHub Desktop:** si salen archivos de `04-desarrollo/BioVR` en *Changes*, clic derecho → **Discard changes** (con Unity cerrado).
- **Terminal:** `git checkout -- 04-desarrollo/BioVR` (con Unity cerrado).

Tus reportes siguen yendo en `05-implementacion/reportes-ux/`, como dice `tutorial-clonar-repo.md`.

## Si algo falla

| Lo que ves | Causa | Qué hacer |
| --- | --- | --- |
| La escena está vacía o dice *Untitled* | No abriste la escena del juego | Paso 5 |
| El Hub dice que la carpeta "no es un proyecto válido" o crea uno vacío | Elegiste `biogamevr` en vez de `04-desarrollo/BioVR` | Quita el proyecto del Hub y repite el paso 3 |
| Modelos **rosas** o invisibles, o errores `File ... is not a FBX file` | Faltan los archivos de Git LFS (ZIP o LFS sin instalar) | `git lfs install` y luego `git lfs pull`. Si bajaste ZIP, clona con el tutorial |
| Muchos errores rojos al abrir o te pide actualizar la API | Versión de Unity distinta | Paso 2 y elige 6000.3.26f1 en el paso 3 |
| No está el menú, el tutorial o la cabina nueva | Tu copia está vieja | Paso 1 (`git pull`) y vuelve a abrir la escena |
| Play no hace nada / la cámara no se mueve | La ventana Game no tiene el foco o falta el simulador | Clic dentro de Game; revisa el final del paso 6 |
| Errores rojos en la **Console** que no se van | Puede ser del código | Captura de la ventana **Console** a Moisés |
| Cualquier otra cosa | — | Captura de la pantalla completa a Moisés |
