# Desarrollo
Programar objetos, funciones y entidades a partir de Investigación y Diseño.

## Crear el proyecto (una sola vez)
1. Unity Hub → New project → plantilla **VR Core** (o 3D URP) con **OpenXR + XR Interaction Toolkit**.
2. Ubicación: esta carpeta; nombre del proyecto: `BioVR`.
3. Edit → Project Settings → Editor: *Version Control* = **Visible Meta Files**, *Asset Serialization* = **Force Text**.
4. Crea la estructura:

```
BioVR/Assets/
├── Minijuegos/
│   ├── ATP/
│   ├── Traduccion/
│   ├── ADN/
│   ├── Citoesqueleto/
│   └── Mitosis/
├── Mecha/          # submarino: cabina, paneles, control de escala
├── Pokedex/        # datos (ScriptableObjects) a partir de las fichas de Investigación
├── Modelos/        # importados desde 03-diseno/exports
├── Escenas/
│   └── 00-EscenaInicial.unity   # hangar → abordaje → viaje → llegada
└── Comun/          # scripts compartidos, UI, audio
```

Todos deben usar **la misma versión de Unity**; anótala aquí cuando se elija:
- Versión de Unity: _por definir_

## Primera entrega (sáb 10 oct)
- [ ] Proyecto con OpenXR + XR Interaction Toolkit, cámara en primera persona en la cabina (cubos provisionales)
- [ ] Secuencia: hangar → abordaje → viaje (transición de escala) → llegada a espacio celular
- [ ] Interacción con paneles y botones de la cabina
- [ ] Sustituir provisionales por los FBX de `03-diseno/exports/` (copiar a `Assets/Modelos/`)
- [ ] Build que corra en el visor → avisar a Implementación

Una persona edita `00-EscenaInicial.unity` a la vez (avísenlo en el chat); el resto trabaja en prefabs dentro de `Mecha/` y `Comun/` para no pisarse.
