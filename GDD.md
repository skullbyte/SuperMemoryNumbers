# Super Memory Numbers · GDD

Documento de diseño del juego (*Game Design Document*). Proyecto 1.1 «Probando Interfaces» de Aplicaciones Multimedia, 2º DAM.

| | |
|---|---|
| **Autor** | Jamie Tanner |
| **Motor** | Unity 6 (6000.6.2f1), UI con uGUI + TextMeshPro |
| **Plataformas objetivo** | Android (Google Play), Linux y Web (WebGL) |
| **Estado** | v0.1 · placeholder jugable |
| **Idea original** | *Super Memory Numbers*, juego de 2016 de Manuel Sánchez Palacios |

## 1. Concepto

Es un juego de memoria visual. En el tablero aparecen unos números durante un instante y después se tapan. El jugador tiene que pulsar las casillas **en orden** (1, 2, 3…) recordando dónde estaba cada número. Cada vez hay más números, se ven menos tiempo y algunos ni siquiera se enseñan: salen como `*` y hay que deducir cuál es.

- **Género:** puzle / memoria (*casual*).
- **Público:** a partir de 7 años; partidas cortas de 1 a 3 minutos, pensadas para el móvil.
- **Gancho:** es fácil de entender en dos segundos y se pone difícil muy rápido. La partida siempre se puede mejorar.

## 2. Mecánicas

### 2.1 Ronda

1. Se colocan los números `1..N` en casillas al azar de un tablero de 3×3.
2. Se enseñan durante `tiempoVisible` (1 s con 3 números, más 0,25 s por cada número de más).
3. Se tapan **todas** las casillas, también las vacías, para que no se sepa dónde había números.
4. El jugador pulsa las casillas en orden. Si acierta la casilla se vuelve verde; si falla, pierde una vida.

### 2.2 Progresión

| Fase | Números | `*` por ronda |
|---|---|---|
| 1 | 3 → 9 (se añade uno cada 2 rondas superadas) | 0 |
| 2 | 3 → 9 | 1 |
| 3 | 3 → 9 | 2 |
| *n* | 3 → 9 | *n* − 1 (como mucho N − 1) |

- **Puntos por ronda:** `N × 10 × fase`.
- **Vidas:** 3. Al fallar se enseña dónde estaban los números y se repite la ronda con otros. Con 0 vidas termina la partida.

> **Pendiente de confirmar con el profesor:** la diapositiva dice que «tras pasar de fase uno de los números será un `*` y se seguirán añadiendo números». Lo he interpretado como que la fase cambia al superar el nivel de 9 números y se vuelve a empezar con 3. Otra lectura posible es que el `*` aparezca sin volver a 3 números. Las dos se pueden configurar desde el Inspector.

### 2.3 Power-ups

| | Nombre | Efecto | Usos |
|---|---|---|---|
| 👁️ | Ojo | Vuelve a enseñar los números que quedan por pulsar | 1 por fase |
| 💡 | Luz | Ilumina la casilla del número que toca | 1 por fase |
| ❤️ | Corazón | El próximo fallo no cuenta; esa casilla se marca con un corazón roto 💔 | 1 por fase |

## 3. Interfaz

- **Vertical (móvil):** tablero arriba; marcador, mensaje, power-ups y botón abajo.
- **Horizontal (PC / tablet):** tablero a la izquierda y controles a la derecha.
- Las casillas siempre son cuadradas y el tablero cabe entero en cualquier proporción (`RejillaCuadrada`). Se respeta el área segura de los móviles (notch, barra de gestos).
- Colores: fondo oscuro, casillas tapadas en lavanda, aciertos en verde, fallos en rojo y la pista en amarillo. Es la misma paleta que el cronómetro.
- Los iconos de los power-ups se generan por código, así que no dependen de emojis de la fuente.

**Por hacer:** animaciones al voltear las casillas y al acertar o fallar, pantalla de título, pausa, sonido y vibración.

## 4. Multiplataforma

| Plataforma | Estado |
|---|---|
| Linux | ✅ Compila y se ha probado (partida automática en vertical y en horizontal) |
| Android | ⏳ Módulo instalado; falta configurar la build (IL2CPP, ARM64, AAB) |
| Web (WebGL) | ⏳ Módulo instalado; falta probarla |

## 5. Monetización (propuesta)

- **Gratis con anuncios:** un anuncio *rewarded* (voluntario) da un power-up extra o una vida para seguir la partida. Nunca anuncios a pantalla completa en mitad de una ronda.
- **Compras dentro de la app (Unity IAP):**
  - Packs de power-ups (por ejemplo 5 ojos, 5 luces y 5 corazones).
  - «Quitar anuncios» (pago único).
  - Temas de colores para el tablero (solo estéticos).

## 6. Analítica (KPIs)

| KPI | Para qué |
|---|---|
| Retención D1 / D7 / D30 | Saber si el juego engancha |
| Duración media de la sesión y partidas por sesión | Ajustar la dificultad |
| Fase y número máximo alcanzados | Ver dónde se atasca la gente |
| Uso de cada power-up | Equilibrar los power-ups y el catálogo de compras |
| Conversión a compra y ARPDAU | Medir la monetización |
| Anuncios *rewarded* vistos por usuario | Medir la monetización |

Eventos propuestos: `partida_inicio`, `ronda_superada {fase, numeros}`, `fallo {fase, numeros}`, `powerup_usado {tipo}`, `partida_fin {puntos, fase}`.

## 7. Adquisición de usuarios

- Ficha de Google Play con capturas en vertical y un vídeo corto de una partida.
- Compartir la puntuación al terminar la partida (enlace a la tienda).
- Reto diario con el mismo tablero para todos.

## 8. Localización

Español e inglés con el paquete **Unity Localization**. Ahora mismo los textos están escritos directamente en `SuperMemory.cs` y en el constructor de la escena: hay que pasarlos a tablas de cadenas.

## 9. Subida a Google Play

- Tamaño máximo pedido: **menos de 30 MB**. La build de Linux de prueba ocupa 92 MB porque incluye el motor completo de escritorio; la de Android (AAB, con *stripping* de código) tiene que bajar de 30 MB.
- Para las pruebas cerradas se usan las cuentas de testers que da el profesor.

## 10. Estado y siguientes pasos

**Hecho (v0.1):**
- Tablero 3×3 con 9 números, rondas, progresión de 3 a 9, fases con `*`, 3 power-ups, vidas y puntos.
- Interfaz *responsive* en vertical y horizontal.
- Constructor de escena (menú **Super Memory**) y prueba automática con capturas (`-captura carpeta`).

**Siguiente:**
1. Confirmar las reglas de las fases con el profesor.
2. Animaciones y sonido.
3. Builds de Android y WebGL.
4. Localización ES/EN.
5. Analítica y monetización (Unity Gaming Services).
6. Tablero SCRUM (Trello) con el backlog de este documento.
