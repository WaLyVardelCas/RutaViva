# Design

## Context

Proyecto nuevo: el repositorio solo contiene OpenSpec. Motivación y alcance en `proposal.md`; comportamiento observable en `specs/`. Restricciones que dan forma al diseño:

- Persistencia exclusivamente en memoria; sin base de datos.
- API ASP.NET Core y proceso programado `RutaViva.Job` que reutiliza los mismos comandos.
- Sin autenticación, sin notificaciones reales, sin MediatR ni AutoMapper. CQRS con un mediador propio (D28).
- Debe poder desarrollarse, probarse y explicarse en una jornada.

## Goals / Non-Goals

**Goals:**
- Reglas de negocio solo en el dominio; API y Job son disparadores delgados.
- Invariantes protegidas por los agregados y probadas sin infraestructura.
- Idempotencia y seguridad ante concurrencia demostrables con pruebas.
- Cada decisión con procedencia trazable (enunciado, evaluador o decisión técnica).

**Non-Goals:**
- Outbox, mensajería, reintentos distribuidos o procesos separados reales.
- Historial de versiones de cotización o edición de intervalos.
- Camino de salida para una rutina `Vencida` cuya orden fue cancelada.
- Ayudas de demostración: bitácora de eventos por HTTP, datos semilla y pantallas para simular telemetría o disparar el proceso diario. Esos flujos se prueban a mano, con Swagger o un archivo `.http`.

## Procedencia de las decisiones

Cada decisión se etiqueta con su origen:

- **[E] Enunciado**: regla escrita en el enunciado original.
- **[V] Evaluador**: respuesta explícita del evaluador a una pregunta de la exploración.
- **[T] Técnica**: decisión nuestra, tomada donde el evaluador dejó libertad o donde el enunciado no dice nada.

| # | Decisión | Origen | Respuesta que la habilita |
|---|---|---|---|
| D1 | La rutina permanece `Vencida` si su orden se cancela o se rechaza | T | V1: post-cancelación fuera de alcance, a criterio |
| D2 | No se genera automáticamente otra orden tras la cancelación | T | V1 |
| D3 | Criterios de kilometraje y tiempo evaluados independientemente; gana el más avanzado | E (reglas 3, 7), reafirmado por V2 | V2 |
| D4 | `PorVencer` con 10 % o menos restante; `Vencida` al alcanzar el límite | E (reglas 5, 6), reafirmado por V2 | V2 |
| D5 | La telemetría evalúa solo el kilometraje; el proceso diario evalúa solo el tiempo | E (reglas 9, 10), reafirmado por V2 | V2 |
| D6 | Se permite `AlDia` a `Vencida` directo; sin retroceso dentro del ciclo | T (interpretación del enunciado) | V3 |
| D7 | Un salto directo a `Vencida` no registra evento de próximo vencimiento | T (interpretación de la regla 8) | V3 |
| D8 | Rutinas con nombre libre, único por vehículo | T | V4 |
| D9 | Intervalo temporal en meses calendario; umbral = 10 % de los días reales del intervalo | T | V5 |
| D10 | Una cotización por orden, inmutable; registrarla equivale a enviarla al cliente | T | V6 |
| D11 | Edición de intervalos fuera de alcance | T | V6 |
| D12 | Aplicación Angular única con simulación de roles cliente y taller | E + T | V7 |
| D13 | Un único taller simulado que ve todas las órdenes | T | V7 |
| D14 | `RutaViva.Job` es una biblioteca con un `BackgroundService` en el proceso de la API | T | Confirmada en la exploración |
| D15 | API y Job comparten las mismas instancias de repositorios en memoria | T | Consecuencia de D14 |
| D16 | `POST /api/procesos/evaluacion-diaria` con fecha obligatoria ejecuta el mismo comando | T | Confirmada |
| D17 | El primer ciclo toma el kilometraje actual y la fecha de configuración | T | Confirmada: se fija al configurar la rutina |
| D18 | El kilometraje actual del vehículo cambia por telemetría y por el completado (nunca baja); el completado rechaza un kilometraje real menor al del vehículo | T | Revisada: las rutinas no pueden quedar con un kilometraje distinto al del vehículo |
| D19 | El completado se rechaza si el kilometraje o la fecha real son anteriores a la referencia del ciclo | T | Consecuencia de la regla 14 |
| D20 | La orden pasa a `Cotizada` solo si el monto requiere aprobación del cliente; si no, va de `Creada` a `Aprobada` | T | Confirmada |
| D21 | Placa única; la telemetría identifica el vehículo por la placa y el kilometraje viene en km | T | Confirmada junto con D24 |
| D22 | "Hoy" del proceso diario = fecha en America/Bogota | T | — |
| D23 | Concurrencia optimista con snapshots: una copia que no coincide no se persiste | T | Confirmada |
| D24 | Cada lectura trae un identificador de evento, aplicado como máximo una vez en la frontera | T | Confirmada |
| D25 | El cliente registra vehículos y consulta las rutinas; el taller las configura | T | El usuario movió el alta de rutinas al taller. El registro sigue en la interfaz, no solo por API |
| D26 | La telemetría y la evaluación diaria no tienen pantalla; la demo de esos flujos es manual por HTTP | T | Confirmada |
| D27 | La fecha real del completado no tiene tope superior ni se compara con el reloj | T | Confirmada |
| D28 | Las escrituras son comandos y las lecturas son consultas, despachadas por un mediador propio | T | El usuario pidió CQRS. MediatR sigue fuera: el despacho de eventos de dominio no cambia |

