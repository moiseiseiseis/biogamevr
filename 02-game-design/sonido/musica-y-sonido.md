# Música y sonido

Los archivos de audio del juego no están en este repositorio, sino en la carpeta de Drive del equipo de sonido. Este documento indica dónde está esa carpeta y cómo está organizada.

**Carpeta de Drive:** [B_Sonora_BioVR](https://drive.google.com/drive/folders/1IgPd9uM3E5jYUjv1GuRE32VXOMCHSpW2?usp=sharing)

**Créditos, licencias y procedencia:** se registran en la propia carpeta. Cada sonido tiene su fila en la plantilla de banda sonora, en la columna «Decisión y descripción». El material previo se anota en la sección 9 de la misma plantilla («Inventario de material existente»). Esta información no se repite aquí.

## Documentos de referencia

Están en `00_Guia_y_Plantilla`:

| Documento | Qué es |
| --- | --- |
| `Guia_de_diseno_sonoro_Mision_Homeostasis.pdf` (Documento 1) | Explica el método: tipos de sonido, flujo de trabajo, nombres de archivo, mezcla, accesibilidad e implementación en Unity |
| `Plantilla_banda_sonora_Mision_Homeostasis.docx` (Documento 2) | Registro vivo de cada sonido: 14 secciones con filas guía por zona del juego, estado, responsable y archivo |

## Organización de la carpeta

| Carpeta | Contenido |
| --- | --- |
| `00_Guia_y_Plantilla` | Guía, plantilla, archivo de versiones y actas de decisiones |
| `01_Direccion_Sonora` | Referencias y pruebas de concepto |
| `02_Musica` | Música (MUS), una subcarpeta por zona más estados y stingers generales |
| `03_Ambientes` | Ambientes (AMB), una subcarpeta por zona |
| `04_Efectos_SFX` | Efectos (SFX), una subcarpeta por sistema: mecha, cabina, cambio de escala, instalaciones, entorno biológico y minijuego |
| `05_Interfaz_UI` | Interfaz (UI): navegación, retroalimentación, alertas e interacción en VR |
| `06_Voz` | Voz (VOZ): ASTRA, narración, personal, guiones y subtítulos |
| `07_Inventario_Existente_Por_Clasificar` | Entrada del material que ya existía, antes de asignarle un ID |
| `08_Proyectos_Fuente` | Sesiones de Ableton Live, stems y grabaciones originales; no se importan a Unity |
| `09_Listo_para_Unity` | Solo versiones aprobadas y con nombre definitivo; es lo que Desarrollo importa |
| `99_Descartado` | Piezas descartadas. Nada se borra |

Zonas del juego: `GEN` (general), `MEN` (menú), `PRO` (intro y prólogo), `HAN` (hangar), `ABO` (abordaje y cabina), `RED` (preparación y reducción), `INS` (inserción vascular), `VAS` (torrente sanguíneo), `CEL` (llegada y célula), `MIN` (minijuego del ribosoma).

## Nombres de archivo

ID: `CAT-ZONA-NN`, por ejemplo `SFX-MEC-01`. Archivo: `CAT-ZONA-NN_descriptor_vNN.wav`, por ejemplo `SFX-MEC-01_compuerta_v01.wav`. El descriptor va en minúsculas, sin acentos ni espacios y con guiones. El mismo ID aparece en la fila de la plantilla, en el nombre del archivo y en la carpeta, así que desde una escena del guion se puede llegar al archivo y viceversa.

## Estados de un sonido

Sin definir → Propuesto → En producción → En revisión → Aprobado → Integrado → Probado (o Descartado / No aplica). Un archivo pasa a `09_Listo_para_Unity` solo cuando está «Aprobado».

## Para Desarrollo

- Importar a Unity solo lo que esté en `09_Listo_para_Unity`.
- La guía propone entregar en WAV y propone grupos del mezclador (Música, Ambientes, Efectos, Interfaz y Voz), con ducking de música y ambientes cuando habla ASTRA. El detalle está en la sección 5 de la guía y en la sección 11 de la plantilla.

## Pendientes

- `09_Listo_para_Unity` está vacía: todavía no hay piezas aprobadas.
- Faltan nombres de archivo con ID y versión. Varios archivos todavía usan nombres libres, con espacios y sin `vNN`.
- La plantilla tiene abiertas varias decisiones de la sección 13: licencias de material de terceros, formato de entrega, objetivo de volumen y alcance del menú y la intro.
