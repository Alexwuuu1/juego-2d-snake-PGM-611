# Cómo se siguió la guía 2D

El PDF muestra un juego de plataformas para explicar Unity. La pizarra asigna Snake al grupo de tres. Se conserva la organización y los componentes aprendidos, adaptando las reglas al Snake.

| Concepto de la guía | Aplicación en Snake |
| --- | --- |
| Sprites en Assets | PNG pixel art originales en Assets/Sprites. Importación Sprite (2D and UI), Point, 32 PPU. |
| Piso con Grid y Tilemap | Suelo de tablero con dos baldosas alternadas. No tiene colisión: se camina sobre él. |
| Rigidbody2D del personaje | Cabeza con Rigidbody2D cinemático, gravedad 0 y Freeze Rotation. Snake se mueve por celdas, no salta. |
| Collider2D | Cabeza, cuerpo y bordes con BoxCollider2D. Comida con collider de tipo Trigger. |
| Physics Material 2D | Material SinFriccion en los prefabs. |
| Scripts C# | SnakeModel, SnakeGame, SnakeContact2D, SnakeScreen y SnakeAudio. |
| Prefabs | Cabeza, Segmento, Comida y Pared. Se instancian al jugar y crecer. |
| Animator y animaciones | Parpadeo de cabeza y brillo de fruta con clips y controllers. |
| Recolección y contador | Al comer: 10 puntos, una fruta más y un segmento nuevo. |
| Colisión dañina | Physics2D consulta la celda de destino y detecta pared/cuerpo antes del avance. |
| AudioSource y efectos | Música de fondo y efectos de botón, comida, golpe y victoria. |
| Canvas y botones | Menú, selección de ritmo, marcador, pausa y resultado. |
| Menú y cambios de escena | Menu → Juego → Resultado; reiniciar o volver al menú. |
| Exportar Windows | Compilación mediante BuildPipeline, distribuida en una carpeta completa. |

## Escenas y componentes visibles en Unity

`Menu` y `Resultado`: Main Camera e Interfaz. El Canvas se construye en C# para adaptar los textos, botones y puntuación.

`Juego`: Main Camera, Interfaz, Controlador Snake, Grid/Suelo Tilemap y cuatro bordes. Al pulsar Play aparecen Cabeza, Segmentos y Comida, instanciados desde los prefabs.

El modelo lógico impide el giro inverso, controla la cola que se libera y coloca fruta solamente en celdas vacías. Los colliders participan en la detección de choques. Hay validación automatizada para comprobar que ambos sistemas se comportan correctamente.

## Validación reproducible

1. En Unity: Snake Trío → Validar reglas.
2. Compilar Windows desde Snake Trío → Compilar Windows.
3. Ejecutar `SnakeTrio.exe --smoke-test -logFile prueba.log`.
4. Comprobar los mensajes RULES_OK, SMOKE_GAME_OK y SMOKE_RESULT_OK en sus registros correspondientes.

No se requiere Android: la pizarra pide un ejecutable funcional.
