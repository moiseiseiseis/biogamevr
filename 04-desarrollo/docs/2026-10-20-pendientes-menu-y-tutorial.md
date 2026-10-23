# Pendientes equipo desarrollo — Menú y tutorial

**Fecha:** 20 de octubre de 2026
**Fecha de entrega:** _[por definir]_

El menú de pausa y el tutorial ya están en `main` (PRs #35 y #36). Ahora toca probarlos y mejorarlos. Cada quien tiene una parte distinta según su rol, para que no se pisen entre ustedes y cada uno sepa qué entregar y cuándo terminó.

---

## Reglas para todos

- Antes de empezar: `git checkout main` y `git pull`. Se trabaja en tu propia rama y se manda PR, **nunca directo a main**.
- **Nadie guarda cambios en `00-EscenaInicial.unity`.** Si dos personas la editan, sus cambios chocan al fusionar los PRs y es muy difícil arreglarlo. Hagan los cambios en scripts y prefabs; si algo necesita la escena, avísenle a Moisés y él lo mete.
- En el PR expliquen **qué cambiaron y por qué**: qué vieron en la prueba que los llevó a ese cambio.

---

## Genaro (rol 2, jugador y VR): Menú de pausa

**Prueba**
- Abre el menú sentado, de pie, mirando hacia los lados y hacia atrás, y dentro y fuera de la cabina.
- Anota si el panel queda tapado por la cabina, si está muy cerca o muy lejos, si se lee bien y si es fácil apuntar a los botones.

**Mejora**
1. Ajusta la distancia y el tamaño del panel y de la letra para que se lea sin esfuerzo. En `MenuPausa.cs` la distancia está en el campo `distancia` (0.6 m).
2. Agrega una confirmación ("¿Seguro?") a **Reiniciar** y **Salir**, para que nadie se salga por accidente.
3. Revisa que los botones de altura del asiento (±5 cm) se entiendan.

**Entrega:** rama `rol2/mejoras-menu` con un PR.

---

## Iker (rol 3, controles de cabina): Que el tutorial señale qué tocar

**Prueba**
- Sigue el tutorial completo y fíjate en qué pasos no queda claro *dónde* está lo que hay que tocar (Iniciar inmersión, palanca, Salto Matrioska, escotilla).

**Mejora**
- Cuando un paso pida tocar un control, ese control debe resaltarse (parpadeo, contorno o flecha encima) y dejar de resaltarse cuando el paso se complete.
- Puedes agregar un campo nuevo al paso del tutorial (`PasoTutorial.cs`) que diga qué objeto resaltar.

**Entrega:** rama `rol3/tutorial-resaltado` con un PR. Si cambias `Tutorial.cs`, avísale antes a Moisés.

---

## Marcos (rol 4, tiene el Quest 2): Prueba en el visor con gente nueva

**Prueba con personas**
- Pon el APK a por lo menos 3 personas que nunca hayan visto el juego. **No les expliques nada.**
- Anota cuánto tardan en total, en qué paso se atoran o preguntan algo y si alguien se marea.

**Intenta romperlo**
- Abre el menú durante el viaje y durante un salto.
- Dale a "Saltar tutorial" y luego a Reiniciar.
- Adelántate a lo que pide el tutorial.
- Sal por la escotilla en distintos momentos.

**Entrega**
- Una tabla con esta forma:

  | Persona | Paso donde se atoró | Qué pasó |
  |---------|---------------------|----------|
  |         |                     |          |

- Un issue en GitHub por cada error que encuentres.

---

## Ascencio (rol 5, sin PC): Textos del tutorial

**Mejora:** reescribe los 11 pasos del tutorial con estas reglas:
- Máximo unas 15 palabras por instrucción.
- Nombres de botones tal como aparecen en el mando (B/Y, stick izquierdo, ☰).
- Tono de piloto de la misión.

Propón también 3 o 4 variantes de "¡Bien!" (el mensaje que sale al completar un paso).

| # | Título actual | Qué pide hacer |
|---|---------------|----------------|
| 1 | ¡Bienvenido, piloto! | Botón Siguiente |
| 2 | Vuelve a tu asiento | B o Y |
| 3 | Camina | Stick izquierdo |
| 4 | Gira | Stick derecho |
| 5 | Menú de pausa | Botón ☰ del mando izquierdo |
| 6 | Ajusta tu altura | Subir / Bajar en el menú (opcional) |
| 7 | Mira la nave por fuera | Tocar la escotilla de atrás (opcional) |
| 8 | Aborda la nave | Iniciar inmersión |
| 9 | Sumérgete | Iniciar inmersión otra vez |
| 10 | Salto Matrioska | Salto Matrioska o la palanca |
| 11 | ¡Listo! | Seguir saltando hasta la célula |

**Entrega:** un documento o mensaje con la tabla título / instrucción. Moisés los pasa al juego.
