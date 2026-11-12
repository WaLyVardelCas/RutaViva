# Proposal

## Why

Los clientes administran hoy el mantenimiento preventivo de sus flotas con archivos Excel: no hay alertas de vencimiento, no hay trazabilidad de órdenes ni de cotizaciones y nada impide duplicar trabajo. RutaViva automatiza ese proceso integrando lecturas de telemetría, una evaluación diaria por tiempo y el flujo de órdenes de servicio entre cliente y taller.

## What Changes

- Registro y consulta de vehículos con rutinas de mantenimiento configurables (no limitadas a aceite, frenos o llantas), con intervalo por kilometraje, por tiempo en meses calendario o por ambos.
- Evaluación del estado de cada rutina (`AlDia`, `PorVencer`, `Vencida`) por kilometraje al recibir telemetría y por tiempo en un proceso diario, con notificación de próximo vencimiento una sola vez por ciclo.
- Ingesta de lecturas de kilometraje desde la plataforma de telemetría externa, cada una con identificador de evento, a través de una frontera anticorrupción. El procesamiento es idempotente y seguro ante lecturas simultáneas, duplicadas o fuera de orden.
- Generación automática de una orden de servicio cuando una rutina vence, con máximo una orden abierta por vehículo y rutina.
- Flujo de la orden: cotización por el taller, aprobación automática (≤ COP 1.500.000) o del cliente (> COP 1.500.000), rechazo, cancelación, inicio y completado con kilometraje y fecha reales, que reinicia el ciclo de la rutina.
- Proceso programado diario dentro del proceso de la API y un endpoint para ejecutar la misma evaluación con una fecha simulada.
- Registro (sin envío real) de los eventos de dominio relevantes.
- Aplicación Angular única con simulación visual de los roles cliente y taller. El cliente registra vehículos y consulta las rutinas; el taller las configura. Cada orden muestra su cotización de forma destacada.

Fuera de alcance: autenticación, pagos, base de datos real, notificaciones reales, selección o programación de talleres, despliegue, microservicios, comportamiento posterior a la cancelación de una orden, modificación de cotizaciones enviadas, edición de intervalos durante un ciclo activo, bitácora de eventos por HTTP, datos semilla y pantallas para simular telemetría o el proceso diario. Esos dos flujos se demuestran a mano, por HTTP.

## Capabilities

### New Capabilities

- `rutinas-mantenimiento`: configuración de vehículos y rutinas, cálculo de estado por criterio, progresión de estados por ciclo, notificación única y reinicio del ciclo.
- `telemetria-kilometraje`: recepción y traducción de lecturas externas con identificador de evento, regla de kilometraje monótono, idempotencia y concurrencia, y disparo de la evaluación por kilometraje.
- `ordenes-servicio`: generación de órdenes por vencimiento, unicidad de orden abierta, cotización, aprobación, rechazo, cancelación, inicio y completado.
- `evaluacion-programada`: evaluación diaria por tiempo, idempotente, y su ejecución manual con fecha simulada.
- `portal-web`: aplicación Angular con vistas de cliente y de taller, registro de vehículos por el cliente, configuración de rutinas por el taller y simulación de roles sin autenticación.

### Modified Capabilities

Ninguna (proyecto nuevo, sin specs previas).

## Impact

- Código nuevo: solución .NET (`RutaViva.Domain`, `RutaViva.Application`, `RutaViva.Infrastructure`, `RutaViva.Api`, `RutaViva.Job`), proyectos de prueba y la aplicación Angular `frontend/rutaviva-web`.
- API HTTP nueva para vehículos, rutinas, telemetría, órdenes y la evaluación diaria.
- Integración entrante con la plataforma de telemetría externa. Contrato: identificador de evento, placa, kilometraje en km y fecha de lectura.
- Dependencias: ninguna en Domain ni Application; pruebas con xUnit, Moq de uso puntual y `Microsoft.AspNetCore.Mvc.Testing`.
