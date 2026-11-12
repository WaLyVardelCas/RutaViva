# RutaViva

Plataforma de mantenimiento preventivo de vehículos. El cliente registra su flota y decide las cotizaciones; el taller cotiza, inicia y completa los servicios. Las lecturas de kilometraje llegan por telemetría y el vencimiento por tiempo lo evalúa un proceso diario dentro de la misma API.

Las decisiones de diseño están en `openspec/changes/rutaviva-mantenimiento-preventivo/`.

## Backend

```powershell
dotnet run --project backend/RutaViva.Api --launch-profile http
```

La API queda en `http://localhost:5080`. Los contratos y las respuestas de cada operación están en Swagger: `http://localhost:5080/swagger` (OpenAPI en `http://localhost:5080/swagger/v1/swagger.json`).

Los grupos Vehículos, Rutinas y Órdenes son el contrato que usa el portal. El grupo **Soporte** identifica las operaciones de demostración: simular una lectura de telemetría y disparar la evaluación diaria con una fecha indicada. Esas dos no tienen pantalla. Se envían desde Swagger o con `backend/RutaViva.Api/RutaViva.http`.

Registrar un vehículo (la placa debe existir antes de enviar lecturas):

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5080/api/vehicles -ContentType "application/json" -Body '{"plate":"ABC123","mileage":20000}'
```

Enviar una lectura de kilometraje. Responde `Accepted` si se aplica, o `Ignored` si el kilometraje no supera el actual o si el `eventId` ya fue procesado:

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5080/api/telemetry/readings -ContentType "application/json" -Body '{"eventId":"evt-1","plate":"ABC123","mileage":29000,"readingDate":"2026-06-01"}'
```

Ejecutar la evaluación diaria con una fecha simulada:

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5080/api/processes/daily-evaluation -ContentType "application/json" -Body '{"date":"2026-07-01"}'
```

En PowerShell, `curl` es un alias de `Invoke-WebRequest` y no acepta las opciones de curl; por eso se usa `Invoke-RestMethod`. Si prefieres curl real, usa `curl.exe`.

Pruebas:

```powershell
dotnet test backend/RutaViva.slnx
```

## Portal

En otra terminal:

```powershell
npm start --prefix frontend/rutaviva-web
```

Abre `http://localhost:4200`. El selector Cliente / Taller cambia la vista; no hay autenticación. El cliente registra vehículos y consulta las rutinas. El taller configura las rutinas y ve todas las órdenes. Cada orden muestra su cotización. El portal vuelve a consultar la API cada 2 segundos mientras la pestaña está visible, y espera a que termine la consulta anterior. Si la pestaña queda en segundo plano, o si la API deja de responder, deja de insistir al mismo ritmo. Así el kilometraje, el estado de las rutinas y las órdenes nuevas aparecen sin recargar la página.

## Fuera de esta demo

No hay base de datos, autenticación, notificaciones, pagos ni selección de talleres. Al reiniciar la API se pierden los datos en memoria.