## Cambios respecto a la exploración inicial

La exploración solo existió en la conversación; estos son los puntos que cambiaron al incorporar las respuestas:

| # | Antes (exploración) | Ahora | Por qué |
|---|---|---|---|
| C1 | Recomendaba un único `Evaluar(km, hoy)` que revisara ambos criterios en cada disparador | La telemetría evalúa solo kilometraje y el proceso diario solo tiempo (D5) | V2 pide seguir el enunciado, y las reglas 9 y 10 asignan cada criterio a un disparador |
| C2 | Al reiniciar el ciclo, reevaluar de inmediato con el kilometraje actual | La rutina queda `AlDia` y la siguiente lectura o ejecución diaria la avanza | La regla 14 dice "regresar a `AlDia`"; V3 pide interpretar la progresión según el enunciado |
| C3 | Post-cancelación: cuatro opciones abiertas | D1 y D2 | V1 lo deja a criterio; se eligió la opción que no inventa reglas |
| C4 | Duda entre días y meses para el intervalo temporal | Meses calendario y umbral sobre días reales (D9) | V5. El ejemplo de 6 meses / 18 días sigue cumpliéndose (181 días, umbral 18,1) |
| C5 | `Cotizacion` como VO condicionado a cómo se modificara | VO inmutable, una por orden (D10) | V6 |
| C6 | Tres alternativas para alojar el Job | D14 y D15 | Elegido por el usuario |
| C7 | Endpoint de evaluación con fecha opcional | Fecha obligatoria (D16) | Decisión confirmada: "recibirá una fecha simulada". Evita que el endpoint y el proceso diario se confundan |
| C8 | Banderas `PorVencerNotificado` y `VencidaNotificada` en el ciclo | Eliminadas | Con D6 (sin retroceso) cada evento sale de una transición que solo ocurre una vez por ciclo; las banderas eran redundantes |
| C9 | El completado validaba contra la rutina leyendo el agregado `Vehiculo` | La orden guarda una copia de la referencia del ciclo y se valida sola (D19) | Evita leer otro agregado dentro de la operación y mantiene la regla en un solo lugar |
| C10 | `GET /api/eventos`, datos semilla y panel de simulación quedaban pendientes | Fuera de alcance (D26) | El usuario pidió una demo manual, sin hacer de más |
| C11 | Emitir o no `RutinaPorVencer` en un salto directo: pendiente | No se emite (D7) | La rutina nunca estuvo en `PorVencer`; la regla 8 limita a "una sola vez", no exige que ocurra |
| C12 | El registro de vehículos y rutinas era solo por API | También se hace en la vista cliente (D25) | El usuario lo pidió de forma explícita |
| C13 | El contrato de telemetría no tenía identificador de evento | Identificador obligatorio, aplicado una sola vez (D24) | El usuario lo pidió. La idempotencia por kilometraje se mantiene para eventos distintos |
| C14 | El completado "no toca el kilometraje" sin decir quién sí lo toca | Solo una lectura de telemetría mayor lo actualiza (D18) | El usuario: depende de lo que llegue en la telemetría |
| C15 | La concurrencia optimista era una propuesta, con lock global como plan B | Queda confirmada y el lock global se descarta (D23) | El usuario pidió impedir el cambio cuando la copia no coincide |
| C16 | La vista cliente registraba vehículos y rutinas (D25) | El cliente registra vehículos y solo consulta rutinas; el taller las configura. La cotización se muestra en un bloque propio de cada orden | El usuario pidió separar quién crea y quién ve las rutinas, y hacer visible que existe una cotización |
| C17 | La aplicación exponía un caso de uso por clase, invocado directo desde el endpoint | CQRS (D28): comando o consulta más su manejador. API y Job hablan solo con `IMediador` | El usuario pidió el patrón. No se agrega MediatR |

