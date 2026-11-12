# Spec Delta

## Purpose

Evalúa una vez al día el criterio de tiempo de todas las rutinas, de forma idempotente, y permite ejecutar esa misma evaluación con una fecha simulada para demostrar y probar los vencimientos por tiempo.

## ADDED Requirements

### Requirement: Evaluación diaria por tiempo
El sistema MUST ejecutar automáticamente, una vez al día, la evaluación del criterio de tiempo de todas las rutinas de todos los vehículos que tengan ese criterio configurado. La fecha evaluada MUST ser la fecha actual en la zona horaria de Colombia (America/Bogota). Esta evaluación MUST NOT evaluar el criterio de kilometraje.

#### Scenario: Rutina que entra al umbral temporal
- **GIVEN** una rutina de 6 meses con referencia 2026-01-01
- **WHEN** el proceso diario se ejecuta el 2026-06-13
- **THEN** la rutina pasa a `PorVencer` y se registra el evento de próximo vencimiento

#### Scenario: Rutina que vence por tiempo
- **GIVEN** una rutina de 6 meses con referencia 2026-01-01
- **WHEN** el proceso diario se ejecuta el 2026-07-01
- **THEN** la rutina pasa a `Vencida` y se genera su orden de servicio

#### Scenario: Rutina solo por kilometraje
- **GIVEN** una rutina configurada solo con 10.000 km
- **WHEN** se ejecuta el proceso diario
- **THEN** el estado de la rutina no cambia

### Requirement: Evaluación diaria idempotente
Ejecutar la evaluación más de una vez para la misma fecha, o para una fecha anterior a otra ya evaluada, MUST NOT producir nuevas transiciones, eventos ni órdenes.

#### Scenario: Dos ejecuciones el mismo día
- **GIVEN** el proceso diario del 2026-07-01 ya llevó una rutina a `Vencida` y generó su orden
- **WHEN** el proceso se ejecuta de nuevo con fecha 2026-07-01
- **THEN** no se registran eventos nuevos y sigue existiendo una única orden

#### Scenario: Ejecución con una fecha anterior
- **GIVEN** una rutina que ya está `Vencida` por tiempo
- **WHEN** el proceso se ejecuta con una fecha en la que la rutina estaría `AlDia`
- **THEN** la rutina permanece `Vencida`

### Requirement: Ejecución con fecha simulada
El sistema MUST exponer una operación HTTP que recibe una fecha obligatoria y ejecuta la misma evaluación por tiempo que el proceso diario, con el mismo comportamiento e idempotencia. Una fecha ausente o inválida MUST rechazarse sin ejecutar la evaluación.

#### Scenario: Evaluación con fecha futura simulada
- **GIVEN** una rutina de 6 meses con referencia 2026-01-01
- **WHEN** se ejecuta la evaluación con fecha simulada 2026-07-01
- **THEN** la rutina pasa a `Vencida` y se genera su orden de servicio
- **AND** la respuesta indica cuántas rutinas cambiaron de estado

#### Scenario: Fecha inválida
- **WHEN** se solicita la evaluación sin fecha o con una fecha mal formada
- **THEN** el sistema rechaza la solicitud y no evalúa ninguna rutina

### Requirement: Fallas aisladas por vehículo
Si la evaluación de un vehículo falla, el sistema MUST continuar con los demás vehículos y registrar la falla.

#### Scenario: Un vehículo falla durante la evaluación
- **GIVEN** tres vehículos con rutinas por tiempo y uno de ellos falla al evaluarse
- **WHEN** se ejecuta la evaluación
- **THEN** los otros dos vehículos quedan evaluados y la falla queda registrada
