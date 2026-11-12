# Spec Delta

## Purpose

Ofrece una única aplicación web con dos modos simulados, cliente y taller, para registrar la flota, configurar y consultar sus rutinas y gestionar las órdenes de servicio sin autenticación. El cliente registra los vehículos y consulta las rutinas. El taller configura las rutinas.

## ADDED Requirements

### Requirement: Simulación de roles sin autenticación
La aplicación MUST permitir elegir entre el modo cliente y el modo taller sin autenticación, y MUST mostrar en todo momento el modo activo. El modo MUST afectar solo lo que se muestra; las reglas de negocio MUST validarse en el backend.

#### Scenario: Cambio de modo
- **GIVEN** la aplicación está en modo cliente
- **WHEN** el usuario selecciona el modo taller
- **THEN** la aplicación muestra la vista del taller e indica que el modo activo es taller

### Requirement: Registro de vehículos en modo cliente
En modo cliente, la aplicación MUST permitir registrar un vehículo con placa y kilometraje inicial, y MUST mostrarlo en el listado con los datos devueltos por el backend.

#### Scenario: Registro desde la interfaz
- **WHEN** el cliente ingresa la placa "ABC123" y el kilometraje 20.000, y confirma
- **THEN** el vehículo aparece en el listado con kilometraje 20.000

#### Scenario: Placa duplicada desde la interfaz
- **GIVEN** ya existe un vehículo con placa "ABC123"
- **WHEN** el cliente intenta registrar de nuevo la placa "ABC123"
- **THEN** la aplicación muestra el motivo del rechazo y no agrega otro vehículo

### Requirement: Configuración de rutinas en modo taller
En modo taller, la aplicación MUST permitir agregar a un vehículo una rutina con nombre y al menos un criterio, de kilometraje, de meses o de ambos, y MUST mostrarla con el estado devuelto por el backend. En modo cliente, la aplicación MUST NOT ofrecer el alta de rutinas.

#### Scenario: Rutina con ambos criterios
- **WHEN** el taller agrega la rutina "Cambio de aceite" con 10.000 km y 6 meses a un vehículo
- **THEN** la rutina aparece en el vehículo con estado `AlDia`

#### Scenario: Rutina rechazada por el backend
- **WHEN** el taller intenta agregar una rutina y el backend la rechaza
- **THEN** la aplicación muestra el motivo del rechazo y no agrega la rutina

#### Scenario: El cliente consulta y no configura
- **WHEN** el cliente consulta un vehículo
- **THEN** ve sus rutinas y la aplicación no ofrece agregar una

### Requirement: Consulta de flota en modo cliente
En modo cliente, la aplicación MUST listar los vehículos con su kilometraje actual y, por cada vehículo, sus rutinas con estado, referencia del ciclo y límites.

#### Scenario: Rutinas de un vehículo
- **GIVEN** un vehículo con una rutina `PorVencer` y otra `AlDia`
- **WHEN** el cliente abre el detalle del vehículo
- **THEN** ve ambas rutinas con su estado diferenciado visualmente

### Requirement: Cotización visible en la orden
En cada orden, la aplicación MUST mostrar un bloque de cotización. Si no hay monto, MUST indicar que todavía no hay cotización. Si hay monto, MUST mostrarlo de forma destacada. Si la orden está `Cotizada`, MUST indicar que la decisión está pendiente del cliente.

#### Scenario: Orden sin cotización
- **GIVEN** una orden `Creada`
- **WHEN** el usuario la consulta
- **THEN** la aplicación indica que todavía no hay cotización

#### Scenario: Monto destacado
- **GIVEN** una orden con cotización de COP 2.000.000
- **WHEN** el usuario la consulta
- **THEN** el monto se muestra como cotización, distinguible del resto de los datos de la orden

### Requirement: Gestión de órdenes en modo cliente
En modo cliente, la aplicación MUST listar las órdenes con su estado y cotización, ofrecer aprobar o rechazar solo en órdenes `Cotizada` y ofrecer cancelar solo en órdenes `Creada`, `Cotizada` o `Aprobada`.

#### Scenario: Orden cotizada pendiente de decisión
- **GIVEN** una orden `Cotizada` de COP 2.000.000
- **WHEN** el cliente la consulta
- **THEN** la aplicación muestra el monto como cotización pendiente de decisión y ofrece aprobar, rechazar y cancelar

#### Scenario: Orden en ejecución
- **GIVEN** una orden `EnEjecucion`
- **WHEN** el cliente la consulta
- **THEN** la aplicación no ofrece aprobar, rechazar ni cancelar

### Requirement: Gestión de órdenes en modo taller
En modo taller, la aplicación MUST listar todas las órdenes y ofrecer registrar cotización solo en órdenes `Creada`, iniciar solo en órdenes `Aprobada` y completar solo en órdenes `EnEjecucion`, solicitando kilometraje y fecha reales.

#### Scenario: Registro de cotización
- **GIVEN** una orden `Creada`
- **WHEN** el taller ingresa un monto y confirma
- **THEN** la aplicación muestra la orden con su nuevo estado devuelto por el backend

#### Scenario: Completado con datos reales
- **GIVEN** una orden `EnEjecucion`
- **WHEN** el taller ingresa kilometraje y fecha reales y confirma
- **THEN** la aplicación muestra la orden `Completada`

### Requirement: Errores del backend visibles
Cuando el backend rechaza una operación, la aplicación MUST mostrar el motivo al usuario y volver a consultar el estado real del elemento afectado.

#### Scenario: Operación rechazada
- **GIVEN** una orden que otro usuario ya canceló
- **WHEN** el taller intenta iniciarla desde una vista desactualizada
- **THEN** la aplicación muestra el motivo del rechazo y actualiza la orden con su estado real