## Decisions

### Modelo de dominio

```
+--------------------------------------------+          +----------------------------------------+
| <<Aggregate Root>> Vehiculo                |          | <<Aggregate Root>> OrdenServicio       |
|--------------------------------------------|          |----------------------------------------|
| Id                                         |   id     | Id                                     |
| Placa : Placa (VO)                         |<---------| VehiculoId                             |
| KilometrajeActual : Kilometraje (VO)       |          | RutinaId                     ---+      |
| Version                                    |   id     | Referencia : ReferenciaCiclo (VO)   |  |
|                                            |<----------------------------------------------+  |
|  +--------------------------------------+  |          |   NumeroCiclo, KmRef, FechaRef         |
|  | <<Entity>> RutinaMantenimiento   0..*|  |          | Estado : EstadoOrden (enum)            |
|  |--------------------------------------|  |          | Cotizacion? : Cotizacion (VO)          |
|  | Id, Nombre                           |  |          |   Monto : Dinero (VO), FechaRegistro   |
|  | Intervalo : IntervaloMantenimiento   |  |          | AprobacionAutomatica : bool?           |
|  |   Km? , Meses?           (VO)        |  |          | Ejecucion? : EjecucionServicio (VO)    |
|  | Ciclo : CicloMantenimiento (VO)      |  |          |   KmReal, FechaReal                    |
|  |   Numero, KmRef, FechaRef            |  |          | Version                                |
|  | Estado : EstadoRutina (enum)         |  |          +----------------------------------------+
|  +--------------------------------------+  |
+--------------------------------------------+
```

- **`Vehiculo` (Aggregate Root)** protege el kilometraje monótono y la evaluación de sus rutinas en la misma transacción. Alternativa descartada: rutina como agregado propio, porque cada lectura modificaría N+1 agregados.
- **`RutinaMantenimiento` (entidad del agregado `Vehiculo`)** tiene identidad (la orden la referencia), pero no tiene un ciclo de vida independiente del vehículo.
- **`OrdenServicio` (Aggregate Root)** tiene su propio ciclo de vida, otros actores y una máquina de estados. Referencia a `Vehiculo` y a la rutina solo por identidad.
- **Value Objects:**
  - `IntervaloMantenimiento`: al menos un criterio, todos > 0.
  - `CicloMantenimiento`: se reemplaza completo al reiniciar.
  - `ReferenciaCiclo`: copia en la orden del número y la referencia del ciclo que venció.
  - `Cotizacion`.
  - `Dinero`: decimal, moneda COP implícita, no negativo.
  - `Kilometraje`: entero ≥ 0.
  - `Placa`: texto normalizado (sin espacios, en mayúsculas).
  - `EjecucionServicio`.
