# Cómo contribuir

## Primera vez
```bash
git lfs install
git clone https://github.com/moiseiseiseis/biogamevr.git
cd biogamevr
```

## Cada tarea
```bash
git checkout main && git pull
git checkout -b <equipo>/<tarea>      # ej. diseno/ribosoma
# ...trabaja...
git add . && git commit -m "Diseño: modelo base del ribosoma"
git push -u origin <equipo>/<tarea>
```
Luego abre un Pull Request en GitHub y enlaza el issue (`Closes #12`).

## Convención de commits
`<Equipo>: <qué hiciste>` — ej. `Investigación: ficha de ATP sintasa`, `Dev: input del mecha`.

## Nombres de archivo
minúsculas, sin acentos ni espacios: `atp-sintasa_v2.blend`, `storyboard-minijuego-adn.png`.
