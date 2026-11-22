# Catálogo de archivos C#

## Versión documentada

Fuentes de la rama `main`, commit [`135daa7eb196b9bca4e40a1aef76e1131a9b24e8`](https://github.com/51215927-spec/proyecto-integrador-final/tree/135daa7eb196b9bca4e40a1aef76e1131a9b24e8), fijados el **2026-11-22**. La documentación se prepara en `docs/spec-004-documetacion-final`. Los cambios posteriores de esta rama son documentales; los enlaces relativos al fuente permiten navegar la misma revisión del PR.

Hay **57 fuentes propios versionados y 57 páginas correspondientes**. Se incluyen aplicación, código compartido, herramientas, pruebas y auxiliares; se excluyen `bin/`, `obj/`, resultados y dependencias no versionados. No hay fuentes generados versionados en esta referencia. Cada ruta se conserva bajo `docs/archivos/` cambiando solo `.cs` por `.md`. Los tres `Program.cs` son independientes.

## Cómo leer y mantener las páginas

Cada página identifica proyecto, namespace, tipos, estado, miembros, dependencias, recorridos y límites. Sus fragmentos `csharp` se toman del archivo enlazado; las omisiones de contexto se explican fuera del código. Un ejemplo deducido por lectura no se presenta como ejecución. Las garantías de una prueba son las de sus aserciones, no las que su nombre pueda sugerir.

`docs/modelos.md` reúne el desarrollo por archivo y tipo y los conceptos compartidos. Al cambiar código, actualizar la página, su desarrollo en modelos, las relaciones y este catálogo, y repetir revisión de miembros, fragmentos y enlaces contra una nueva referencia común. Tras mover o eliminar un fuente, actualizar también su ruta documental; no dejar páginas obsoletas en el catálogo vigente.

[Modelos](../modelos.md) · [Arquitectura](../arquitectura.md) · [Estructura](../estructura-del-repositorio.md) · [Uso](../instalacion-y-uso.md) · [Evidencia de ejecución](../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004) · [Revisión documental](verificacion.md)

## src/cliente

| Página | Fuente | Responsabilidad |
|---|---|---|
| [ClientePartidaHttp.cs](src/cliente/ClientePartidaHttp.md) | [src/cliente/ClientePartidaHttp.cs](../../src/cliente/ClientePartidaHttp.cs) | Peticiones autenticadas, DTO y clasificación de fallos de transporte. |
| [ConfiguracionCliente.cs](src/cliente/ConfiguracionCliente.md) | [src/cliente/ConfiguracionCliente.cs](../../src/cliente/ConfiguracionCliente.cs) | Preguntas cancelables y validación de conexión e identidad. |
| [ContratosPartida.cs](src/cliente/ContratosPartida.md) | [src/cliente/ContratosPartida.cs](../../src/cliente/ContratosPartida.cs) | Mensajes JSON propios del cliente, sin referencia al servidor. |
| [DemostracionConsola.cs](src/cliente/DemostracionConsola.md) | [src/cliente/DemostracionConsola.cs](../../src/cliente/DemostracionConsola.cs) | Recorridos simulados internos conservados para pruebas. |
| [EntradaCliente.cs](src/cliente/EntradaCliente.md) | [src/cliente/EntradaCliente.cs](../../src/cliente/EntradaCliente.cs) | Eventos cancelables de teclas o líneas y descarte de entrada sobrante. |
| [EstadoCliente.cs](src/cliente/EstadoCliente.md) | [src/cliente/EstadoCliente.cs](../../src/cliente/EstadoCliente.cs) | Validación de vistas completas y cola de resultados nuevos. |
| [InterfazConsola.Red.cs](src/cliente/InterfazConsola.Red.md) | [src/cliente/InterfazConsola.Red.cs](../../src/cliente/InterfazConsola.Red.cs) | Adaptación de vistas HTTP, menús y marcadores históricos. |
| [InterfazConsola.cs](src/cliente/InterfazConsola.md) | [src/cliente/InterfazConsola.cs](../../src/cliente/InterfazConsola.cs) | Composición Spectre.Console y selección de opciones. |
| [Program.cs](src/cliente/Program.md) | [src/cliente/Program.cs](../../src/cliente/Program.cs) | Arranque REST, cancelación, recursos y códigos de salida. |
| [SesionPartidaCliente.cs](src/cliente/SesionPartidaCliente.md) | [src/cliente/SesionPartidaCliente.cs](../../src/cliente/SesionPartidaCliente.cs) | Bucle de coordinación, presencia, recuperación, elecciones y salida. |
| [VistasConsola.cs](src/cliente/VistasConsola.md) | [src/cliente/VistasConsola.cs](../../src/cliente/VistasConsola.cs) | Tipos visibles de cartas, mesa y resultados. |
## src/comun

| Página | Fuente | Responsabilidad |
|---|---|---|
| [EntradaConsola.cs](src/comun/EntradaConsola.md) | [src/comun/EntradaConsola.cs](../../src/comun/EntradaConsola.cs) | Lectura compartida, secretos, URL y fin de entrada. |
## src/servidor/Api

| Página | Fuente | Responsabilidad |
|---|---|---|
| [BaseJuego.cs](src/servidor/Api/BaseJuego.md) | [src/servidor/Api/BaseJuego.cs](../../src/servidor/Api/BaseJuego.cs) | DbContext, claves compuestas y relaciones SQLite. |
| [ConfiguracionServidor.cs](src/servidor/Api/ConfiguracionServidor.md) | [src/servidor/Api/ConfiguracionServidor.cs](../../src/servidor/Api/ConfiguracionServidor.cs) | Dirección de escucha, archivo de base y credencial de catálogo. |
| [ConsolaServidor.cs](src/servidor/Api/ConsolaServidor.md) | [src/servidor/Api/ConsolaServidor.cs](../../src/servidor/Api/ConsolaServidor.cs) | Comandos administrativos y preguntas por identificadores. |
| [EstadoPartida.cs](src/servidor/Api/EstadoPartida.md) | [src/servidor/Api/EstadoPartida.cs](../../src/servidor/Api/EstadoPartida.cs) | Entidades persistidas, contratos de elección, recibos y errores. |
| [LotePersonajes.cs](src/servidor/Api/LotePersonajes.md) | [src/servidor/Api/LotePersonajes.cs](../../src/servidor/Api/LotePersonajes.cs) | Lectura estricta de lista JSON y duplicados normalizados. |
| [PersonajeCatalogo.cs](src/servidor/Api/PersonajeCatalogo.md) | [src/servidor/Api/PersonajeCatalogo.cs](../../src/servidor/Api/PersonajeCatalogo.cs) | Entidad de catálogo, validación y conversión al dominio. |
| [PresentacionPartida.cs](src/servidor/Api/PresentacionPartida.md) | [src/servidor/Api/PresentacionPartida.cs](../../src/servidor/Api/PresentacionPartida.cs) | Vista privada por plaza e historial revelado. |
| [RepositorioJuego.cs](src/servidor/Api/RepositorioJuego.md) | [src/servidor/Api/RepositorioJuego.cs](../../src/servidor/Api/RepositorioJuego.cs) | Catálogo, credenciales, sesiones y transacciones. |
| [ServidorHttp.cs](src/servidor/Api/ServidorHttp.md) | [src/servidor/Api/ServidorHttp.cs](../../src/servidor/Api/ServidorHttp.cs) | Host REST, autenticación, rutas y traducción de errores. |
| [TransicionesPartida.cs](src/servidor/Api/TransicionesPartida.md) | [src/servidor/Api/TransicionesPartida.cs](../../src/servidor/Api/TransicionesPartida.cs) | Inicio, idempotencia, solicitudes, combate, reparto y final. |
## src/servidor

| Página | Fuente | Responsabilidad |
|---|---|---|
| [Combate.cs](src/servidor/Combate.md) | [src/servidor/Combate.cs](../../src/servidor/Combate.cs) | Consumo de cartas, comparación y desempate local. |
| [Jugador.cs](src/servidor/Jugador.md) | [src/servidor/Jugador.cs](../../src/servidor/Jugador.cs) | Nombre, mano y contador local de rondas ganadas. |
| [Mazo.cs](src/servidor/Mazo.md) | [src/servidor/Mazo.cs](../../src/servidor/Mazo.cs) | Mezcla, reparto de prefijo y devolución de referencias. |
| [Partida.cs](src/servidor/Partida.md) | [src/servidor/Partida.cs](../../src/servidor/Partida.cs) | Coordinador local incompleto y sus efectos antes del fallo. |
| [Personaje.cs](src/servidor/Personaje.md) | [src/servidor/Personaje.cs](../../src/servidor/Personaje.cs) | Datos de carta, seis atributos y validación numérica. |
| [Program.cs](src/servidor/Program.md) | [src/servidor/Program.cs](../../src/servidor/Program.cs) | Arranque del host, reconfiguración y consola administrativa. |
| [Ronda.cs](src/servidor/Ronda.md) | [src/servidor/Ronda.cs](../../src/servidor/Ronda.cs) | Registro de combates, alternancia y umbral de dos triunfos. |
## src/subir-personajes

| Página | Fuente | Responsabilidad |
|---|---|---|
| [Program.cs](src/subir-personajes/Program.md) | [src/subir-personajes/Program.cs](../../src/subir-personajes/Program.cs) | Ruta JSON, validación, resumen, confirmación y único PUT. |
## test/proyecto-integrador-final-tests

| Página | Fuente | Responsabilidad |
|---|---|---|
| [ApiHttpTests.cs](test/proyecto-integrador-final-tests/ApiHttpTests.md) | [test/proyecto-integrador-final-tests/ApiHttpTests.cs](../../test/proyecto-integrador-final-tests/ApiHttpTests.cs) | Escenarios, preparación, aserciones y límites de ApiHttp. |
| [ArranqueRestTests.cs](test/proyecto-integrador-final-tests/ArranqueRestTests.md) | [test/proyecto-integrador-final-tests/ArranqueRestTests.cs](../../test/proyecto-integrador-final-tests/ArranqueRestTests.cs) | Escenarios, preparación, aserciones y límites de ArranqueRest. |
| [CargaJsonTests.cs](test/proyecto-integrador-final-tests/CargaJsonTests.md) | [test/proyecto-integrador-final-tests/CargaJsonTests.cs](../../test/proyecto-integrador-final-tests/CargaJsonTests.cs) | Escenarios, preparación, aserciones y límites de CargaJson. |
| [CartasDePrueba.cs](test/proyecto-integrador-final-tests/CartasDePrueba.md) | [test/proyecto-integrador-final-tests/CartasDePrueba.cs](../../test/proyecto-integrador-final-tests/CartasDePrueba.cs) | Factoría de cartas válidas para los escenarios del dominio. |
| [ClienteConsolaContratoTests.cs](test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.md) | [test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs](../../test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs) | Escenarios, preparación, aserciones y límites de ClienteConsolaContrato. |
| [ClienteProcesoRestTests.cs](test/proyecto-integrador-final-tests/ClienteProcesoRestTests.md) | [test/proyecto-integrador-final-tests/ClienteProcesoRestTests.cs](../../test/proyecto-integrador-final-tests/ClienteProcesoRestTests.cs) | Escenarios, preparación, aserciones y límites de ClienteProcesoRest. |
| [ClienteRestIntegracionTests.cs](test/proyecto-integrador-final-tests/ClienteRestIntegracionTests.md) | [test/proyecto-integrador-final-tests/ClienteRestIntegracionTests.cs](../../test/proyecto-integrador-final-tests/ClienteRestIntegracionTests.cs) | Escenarios, preparación, aserciones y límites de ClienteRestIntegracion. |
| [CombateTests.cs](test/proyecto-integrador-final-tests/CombateTests.md) | [test/proyecto-integrador-final-tests/CombateTests.cs](../../test/proyecto-integrador-final-tests/CombateTests.cs) | Escenarios, preparación, aserciones y límites de Combate. |
| [ConfiguracionClienteTests.cs](test/proyecto-integrador-final-tests/ConfiguracionClienteTests.md) | [test/proyecto-integrador-final-tests/ConfiguracionClienteTests.cs](../../test/proyecto-integrador-final-tests/ConfiguracionClienteTests.cs) | Escenarios, preparación, aserciones y límites de ConfiguracionCliente. |
| [ConfiguracionServidorTests.cs](test/proyecto-integrador-final-tests/ConfiguracionServidorTests.md) | [test/proyecto-integrador-final-tests/ConfiguracionServidorTests.cs](../../test/proyecto-integrador-final-tests/ConfiguracionServidorTests.cs) | Escenarios, preparación, aserciones y límites de ConfiguracionServidor. |
| [ConsolaAltaTests.cs](test/proyecto-integrador-final-tests/ConsolaAltaTests.md) | [test/proyecto-integrador-final-tests/ConsolaAltaTests.cs](../../test/proyecto-integrador-final-tests/ConsolaAltaTests.cs) | Escenarios, preparación, aserciones y límites de ConsolaAlta. |
| [ConsolaServidorTests.cs](test/proyecto-integrador-final-tests/ConsolaServidorTests.md) | [test/proyecto-integrador-final-tests/ConsolaServidorTests.cs](../../test/proyecto-integrador-final-tests/ConsolaServidorTests.cs) | Escenarios, preparación, aserciones y límites de ConsolaServidor. |
| [ContratoFactAttribute.cs](test/proyecto-integrador-final-tests/ContratoFactAttribute.md) | [test/proyecto-integrador-final-tests/ContratoFactAttribute.cs](../../test/proyecto-integrador-final-tests/ContratoFactAttribute.cs) | Condiciones de ejecución de pruebas de procesos. |
| [EntradaProcesosContratoTests.cs](test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) | [test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.cs](../../test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.cs) | Escenarios, preparación, aserciones y límites de EntradaProcesosContrato. |
| [EstadoClienteTests.cs](test/proyecto-integrador-final-tests/EstadoClienteTests.md) | [test/proyecto-integrador-final-tests/EstadoClienteTests.cs](../../test/proyecto-integrador-final-tests/EstadoClienteTests.cs) | Escenarios, preparación, aserciones y límites de EstadoCliente. |
| [IntegracionRespuestaPerdidaTests.cs](test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.md) | [test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.cs](../../test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.cs) | Escenarios, preparación, aserciones y límites de IntegracionRespuestaPerdida. |
| [InterfazConsolaTests.cs](test/proyecto-integrador-final-tests/InterfazConsolaTests.md) | [test/proyecto-integrador-final-tests/InterfazConsolaTests.cs](../../test/proyecto-integrador-final-tests/InterfazConsolaTests.cs) | Escenarios, preparación, aserciones y límites de InterfazConsola. |
| [JugadorTests.cs](test/proyecto-integrador-final-tests/JugadorTests.md) | [test/proyecto-integrador-final-tests/JugadorTests.cs](../../test/proyecto-integrador-final-tests/JugadorTests.cs) | Escenarios, preparación, aserciones y límites de Jugador. |
| [MazoTests.cs](test/proyecto-integrador-final-tests/MazoTests.md) | [test/proyecto-integrador-final-tests/MazoTests.cs](../../test/proyecto-integrador-final-tests/MazoTests.cs) | Escenarios, preparación, aserciones y límites de Mazo. |
| [PantallasConsolaTests.cs](test/proyecto-integrador-final-tests/PantallasConsolaTests.md) | [test/proyecto-integrador-final-tests/PantallasConsolaTests.cs](../../test/proyecto-integrador-final-tests/PantallasConsolaTests.cs) | Escenarios, preparación, aserciones y límites de PantallasConsola. |
| [PartidaTests.cs](test/proyecto-integrador-final-tests/PartidaTests.md) | [test/proyecto-integrador-final-tests/PartidaTests.cs](../../test/proyecto-integrador-final-tests/PartidaTests.cs) | Escenarios, preparación, aserciones y límites de Partida. |
| [PersonajeTests.cs](test/proyecto-integrador-final-tests/PersonajeTests.md) | [test/proyecto-integrador-final-tests/PersonajeTests.cs](../../test/proyecto-integrador-final-tests/PersonajeTests.cs) | Escenarios, preparación, aserciones y límites de Personaje. |
| [PresentacionRedTests.cs](test/proyecto-integrador-final-tests/PresentacionRedTests.md) | [test/proyecto-integrador-final-tests/PresentacionRedTests.cs](../../test/proyecto-integrador-final-tests/PresentacionRedTests.cs) | Escenarios, preparación, aserciones y límites de PresentacionRed. |
| [RondaTests.cs](test/proyecto-integrador-final-tests/RondaTests.md) | [test/proyecto-integrador-final-tests/RondaTests.cs](../../test/proyecto-integrador-final-tests/RondaTests.cs) | Escenarios, preparación, aserciones y límites de Ronda. |
| [ServidorContratoTests.cs](test/proyecto-integrador-final-tests/ServidorContratoTests.md) | [test/proyecto-integrador-final-tests/ServidorContratoTests.cs](../../test/proyecto-integrador-final-tests/ServidorContratoTests.cs) | Escenarios, preparación, aserciones y límites de ServidorContrato. |
| [SesionClienteTests.cs](test/proyecto-integrador-final-tests/SesionClienteTests.md) | [test/proyecto-integrador-final-tests/SesionClienteTests.cs](../../test/proyecto-integrador-final-tests/SesionClienteTests.cs) | Escenarios, preparación, aserciones y límites de SesionCliente. |
| [TransporteClienteTests.cs](test/proyecto-integrador-final-tests/TransporteClienteTests.md) | [test/proyecto-integrador-final-tests/TransporteClienteTests.cs](../../test/proyecto-integrador-final-tests/TransporteClienteTests.cs) | Escenarios, preparación, aserciones y límites de TransporteCliente. |

Las pruebas físicas entre dos Windows siguen pendientes en el spec 002. La suite local aprobada no sustituye esa comprobación. El límite de `Partida.Jugar()` se documenta como comportamiento observado, sin cambiar el código.