- **Estados como `enum`.** Las transiciones viven en métodos del agregado con guardas. El patrón State se descartó porque son 6 estados con guardas simples.
- **El cálculo del estado de un criterio** es una función pura del intervalo, la referencia y el valor evaluado (km o fecha). Se prueba sin agregados. No requiere un servicio de dominio.
- **La política de aprobación** es una constante del dominio (COP 1.500.000; `>` requiere aprobación). No se hace configurable porque nada lo exige.

### Invariantes y dónde viven

| Invariante | Responsable |
|---|---|
| Kilometraje actual nunca disminuye | `Vehiculo` |
| Intervalo con al menos un criterio positivo; inmutable | `IntervaloMantenimiento` |
| Nombre de rutina único por vehículo | `Vehiculo` |
| Estado de la rutina no retrocede dentro del ciclo | `RutinaMantenimiento` |
| Un evento `PorVencer` y uno `Vencida` como máximo por ciclo | Consecuencia de la invariante anterior |
| Transiciones válidas de la orden | `OrdenServicio` |
| Cotización única, inmutable, monto > 0 | `OrdenServicio` + `Cotizacion` |
| Kilometraje y fecha reales ≥ referencia del ciclo | `OrdenServicio` (vía `ReferenciaCiclo`) |
| Una sola orden abierta por vehículo y rutina | Cruza agregados: generación única por ciclo + índice único en el repositorio |
| Un identificador de evento de telemetría se aplica como máximo una vez | Frontera de telemetría, no el agregado |
| Una copia cuya versión no coincide con la almacenada no se persiste | Repositorio |

### Ciclo de vida de la rutina

```
            criterio evaluado <= 10% restante
  +-------+ --------------------------------> +-----------+
  | AlDia |   [registra RutinaPorVencer]      | PorVencer |
  +-------+                                   +-----------+
     ^  |                                           |
     |  | criterio evaluado alcanza el limite       | criterio evaluado alcanza el limite
     |  | [registra RutinaVencida]                  | [registra RutinaVencida]
     |  v                                           v
     |  +-------------------------------------------------+
     |  |                    Vencida                      |  orden cancelada/rechazada:
     |  +-------------------------------------------------+  permanece Vencida (D1, D2)
     |                         |
     +-------------------------+
       ReiniciarCiclo (orden completada): Numero+1, referencia = km/fecha reales,
       estado AlDia, sin reevaluacion inmediata (C2)
```

Disparadores: una lectura aceptada evalúa solo el criterio de kilometraje y el proceso diario evalúa solo el de tiempo (D5). El nuevo estado es el más avanzado entre el estado actual y el resultado del criterio evaluado. Así, una evaluación favorable nunca retrocede la rutina.

### Ciclo de vida de la orden

```
                cotizar > 1.5M                 aprobar (cliente)
  +--------+ -----------------> +----------+ ------------------> +----------+   iniciar    +-------------+
  | Creada |                    | Cotizada |                     | Aprobada | -----------> | EnEjecucion |
  +--------+ ------------------------------------------------->  +----------+   (taller)   +-------------+
      |        cotizar <= 1.5M: aprobacion automatica (D20)           |                         |
      |                              |                                |                         | completar(km, fecha)
      |                              | rechazar (cliente)             |                         v
      |  cancelar (cliente)          v          cancelar (cliente)    |                  +------------+
      +-------------------------> +-----------+ <---------------------+                  | Completada |
                 cancelar ------> | Cancelada |                                          +------------+
                 desde Cotizada   +-----------+
```

`EnEjecucion`, `Completada` y `Cancelada` no admiten cancelación. `Completada` y `Cancelada` son terminales.

**D20, confirmada.** La orden solo permanece en `Cotizada` cuando el monto supera COP 1.500.000 y hace falta la decisión del cliente. Con un monto menor o igual, `Cotizar()` termina en `Aprobada` dentro de la misma operación. Alternativa descartada: persistir `Cotizada` y aprobar en un segundo paso. Crea un estado en el que la orden nunca permanece y muestra al cliente una cotización pendiente que no requiere su decisión. El paso por la cotización queda en los eventos `OrdenCotizada` y `CotizacionAprobada` (automática).

