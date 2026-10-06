# Snake Trío · PGM-611

Juego de Snake 2D hecho con Unity 6. Mueve la serpiente, recoge fruta y evita chocar contra los bordes o tu propio cuerpo. Una fruta suma 10 puntos; completar el tablero gana la partida.

## Equipo

| Integrante | GitHub |
| --- | --- |
| Alejandro Villalpando Rojas | Alexwuuu1 |
| Galilea Alison Llusco Asistiri | Galileya |
| Cristopher Iori Lazcano Gutierrez | Crisshubb |

Cada integrante debe revisar, comprender y explicar los cambios que presenta.

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

- `Assets/Scripts/Core`: reglas y estado de la partida.
- `Assets/Scripts/Presentation`: interfaz, controles y sonidos.
- `Assets/Editor`: preparación de escenas, validación y compilación.
- `docs`: entrega, explicación y colaboración.

No se requieren recursos artísticos ni sonoros descargados: la interfaz y los sonidos se generan en el proyecto.

## Colaboración

Cada persona debe aceptar su invitación al repositorio. Usen una rama por tarea y commits que describan cambios reales. No modifiquen el nombre del autor de commits anteriores.

El archivo `docs/PARA_CRISS.md` contiene tareas y cambios preparados para que Cristopher revise, pruebe y suba desde su propia cuenta.


## Vista del juego

![Menú](docs/capturas/Menu.png)

![Partida](docs/capturas/Juego.png)

![Resultado](docs/capturas/Resultado.png)
