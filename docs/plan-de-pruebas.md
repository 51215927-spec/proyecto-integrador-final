# Actividad 7 — Plan de pruebas y posibles fallos

Este plan de TI especifica resultados esperados. La ejecución y el registro de resultados corresponden a Taller de Programación. Los casos se ejecutan con datos aislados y se restaura el estado inicial antes de repetirlos.

**Caja negra** comprueba resultados observables por consola, API o contrato público del dominio. **Caja blanca** inspecciona el estado persistido o controla un punto de fallo para comprobar efectos que la respuesta por sí sola no demuestra.

Los resultados esperados se basan en la [consigna](consigna.md), las [reglas](reglas-del-juego.md), la [validación de personajes](reglas-del-juego.md#personajes-y-preparación) y el [contrato HTTP](protocolo-de-partida.md).

## Personajes y catálogo

Los vectores siguen el orden Fuerza, Velocidad, Resistencia, Inteligencia, Energía y Habilidad. En cada caso se mantiene válido el resto de los campos. Ejecutar cada vector de un caso por separado.

| Caso y técnica | Condición inicial y acción | Datos | Resultado esperado |
|---|---|---|---|
| P01 · Caja negra | Construir un personaje con nombre y ASCII válidos. | (600, 450, 700, 500, 400, 350) | Acepta el personaje y devuelve los seis puntajes ingresados. |
| P02 · Caja negra | Construir con total incorrecto. | (500, 450, 700, 500, 400, 350), total 2900 | Rechaza por el total distinto de 3000. |
| P03 · Caja negra | Construir con los límites exactos de rango. | (250, 750, 500, 500, 500, 500) | Acepta 250 y 750, ambos inclusive. |
| P04 · Caja negra | Construir con un solo valor fuera de rango. | (249, 750, 501, 500, 500, 500) y (250, 751, 499, 500, 500, 500) | Rechaza por el rango. Ambos vectores suman 3000 y cumplen los umbrales de especialización. |
| P05 · Caja negra | Construir en los umbrales exactos de especialización. | (650, 350, 500, 500, 500, 500) | Acepta 650 como alto y 350 como bajo. |
| P06 · Caja negra | Construir sin cumplir uno o ambos umbrales. | (649, 350, 501, 500, 500, 500), (650, 351, 499, 500, 500, 500) y (500, 500, 500, 500, 500, 500) | Rechaza, respectivamente, por falta de atributo alto, bajo o ambos. |
| P07 · Caja negra | Enviar un alta incompleta. | Solicitud sin un campo obligatorio | Responde con un error de entrada y conserva el catálogo vigente. |
| P08 · Caja negra | Cargar el catálogo obligatorio. | Ruta inexistente y archivo ilegible, en ejecuciones separadas | Informa el error de carga y el juego no comienza con un catálogo parcial. |
| P09 · Caja negra | Dar de alta un personaje con descripción vacía. | Descripción vacía, ASCII válido y puntajes P01 | Acepta el alta y conserva la descripción vacía. |
| P10 · Caja negra | Ingresar un puntaje no numérico en la consola. | Escribir `abc` en un atributo | Vuelve a pedir el atributo antes de enviar el alta. |

El catálogo de referencia se prepara sin duplicados, según la actividad 5. La ausencia de duplicados se verifica al preparar los datos de prueba.

## Mazo, combate y finalización

| Caso y técnica | Condición inicial y acción | Datos | Resultado esperado |
|---|---|---|---|
| J01 · Caja negra | Intentar iniciar con plazas presentes y un catálogo insuficiente. | 0 y 5 personajes válidos | Se informa que hacen falta seis personajes. La partida sigue esperando inicio y no entrega manos. |
| J02 · Caja negra | Con el mazo ya vacío, intentar repartir una carta más. | Mazo sin cartas y solicitud de reparto de 1 | El reparto se rechaza sin entregar cartas ni alterar la cantidad disponible. |
| J03 · Caja negra | Resolver un combate normal de la primera ronda. | J1 elige Fuerza, puntajes de J1 y J2 de 600 y 500 | Gana J1. Se consumen ambas cartas y J2 elige el atributo del combate siguiente si continúa la ronda. |
| J04 · Caja negra | Resolver el empate con J1 como elector y repetir con J2 como elector. | Ambos puntajes de Fuerza son 500 | Gana el jugador que no eligió el atributo, J2 y J1 respectivamente. |
| J05 · Caja negra | Resolver el tercer combate de una ronda empatada 1–1 usando las últimas cartas. | Marcador de partida 3–2, J1 gana el tercer combate | J1 suma la ronda. Ambas manos se reponen en la ronda siguiente y cambia el jugador mano. La partida continúa con marcador 4–2. |
| J06 · Caja blanca | Resolver dos combates seguidos a favor de J1 con ocho personajes iniciales. | Dos personajes quedaron en el mazo y cada jugador conserva una carta de su mano | Se cierra al segundo combate. Antes de repartir, la recuperación contiene exactamente los ocho identificadores iniciales una vez cada uno. |
| J07 · Caja negra | Resolver el combate que da la segunda victoria de una ronda al líder de la partida. | J1 tiene 14 rondas y gana la ronda actual | J1 alcanza 15 y la partida finaliza por victoria. No se abre otra ronda ni se emite una nueva solicitud. |
| J08 · Caja blanca | Iniciar con exactamente seis personajes y controlar las decisiones de mezcla para obtener un orden distinto del original. | Seis IDs diferentes y secuencia de mezcla que intercambia el primero con el último | El orden resultante refleja los intercambios fijados. Cada mano recibe tres IDs distintos, las manos son disjuntas y el mazo queda vacío. |

## Red local y consistencia HTTP

Usar dos plazas, una sesión por cliente y la URL configurada del servidor. Capturar la revisión y la solicitud de la presentación propia antes de cada envío. En los casos de pérdida de respuesta, el cliente conserva el identificador de la operación original.

| Caso y técnica | Condición inicial y acción | Datos | Resultado esperado |
|---|---|---|---|
| H01 · Caja negra | Intentar conectar sin servidor y luego iniciarlo. | Dirección configurada y TCP 5000 | El cliente informa la indisponibilidad y ofrece reintentar o salir. Tras iniciar el servidor, reintentar permite incorporarse. |
| H02 · Caja negra | J1 intenta elegir cuando la solicitud corresponde a J2. | Credencial válida de J1 y solicitud vigente de J2 | `409 solicitudNoVigente`. La solicitud de J2 y la revisión permanecen vigentes. |
| H03 · Caja negra | Enviar una jugada de un combate anterior con un identificador nuevo. | Solicitud consumida y revisión anterior | `409 revisionDesactualizada` o `solicitudNoVigente`, según la validación que detecte el desfase. La vista actual conserva mano, marcador y solicitud. |
| H04 · Caja blanca | Interrumpir la respuesta tras la confirmación de la elección y reenviar el mismo cuerpo. | Mismos UUID de operación y solicitud, revisión y elección originales | La consulta del recibo devuelve `200`. El reenvío devuelve el mismo recibo. Hay una sola operación persistida y un único incremento de revisión. |
| H05 · Caja negra | Reutilizar el UUID de una operación aceptada cambiando la selección. | Mismo UUID y otro identificador de personaje | `409 operacionIncompatible`. El recibo original permanece disponible. |
| H06 · Caja blanca | Enviar dos elecciones válidas simultáneas desde el titular de una solicitud. | UUID de operación distintos, misma solicitud y revisión, dos cartas permitidas | Una elección se confirma. La otra devuelve `409` si la solicitud ya se consumió, o `503` si la escritura está temporalmente ocupada. Si devuelve `503`, reenviar con el mismo UUID tras el primer commit devuelve `409`. Persiste un recibo y una sola transición, sin pérdida ni doble retirada de cartas. |
| H07 · Caja negra | Desconectar a J2 durante una elección pendiente de J1 durante más de 15 segundos. | Ambas plazas estaban activas. J1 mantiene su presencia | J1 ve la pausa con la misma revisión y sin solicitud visible. Un envío directo durante la pausa recibe `409 partidaPausada`. Al reincorporarse J2, la consola de J1 vuelve a mostrar la misma solicitud y mano, con la revisión intacta. |
| H08 · Caja blanca | Interrumpir el proceso servidor antes de confirmar la segunda elección del combate. | Fallo inducido después de preparar cambios y antes del commit | Al reiniciar permanecen el estado, la revisión y la solicitud previos. No hay recibo ni resultado parcial. Una vez reincorporados ambos clientes, reenviar confirma una sola elección. |
| H09 · Caja negra | Consultar ambas presentaciones antes y después de que el rival elija. | J1 ya eligió carta y atributo. J2 todavía debe elegir | Antes de resolver, J2 solo ve la mano propia y no puede deducir la decisión de J1 a partir de la respuesta. Después ambos ven las dos cartas, el atributo y el resultado. |
| H10 · Caja negra | Intentar tomar la plaza de J1 mientras su sesión sigue activa. | Credencial de J1 válida, UUID de sesión diferente | `409 sesionOcupada`. La sesión original conserva su plaza y su mano. Al vencer, J1 puede reincorporarse sin cambiar de rol. |
| H11 · Caja negra | Abandonar de forma explícita durante una partida activa y repetir el envío. | Solicitud de abandono de J2 | Devuelve `204` en ambos envíos. La partida queda finalizada por abandono, sin sumar una victoria ni emitir más solicitudes. |
| H12 · Caja blanca | Actualizar o desactivar un personaje del catálogo usado en una partida y recuperar el estado. | En una partida iniciada, el personaje sigue en una mano. Se cambia el catálogo y se reinicia el servidor | La partida conserva la copia y puntajes originales. Una nueva partida usa los datos vigentes y excluye los personajes desactivados. |

Para la ejecución registrar el ID del caso, versión probada, entorno, resultado observado, conformidad y evidencia. Para H01 y los demás casos que se ejecuten en red, registrar también los dos equipos, dirección del servidor y puerto. Los casos con fallo inducido requieren un entorno de pruebas controlado.

## Ejecución y evidencia

Desde la carpeta que contiene la solución ejecutá `dotnet test proyecto-integrador-final.sln`. El reporte indica cuáles casos se ejecutaron. Para la comprobación manual en red local, registrá los equipos, condiciones iniciales, solicitud enviada, resultado esperado y observado. [Proyecto de pruebas](../test/proyecto-integrador-final-tests/README.md) · [Verificación en dos equipos](protocolo-de-partida.md#verificación-en-dos-equipos)

## Entrada interactiva

El spec 003 se verifica con casos automatizados de configuración inicial, acciones administrativas, cinco variantes de demostración, corrección de campos de personaje y fin de entrada. Los contratos arrancan los ejecutables sin argumentos y responden por stdin. Para ejecutar también esos casos, compilá y definí las tres rutas de DLL indicadas en el [proyecto de pruebas](../test/proyecto-integrador-final-tests/README.md). La lectura oculta de credenciales se verifica adicionalmente en terminal interactiva. [Evidencia por criterio](plan-de-pruebas.md#evidencia-del-spec-003)

### Evidencia del spec 003

Verificación del 20 de noviembre de 2026 sobre `feature/entrada-interactiva`, commit `61c062f`, con .NET SDK 8.0.133 en Linux. La documentación de uso se prepara separadamente en `docs/entrada-interactiva`. La revisión de un compañero y la integración permanecen pendientes.

| Criterio | Evidencia | Resultado |
|---|---|---|
| CA1 | Contratos de procesos para los tres ejecutables, sin argumentos de aplicación ni variables de usuario; arranque adicional mediante `dotnet run --no-build --project src/<proyecto>`. | Aprobado. |
| CA2 | `ConfiguracionServidorTests`, `ConsolaServidorTests` y contratos del servidor: valores iniciales, configuración personalizada, alta deshabilitada, UUID y plaza por separado, inicio y recuperación ante una base inválida. Ensayo local con Enter verificó escucha en `0.0.0.0:5000` y archivo `guerra.db`. | Aprobado. |
| CA3 | `ClienteConsolaContratoTests` recorre victoria, pausa, abandono, cancelación y salida seleccionadas por stdin, más corrección de una opción inválida. | Aprobado. |
| CA4 | `ConsolaAltaTests` y contrato del ejecutable: URL y credencial antes del personaje, corrección de campos y alta HTTP `201` con ID. Ensayo con URL vacía confirmó el alta contra `localhost:5000`. | Aprobado. |
| CA5 | Pruebas de corrección y EOF durante configuración, menú, acciones y carga; no se confirma la acción incompleta. Una pseudoterminal verificó que servidor y carga no muestran la credencial ingresada. Los diagnósticos comprobados no contienen esa credencial. | Aprobado. |

Se ejecutó `dotnet build proyecto-integrador-final.sln`: cero errores y cero advertencias. Después se ejecutó `dotnet test proyecto-integrador-final.sln --no-build` con `GUERRA_CLIENTE_DLL`, `GUERRA_SERVIDOR_DLL` y `GUERRA_CARGA_DLL` apuntando a las DLL del worktree: **104 aprobadas, cero fallidas y cero omitidas**.

Evidencia de TDD: las nuevas pruebas fallaron primero por falta de preguntas administrativas, menú del cliente y configuración de carga; las firmas nuevas lanzaban `NotImplementedException`. Una prueba posterior reprodujo el cierre ante una base inválida antes de agregar el reintento. La suite original había aprobado sus 85 casos antes de los cambios. Tras implementar y simplificar la ayuda se obtuvo el resultado final anterior.

Las pruebas de este cambio se realizaron en una computadora Linux. No acreditan ejecución en Windows, comunicación entre dos equipos ni implementación del cliente REST; esas comprobaciones siguen en el plan de continuidad. `specs/` continúa como material local excluido de Git según la convención existente; su copia de seguimiento se mantiene en el worktree documental.

## Cliente REST (spec 001)

Verificación del 21 de noviembre de 2026 en Linux, .NET SDK 8.0.133, rama `feature/cliente-rest`, worktree `worktree-feature-cliente-rest`, cambios locales sobre `3408144`. La documentación se prepara por separado en `docs/cliente-rest`, worktree `worktree-docs-cliente-rest`. La implementación y las guías se publican en PR separados; las comprobaciones siguientes se ejecutaron sobre los cambios locales de esas ramas.

| Criterio | Comprobaciones | Resultado |
|---|---|---|
| CA1 | Configuración y corrección por campo; URL con prefijo; transporte con identidad estable y errores; incorporación de J1/J2 contra HTTP real; reapertura del ejecutable con sesión ocupada y posterior liberación. Pseudoterminal: credencial oculta. | Aprobado localmente. |
| CA2 | Presentaciones completas y pausa con igual revisión; privacidad de manos; historial inicial omitido y resultados nuevos ordenados; marcador histórico; Enter de resultado separado del turno; consultas y presencia durante menú, selección y lectura; vista idéntica sin reiniciar foco; renderizado angosto y controles remotos neutralizados. | Aprobado localmente. |
| CA3 | Apertura y respuesta; selección invalidada por pausa; una escritura pendiente; pérdidas antes/después del commit, consulta de recibo y reenvío idéntico; pausa con operación conservada; operación obsoleta o incompatible sin reenvío. | Aprobado localmente. |
| CA4 | Reincorporación con UUID estable, reinicio real del servidor y SQLite temporal, respuesta tardía descartada; Esc, No/Sí al abandono y abandono incierto serializado tras la elección; EOF y salida local sin abandono; victoria tras lectura del último resultado; cero tráfico nuevo al finalizar. Pseudoterminal: Esc y Ctrl+C durante configuración y reconexión. Tubería cerrada: código 1 y diagnóstico sin credencial. | Aprobado localmente. |

Desde el worktree de código se ejecutó:

```bash
dotnet build proyecto-integrador-final.sln --no-restore --nologo
GUERRA_CLIENTE_DLL="$PWD/src/cliente/bin/Debug/net8.0/cliente.dll" \
GUERRA_SERVIDOR_DLL="$PWD/src/servidor/bin/Debug/net8.0/servidor.dll" \
GUERRA_CARGA_DLL="$PWD/src/subir-personajes/bin/Debug/net8.0/subir-personajes.dll" \
dotnet test proyecto-integrador-final.sln --no-build --nologo
```

Resultado final: build con **cero errores y cero advertencias**; suite con **141 aprobadas, cero fallidas y cero omitidas**. Incluye servidor local y SQLite temporal, reloj sustituible, transporte controlado y procesos reales. Los logs de la sesión quedaron en `/tmp/spec001-entrega-build.log` y `/tmp/spec001-entrega-suite.log`.

Inventario de expectativas adaptadas: los cinco recorridos de demostración se conservan como pruebas directas del auxiliar; se retira el contrato del menú de demostración y se reemplaza por configuración REST, corrección de campos, incorporación, reapertura y salida del ejecutable. No hay selector oculto. La evidencia del spec 003 de arriba describe su versión histórica y no el arranque actual.

Se observaron ciclos rojo/verde en el arranque REST, estado, recorte de credencial y cierre de tubería; las pruebas adicionales de transporte, coordinación e integración se ampliaron junto con la implementación y después de ella. No se atribuye TDD completo a toda la suite.

La comprobación interactiva usó una pseudoterminal con respuesta a consultas de cursor: ingresar configuración, verificar que el secreto no aparece, abrir Esc y elegir salida; repetir con Ctrl+C en reconexión y en la primera pregunta. Los tres procesos terminaron con código 0. La entrada redirigida se comprueba también mediante los contratos de proceso. Se revisaron el diff y los enlaces locales de las guías.

Esta evidencia corresponde a una computadora Linux. No acredita Windows ni una partida entre dos equipos; esa validación permanece en el spec 002. No hubo cambios de API, reglas ni esquema SQLite. Para usar las guías junto al código deben incorporarse ambos PR, primero el cliente y después su documentación.

## Sistema de carga JSON (spec 005)

Verificado el 2026-11-22 en `feature/005-sistema-de-carga`, usando exclusivamente SQLite temporal. `CargaJsonTests` comprueba formato, tipos, duplicados, validación completa, reemplazo persistente y rollback ante un trigger que falla después del borrado. `ConsolaAltaTests` cubre rutas, cantidades, confirmación/cancelación, errores y resultado incierto sin reintentos. `EntradaProcesosContratoTests.Carga_RecorridoCompletoDesdeProcesoConBaseTemporal` ejecuta la herramienta con servidor HTTP real: catálogo vacío, seis ejemplos, cancelación, reemplazo, repetición y rechazo.

`dotnet restore` y `dotnet build proyecto-integrador-final.sln` aprobaron (0 advertencias y errores). `dotnet test proyecto-integrador-final.sln --no-build --verbosity quiet`, con las tres variables `GUERRA_*_DLL` apuntando a este mismo worktree, terminó con **179 aprobadas, 0 fallidas y 0 omitidas**. Se conserva la regresión de POST individual y juego. La respuesta perdida se simula con un handler HTTP; el rollback se comprueba con un fallo real de inserción en SQLite y reapertura. Esta validación local no acredita las pruebas entre dos equipos Windows del spec 002.
