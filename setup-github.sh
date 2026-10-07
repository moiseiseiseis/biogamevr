#!/usr/bin/env bash
# Crea etiquetas, el milestone de la Entrega 1 y sus issues. Uso: ./setup-github.sh (requiere gh autenticado). Ejecútalo una sola vez.
R=moiseiseiseis/biogamevr
mk(){ gh label create "$1" --color "$2" --description "$3" --repo $R --force; }
mk "equipo:investigacion"  1D76DB "Equipo de Investigación"
mk "equipo:game-design"    5319E7 "Equipo de Game Design"
mk "equipo:diseno"         D93F0B "Equipo de Diseño"
mk "equipo:desarrollo"     0E8A16 "Equipo de Desarrollo"
mk "equipo:implementacion" FBCA04 "Equipo de Implementación"
mk "equipo:management"     B60205 "Equipo de Management"
mk "minijuego:atp"           C5DEF5 "Energía y metabolismo"
mk "minijuego:traduccion"    C5DEF5 "Síntesis de proteínas"
mk "minijuego:adn"           C5DEF5 "Información genética"
mk "minijuego:citoesqueleto" C5DEF5 "Estructura celular"
mk "minijuego:mitosis"       C5DEF5 "Reproducción y regeneración"
mk "estructura"          0052CC "Estructura biológica"
mk "ux"                  F9D0C4 "Experiencia de usuario"
mk "decision-pendiente"  E99695 "Requiere decisión del equipo"

# ---- Milestone e issues de la Entrega 1 ----
M="Entrega 1 — Escena inicial"
gh api repos/$R/milestones -f title="$M" -f due_on="2026-10-12T18:00:00Z" \
  -f description="Escena inicial jugable en VR: hangar → abordaje → viaje → llegada" >/dev/null || true
is(){ gh issue create --repo $R --title "$1" --label "$2" --milestone "$M" --body "$3"; }
is "Documento: submarino y reducción de escala (jue 8)" "equipo:investigacion" "Completar \`01-investigacion/submarino-y-escala.md\`. Versión mínima el jueves."
is "Descripción visual del espacio celular de llegada (jue 8)" "equipo:investigacion" "Estructuras visibles, tamaños y colores reales."
is "Storyboard de la escena inicial — 4 momentos (vie 9)" "equipo:game-design" "Hangar, abordaje, viaje, llegada. En \`02-game-design/storyboards/escena-inicial/\`."
is "Sketches exterior y cabina del submarino (vie 9)" "equipo:game-design" "Incluir qué paneles/botones son interactivos."
is "Guía de estilo: color, animación y música (vie 9)" "equipo:game-design" "\`02-game-design/guia-de-estilo.md\`. Consultar con Diseño."
is "Modelo 3D: cabina interior (prelim vie 9, final sáb 10)" "equipo:diseno" "Alto detalle. Ver pipeline en \`03-diseno/README.md\`."
is "Modelo 3D: exterior del submarino (prelim vie 9, final sáb 10)" "equipo:diseno" "Bajo detalle."
is "Modelo 3D: escenario celular básico (prelim vie 9, final sáb 10)" "equipo:diseno" ""
is "Proyecto Unity con OpenXR + XRI y cabina provisional" "equipo:desarrollo" "Ver \`04-desarrollo/README.md\`."
is "Secuencia de la escena y transición de escala" "equipo:desarrollo" "Hangar → abordaje → viaje → llegada."
is "Interacción con paneles y botones de la cabina" "equipo:desarrollo" ""
is "Configurar visor para Unity y probar escena de ejemplo" "equipo:implementacion" "Modo desarrollador + build directo. Documentar en \`05-implementacion/guia-visor-vr.md\`."
is "Prueba del build del sábado y reporte" "equipo:implementacion" "Usar el checklist de \`05-implementacion/plan-de-pruebas.md\`."
is "Confirmar integrantes, canal de archivos y versiones de Unity/Blender" "equipo:management" "Llenar \`06-management/equipos.md\`."
is "Resumen para el coordinador (dom 11)" "equipo:management" ""
