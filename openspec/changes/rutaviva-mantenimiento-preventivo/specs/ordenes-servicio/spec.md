# Spec Delta

## Purpose

Gestiona las órdenes de servicio que se generan cuando vence una rutina: su cotización, la aprobación automática o del cliente, el rechazo, la cancelación, la ejecución por el taller y el completado que reinicia el ciclo de la rutina.

## ADDED Requirements

### Requirement: Generación de orden por vencimiento
Cuando una rutina pasa a `Vencida`, el sistema MUST generar una orden de servicio en estado `Creada` asociada a ese vehículo y a esa única rutina, y MUST registrar el evento de orden generada.

#### Scenario: Rutina vencida por kilometraje
- **WHEN** una lectura lleva una rutina a `Vencida`
- **THEN** existe una orden `Creada` para ese vehículo y esa rutina
- **AND** se registra el evento de orden generada

#### Scenario: Rutina vencida por tiempo
- **WHEN** el proceso diario lleva una rutina a `Vencida`
- **THEN** existe una orden `Creada` para ese vehículo y esa rutina

### Requirement: Una sola orden abierta por vehículo y rutina
El sistema MUST NOT permitir más de una orden abierta para la misma combinación de vehículo y rutina. Una orden está abierta si no está `Completada` ni `Cancelada`.

#### Scenario: Intento de generar una segunda orden abierta
- **GIVEN** existe una orden `Cotizada` para el vehículo V y la rutina R
- **WHEN** se intenta generar otra orden para V y R
- **THEN** no se crea una segunda orden y la existente no cambia

#### Scenario: Generación repetida por reintento
- **GIVEN** el vencimiento de la rutina R ya generó una orden
- **WHEN** el mismo vencimiento se procesa de nuevo
- **THEN** sigue existiendo una única orden abierta para V y R

#### Scenario: Orden anterior completada
- **GIVEN** la orden anterior de V y R está `Completada` y la rutina venció en su nuevo ciclo
- **WHEN** se procesa el nuevo vencimiento
- **THEN** se genera una nueva orden `Creada` para V y R

### Requirement: Consulta de órdenes
El sistema MUST permitir consultar las órdenes de servicio, filtrarlas por estado y por vehículo, y consultar el detalle de una orden con su cotización y su ejecución cuando existan.

#### Scenario: Filtro por estado
- **GIVEN** existen órdenes `Creada` y `Cotizada`
- **WHEN** se consultan las órdenes con estado `Cotizada`
- **THEN** la respuesta contiene solo las órdenes `Cotizada`

### Requirement: Registro de cotización
El taller MUST poder registrar una única cotización, con monto en COP mayor que cero, solo sobre una orden `Creada`. Una vez registrada, la cotización MUST NOT modificarse ni reemplazarse.

#### Scenario: Cotización sobre orden creada
- **GIVEN** una orden `Creada`
- **WHEN** el taller registra una cotización de COP 800.000
- **THEN** la orden queda con esa cotización

#### Scenario: Monto no positivo
- **WHEN** el taller intenta registrar una cotización de COP 0
- **THEN** el sistema rechaza la cotización y la orden no cambia

#### Scenario: Segunda cotización
- **GIVEN** una orden que ya tiene cotización
- **WHEN** el taller intenta registrar otra cotización
- **THEN** el sistema rechaza la operación y la cotización original se conserva

### Requirement: Aprobación automática
Si el monto cotizado es menor o igual a COP 1.500.000, la orden MUST quedar `Aprobada` al registrar la cotización, marcada como aprobada automáticamente, sin requerir acción del cliente.

#### Scenario: Monto en el límite
- **GIVEN** una orden `Creada`
- **WHEN** el taller registra una cotización de COP 1.500.000
- **THEN** la orden queda `Aprobada` con aprobación automática y no permanece en `Cotizada`

#### Scenario: Monto inferior al límite
- **GIVEN** una orden `Creada`
- **WHEN** el taller registra una cotización de COP 300.000
- **THEN** la orden queda `Aprobada` con aprobación automática

### Requirement: Cotización que requiere aprobación del cliente
Si el monto cotizado supera COP 1.500.000, la orden MUST quedar `Cotizada` a la espera de la decisión del cliente.

#### Scenario: Monto superior al límite
- **GIVEN** una orden `Creada`
- **WHEN** el taller registra una cotización de COP 1.500.001
- **THEN** la orden queda `Cotizada`

### Requirement: Aprobación del cliente
El cliente MUST poder aprobar la cotización solo cuando la orden está `Cotizada`; al aprobarla, la orden pasa a `Aprobada`.

#### Scenario: Aprobación de una orden cotizada
- **GIVEN** una orden `Cotizada`
- **WHEN** el cliente aprueba la cotización
- **THEN** la orden queda `Aprobada` con aprobación del cliente