**D18, revisada.** Completar el servicio rechaza un kilometraje real menor al kilometraje actual del vehículo. Si es mayor, el kilometraje actual del vehículo sube a ese valor, igual que con una lectura de telemetría. Así la referencia del ciclo nuevo coincide siempre con el kilometraje del vehículo y no queda por encima ni por debajo de él.

### Arquitectura y dependencias

```
                      +-------------------+
                      |  RutaViva.Domain  |  sin dependencias externas
                      +-------------------+
                                ^
                      +----------------------+
                      | RutaViva.Application |  comandos, consultas, manejadores, puertos, DTOs
                      +----------------------+
                        ^         ^        ^
                        |         |        |
   +-------------------------+    |    +--------------+
   | RutaViva.Infrastructure |    |    | RutaViva.Job |  BackgroundService -> comando EvaluarRutinasPorTiempo
   | repos en memoria,       |    |    +--------------+
   | bitacora, mediador      |    |           ^
   +-------------------------+    |           |
                ^                 |           |
                +-------- RutaViva.Api -------+   composition root: DI, endpoints,
                          (un solo proceso)        ACL de telemetria, ProblemDetails
```

- **Domain:** entidades, VOs, eventos de dominio y reglas. No lee el reloj: recibe la fecha como parámetro.
- **Application:** un comando o una consulta por escritura o lectura, con su manejador. Sin reglas de negocio. Flujo del manejador: cargar, invocar el dominio, guardar, despachar eventos. También aloja los handlers de eventos de dominio, los puertos (`IRepositorioVehiculos`, `IRepositorioOrdenes`, `IPublicadorEventos`, `IMediador`), el reloj (`TimeProvider`, que viene en el BCL) y los reintentos por concurrencia.
- **Infrastructure:** repositorios en memoria, registro de identificadores de telemetría ya procesados, bitácora de eventos y el mediador que resuelve el manejador del comando o de la consulta.
- **Api:**
  - endpoints;
  - ACL de telemetría en una carpeta propia: el contrato externo se traduce a un comando interno;
  - mapeo de errores: datos inválidos → 400, no encontrado → 404, transición, regla violada o conflicto de concurrencia no resuelto → 409;
  - CORS para Angular;
  - registro del `BackgroundService` de `RutaViva.Job`.
- **Job:** solo envía el comando `EvaluarRutinasPorTiempo(hoy)` por el mediador. Se ejecuta al iniciar el proceso y luego cada 24 horas; la idempotencia hace inofensiva una ejecución extra tras un reinicio.

No se agregan proyectos `Shared` ni `Contracts`. Alternativa descartada: Worker independiente, porque no compartiría memoria con la API (C6). Tampoco se agrega MediatR: el mediador propio resuelve el manejador por el tipo del mensaje.

**D28.** Comandos: `RegistrarVehiculo`, `AgregarRutina`, `RegistrarLecturaKilometraje`, `EvaluarRutinasPorTiempo`, `RegistrarCotizacion`, `AprobarCotizacion`, `RechazarCotizacion`, `CancelarOrden`, `IniciarServicio`, `CompletarServicio`. Consultas: vehículos, un vehículo, rutinas, órdenes y una orden. API y Job no conocen los manejadores; solo `IMediador`.

### Endpoints

```
POST /api/vehiculos                         GET /api/vehiculos[/{id}]
POST /api/vehiculos/{id}/rutinas            GET /api/vehiculos/{id}/rutinas
POST /api/telemetria/lecturas               { idEvento, placa, kilometraje, fechaLectura }
                                            -> 200 { resultado: Aceptada | Ignorada }
GET  /api/ordenes?estado=&vehiculoId=       GET /api/ordenes/{id}
POST /api/ordenes/{id}/cotizacion           POST /api/ordenes/{id}/aprobacion
POST /api/ordenes/{id}/rechazo              POST /api/ordenes/{id}/cancelacion
POST /api/ordenes/{id}/inicio               POST /api/ordenes/{id}/completado
POST /api/procesos/evaluacion-diaria        body { fecha: "YYYY-MM-DD" } obligatorio
```

