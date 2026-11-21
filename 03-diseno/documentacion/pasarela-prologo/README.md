# Pasarela de modelos del prólogo

Durante el prólogo (13 audios de Emi, unos 4 minutos) el jugador se queda quieto y los modelos pasan frente a él, uno por audio, como en una pasarela. Cada modelo entra por un lado, se detiene al centro, gira despacio mientras suena su audio y sale cuando empieza el siguiente. Hay como máximo 2 modelos a la vez.

La ficha de cada modelo, con render, cita de la narración, datos y qué hace en la pasarela, está en [enciclopedia-pasarela-cellverse.pdf](enciclopedia-pasarela-cellverse.pdf). Los renders están en `renders/` y sus datos (triángulos y medidas) en `datos-renders.json`.

## Los 13 modelos

| № | Modelo | Audio | Archivo | Estado |
|---|---|---|---|---|
| 01 | La Tierra | VOZ-NAR-01_prologo-01 (21.9 s) | `exports/pasarela-tierra.fbx` | **Nuevo** en este PR |
| 02 | La Tierra seca | prologo-02 (33.1 s) | `exports/pasarela-tierra-seca.fbx` | **Nuevo** en este PR |
| 03 | Ciudad bajo el domo | prologo-03 (16.9 s) | `exports/pasarela-ciudad-domo.fbx` | **Nuevo** en este PR |
| 04 | Silueta humana (holograma) | prologo-04 (12.9 s) | `exports/paciente.fbx` | Ya en el repo; en Unity lleva un material de holograma cian (emisivo, transparente, Fresnel) |
| 05 | Logo «Proyecto Cellverse» | prologo-05 (30.5 s) | `exports/pasarela-logo-cellverse.fbx` | **Nuevo** en este PR |
| 06 | Laboratorio | prologo-06 (11.3 s) | `exports/minijuego-cocina/props/proyector-holograma.fbx` sobre `mesa-curva.fbx` | Ya en el repo; el proyector va sobre la mesa a 0.78 m |
| 07 | La célula | prologo-07 (30.2 s) | `exports/escenario-celular_lowpoly.fbx` + `exports/texturas/escenario-celular-lowpoly/` | Ya en el repo |
| 08 | Pantalla de alerta | prologo-08 (8.8 s) | `exports/minijuego-cocina/props/tablero-pedido.fbx` | Ya en el repo; en Unity se le pone encima una pantalla roja emisiva con el triángulo de alerta |
| 09 | Grumo de amilina | prologo-09 (26.7 s) | `exports/amilina_modelo.fbx` (7 copias) | Ya en el repo |
| 10 | Ribosoma 80S | prologo-10 (11.7 s) | `exports/ribosoma_80S_4ug0.fbx` | Ya en el repo |
| 11 | El Mecha Seraphim | prologo-11 (22.1 s) | `exports/seraphim-mecha.fbx` | **Nuevo** en este PR (Mecha final, decisión del 21 nov 2026) |
| 12 | Eritrocitos | prologo-12 (12.7 s) | `exports/eritrocito.fbx` (3 copias) | Ya en el repo |
| 13 | El paciente | prologo-13 (7.3 s) | `exports/paciente-en-camilla_lowpoly.fbx` | Ya en el repo |

Versión corta para la Feria: entradas 01, 05, 09 y 11.

## Modelos nuevos

| Modelo | Fuente | Export | Triángulos | Tamaño | Texturas |
|---|---|---|---|---|---|
| La Tierra | `fuentes/pasarela-tierra.blend` | `.fbx` | 3,744 | esfera de 1 m | `texturas/pasarela-prologo/tierra_dia_2k.jpg`, `tierra_noche_emision_2k.jpg` |
| La Tierra seca | `fuentes/pasarela-tierra-seca.blend` | `.fbx` | 3,744 | esfera de 1 m | `tierra_seca_dia_2k.jpg`, `tierra_seca_noche_emision_2k.jpg` |
| Ciudad bajo el domo | `fuentes/pasarela-ciudad-domo.blend` | `.fbx` | 22,226 | 1.00 × 0.99 × 0.75 m | `ciudad_domo_basecolor.png` |
| Logo Cellverse | `fuentes/pasarela-logo-cellverse.blend` | `.fbx` | 1,564 | 1.20 × 0.09 × 0.84 m | ninguna (materiales planos y emisivos) |
| Mecha Seraphim | `fuentes/seraphim-mecha.blend` | `.fbx` | 18,416 | 14.6 m de largo | ninguna |

- Las texturas también van empaquetadas dentro de cada `.blend`.
- La noche de la Tierra es un mapa de emisión: en Unity va en *Emission* para que las ciudades brillen en el lado oscuro.
- La Tierra seca está pensada para aparecer con un fundido desde la Tierra (cambio de textura), no como un modelo que entra.
- **Seraphim:** trae la animación de despliegue de 6 brazos y 2 piernas (cuadros 1 a 60). El FBX sale **sin Apply Transform** porque con esa opción se rompía la animación; en Unity usar *Bake Axis Conversion*.

## Notas para Desarrollo

- Escala: los modelos 07, 09, 10 y 12 están en escala de juego, no en metros. En la pasarela conviene escalarlos para que todos se vean de un tamaño parecido (alrededor de 1 a 1.5 m frente al jugador).
- Límite de 30k triángulos por modelo: la silueta (04, `paciente.fbx`, 77,817) y la célula (07, 55,328) lo pasan. Como en la pasarela solo hay 2 modelos a la vez, se puede probar tal cual en el Quest antes de decidir aligerarlos.