#### Scenario: Aprobación fuera de estado
- **GIVEN** una orden `Aprobada` automáticamente
- **WHEN** el cliente intenta aprobarla
- **THEN** el sistema rechaza la operación y la orden no cambia

### Requirement: Rechazo de la cotización
El cliente MUST poder rechazar la cotización solo cuando la orden está `Cotizada`; al rechazarla, la orden pasa a `Cancelada`.

#### Scenario: Rechazo de una orden cotizada
- **GIVEN** una orden `Cotizada`
- **WHEN** el cliente rechaza la cotización
- **THEN** la orden queda `Cancelada`
- **AND** la rutina asociada permanece `Vencida`

#### Scenario: Rechazo fuera de estado
- **GIVEN** una orden `Creada`
- **WHEN** el cliente intenta rechazar la cotización
- **THEN** el sistema rechaza la operación y la orden no cambia

### Requirement: Cancelación por el cliente
El cliente MUST poder cancelar una orden en estado `Creada`, `Cotizada` o `Aprobada`. La cancelación MUST rechazarse en `EnEjecucion`, `Completada` o `Cancelada`.

#### Scenario: Cancelación permitida
- **GIVEN** una orden `Aprobada`
- **WHEN** el cliente la cancela
- **THEN** la orden queda `Cancelada`

#### Scenario: Cancelación en ejecución
- **GIVEN** una orden `EnEjecucion`
- **WHEN** el cliente intenta cancelarla
- **THEN** el sistema rechaza la operación y la orden permanece `EnEjecucion`

#### Scenario: Cancelación de una orden terminada
- **GIVEN** una orden `Completada`
- **WHEN** el cliente intenta cancelarla
- **THEN** el sistema rechaza la operación

### Requirement: Inicio del servicio
El taller MUST poder iniciar una orden solo cuando está `Aprobada`; al iniciarla, la orden pasa a `EnEjecucion`.

#### Scenario: Inicio de una orden aprobada
- **GIVEN** una orden `Aprobada`
- **WHEN** el taller inicia el servicio
- **THEN** la orden queda `EnEjecucion`

#### Scenario: Inicio sin aprobación
- **GIVEN** una orden `Cotizada`
- **WHEN** el taller intenta iniciar el servicio
- **THEN** el sistema rechaza la operación y la orden no cambia

### Requirement: Completado del servicio
El taller MUST poder completar una orden solo cuando está `EnEjecucion`, reportando el kilometraje real y la fecha real del mantenimiento. Al completarla, la orden pasa a `Completada`, se registra el evento de orden completada y la rutina reinicia su ciclo con esos valores. El kilometraje real MUST NOT ser menor al kilometraje actual del vehículo; si es mayor, el kilometraje actual del vehículo MUST actualizarse a ese valor, de modo que la referencia del ciclo nuevo coincida con el vehículo.

#### Scenario: Completado exitoso
- **GIVEN** una orden `EnEjecucion`
- **WHEN** el taller la completa con kilometraje 50.000 y fecha 2026-07-05
- **THEN** la orden queda `Completada` con esa ejecución registrada
- **AND** la rutina asociada queda `AlDia` con referencia 50.000 km y 2026-07-05

#### Scenario: Completado sin datos reales
- **GIVEN** una orden `EnEjecucion`
- **WHEN** el taller intenta completarla sin kilometraje o sin fecha
- **THEN** el sistema rechaza la operación y la orden permanece `EnEjecucion`

#### Scenario: Completado fuera de estado
- **GIVEN** una orden `Aprobada`
- **WHEN** el taller intenta completarla
- **THEN** el sistema rechaza la operación y la orden no cambia

#### Scenario: Kilometraje real mayor al actual del vehículo
- **GIVEN** un vehículo con kilometraje actual 50.000 y una orden `EnEjecucion`
- **WHEN** el taller la completa con kilometraje real 50.300
- **THEN** la orden queda `Completada` y la rutina toma como referencia 50.300 km
- **AND** el kilometraje actual del vehículo pasa a 50.300

#### Scenario: Kilometraje real menor al actual del vehículo
- **GIVEN** un vehículo con kilometraje actual 52.000 y una orden `EnEjecucion`
- **WHEN** el taller intenta completarla con kilometraje real 50.000
- **THEN** el sistema rechaza la operación, la orden permanece `EnEjecucion` y el kilometraje del vehículo sigue en 52.000

### Requirement: Registro de eventos de la orden
El sistema MUST registrar un evento por cada transición de la orden: generación, cotización, aprobación (indicando si fue automática), rechazo, cancelación, inicio y completado. El registro MUST NOT enviar notificaciones reales.

#### Scenario: Cotización con aprobación automática
- **WHEN** el taller registra una cotización de COP 900.000 sobre una orden `Creada`
- **THEN** se registran el evento de cotización y el de aprobación automática
