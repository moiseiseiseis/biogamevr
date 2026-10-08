# Desarrollo
Programar objetos, funciones y entidades a partir de Investigación y Diseño.

## Versión y paquetes (iguales para todos)

| Qué | Versión |
| --- | --- |
| **Unity** | **6000.3.26f1** (Unity 6.3 LTS) |
| Módulos de Unity Hub | Android Build Support + OpenJDK + Android SDK & NDK Tools |
| Render pipeline | URP |
| XR Interaction Toolkit | 3.6.1 (samples: *Starter Assets* y *XR Device Simulator*) |
| OpenXR | 1.18.0 (perfil *Oculus Touch*; *Meta Quest Support* en Android) |

> Si Unity Hub te ofrece "actualizar" el proyecto a otra versión, **di que no**. Abrirlo con otra versión cambia archivos de todo el proyecto y genera conflictos para los demás.

## Abrir el proyecto (cada integrante, una sola vez)
1. En Unity Hub → *Installs → Install Editor → Archive* instala **6000.3.26f1** con los módulos de Android de la tabla.
2. Clona el repo (ver `GUIA-EQUIPOS` / `CONTRIBUTING.md`).
3. Unity Hub → *Projects → Add → Add project from disk* → elige `04-desarrollo/BioVR`.
4. La primera vez tarda varios minutos (genera `Library/`, que no se sube).
5. Abre `Assets/_BioVR/Escenas/00-EscenaInicial.unity` y dale *Play*. Sin visor, activa el simulador: *Edit → Project Settings → XR Interaction Toolkit → Use XR Device Simulator in scenes*.

## Estructura de `BioVR/Assets`

Una carpeta por rol: **cada quien trabaja solo dentro de la suya** y así nadie pisa el trabajo de otro.

```
Assets/
├── _BioVR/                     ← todo lo del equipo
│   ├── Escenas/
│   │   ├── 00-EscenaInicial.unity   rol 1 (solo él ensambla)
│   │   └── Pruebas/            una escena por persona: <nombre>.unity
│   ├── Jugador/                rol 2 · XR Origin, controles, comodidad
│   ├── Cabina/                 rol 3 · botones, palancas, pantallas
│   ├── Secuencia/              rol 4 · GameManager, estados, fundidos
│   ├── Escala/                 rol 5 · reducción, partículas, post-procesado
│   ├── Entornos/               rol 6 · Hangar/, EspacioCelular/
│   ├── Modelos/                rol 6 · FBX copiados de 03-diseno/exports
│   ├── Comun/                  compartido · Audio/, Materiales/, UI/, Utilidades/
│   └── Minijuegos/             después de la primera entrega
├── Samples/                    XR Interaction Toolkit (importado, no editar)
├── Settings/                   URP (no editar sin avisar al rol 1)
├── XR/                         OpenXR (autogenerado)
└── XRI/                        ajustes de XR Interaction Toolkit y del simulador (autogenerado)
```

Dentro de cada carpeta de rol crea solo las subcarpetas que uses: `Scripts/`, `Prefabs/`, `Materiales/`.

| Rol | Dueño de | Puede tocar también |
| --- | --- | --- |
| 1. Líder técnico e integración | `Escenas/00-EscenaInicial.unity`, `Settings/`, `XR/`, `ProjectSettings/`, `Packages/` | Todo, solo para integrar |
| 2. Rig VR y cámara | `Jugador/` | Su escena en `Pruebas/` |
| 3. Interacción de cabina | `Cabina/` | Su escena en `Pruebas/` |
| 4. Secuencia y flujo | `Secuencia/` | Su escena en `Pruebas/` |
| 5. Escala y efectos | `Escala/` | Su escena en `Pruebas/` |
| 6. Entornos, assets y build | `Entornos/`, `Modelos/` | Su escena en `Pruebas/` |

**Reglas**
- Trabajas y pruebas en **tu escena de `Pruebas/`** con tus prefabs. Cuando algo funciona, el rol 1 lo coloca en `00-EscenaInicial.unity`.
- Lo que se use en la escena principal debe ser un **prefab** dentro de tu carpeta.
- Si necesitas cambiar algo de `Comun/`, `Settings/`, `Packages/` o `ProjectSettings/`, avisa antes en el chat.
- Siempre sube los `.meta` junto con su archivo.
- Namespaces de C#: `BioVR.Jugador`, `BioVR.Cabina`, `BioVR.Secuencia`, `BioVR.Escala`, `BioVR.Entornos`, `BioVR.Comun`.

## Primera entrega (sáb 10 oct)
- [x] Proyecto con OpenXR + XR Interaction Toolkit y escena base con XR Origin
- [ ] Jugador sentado en la cabina (cubos provisionales)
- [ ] Secuencia: hangar → abordaje → viaje (transición de escala) → llegada a espacio celular
- [ ] Interacción con paneles y botones de la cabina
- [ ] Sustituir provisionales por los FBX de `03-diseno/exports/` (copiar a `Assets/_BioVR/Modelos/`)
- [ ] Build que corra en el visor → avisar a Implementación
