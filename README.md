# RutaCarritoESP32

Aplicacion WinForms para dibujar rutas y enviarlas por Bluetooth a un carrito ESP32.

## Estructura del proyecto

- `Application/Geometry`
  - `PathPlanner.cs`: suavizado, remuestreo y conversion de trazo a segmentos para el robot.
- `Domain/Models`
  - `PathPlan.cs`: resultado de planificacion (trayectoria suavizada, segmentos y distancia).
  - `RouteSegment.cs`: modelo de comando `D/A` para firmware.
- `Infrastructure/Serial`
  - `BluetoothRouteClient.cs`: conexion y envio por puerto serial.
  - `RouteCommandSerializer.cs`: serializacion de tramas `START ... END`.
- `UI/Forms`
  - `Form1.cs`: logica de interfaz y orquestacion.
  - `Form1.Designer.cs`: layout WinForms.
  - `Form1.resx`: recursos de formulario.
- `UI/Theming`
  - `FluentPalette.cs`: paleta visual basada en Fluent 2.

## Flujo funcional

1. El usuario dibuja la ruta en el lienzo.
2. La UI invoca `PathPlanner` para producir un `PathPlan`.
3. Al enviar, `BluetoothRouteClient` serializa la ruta y la transmite al ESP32.

## Compilacion

```powershell
dotnet build
```
