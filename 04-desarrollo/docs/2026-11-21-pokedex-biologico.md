# Pokédex biológico: fichas cortas

Durante el viaje, el jugador apunta con el mando a una célula de la sangre o a un organelo y aprieta el gatillo. En el panel de la derecha de la cabina aparece su ficha corta y queda registrada ("Registradas: 7 de 15"). El registro se borra al volver al hangar, para que cada jugador de la feria empiece de cero.

Los textos son un **borrador de Desarrollo** resumido de las *Fichas de biología celular* de Investigación (2ª versión, 18 de noviembre). **Investigación los revisa.** Cada uno se corrige en su asset de `Assets/_BioVR/Pokedex/Fichas/` (Inspector), sin tocar código; el armador no sobrescribe un asset que ya existe.

## Dónde se encuentra cada una

- **Torrente (vasos, 3 µm):** modelos de Diego en el flujo: 30 glóbulos rojos, 6 plaquetas, 2 neutrófilos, 1 linfocito, 1 monocito. **Eosinófilo y basófilo no tienen modelo**: se usa el neutrófilo teñido (rojo anaranjado y violeta). Si Diseño hace los suyos, se cambian en `CrearPokedex.cs`.
- **Célula (citoplasma, 50 nm, y llegada):** piezas del escenario low poly: `Mitocondria` → mitocondria, `REL`/`RER` → retículo endoplasmático, `Nucleo` → membrana nuclear, `Vesicula` → vacuola, `Microtubulo`/`Actina`/`Filamento` → citoesqueleto, `Polirribosoma` → ribosoma, `Golgi` y `Lisosoma`.
- **Sin nada que apuntar:** el **nucléolo** (está dentro del núcleo y el escenario no lo muestra). Por eso el total es 15.

## Preguntas para Investigación

1. ¿Está bien que la **vesícula** del escenario cuente como **vacuola**? La ficha dice que en las células animales las vacuolas son vesículas reducidas.
2. ¿Está bien que el **núcleo** del escenario registre la **membrana nuclear**?
3. ¿Cómo mostramos el **nucléolo**? Opciones: dejarlo fuera del demo, o que al registrar el núcleo también se registre.

## Textos

| # | Nombre | Zona | Tamaño | Descripción (lo que ve el jugador) | Curiosidad |
|---|---|---|---|---|---|
| 1 | Glóbulo rojo (eritrocito) | Torrente | 6 a 8 µm | Disco flexible lleno de hemoglobina: lleva oxígeno de los pulmones a las células y regresa con dióxido de carbono. | No tiene núcleo ni mitocondrias. Un adulto tiene unos 25 billones. |
| 2 | Eosinófilo | Torrente | 12 a 17 µm | Glóbulo blanco de la inmunidad innata: combate parásitos y regula las alergias y la inflamación. | Sus gránulos se tiñen de rojo anaranjado con eosina y dañan a los parásitos. |
| 3 | Basófilo | Torrente | 10 a 14 µm | Libera histamina, que inflama y dilata los vasos en alergias y asma, y heparina, un anticoagulante natural. | Es el glóbulo blanco menos abundante. |
| 4 | Neutrófilo | Torrente | 10 a 15 µm | Primera línea de defensa: llega al sitio de la infección y fagocita bacterias y hongos. | Es el glóbulo blanco más abundante: del 50 al 70 %. |
| 5 | Linfocito | Torrente | 7 a 15 µm | Los B fabrican anticuerpos; los T destruyen células infectadas o tumorales; las NK eliminan células anómalas. | Tienen memoria inmunológica a largo plazo. |
| 6 | Plaqueta (trombocito) | Torrente | 1.5 a 4 µm | Se pega y se agrupa donde se lastima un vaso para formar un tapón y detener el sangrado. | No es una célula completa: es un fragmento de un megacariocito, sin núcleo. |
| 7 | Monocito | Torrente | 12 a 20 µm | Fagocita microbios y restos de células. Circula unas horas y pasa a los tejidos, donde se vuelve macrófago. | Es el glóbulo blanco más grande de la sangre. |
| 8 | Mitocondria | Célula | 0.5 a 1 µm de ancho | Hace la respiración celular: produce ATP, la energía que usa la célula, y ayuda a regular el calcio. | Tiene su propio ADN, una pista de su origen evolutivo. |
| 9 | Retículo endoplasmático | Célula | red de cisternas y tubos | El rugoso fabrica y transporta proteínas; el liso fabrica lípidos, desintoxica y guarda calcio. | Envía lo que fabrica al aparato de Golgi dentro de vesículas. |
| 10 | Membrana nuclear | Célula | dos bicapas con poros | Envuelve el núcleo y separa el ADN del citoplasma; sus poros controlan el paso de ARN y proteínas. | Su capa externa se continúa con el retículo endoplasmático. |
| 11 | Nucléolo | Célula | zona del núcleo sin membrana | Fabrica el ARN ribosómico y arma las subunidades de los ribosomas. | Las subunidades salen al citoplasma por los poros nucleares. |
| 12 | Vacuola | Célula | en animales, vesículas pequeñas | Guarda agua, sales, nutrientes y desechos; en las células animales, por poco tiempo. | En las plantas puede ocupar hasta el 90 % de la célula. |
| 13 | Citoesqueleto | Célula | red de filamentos | Microfilamentos, filamentos intermedios y microtúbulos: dan forma, mueven la célula y llevan vesículas. | Se arma y se desarma todo el tiempo, según lo que necesite la célula. |
| 14 | Ribosoma | Célula | dos subunidades de ARN y proteínas | Lee el ARN mensajero y une aminoácidos para fabricar proteínas. | Sus subunidades se arman en el nucléolo. |
| 15 | Aparato de Golgi | Célula | cisternas aplanadas apiladas | Recibe proteínas y lípidos del retículo, los modifica, los empaca en vesículas y los envía. | Es el centro de clasificación y envío de la célula. |
| 16 | Lisosoma | Célula | vesículas con enzimas | Sus enzimas digieren desechos, organelos dañados y microbios. | Es el sistema de limpieza y defensa de la célula. |
