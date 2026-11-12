# Tasks

## 1. Solución

- [x] 1.1 Crear la solución .NET 10 (`Domain`, `Application`, `Infrastructure`, `Api`, `Job` y los tres proyectos de prueba) con las referencias del diseño, y verificar que `dotnet build` termina sin errores
- [x] 1.2 Agregar `.gitignore` de .NET y Node, y verificar que ignora `bin/`, `obj/` y `node_modules/`

## 2. Dominio de vehículos y rutinas

- [x] 2.1 Implementar los value objects, el cálculo puro de cada criterio y `Vehiculo`/`RutinaMantenimiento`, y verificar que `dotnet test backend/tests/RutaViva.Domain.Tests` cubre kilometraje, tiempo en meses calendario, criterios independientes, progresión sin retroceso, notificación única y reinicio de ciclo

## 3. Dominio de órdenes

- [x] 3.1 Implementar `OrdenServicio` con cotización inmutable, aprobación automática solo hasta COP 1.500.000 y las transiciones del diseño, y verificar que las pruebas de dominio de órdenes pasan

## 4. Casos de uso y persistencia en memoria

- [x] 4.1 Implementar repositorios con snapshots y versión, el registro de identificadores de telemetría, los casos de uso, el despachador y los handlers, y verificar que `dotnet test backend/tests/RutaViva.Application.Tests` cubre lecturas, evaluación diaria, órdenes, cancelación sin reorden y concurrencia

## 5. API y proceso diario

- [x] 5.1 Exponer los endpoints, la frontera de telemetría y el `BackgroundService` que reutiliza el caso de uso diario, y verificar que `dotnet test backend/tests/RutaViva.Api.IntegrationTests` cubre el flujo feliz, el rechazo, la lectura ignorada, el identificador repetido, la fecha inválida y la transición inválida

## 6. Portal Angular

- [x] 6.1 Crear la aplicación con modos cliente y taller: el cliente registra vehículos y consulta rutinas, el taller configura las rutinas, y las acciones de órdenes siguen su estado. La cotización se muestra en cada orden. Verificar que `npm run build` termina sin errores

## 7. Cómo ejecutar la demo

- [x] 7.1 Documentar en el README el arranque de la API y del portal, y un archivo `.http` para telemetría y evaluación diaria, y verificar que los comandos documentados coinciden con los proyectos generados
