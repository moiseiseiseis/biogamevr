# Entrega de personajes — School Redemption Game

30 de octubre de 2026 · Revisión del PR 44 · Blender 5.2.2 LTS · rama `Chris_Desing`

Fuentes editables del **Explorador Celular** y **El Investigador**, junto con exportaciones estáticas, vistas de revisión, scripts e informes. El investigador incluye la revisión 03 de cara y hombros.

## Abrir los personajes

- [Explorador Celular](../../fuentes/explorador-celular.blend): blockout de 1,70 m, casco, visor, comunicadores, mochila de soporte vital y equipo de análisis.
- [El Investigador](../../fuentes/investigador.blend): personaje de 1,78 m con chaqueta, gafas, cabello, credencial, mochila y tableta. En el selector de escenas, `Investigador_Blockout` muestra la lectura; `Investigador_Pose_A` muestra los brazos ligeramente separados para inspección. Esta segunda variante requiere ajustar la pose antes del rigging.

Abre los archivos con Blender 5.2.2 LTS. Las piezas y los modificadores permanecen editables. Las referencias se conservan solo en la copia local; las fuentes públicas no contienen sus imágenes. Unidades métricas: 1 unidad = 1 m; frente hacia -Y; arriba +Z; plantas en Z = 0. La colección `PRESENTATION` contiene el estudio. `REFERENCES` se conserva vacía en la entrega pública.

## Variante ligera para personajes secundarios

[Fuente editable de la variante ligera](../../fuentes/investigador-ligero.blend). Conserva las dos escenas del investigador y modificadores de reducción reversibles. La variante normal mantiene la geometría de mayor detalle.

Consulta el [informe de revisión del PR 44](revision-pr44-personajes.md) para las decisiones sobre personajes, reemplazo del Explorador, procedencia y reducción medida.

## Archivos de intercambio

| Personaje / variante | FBX | GLB | Triángulos evaluados |
| --- | --- | --- | ---: |
| Explorador Celular | [FBX](../../exports/explorador-celular.fbx) | [GLB](../../exports/explorador-celular.glb) | 19.684 |
| Investigador, brazos separados | [FBX](../../exports/investigador.fbx) | [GLB](../../exports/investigador.glb) | 39.476 |
| Investigador, lectura con tableta | [FBX](../../exports/investigador-lectura.fbx) | [GLB](../../exports/investigador-lectura.glb) | 42.510 |
| Investigador ligero, brazos separados | [FBX](../../exports/investigador-ligero.fbx) | [GLB](../../exports/investigador-ligero.glb) | 24.214 |
| Investigador ligero, lectura | [FBX](../../exports/investigador-lectura-ligero.fbx) | [GLB](../../exports/investigador-lectura-ligero.glb) | 27.060 |

Las exportaciones incluyen solo el personaje y sus accesorios, con materiales de color plano. Las variantes ligeras agrupan piezas por función en 14 o 15 mallas; las fuentes Blender conservan las piezas independientes. Los modificadores se evalúan en copias de exportación; las fuentes conservan su edición. FBX usa Forward -Z, Up Y y transformación aplicada. GLB contiene sus datos en un único archivo.

Son modelos estáticos. Quedan pendientes rig, pesos, pose A definitiva, UV y texturas finales, animaciones, LOD y pruebas de integración y rendimiento en Unity y en el dispositivo VR. No incluyen cámaras, luces, suelo ni láminas en los archivos de intercambio.

## Bitácora y revisión visual

- [Bitácora editable en Word](bitacora-personajes.docx)
- [Bitácora en PDF](bitacora-personajes.pdf)
- [Bitácora en Markdown](bitacora-personajes.md)
- [Vistas del Explorador](../../vistas/personajes/explorador/explorador-celular-vistas.png)
- [Comparación de cara y hombros del Investigador](../../vistas/personajes/investigador/investigador-comparacion-v03.png)

La bitácora registra construcción, correcciones, medidas y límites del trabajo. Su apartado «Nota final» queda disponible para completar al preparar la entrega definitiva.

## Estructura

```text
fuentes/                       Fuentes .blend editables
exports/                       FBX y GLB estáticos
scripts/personajes/            Construcción, exportación y validación
referencias/personajes/        Nota sobre referencias conservadas localmente
vistas/personajes/             Vistas generales y primeros planos
validacion/personajes/         Inventarios y comprobaciones JSON
documentacion/personajes/      Bitácora Word, PDF y Markdown
personajes-manifiesto.json     Tamaños y SHA-256 de la entrega
```

En el repositorio esta estructura se encuentra dentro de `03-diseno/`. La copia actualizada del Escritorio conserva también la carpeta `03-diseno/`, junto con `CREDITOS.md` en la raíz del paquete. Las cuatro referencias se trasladaron a la copia local y se eliminaron de la publicación, incluidas las imágenes empaquetadas. Su procedencia conocida se registra en CREDITOS.md. No se incorporaron modelos 3D descargados de terceros.

## Reproducir y validar

Conserva la estructura de carpetas. Ejecuta los siguientes comandos desde `03-diseno/`, tanto en el paquete como en el repositorio, usando Blender 5.2.2 LTS. Si `blender` no está en PATH, en macOS sustituye ese nombre por `/Applications/Blender.app/Contents/MacOS/Blender`.

**Construcción:** estos comandos regeneran las fuentes y los informes; utilízalos en una copia del paquete si has editado manualmente los modelos entregados.

```bash
blender --python-exit-code 1 -b --factory-startup --python scripts/personajes/explorador/crear_explorador.py
blender --python-exit-code 1 -b --factory-startup --python scripts/personajes/investigador/crear_investigador.py
blender --python-exit-code 1 -b --factory-startup --python scripts/personajes/optimizar_investigador.py
```

En los dos constructores, añade `-- --render` al final para generar vistas adicionales. El módulo `cabeza_investigador.py` debe permanecer junto al constructor del investigador.

**Comprobación de fuentes:** abre cada archivo y ejecuta su validador. Los resultados se guardan en `validacion/personajes/`.

```bash
blender --python-exit-code 1 -b fuentes/explorador-celular.blend --python scripts/personajes/explorador/validar_explorador.py
blender --python-exit-code 1 -b fuentes/investigador.blend --python scripts/personajes/investigador/validar_investigador.py
blender --python-exit-code 1 -b fuentes/investigador-ligero.blend --python scripts/personajes/investigador/validar_investigador.py
```

**Exportación y comprobación de intercambio:**

```bash
blender --python-exit-code 1 -b --factory-startup --python scripts/personajes/exportar_personajes.py
blender --python-exit-code 1 -b --factory-startup --python scripts/personajes/validar_exportaciones.py
```

El segundo comando reimporta los diez archivos y comprueba escala, apoyo, geometría, materiales y consistencia con las fuentes. Este control en Blender no sustituye las pruebas dentro de Unity.

## Descargar desde GitHub

Las fuentes, las imágenes, los documentos binarios y las exportaciones utilizan Git LFS. Para obtener el contenido real:

```bash
git lfs install
git clone --branch Chris_Desing https://github.com/moiseiseiseis/biogamevr.git
cd biogamevr
git lfs pull
```

Si ya tienes el repositorio, actualiza tu copia y cambia a `Chris_Desing` antes de ejecutar `git lfs pull`. Los archivos pequeños que empiezan por `version https://git-lfs.github.com/spec/v1` son punteros LFS y requieren esta descarga.
