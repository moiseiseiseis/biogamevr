# Tutorial: clonar el repo de biogamevr

Para el equipo de Implementación · Windows · ~15 minutos

No necesitas Unity. Con el repo puedes leer la guía del visor y el plan de pruebas, y subir tus reportes de cada sesión.

## 1. Acepta la invitación

1. Entra a https://github.com/moiseiseiseis/biogamevr/invitations con tu cuenta de GitHub (o abre el correo de invitación).
2. Da clic en **Accept invitation**.

La invitación caduca en 7 días. Si ya caducó, pídele a Moisés que te la reenvíe.

## 2. Instala lo necesario

Elige **una** de las dos opciones.

**Opción A: GitHub Desktop (recomendada si nunca has usado Git)**

1. Descarga e instala https://desktop.github.com
2. Ábrelo y entra con tu cuenta de GitHub (*Sign in to GitHub.com*).

**Opción B: terminal**

1. Descarga e instala Git desde https://git-scm.com/download/win. Deja todas las opciones por defecto; ya incluye Git LFS.
2. Abre **Git Bash** y corre una sola vez:

```
git lfs install
git config --global user.name "Tu Nombre"
git config --global user.email "tu-correo@ejemplo.com"
```

Usa el mismo correo de tu cuenta de GitHub.

## 3. Clona el repo

**GitHub Desktop:** *File → Clone repository → URL*, pega `https://github.com/moiseiseiseis/biogamevr.git`, elige la carpeta donde quieres guardarlo (por ejemplo `Documentos`) y da clic en **Clone**.

**Terminal:**

```
cd ~/Documents
git clone https://github.com/moiseiseiseis/biogamevr.git
```

La primera vez tarda unos minutos porque descarga modelos e imágenes.

## 4. Lo que te toca a ti

Todo lo de Implementación está en la carpeta `05-implementacion/`:

- `guia-visor-vr.md`: cómo instalar los builds y configurar el visor
- `plan-de-pruebas.md`: qué se prueba y cómo
- `reportes-ux/`: un reporte por sesión de prueba

Los archivos `.md` se leen bien desde GitHub en el navegador o en cualquier editor de texto (por ejemplo VS Code o el Bloc de notas).

**El APK no está en el repo.** Es muy pesado para Git; Desarrollo te lo pasa por otro medio cada vez que haya un build nuevo.

## 5. Antes de empezar a trabajar, actualiza

Siempre trae lo último antes de tocar algo:

- **GitHub Desktop:** **Fetch origin** y luego **Pull origin**.
- **Terminal:** dentro de la carpeta `biogamevr`, `git checkout main` y luego `git pull`.

## 6. Subir un reporte

Nadie sube directo a `main`: se trabaja en una rama y se pide revisión con un *pull request*.

**GitHub Desktop**

1. Arriba, en **Current branch → New branch**, nómbrala `implementacion/<tarea>`, por ejemplo `implementacion/reporte-sesion-1`.
2. Guarda tu reporte en `05-implementacion/reportes-ux/`, con nombre en minúsculas, sin acentos ni espacios: `sesion-1_2026-10-10.md`.
3. Abajo a la izquierda escribe un mensaje como `Implementación: reporte de la sesión 1` y da clic en **Commit**.
4. Da clic en **Publish branch** y luego en **Create Pull Request**. Se abre GitHub en el navegador; da clic en **Create pull request**.

**Terminal**

```
git checkout main
git pull
git checkout -b implementacion/reporte-sesion-1
# ...guarda tu reporte en 05-implementacion/reportes-ux/...
git add 05-implementacion
git commit -m "Implementación: reporte de la sesión 1"
git push -u origin implementacion/reporte-sesion-1
```

Luego entra al repo en GitHub y da clic en **Compare & pull request → Create pull request**.

Si en una prueba encuentras un problema importante, además del reporte abre un *issue* en GitHub con la plantilla **Hallazgo de prueba UX**.

## Si algo falla

| Mensaje | Qué hacer |
| --- | --- |
| `Permission denied` o `403` al subir | No has aceptado la invitación (paso 1) |
| `git-lfs` no encontrado | Corre `git lfs install` o reinstala Git con las opciones por defecto |
| GitHub Desktop pide iniciar sesión otra vez | *File → Options → Accounts* y vuelve a entrar |
| Cualquier otra cosa | Mándale captura a Moisés |
