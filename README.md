# Snake Trío · PGM-611

Juego de Snake 2D hecho con Unity 6. Mueve la serpiente, recoge fruta y evita chocar contra los bordes o tu propio cuerpo. Una fruta suma 10 puntos; completar el tablero gana la partida.

## Equipo

| Integrante | GitHub |
| --- | --- |
| Alejandro Villalpando Rojas | Alexwuuu1 |
| Galilea Alison Llusco Asistiri | Galileya |
| Cristopher Iori Lazcano Gutierrez | Crisshubb |

Cada integrante revisa y explica los cambios que presenta. El proyecto conserva las ramas de trabajo y el historial de integración del equipo.

## Requisitos de la entrega

- Snake para un grupo de tres integrantes.
- Sonidos de comida, choque y botones, más música de fondo.
- Colisiones contra paredes y contra la serpiente.
- Marcador, récord local y final de partida.
- Tres escenas: Menu, Juego y Resultado.
- Ejecutable Windows y repositorio GitHub.
- Exposición breve del funcionamiento y aportes del equipo.

## Ejecutar y editar

Abre el proyecto en Unity **6000.3.11f1**. Usa **Snake Trío → Preparar escenas** y abre `Assets/Scenes/Menu.unity`. Pulsa Play.

Para compilar: **Snake Trío → Compilar Windows**. El ejecutable se genera en `Builds/Windows/SnakeTrio.exe`; distribuye la carpeta completa, incluyendo `SnakeTrio_Data` y las DLL. La compilación descargable se adjunta al repositorio como Release.

## Controles

- Flechas: cambiar dirección.
- Esc o P: pausar y continuar.
- Enter: empezar desde el menú o volver a jugar desde el resultado.
- R: reiniciar desde el resultado.
- F12: guardar una captura junto al ejecutable.

La serpiente no puede girar 180 grados directamente. Solo se procesa un giro por avance, para evitar colisiones causadas por pulsaciones rápidas.

## Estructura

- `Assets/Sprites`: imágenes PNG originales de serpiente, comida y escenario.
- `Assets/Prefabs`: cabeza con Rigidbody2D, segmentos, comida con trigger y paredes con Collider2D.
- `Assets/Animations`: animaciones y controladores del parpadeo y brillo de la fruta.
- `Assets/Tiles`: baldosas del suelo, usadas por el Grid y Tilemap de la escena Juego.
- `Assets/Materials`: material físico sin fricción.
- `Assets/Scripts/Core`: reglas, consultas Physics2D y estado de la partida.
- `Assets/Scripts/Presentation`: interfaz, controles y sonidos.
- `Assets/Editor`: preparación de escenas, validación y compilación.
- `docs`: entrega, explicación y colaboración.

Los sprites PNG se importan como Sprite (2D and UI), con filtro Point y 32 píxeles por unidad. La cabeza utiliza Rigidbody2D cinemático sin gravedad y rotación bloqueada. El movimiento se programa por celdas en C# y las consultas Physics2D contra los Collider2D detectan choques. No se necesita gravedad en Snake. Los sonidos se sintetizan en C#.

## Colaboración

Cada persona debe aceptar su invitación al repositorio. Las ramas de trabajo son `feature/alejandro`, `feature/galilea` y `feature/criss`. Se integran por pull request a `develop`; después de probar, se pasa `develop` a `main`. `main` contiene la entrega. No modifiquen el autor de commits anteriores.

El archivo `docs/PARA_CRISS.md` contiene tareas y cambios preparados para que Cristopher revise, pruebe y suba desde su propia cuenta.


## Vista del juego

![Menú](docs/capturas/Menu.png)

![Partida](docs/capturas/Juego.png)

![Resultado](docs/capturas/Resultado.png)