### Eventos de dominio y comunicación entre agregados

| Evento | Origen | Cuándo | Consumidores |
|---|---|---|---|
| `RutinaPorVencer` | Vehiculo | Transición a `PorVencer` | Bitácora |
| `RutinaVencida` (VehiculoId, RutinaId, ReferenciaCiclo) | Vehiculo | Transición a `Vencida` | Handler: generar orden; bitácora |
| `CicloRutinaReiniciado` | Vehiculo | Reinicio de ciclo | Bitácora |
| `OrdenServicioGenerada` | OrdenServicio | Creación | Bitácora |
| `OrdenCotizada`, `CotizacionAprobada` (automática o del cliente), `CotizacionRechazada`, `OrdenCancelada`, `ServicioIniciado` | OrdenServicio | En cada transición | Bitácora |
| `OrdenServicioCompletada` (RutinaId, NumeroCiclo, KmReal, FechaReal) | OrdenServicio | Completado | Handler: reiniciar ciclo; bitácora |

Mecánica:
- Los agregados acumulan eventos.
- El manejador del comando guarda y, **solo después de guardar**, un despachador propio en el proceso los entrega a los handlers registrados por DI. Ese despacho no pasa por el mediador de CQRS. MediatR se descartó porque no aporta nada a este alcance.
- Un reintento por concurrencia no duplica eventos, porque solo se despachan los del intento que logró guardar.

Idempotencia de los handlers:
- **Generar orden:** si ya hay una orden abierta para el par vehículo y rutina, no hace nada (el índice único del repositorio es la red de seguridad).
- **Reiniciar ciclo:** si el número de ciclo de la rutina ya no coincide con el del evento, no hace nada.

Eventos de integración: no se crean clases de integración separadas mientras no exista un consumidor externo. La bitácora (`IPublicadorEventos`) es el punto donde se conectarían las notificaciones reales. La telemetría entrante sí tiene su propio contrato externo, aislado por la ACL; el dominio nunca lo ve.

### Persistencia en memoria y concurrencia (D23, confirmada)

- Los repositorios son **singleton** y los comparten la API y el Job (D15).
- Guardan **snapshots** (copias) de cada agregado con su `Version`. Cada operación trabaja sobre su propia copia. `Guardar` compara la versión esperada con la de la copia almacenada. Si no coinciden, **no escribe**: la copia obsoleta nunca pisa el estado guardado.
- El manejador, ante ese rechazo, recarga la copia vigente y vuelve a aplicar las reglas de dominio, hasta 3 veces. El reintento no fusiona ni fuerza el cambio. Si sobre la copia vigente la operación ya no es válida (por ejemplo, la rutina ya no admite esa transición), se rechaza con 409 y el estado almacenado queda como estaba. Si los 3 intentos pierden la carrera, también se rechaza con 409.
- Dos lecturas concurrentes siguen convergiendo: la que pierde recarga el kilometraje ya guardado y la regla monótona decide si se acepta o se ignora. El resultado observable es el de procesarlas una tras otra.
- `IRepositorioOrdenes.Agregar` verifica y agrega de forma atómica la unicidad de orden abierta. Es el equivalente a un índice único filtrado.
- El registro de identificadores de telemetría guarda, de forma atómica, el resultado de cada `idEvento` solo después de aplicar la lectura. Una repetición devuelve ese resultado y no entra al agregado. Si la aplicación falla, el identificador no queda marcado como procesado, así que un reintento puede volver a intentarlo. Dos llegadas simultáneas del mismo identificador se resuelven con una reserva atómica: una aplica y la otra espera el resultado.
- La bitácora es una lista en memoria segura para concurrencia, más un registro en `ILogger`. No se expone por HTTP (D26).

Alternativas descartadas: el lock global, porque esconde el conflicto en vez de impedirlo; el lock por vehículo, porque no protege las lecturas de colecciones compartidas; guardar la referencia viva del agregado, porque los cambios se filtrarían sin llamar a `Guardar`. El identificador de evento no vive dentro de `Vehiculo`: el agregado crecería con cada lectura y el contrato externo entraría al dominio.

