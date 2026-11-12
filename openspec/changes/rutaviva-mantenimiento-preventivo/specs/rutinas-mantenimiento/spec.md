# Spec Delta

## Purpose

Permite registrar vehículos con rutinas de mantenimiento configurables y determinar, por ciclo de mantenimiento, si cada rutina está al día, por vencer o vencida según su kilometraje y su tiempo.

## ADDED Requirements

### Requirement: Registro de vehículos
El sistema MUST permitir registrar un vehículo con una placa única y un kilometraje inicial mayor o igual a cero, y MUST permitir consultarlo junto con su kilometraje actual.

#### Scenario: Registro exitoso
- **WHEN** se registra un vehículo con placa "ABC123" y kilometraje inicial 20.000
- **THEN** el vehículo queda registrado con kilometraje actual 20.000 y puede consultarse

#### Scenario: Kilometraje inicial negativo
- **WHEN** se intenta registrar un vehículo con kilometraje inicial -1
- **THEN** el sistema rechaza el registro y no crea el vehículo

#### Scenario: Placa duplicada
- **GIVEN** existe un vehículo con placa "ABC123"
- **WHEN** se intenta registrar otro vehículo con placa "abc123"
- **THEN** el sistema rechaza el registro

### Requirement: Configuración de rutinas
El sistema MUST permitir agregar a un vehículo rutinas con un nombre libre, único por vehículo, y un intervalo por kilometraje, por meses calendario o por ambos. Cada criterio configurado MUST ser mayor que cero y al menos uno MUST estar presente.

#### Scenario: Rutina con ambos criterios
- **WHEN** se agrega la rutina "Cambio de aceite" con intervalo de 10.000 km y 6 meses
- **THEN** la rutina queda configurada en el vehículo con estado `AlDia`

#### Scenario: Rutina con un nombre no incluido en los ejemplos
- **WHEN** se agrega la rutina "Revisión de suspensión" con intervalo de 20.000 km
- **THEN** la rutina queda configurada en el vehículo

#### Scenario: Rutina sin criterios
- **WHEN** se intenta agregar una rutina sin kilometraje ni meses
- **THEN** el sistema rechaza la rutina

#### Scenario: Criterio no positivo
- **WHEN** se intenta agregar una rutina con intervalo de 0 km
- **THEN** el sistema rechaza la rutina

#### Scenario: Nombre repetido en el mismo vehículo
- **GIVEN** el vehículo ya tiene la rutina "Cambio de aceite"
- **WHEN** se intenta agregar otra rutina llamada "cambio de aceite"
- **THEN** el sistema rechaza la rutina

### Requirement: Inicio del primer ciclo
Al configurar una rutina, el sistema MUST iniciar su primer ciclo tomando como referencia el kilometraje actual del vehículo y la fecha de configuración.

#### Scenario: Referencia del primer ciclo
- **GIVEN** un vehículo con kilometraje actual 20.000
- **WHEN** el 2026-01-01 se agrega una rutina de 10.000 km y 6 meses
- **THEN** el ciclo inicia con referencia 20.000 km y 2026-01-01, con límite en 30.000 km y 2026-07-01

### Requirement: Intervalo inmutable durante el ciclo
El sistema MUST NOT ofrecer la modificación del intervalo de una rutina una vez configurada.

#### Scenario: No existe operación de edición de intervalo
- **WHEN** se consultan las operaciones disponibles sobre una rutina
- **THEN** no existe una operación para modificar su intervalo

### Requirement: Evaluación del criterio de kilometraje
Para el criterio de kilometraje, el sistema MUST calcular el estado así: `Vencida` si el kilometraje actual es mayor o igual al kilometraje de referencia más el intervalo; `PorVencer` si el kilometraje restante es menor o igual al 10 % del intervalo; `AlDia` en otro caso.

#### Scenario: Fuera del umbral
- **GIVEN** una rutina de 10.000 km con referencia 20.000 km
- **WHEN** se evalúa con kilometraje 28.999
- **THEN** el criterio de kilometraje resulta `AlDia`

