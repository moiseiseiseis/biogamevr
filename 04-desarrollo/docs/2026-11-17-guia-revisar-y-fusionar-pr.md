# Guía: revisar y fusionar un PR

**Para:** Genaro (y cualquiera de Desarrollo que revise)
**Desde:** semana del 17 al 21 de noviembre de 2026

Esta semana revisamos los PR **entre dos**: Moisés y Genaro. Así la integración no depende de una sola persona. Main está protegida: nadie sube directo a main, todo entra por PR. Tú ya tienes permiso para fusionar.

---

## 0. ¿Lo reviso yo o se lo paso a Moisés?

| Lo revisas tú | Se lo pasas a Moisés |
|---|---|
| PR de un módulo de la Cocina que **no es tuyo** (M4, M5, M7, M8, M10) | **Tus propios PR** (M2 y M3): nadie fusiona lo suyo |
| Cambios dentro de `Assets/_BioVR/Minijuegos/Cocina/` y su escena `Cocina_MX` | Cambios en `ProjectSettings/`, `Packages/`, `00-EscenaInicial.unity`, `Cocina.unity` o `Comun/` |
| Documentos y modelos en `01-investigacion/`, `03-diseno/` | Cambios en M1 (`Cocina/Scripts/Nucleo/`), porque todos dependen de él |
| | Cualquier PR con conflictos (ver §5) |

---

## 1. Lee el PR en GitHub (5 minutos)

Abre el PR y revisa:

1. **Base y rama.** Arriba dice `main ← rolN/cocina-mX`. La base tiene que ser `main`.
2. **Descripción.** Qué hace y cómo se prueba. Si no lo dice, pídelo.
3. **Pestaña "Files changed".** Revisa la lista de archivos:

| Revisa | Está mal si… |
|---|---|
| Solo toca su carpeta y su escena | Toca escenas o scripts de otro módulo |
| Cada archivo nuevo de `Assets/` trae su `.meta` | Hay un `.cs`, `.prefab` o `.fbx` sin `.meta` (en Unity aparecen referencias rotas) |
| Los `.fbx`, `.blend`, `.png` y `.jpg` dicen **"Stored with Git LFS"** | Un modelo o imagen aparece como archivo normal de muchos MB |
| No sube basura de Unity | Aparecen `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Builds/`, `*.apk` o `_Recovery/` |
| Nombres sin espacios ni acentos | `New Folder`, `Copia de…`, `Material (1)` |
| Los textos y números van en `.asset` (ScriptableObjects) | Mensajes del juego escritos dentro del `.cs` |
| No cambia cosas que no explica | `ProjectSettings.asset`, `QualitySettings.asset` o `manifest.json` modificados sin decir por qué |

4. **El código.** No tienes que entender cada línea, pero sí:
   - Que haya comentarios que expliquen qué hace cada clase.
   - Que no haya `Debug.Log` en cada cuadro (en `Update`), porque en el Quest baja los fps.
   - Que no use `Input.GetKey`, `Input.mousePosition` ni `OnMouseDown`: el proyecto usa el **Input System nuevo** y XRI.
   - Que no haga `GameObject.Find` ni `GetComponent` dentro de `Update`.

---

## 2. Pruébalo en Unity

### 2.1 Bajar la rama

**Antes:** guarda y haz commit de tu propio trabajo, o Unity se puede confundir al cambiar de rama. **Cierra Unity.**

En la terminal (Git Bash), dentro de la carpeta del repo:

```bash
git fetch origin
git switch rolN/cocina-mX      # el nombre de la rama del PR
git lfs pull                    # baja los modelos y texturas
```

> Si prefieres GitHub Desktop: *Current branch* → busca la rama → *Fetch origin*. GitHub Desktop baja LFS solo.

### 2.2 Abrir y probar

1. Abre el proyecto `04-desarrollo/BioVR` con **Unity 6000.3.26f1** (no otra versión).
2. Espera a que termine de importar. **La consola no debe tener errores rojos.** Si los tiene, el PR no pasa: copia el error en el comentario.
3. Abre su escena de prueba: `Assets/_BioVR/Escenas/Pruebas/Cocina_MX.unity`.
4. Dale **Play** y pruébalo con el XR Device Simulator (o en el visor si te toca probar el APK).
5. Recorre la lista **"Hecho cuando"** de ese módulo en el documento `Desarrollo — Especificación de la Cocina por módulo (17 nov)`. Cada punto: ✅ funciona, ❌ no funciona (con qué pasó) o ➖ no se pudo probar.
6. **No guardes cambios** en su escena. Si Unity te pregunta al cerrar, di que **no**.

### 2.3 Volver a tu rama

Cierra Unity y:

```bash
git status                  # no debe haber cambios; si los hay, son de Unity al abrir
git restore .               # descarta esos cambios (solo si no son tuyos)
git switch rol2/cocina-m2-m3
```

---

## 3. Comenta

En el PR, pestaña **"Files changed"** → botón **"Review changes"** (arriba a la derecha):

- **Approve:** todo bien. Pasa a §4.
- **Request changes:** algo no funciona o no cumple la lista. El autor lo corrige y te vuelve a avisar.
- **Comment:** dudas o sugerencias que no bloquean.

Para comentar una línea específica: en "Files changed", pasa el mouse sobre la línea y da clic en el **+** azul.

**Plantilla de comentario:**

```
Probé en Cocina_MX con el simulador.

✅ La corriente trae el ARNt que pide el codón
✅ Máximo 9 ARNt
❌ La pista no brilla a los 9 s (sigue apagada a los 20 s)
➖ No probé en el visor

Falta: arreglar la pista. Lo demás bien.
```

Sé concreto: qué hiciste, qué esperabas y qué pasó. Nada de "no sirve".

---

## 4. Fusiona

Solo si lo **aprobaste** y GitHub dice **"This branch has no conflicts with the base branch"**:

1. Botón verde **"Merge pull request"** → **"Confirm merge"**. Usa siempre **"Create a merge commit"** (la opción normal), no *Squash* ni *Rebase*, salvo que Moisés diga otra cosa.
2. Botón **"Delete branch"**, que aparece justo después.
3. Avisa en el grupo: *"Fusioné el PR #NN (M4 ribosomas). Hagan `git pull` en main antes de seguir."*

Si después de fusionar algo se rompe: en el PR fusionado hay un botón **"Revert"** que crea un PR que deshace el cambio. Avísale a Moisés antes de usarlo.

---

## 5. Si hay conflictos

Si GitHub dice **"This branch has conflicts that must be resolved"**:

- **No los resuelvas en la página de GitHub** si son archivos `.unity`, `.prefab`, `.asset` o `.meta`: el editor web los rompe.
- Pídele al autor que actualice su rama desde main:

```bash
git switch rolN/cocina-mX
git fetch origin
git merge origin/main
# si hay conflictos en archivos de Unity, avisar a Moisés antes de seguir
git push
```

- Si el conflicto es en una escena o prefab compartido, pásaselo a Moisés.

---

## 6. Lista corta (cópiala en tu comentario)

```
[ ] Base: main
[ ] Solo su carpeta y su escena
[ ] .meta de cada archivo nuevo
[ ] Modelos e imágenes en LFS
[ ] Sin Library/Temp/Builds/_Recovery
[ ] Sin errores en la consola de Unity
[ ] Probé su escena Cocina_MX
[ ] "Hecho cuando" de su módulo: ✅ / ❌ / ➖
[ ] Sin conflictos
```
