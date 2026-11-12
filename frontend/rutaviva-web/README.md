# Portal RutaViva

Cliente y taller del mantenimiento preventivo. El cliente registra vehículos y consulta las rutinas. El taller configura las rutinas y trabaja las órdenes. No hay autenticación: el selector Cliente / Taller cambia la vista.

La API y sus contratos están documentados en Swagger. Con la API en marcha, abre `http://localhost:5080/swagger`. El grupo Soporte (simulación de telemetría y evaluación diaria) no tiene pantalla en este portal.

El arranque de la API y del portal, y los ejemplos de esas operaciones de soporte, están en el [README](../../README.md) de la raíz.

## Desarrollo

```powershell
npm start --prefix frontend/rutaviva-web
```

Abre `http://localhost:4200`. El portal vuelve a consultar la API cada 2 segundos mientras la pestaña está visible, y se detiene si queda en segundo plano.

## Build y pruebas

```powershell
npm run build --prefix frontend/rutaviva-web
npm test --prefix frontend/rutaviva-web
```