### Dependencias externas

- Ninguna en Domain ni Application.
- Pruebas: xUnit; `Microsoft.AspNetCore.Mvc.Testing` (pruebas de integración HTTP); Moq solo donde un fake real no sirve, por ejemplo para forzar la falla de un vehículo en la evaluación diaria. El `FakeTimeProvider` se escribe a mano o se toma de `Microsoft.Extensions.TimeProvider.Testing`.
- Frontend: Angular con standalone components y servicios HTTP. Sin NgRx.

### Estrategia de pruebas

Proyectos: `RutaViva.Domain.Tests`, `RutaViva.Application.Tests` y `RutaViva.Api.IntegrationTests`. Los escenarios de `specs/` son los criterios de aceptación; cada uno se cubre al menos en un nivel.

| Área | Nivel | Casos clave |
|---|---|---|
| Criterio de kilometraje | Dominio | 28.999 `AlDia`, 29.000 `PorVencer`, 30.000 `Vencida` |
| Criterio de tiempo | Dominio | 19 días `AlDia`, 18 días `PorVencer`, 0 días `Vencida`; 31-ene + 1 mes = 28-feb; umbral sobre días reales |
| Ambos criterios | Dominio | El más avanzado gana; criterios independientes |
| Progresión | Dominio | Salto directo a `Vencida`; una evaluación favorable no retrocede |
| Notificación única | Dominio | Un `RutinaPorVencer` por ciclo; ninguno en el salto directo; ninguno al entrar el segundo criterio |
| Lecturas | Dominio + Application | Igual, menor, mayor; otro evento con el mismo km; el mismo `idEvento` repetido, también con otro cuerpo; `idEvento` ausente; fecha anterior con mayor km; concurrentes con `Task.WhenAll` |
| Disparadores separados | Application | La lectura no evalúa el tiempo; el proceso diario no evalúa el kilometraje |
| Orden única | Application | Vencimiento reprocesado; lecturas concurrentes que vencen; nueva orden tras completar |
| Cotización | Dominio | 1.500.000 automática; 1.500.001 `Cotizada`; monto 0; segunda cotización rechazada |
| Aprobar, rechazar, cancelar | Dominio | Cada estado permitido y no permitido |
| Iniciar, completar | Dominio | Fuera de estado; sin datos; km o fecha anteriores a la referencia |
| Reinicio de ciclo | Application | Queda `AlDia` con la nueva referencia; el km del vehículo no cambia; reentrega del evento sin efecto |
| Cancelación | Application | Rutina sigue `Vencida`; evaluaciones posteriores no generan orden |
| Proceso diario | Application | Dos ejecuciones el mismo día; fecha anterior; falla aislada de un vehículo (Moq) |
| Concurrencia | Application | Una copia con versión vieja no pisa la almacenada; si al recargar la operación ya no es válida, responde 409 y el estado no cambia |
| Registro en la interfaz | Portal | Alta de vehículo en modo cliente; alta de rutina en modo taller; el cliente consulta rutinas y no las crea; placa duplicada muestra el error; la cotización se distingue en la orden |
| Flujos HTTP | Integración | Flujo feliz completo; rechazo; lectura ignorada con 200; el mismo `idEvento` no aplica dos veces; evaluación con fecha inválida → 400; transición inválida → 409 |

Flujos de aceptación de la demostración. El registro, la flota y las órdenes se recorren en la interfaz. Las lecturas y la evaluación diaria se envían a mano, con Swagger o un archivo `.http` (D26):

1. En la vista cliente, registrar el vehículo. En la vista taller, configurar la rutina. Por HTTP, lectura al 90 % → `PorVencer`; lectura al límite → orden `Creada`. En la vista taller, cotización de COP 900.000 → `Aprobada` → iniciar → completar. La rutina vuelve a `AlDia`.
2. Igual hasta la orden, con cotización de COP 2.000.000 → `Cotizada`. En la vista cliente, rechazar → `Cancelada`. La rutina sigue `Vencida` y no aparece otra orden.
3. Rutina por tiempo. Por HTTP, evaluación diaria con fecha simulada → `PorVencer` y luego `Vencida`. Repetir la misma fecha no produce efectos nuevos.