#### Scenario: Exactamente en el umbral del 10 %
- **GIVEN** una rutina de 10.000 km con referencia 20.000 km
- **WHEN** se evalúa con kilometraje 29.000
- **THEN** el criterio de kilometraje resulta `PorVencer`

#### Scenario: Exactamente en el límite
- **GIVEN** una rutina de 10.000 km con referencia 20.000 km
- **WHEN** se evalúa con kilometraje 30.000
- **THEN** el criterio de kilometraje resulta `Vencida`

### Requirement: Evaluación del criterio de tiempo
Para el criterio de tiempo, el límite MUST ser la fecha de referencia más los meses del intervalo en meses calendario. El umbral MUST ser el 10 % de los días reales entre la referencia y el límite. El estado es `Vencida` si la fecha evaluada es igual o posterior al límite, `PorVencer` si los días restantes son menores o iguales al umbral, y `AlDia` en otro caso.

#### Scenario: Fuera del umbral temporal
- **GIVEN** una rutina de 6 meses con referencia 2026-01-01 (límite 2026-07-01, 181 días, umbral 18,1 días)
- **WHEN** se evalúa con fecha 2026-06-12 (restan 19 días)
- **THEN** el criterio de tiempo resulta `AlDia`

#### Scenario: Dentro del umbral temporal
- **GIVEN** una rutina de 6 meses con referencia 2026-01-01
- **WHEN** se evalúa con fecha 2026-06-13 (restan 18 días)
- **THEN** el criterio de tiempo resulta `PorVencer`

#### Scenario: Límite temporal alcanzado
- **GIVEN** una rutina de 6 meses con referencia 2026-01-01
- **WHEN** se evalúa con fecha 2026-07-01
- **THEN** el criterio de tiempo resulta `Vencida`

#### Scenario: Referencia a fin de mes
- **GIVEN** una rutina de 1 mes con referencia 2026-01-31
- **WHEN** se calcula su límite temporal
- **THEN** el límite es 2026-02-28

### Requirement: Criterios independientes
Cuando una rutina tiene ambos criterios, el sistema MUST evaluar cada uno de forma independiente y aplicar el estado más avanzado entre ellos (`Vencida` sobre `PorVencer` sobre `AlDia`).

#### Scenario: Kilometraje por vencer y tiempo al día
- **GIVEN** una rutina de 10.000 km y 6 meses
- **WHEN** el criterio de kilometraje resulta `PorVencer` y el de tiempo `AlDia`
- **THEN** la rutina pasa a `PorVencer`

#### Scenario: Tiempo vencido y kilometraje al día
- **GIVEN** una rutina de 10.000 km y 6 meses
- **WHEN** el criterio de tiempo resulta `Vencida` y el de kilometraje `AlDia`
- **THEN** la rutina pasa a `Vencida`

### Requirement: Progresión de estados dentro del ciclo
Dentro de un mismo ciclo, el estado de una rutina MUST NOT retroceder. Se permiten las transiciones `AlDia` a `PorVencer`, `PorVencer` a `Vencida` y `AlDia` a `Vencida` directamente.

#### Scenario: Salto directo a vencida
- **GIVEN** una rutina `AlDia` de 10.000 km con referencia 20.000 km
- **WHEN** se evalúa el kilometraje 31.000
- **THEN** la rutina pasa directamente a `Vencida`

#### Scenario: Una evaluación posterior más favorable no retrocede
- **GIVEN** una rutina `PorVencer` por el criterio de kilometraje
- **WHEN** el proceso diario evalúa el criterio de tiempo y este resulta `AlDia`
- **THEN** la rutina permanece `PorVencer`

### Requirement: Notificación única de próximo vencimiento
El sistema MUST registrar el evento de próximo vencimiento como máximo una vez por ciclo, al pasar la rutina a `PorVencer`. Si la rutina pasa directamente de `AlDia` a `Vencida`, el sistema MUST NOT registrar ese evento.

#### Scenario: Primera entrada a por vencer
- **WHEN** una rutina pasa de `AlDia` a `PorVencer`
- **THEN** se registra un único evento de próximo vencimiento para el ciclo

