# Spec Delta

## Purpose

Recibe lecturas de kilometraje de la plataforma de telemetría externa, las traduce al modelo de RutaViva y actualiza de forma idempotente y segura ante concurrencia el kilometraje de los vehículos, disparando la evaluación por kilometraje de sus rutinas.

## ADDED Requirements

### Requirement: Frontera con la plataforma de telemetría
El sistema MUST recibir cada lectura con identificador de evento, placa, kilometraje en kilómetros y fecha de lectura, y traducirla a una lectura interna (vehículo, kilometraje, fecha de lectura) antes de aplicarla. El vehículo MUST identificarse por su placa. Una lectura con datos incompletos o de un vehículo no registrado MUST rechazarse sin modificar ningún estado.

#### Scenario: Lectura válida
- **WHEN** la plataforma envía una lectura para la placa "ABC123" con kilometraje 25.000
- **THEN** el sistema la traduce y la aplica al vehículo "ABC123"

#### Scenario: Vehículo no registrado
- **WHEN** llega una lectura para una placa que no existe
- **THEN** el sistema responde con un error de vehículo no encontrado y no modifica ningún estado

#### Scenario: Lectura incompleta
- **WHEN** llega una lectura sin kilometraje
- **THEN** el sistema la rechaza por datos inválidos y no modifica ningún estado

#### Scenario: Lectura sin identificador de evento
- **WHEN** llega una lectura sin identificador de evento
- **THEN** el sistema la rechaza por datos inválidos y no modifica ningún estado

### Requirement: Identificador de evento
Cada lectura MUST incluir un identificador de evento y el sistema MUST aplicar cada identificador como máximo una vez. Repetir un identificador ya procesado MUST devolver el mismo resultado de la primera vez, sin modificar el estado ni registrar eventos, aunque el kilometraje o la fecha difieran.

#### Scenario: Repetición de una lectura aceptada
- **GIVEN** el evento "evt-1" ya actualizó el kilometraje a 29.000 y llevó una rutina a `PorVencer`
- **WHEN** vuelve a llegar el evento "evt-1" con kilometraje 31.000
- **THEN** el sistema responde con el mismo resultado de la primera vez
- **AND** el kilometraje sigue en 29.000 y no se registra otro evento

#### Scenario: Repetición de una lectura ignorada
- **GIVEN** el evento "evt-2" fue ignorado porque su kilometraje no superaba el actual
- **WHEN** vuelve a llegar el evento "evt-2"
- **THEN** el sistema responde que la lectura fue ignorada y no modifica ningún estado

#### Scenario: El mismo evento llega dos veces al mismo tiempo
- **WHEN** el evento "evt-3", de 25.500 km, llega dos veces al mismo tiempo
- **THEN** el kilometraje queda en 25.500 y solo se aplican los efectos de una de las dos llegadas

### Requirement: Kilometraje monótono
El kilometraje actual de un vehículo MUST NOT disminuir. Una lectura con kilometraje mayor al actual MUST aceptarse y reemplazar el kilometraje actual. Una lectura con kilometraje menor o igual al actual MUST ignorarse.

#### Scenario: Lectura mayor
- **GIVEN** un vehículo con kilometraje actual 25.000
- **WHEN** llega una lectura de 25.500
- **THEN** el kilometraje actual pasa a 25.500

#### Scenario: Lectura igual
- **GIVEN** un vehículo con kilometraje actual 25.000
- **WHEN** llega una lectura de 25.000
- **THEN** la lectura se ignora y el kilometraje actual sigue en 25.000

#### Scenario: Lectura menor
- **GIVEN** un vehículo con kilometraje actual 25.000
- **WHEN** llega una lectura de 24.000
- **THEN** la lectura se ignora y el kilometraje actual sigue en 25.000

### Requirement: Lecturas ignoradas no son errores
Una lectura ignorada MUST responderse como recibida, indicando que fue ignorada, y MUST NOT producir eventos ni evaluaciones.

#### Scenario: Respuesta a una lectura ignorada
- **WHEN** llega una lectura con kilometraje menor o igual al actual
- **THEN** el sistema responde exitosamente indicando que la lectura fue ignorada
- **AND** no se registran eventos

### Requirement: Procesamiento idempotente
Recibir la misma lectura más de una vez MUST producir el mismo estado final que recibirla una sola vez.

#### Scenario: Otro evento con el mismo kilometraje
- **GIVEN** el evento "evt-1", de 29.000 km, ya llevó una rutina a `PorVencer`
- **WHEN** llega el evento "evt-4" con el mismo kilometraje de 29.000
- **THEN** la lectura se ignora y no se registra otro evento de próximo vencimiento

### Requirement: Lecturas fuera de orden
La aceptación de una lectura MUST decidirse solo por su kilometraje frente al kilometraje actual, sin importar su fecha de lectura.

#### Scenario: Lectura atrasada con menor kilometraje
- **GIVEN** se procesó una lectura de 26.000 km con fecha 2026-03-10
- **WHEN** llega una lectura de 25.800 km con fecha 2026-03-09
- **THEN** la lectura se ignora

#### Scenario: Lectura con fecha anterior y mayor kilometraje
- **GIVEN** se procesó una lectura de 26.000 km con fecha 2026-03-10
- **WHEN** llega una lectura de 26.200 km con fecha 2026-03-09
- **THEN** la lectura se acepta y el kilometraje actual pasa a 26.200

### Requirement: Lecturas simultáneas
Cuando llegan varias lecturas del mismo vehículo al mismo tiempo, el resultado MUST ser equivalente a procesarlas una tras otra: el kilometraje final es el mayor recibido y cada transición de rutina y cada orden se generan una sola vez.

#### Scenario: Dos lecturas concurrentes
- **GIVEN** un vehículo con kilometraje actual 25.000
- **WHEN** llegan al mismo tiempo lecturas de 25.100 y 25.200
- **THEN** el kilometraje actual final es 25.200

#### Scenario: Lecturas concurrentes que vencen una rutina
- **GIVEN** una rutina de 10.000 km con referencia 20.000 km
- **WHEN** llegan al mismo tiempo lecturas de 30.000 y 30.100
- **THEN** se registra un único evento de vencimiento y se genera una única orden

### Requirement: Evaluación por kilometraje al aceptar una lectura
Al aceptar una lectura, el sistema MUST evaluar el criterio de kilometraje de todas las rutinas del vehículo que lo tengan configurado. La lectura MUST NOT evaluar el criterio de tiempo, que corresponde al proceso diario.

#### Scenario: Lectura que deja una rutina por vencer
- **GIVEN** una rutina de 10.000 km con referencia 20.000 km
- **WHEN** se acepta una lectura de 29.000
- **THEN** la rutina pasa a `PorVencer`

#### Scenario: Rutina solo por tiempo
- **GIVEN** una rutina configurada solo con 6 meses
- **WHEN** se acepta una lectura de kilometraje
- **THEN** el estado de la rutina no cambia

#### Scenario: Tiempo cumplido sin proceso diario
- **GIVEN** una rutina de 10.000 km y 6 meses cuyo límite temporal ya pasó pero el proceso diario no se ha ejecutado
- **WHEN** se acepta una lectura que deja el kilometraje `AlDia`
- **THEN** la rutina no cambia de estado hasta la siguiente evaluación por tiempo