## Risks / Trade-offs

- [Rutina `Vencida` sin salida tras cancelar su orden (D1, D2)] → Es la consecuencia aceptada de dejar fuera de alcance la post-cancelación. La interfaz muestra "Vencida, sin orden abierta". Resolverlo sería un change futuro.
- [El despacho de eventos después de guardar no es atómico: si un handler falla, el primer agregado ya quedó guardado] → Handlers idempotentes y fallas registradas. El outbox queda fuera de alcance.
- [Datos perdidos al reiniciar el proceso] → Aceptado por la restricción de persistencia en memoria.
- [El Job no se despliega por separado (D14)] → Cuando exista persistencia real, basta un host Worker que registre el mismo `BackgroundService`.
- [Fechas simuladas frente al reloj real] → Como los estados no retroceden, la ejecución diaria real con una fecha anterior a una simulada no deshace nada. La fecha real del completado no se compara contra el reloj (D27, confirmada).
- [El identificador de evento se resuelve fuera del agregado] → Si el proceso se reinicia, el registro en memoria se pierde junto con el resto de los datos. Un cambio de contrato de telemetría solo toca la frontera.
- [Contención en el agregado `Vehiculo`] → Aceptable con pocas rutinas por vehículo. La concurrencia optimista lo hace visible y medible.
- [Licencia de Moq (SponsorLink en 4.20)] → Uso mínimo; NSubstitute como reemplazo directo.

## Migration Plan

No aplica: proyecto nuevo, sin datos ni despliegue.

## Cierre de la segunda revisión

Las ocho preguntas abiertas quedaron respondidas. Ninguna sigue bloqueando `tasks.md`. Qué cambió:

| # | Antes | Ahora | Por qué |
|---|---|---|---|
| 1 | D17 propuesta, sin confirmación | El primer ciclo se fija al configurar la rutina, con el kilometraje y la fecha de ese momento | El usuario lo confirmó. La spec de rutinas ya lo decía; no cambió su comportamiento |
| 2 | El completado no escribe el kilometraje, sin decir la fuente | El kilometraje actual solo sube cuando la telemetría trae un valor mayor (D18) | El usuario: depende de lo que llegue en la telemetría |
| 3 | Fecha real sin tope, planteada como duda | Confirmada (D27). Solo se rechaza si es anterior a la referencia del ciclo | El usuario lo aceptó |
| 4 | D20 propuesta | `Cotizada` solo cuando el cliente debe aprobar. Hasta COP 1.500.000 la orden pasa a `Aprobada` en la misma operación | El usuario lo confirmó. Se hizo explícito en la spec de órdenes |
| 5 | Concurrencia optimista, con lock global como plan B | Confirmada (D23). Si la copia no coincide, el cambio no se guarda | El usuario lo pidió. El reintento recarga y reaplica; no fuerza la copia vieja |
| 6 | Bitácora HTTP, datos semilla y panel de simulación, pendientes | Fuera de alcance (D26). Demo manual por HTTP | El usuario pidió no hacer de más |
| 7 | Registro de vehículos y rutinas solo por API (non-goal) | La vista cliente registra ambos (D25) | El usuario lo pidió. Es el único non-goal que se revirtió |
| 8 | Identificador de evento, desconocido | Obligatorio (D24). La frontera lo aplica una sola vez; el dominio no lo ve | El usuario lo pidió |

No queda ninguna ambigüedad que cambie las specs, el enfoque o el desglose de tareas.

Pueden esperar, sin cambiar el enfoque:

- Si los montos en COP se guardan con decimales. Hoy se aceptan decimales y se muestran sin centavos.
- La hora exacta del proceso diario. Hoy corre al iniciar el proceso y luego cada 24 horas.