#### Scenario: Evaluaciones repetidas
- **GIVEN** una rutina `PorVencer` que ya registró el evento de próximo vencimiento
- **WHEN** llegan nuevas evaluaciones por kilometraje o por tiempo que la mantienen `PorVencer`
- **THEN** no se registra otro evento de próximo vencimiento

#### Scenario: Segundo criterio entra al umbral
- **GIVEN** una rutina `PorVencer` por kilometraje que ya registró el evento
- **WHEN** el criterio de tiempo también entra al umbral
- **THEN** no se registra otro evento de próximo vencimiento

#### Scenario: Salto directo sin aviso previo
- **WHEN** una rutina pasa directamente de `AlDia` a `Vencida`
- **THEN** se registra solo el evento de vencimiento

### Requirement: Registro único de vencimiento
El sistema MUST registrar el evento de vencimiento exactamente una vez por ciclo, al pasar la rutina a `Vencida`.

#### Scenario: Entrada a vencida
- **WHEN** una rutina pasa a `Vencida`
- **THEN** se registra un único evento de vencimiento para el ciclo

#### Scenario: Vencida sigue recibiendo evaluaciones
- **GIVEN** una rutina `Vencida`
- **WHEN** llegan nuevas lecturas o ejecuciones del proceso diario
- **THEN** no se registra otro evento de vencimiento

### Requirement: Reinicio del ciclo al completar el servicio
Cuando se completa la orden de servicio de una rutina, el sistema MUST iniciar un nuevo ciclo con referencia en el kilometraje y la fecha reales reportados, dejar la rutina en `AlDia` y habilitar nuevamente los eventos de próximo vencimiento y vencimiento.

#### Scenario: Reinicio con kilometraje real menor al actual del vehículo
- **GIVEN** un vehículo con kilometraje actual 52.000 y una rutina de 10.000 km vencida
- **WHEN** se completa su orden con kilometraje real 50.000 y fecha 2026-07-05
- **THEN** la rutina queda `AlDia` con referencia 50.000 km y 2026-07-05
- **AND** el kilometraje actual del vehículo sigue en 52.000

#### Scenario: El nuevo ciclo se evalúa con la siguiente lectura
- **GIVEN** una rutina reiniciada con referencia 50.000 km y un vehículo en 52.000 km
- **WHEN** llega una lectura de 59.100 km
- **THEN** la rutina pasa a `PorVencer` y registra el evento de próximo vencimiento del nuevo ciclo

#### Scenario: Kilometraje real anterior a la referencia del ciclo
- **GIVEN** una rutina cuyo ciclo tiene referencia 40.000 km
- **WHEN** se intenta completar su orden con kilometraje real 39.000
- **THEN** el sistema rechaza el completado y la orden permanece `EnEjecucion`

#### Scenario: Fecha real anterior a la referencia del ciclo
- **GIVEN** una rutina cuyo ciclo tiene referencia 2026-01-01
- **WHEN** se intenta completar su orden con fecha real 2025-12-31
- **THEN** el sistema rechaza el completado y la orden permanece `EnEjecucion`

### Requirement: Rutina vencida tras cancelación de su orden
Cuando la orden abierta de una rutina se cancela o su cotización se rechaza, la rutina MUST permanecer `Vencida` y el sistema MUST NOT generar automáticamente otra orden para ese ciclo.

#### Scenario: Orden cancelada
- **GIVEN** una rutina `Vencida` con una orden abierta
- **WHEN** el cliente cancela la orden
- **THEN** la rutina permanece `Vencida` sin orden abierta

#### Scenario: Nuevas evaluaciones tras la cancelación
- **GIVEN** una rutina `Vencida` cuya orden fue cancelada
- **WHEN** llegan nuevas lecturas o ejecuciones del proceso diario
- **THEN** no se genera una nueva orden

### Requirement: Consulta de rutinas
El sistema MUST permitir consultar las rutinas de un vehículo con su intervalo, la referencia del ciclo actual, sus límites y su estado.

#### Scenario: Consulta de estado
- **GIVEN** un vehículo con una rutina `PorVencer`
- **WHEN** se consultan sus rutinas
- **THEN** la respuesta incluye la rutina con estado `PorVencer`, su referencia y sus límites de kilometraje y fecha
