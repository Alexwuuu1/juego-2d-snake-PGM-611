# Cambios preparados para Cristopher

GitHub: **Crisshubb**. Rama: **feature/criss**.

Primero acepta la invitación: https://github.com/Alexwuuu1/juego-2d-snake-PGM-611/invitations

El paquete **Para-Criss.zip** contiene dos parches. Son cambios sin commit: revísalos, aplícalos, pruébalos y luego crea tus propios commits. No se han creado commits con tu nombre.

## Preparar la rama

En tu copia del repositorio:

```powershell
git fetch origin
git switch feature/criss
git pull --ff-only origin feature/criss
git config --local user.name "Cristopher Iori Lazcano Gutierrez"
```

Configura `git config --local user.email` con un correo agregado y verificado en **tu cuenta Crisshubb**. Usa tu propio inicio de sesión de GitHub; las claves de Alejandro/Galilea de esta PC no autentican como Crisshubb.

## Commit 1: controles WASD

`01-controles-wasd.patch` agrega W/A/S/D como alternativa a las flechas y actualiza las indicaciones del menú y del README.

```powershell
git apply --check "RUTA/01-controles-wasd.patch"
git apply "RUTA/01-controles-wasd.patch"
```

Prueba W/A/S/D y las flechas en Unity. Verifica también que no se permita invertir el sentido directamente.

```powershell
git add Assets/Scripts/Core/SnakeGame.cs Assets/Scripts/Presentation/SnakeScreen.cs README.md
git commit -m "Agregar controles WASD"
git push origin feature/criss
```

## Commit 2: control de sonido

`02-control-sonido.patch` agrega un botón SONIDO en el menú y la tecla M. Conserva la preferencia al cerrar y abrir el juego. Actualiza la comprobación del menú para incluir el botón.

```powershell
git apply --check "RUTA/02-control-sonido.patch"
git apply "RUTA/02-control-sonido.patch"
```

Prueba silenciar/reactivar, cambiar de escena y reiniciar el ejecutable. Luego:

```powershell
git add Assets/Scripts/Presentation/SnakeAudio.cs Assets/Scripts/Presentation/SnakeScreen.cs Assets/Scripts/Presentation/SnakeDiagnostics.cs README.md
git commit -m "Agregar botón de silencio"
git push origin feature/criss
```

## Integrar

Crea un pull request **feature/criss → develop**. El equipo revisa y prueba los cambios; después actualiza **main** y genera una nueva versión Windows. Si Unity está abierto, espera a que termine la importación antes de probar. Nunca subas Library, Temp ni las claves SSH.

Puedes modificar los parches para proponer tu propia solución. En la exposición explica lo que realmente revisaste, cambiaste y probaste.
