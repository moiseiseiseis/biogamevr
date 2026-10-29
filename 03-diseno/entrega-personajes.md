# Entrega de personajes — School Redemption Game

29 de octubre de 2026 · Blender 5.2.2 LTS · rama `Chris_Desing`

Fuentes editables del **Explorador Celular** y **El Investigador**, junto con exportaciones estáticas, referencias, vistas de revisión, scripts e informes. El investigador incluye la revisión 03 de cara y hombros.

## Abrir los personajes

- [Explorador Celular](fuentes/explorador-celular.blend): blockout de 1,70 m, casco, visor, comunicadores, mochila de soporte vital y equipo de análisis.
- [El Investigador](fuentes/investigador.blend): personaje de 1,78 m con chaqueta, gafas, cabello, credencial, mochila y tableta. En el selector de escenas, `Investigador_Blockout` muestra la lectura; `Investigador_Pose_A` muestra los brazos ligeramente separados para inspección. Esta segunda variante requiere ajustar la pose antes del rigging.

Abre los archivos con Blender 5.2.2 LTS. Las piezas y los modificadores permanecen editables y las referencias están empaquetadas. Unidades métricas: 1 unidad = 1 m; frente hacia -Y; arriba +Z; plantas en Z = 0. Las colecciones `PRESENTATION` y `REFERENCES` contienen el estudio y las referencias, respectivamente.

## Archivos de intercambio

| Personaje / variante | FBX | GLB | Triángulos evaluados |
| --- | --- | --- | ---: |
| Explorador Celular | [FBX](exports/explorador-celular.fbx) | [GLB](exports/explorador-celular.glb) | 19.684 |
| Investigador, brazos separados | [FBX](exports/investigador.fbx) | [GLB](exports/investigador.glb) | 39.476 |
| Investigador, lectura con tableta | [FBX](exports/investigador-lectura.fbx) | [GLB](exports/investigador-lectura.glb) | 42.510 |

Las exportaciones incluyen solo el personaje y sus accesorios, con materiales de color plano y piezas separadas. Los modificadores se evalúan en copias de exportación; las fuentes conservan su edición. FBX usa Forward -Z, Up Y y transformación aplicada. GLB contiene sus datos en un único archivo.

Son modelos estáticos. Quedan pendientes rig, pesos, pose A definitiva, UV y texturas finales, animaciones, LOD y pruebas de integración y rendimiento en Unity y en el dispositivo VR. No incluyen cámaras, luces, suelo ni láminas en los archivos de intercambio.

## Bitácora y revisión visual

- [Bitácora editable en Word](documentacion/personajes/bitacora-personajes.docx)
- [Bitácora en PDF](documentacion/personajes/bitacora-personajes.pdf)
- [Bitácora en Markdown](documentacion/personajes/bitacora-personajes.md)
- [Vistas del Explorador](vistas/personajes/explorador/explorador-celular-vistas.png)
- [Comparación de cara y hombros del Investigador](vistas/personajes/investigador/investigador-comparacion-v03.png)

La bitácora registra construcción, correcciones, medidas y límites del trabajo. Su apartado «Nota final» queda disponible para completar al preparar la entrega definitiva.

## Estructura

```text
fuentes/                       Fuentes .blend editables
exports/                       FBX y GLB estáticos
scripts/personajes/            Construcción, exportación y validación
referencias/personajes/        Imágenes proporcionadas para el diseño
vistas/personajes/             Vistas generales y primeros planos
validacion/personajes/         Inventarios y comprobaciones JSON
documentacion/personajes/      Bitácora Word, PDF y Markdown
personajes-manifiesto.json     Tamaños y SHA-256 de la entrega
```

En el repositorio esta estructura se encuentra dentro de `03-diseno/`. En la copia del Escritorio es la raíz del paquete. Las referencias son las imágenes proporcionadas durante el desarrollo; su autoría y licencia no se especificaron en los archivos recibidos. No se incorporaron modelos 3D descargados de terceros.

## Reproducir y validar

Conserva la estructura de carpetas. Ejecuta los siguientes comandos desde la raíz del paquete (o desde `03-diseno/` en el repositorio), usando Blender 5.2.2 LTS. Si `blender` no está en PATH, en macOS sustituye ese nombre por `/Applications/Blender.app/Contents/MacOS/Blender`.

**Construcción:** estos comandos regeneran las fuentes y los informes; utilízalos en una copia del paquete si has editado manualmente los modelos entregados.

```bash
blender -b --factory-startup --python scripts/personajes/explorador/crear_explorador.py
blender -b --factory-startup --python scripts/personajes/investigador/crear_investigador.py
```

Añade `-- --render` al final para generar vistas adicionales. El módulo `cabeza_investigador.py` debe permanecer junto al constructor del investigador.

**Comprobación de fuentes:** abre cada archivo y ejecuta su validador. Los resultados se guardan en `validacion/personajes/`.

```bash
blender -b fuentes/explorador-celular.blend --python scripts/personajes/explorador/validar_explorador.py
blender -b fuentes/investigador.blend --python scripts/personajes/investigador/validar_investigador.py
```

**Exportación y comprobación de intercambio:**

```bash
blender -b --factory-startup --python scripts/personajes/exportar_personajes.py
blender -b --factory-startup --python scripts/personajes/validar_exportaciones.py
```

El segundo comando reimporta los seis archivos y comprueba escala, apoyo, geometría, materiales y consistencia con las fuentes. Este control en Blender no sustituye las pruebas dentro de Unity.

## Descargar desde GitHub

Las fuentes, las imágenes, los documentos binarios y las exportaciones utilizan Git LFS. Para obtener el contenido real:

```bash
git lfs install
git clone --branch Chris_Desing https://github.com/moiseiseiseis/biogamevr.git
cd biogamevr
git lfs pull
```

Si ya tienes el repositorio, actualiza tu copia y cambia a `Chris_Desing` antes de ejecutar `git lfs pull`. Los archivos pequeños que empiezan por `version https://git-lfs.github.com/spec/v1` son punteros LFS y requieren esta descarga.
