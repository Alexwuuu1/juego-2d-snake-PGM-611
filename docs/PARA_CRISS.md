# Tres cambios para Cristopher · v1.1

Cuenta: Crisshubb. Rama: feature/criss. Estos parches reemplazan el paquete anterior.
Son cambios sin commit. Revisa, aplica y prueba cada uno antes de hacer tu commit.

## Preparar

```powershell
git fetch origin
git switch feature/criss
git pull --ff-only origin feature/criss
git config --local user.name "Cristopher Iori Lazcano Gutierrez"
```

Configura tu correo verificado de Crisshubb con `git config --local user.email "TU_CORREO"`.
Autentícate con tu propia cuenta al subir. No uses los autores de Alejandro o Galilea.
Si tu rama ya tiene trabajo, consérvalo e integra develop; no hagas reset.

## Aplicar en orden

Sustituye RUTA por la carpeta donde extraíste este paquete. Para cada parche ejecuta
`git apply --check "RUTA/ARCHIVO.patch"` y luego `git apply "RUTA/ARCHIVO.patch"`.
Abre Unity 6000.3.11f1, espera la importación y prueba antes de crear el commit.

1. **01-controles-wasd.patch**: W/A/S/D además de flechas; instrucciones actualizadas.
   Prueba ambos controles, un giro por paso y que no se permita invertir el sentido.
   Mensaje: `Agregar controles WASD`.
2. **02-pantalla-completa.patch**: F11 alterna ventana/pantalla completa y recuerda la elección.
   Prueba ambas vistas, cambio de escena y cerrar/abrir; confirma que los botones se alinean.
   Mensaje: `Agregar pantalla completa`.
3. **03-restablecer-sonido.patch**: botón para volver a los valores iniciales de sonido.
   Cambia ambos volúmenes, silencia, restablece y reinicia: debe quedar 25%/32%, sonido activo.
   Mensaje: `Restablecer sonido`.

Después de probar cada cambio:

```powershell
git add Assets/Scripts README.md
git commit -m "MENSAJE_DEL_CAMBIO"
git push origin feature/criss
```

Al terminar abre un pull request feature/criss → develop. Estos tres cambios todavía
no están en main ni en el ejecutable v1.1; se integrarán después de tus pruebas y revisión.
