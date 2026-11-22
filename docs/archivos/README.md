# Catálogo de archivos C#

## Versión documentada

Código final del spec 006: commit `f0f292b`, rama `refactor/006-separacion-responsabilidades`, **2026-11-22**. Documentación en la rama separada `docs/006-separacion-responsabilidades`, basada en ese commit. Los enlaces relativos apuntan a esa implementación dentro de esta revisión.

Hay **66 fuentes propios versionados y 66 páginas correspondientes**. Se incluyen aplicación, código compartido, herramientas, pruebas y auxiliares; se excluyen `bin/`, `obj/`, resultados y dependencias no versionados. Cada ruta se conserva bajo `docs/archivos/`, cambiando `.cs` por `.md`. Los tres `Program.cs` son independientes.

## Cómo leer y mantener las páginas

Cada página documenta responsabilidad, estado, miembros, dependencias, ejecución y límites. Los bloques `csharp` son fragmentos literales de la fuente enlazada, o el archivo completo cuando así se indica. Un recorrido deducido por lectura no representa una ejecución adicional; las pruebas garantizan sus aserciones concretas.

[Modelos](../modelos.md) reúne los desarrollos por fuente. Al cambiar código, actualizar página, modelos, relaciones e índice y repetir revisión de miembros, fragmentos y enlaces. La auditoría del spec 004 se conserva como historia fechada en [verificación](verificacion.md); el diseño anterior de RepositorioJuego y Jugar no describe el código vigente.

[Arquitectura](../arquitectura.md) · [Estructura](../estructura-del-repositorio.md) · [Uso](../instalacion-y-uso.md) · [Pruebas](../plan-de-pruebas.md) · [Auditoría006](verificacion.md#auditoría-del-spec-006--2026-11-22)

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
| [AdministradorSesiones.cs](src/servidor/Api/AdministradorSesiones.md) | [src/servidor/Api/AdministradorSesiones.cs](../../src/servidor/Api/AdministradorSesiones.cs) | Presencia e identidad temporal por plaza. |
| [AlmacenPartidas.cs](src/servidor/Api/AlmacenPartidas.md) | [src/servidor/Api/AlmacenPartidas.cs](../../src/servidor/Api/AlmacenPartidas.cs) | Reconstrucción y persistencia de partidas SQLite. |
| [BaseJuego.cs](src/servidor/Api/BaseJuego.md) | [src/servidor/Api/BaseJuego.cs](../../src/servidor/Api/BaseJuego.cs) | DbContext, claves compuestas y relaciones SQLite. |
| [CatalogoPersonajes.cs](src/servidor/Api/CatalogoPersonajes.md) | [src/servidor/Api/CatalogoPersonajes.cs](../../src/servidor/Api/CatalogoPersonajes.cs) | Administración y copia del catálogo. |
| [ConfiguracionServidor.cs](src/servidor/Api/ConfiguracionServidor.md) | [src/servidor/Api/ConfiguracionServidor.cs](../../src/servidor/Api/ConfiguracionServidor.cs) | Dirección de escucha, archivo de base y credencial de catálogo. |
| [ConsolaServidor.cs](src/servidor/Api/ConsolaServidor.md) | [src/servidor/Api/ConsolaServidor.cs](../../src/servidor/Api/ConsolaServidor.cs) | Comandos administrativos del operador. |
| [CoordinadorPartidas.cs](src/servidor/Api/CoordinadorPartidas.md) | [src/servidor/Api/CoordinadorPartidas.cs](../../src/servidor/Api/CoordinadorPartidas.cs) | Coordinación de acciones, acceso y recibos. |
| [EstadoPartida.cs](src/servidor/Api/EstadoPartida.md) | [src/servidor/Api/EstadoPartida.cs](../../src/servidor/Api/EstadoPartida.cs) | Estado reanudable e independiente de persistencia. |
| [LotePersonajes.cs](src/servidor/Api/LotePersonajes.md) | [src/servidor/Api/LotePersonajes.cs](../../src/servidor/Api/LotePersonajes.cs) | Lectura estricta de lista JSON y duplicados normalizados. |
| [PersonajeCatalogo.cs](src/servidor/Api/PersonajeCatalogo.md) | [src/servidor/Api/PersonajeCatalogo.cs](../../src/servidor/Api/PersonajeCatalogo.cs) | Entidad de catálogo, validación y conversión al dominio. |
| [PresentacionPartida.cs](src/servidor/Api/PresentacionPartida.md) | [src/servidor/Api/PresentacionPartida.cs](../../src/servidor/Api/PresentacionPartida.cs) | Contratos JSON de presentación. |
| [ServidorHttp.cs](src/servidor/Api/ServidorHttp.md) | [src/servidor/Api/ServidorHttp.cs](../../src/servidor/Api/ServidorHttp.cs) | Rutas HTTP y traducción del protocolo. |
| [SqliteJuego.cs](src/servidor/Api/SqliteJuego.md) | [src/servidor/Api/SqliteJuego.cs](../../src/servidor/Api/SqliteJuego.cs) | Conexiones, transacciones y exclusión compartida. |
| [UnidadPartida.cs](src/servidor/Api/UnidadPartida.md) | [src/servidor/Api/UnidadPartida.cs](../../src/servidor/Api/UnidadPartida.cs) | Estado y operaciones de una transacción cargada. |
| [VistaPartida.cs](src/servidor/Api/VistaPartida.md) | [src/servidor/Api/VistaPartida.cs](../../src/servidor/Api/VistaPartida.cs) | Proyección privada y sin efectos por plaza. |
## src/servidor

| Página | Fuente | Responsabilidad |
|---|---|---|
| [Combate.cs](src/servidor/Combate.md) | [src/servidor/Combate.cs](../../src/servidor/Combate.cs) | Consumo de cartas, comparación y desempate local. |
| [EstadoPartida.cs](src/servidor/EstadoPartida.md) | [src/servidor/EstadoPartida.cs](../../src/servidor/EstadoPartida.cs) | Estado reanudable e independiente de persistencia. |
| [Jugador.cs](src/servidor/Jugador.md) | [src/servidor/Jugador.cs](../../src/servidor/Jugador.cs) | Nombre, mano y contador local de rondas ganadas. |
| [Mazo.cs](src/servidor/Mazo.md) | [src/servidor/Mazo.cs](../../src/servidor/Mazo.cs) | Mezcla, reparto de prefijo y devolución de referencias. |
| [Partida.cs](src/servidor/Partida.md) | [src/servidor/Partida.cs](../../src/servidor/Partida.cs) | Dominio reanudable por acciones sucesivas. |
| [Personaje.cs](src/servidor/Personaje.md) | [src/servidor/Personaje.cs](../../src/servidor/Personaje.cs) | Datos de carta, seis atributos y validación numérica. |
| [Program.cs](src/servidor/Program.md) | [src/servidor/Program.cs](../../src/servidor/Program.cs) | Arranque y ciclo de vida del servidor. |
| [Ronda.cs](src/servidor/Ronda.md) | [src/servidor/Ronda.cs](../../src/servidor/Ronda.cs) | Reglas compartidas de ronda y alternancia. |
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
| [CompatibilidadPersistenciaTests.cs](test/proyecto-integrador-final-tests/CompatibilidadPersistenciaTests.md) | [test/proyecto-integrador-final-tests/CompatibilidadPersistenciaTests.cs](../../test/proyecto-integrador-final-tests/CompatibilidadPersistenciaTests.cs) | Escenarios, preparación, aserciones y límites de CompatibilidadPersistenciaTests. |
| [ConfiguracionClienteTests.cs](test/proyecto-integrador-final-tests/ConfiguracionClienteTests.md) | [test/proyecto-integrador-final-tests/ConfiguracionClienteTests.cs](../../test/proyecto-integrador-final-tests/ConfiguracionClienteTests.cs) | Escenarios, preparación, aserciones y límites de ConfiguracionCliente. |
| [ConfiguracionServidorTests.cs](test/proyecto-integrador-final-tests/ConfiguracionServidorTests.md) | [test/proyecto-integrador-final-tests/ConfiguracionServidorTests.cs](../../test/proyecto-integrador-final-tests/ConfiguracionServidorTests.cs) | Escenarios, preparación, aserciones y límites de ConfiguracionServidor. |
| [ConsolaAltaTests.cs](test/proyecto-integrador-final-tests/ConsolaAltaTests.md) | [test/proyecto-integrador-final-tests/ConsolaAltaTests.cs](../../test/proyecto-integrador-final-tests/ConsolaAltaTests.cs) | Escenarios, preparación, aserciones y límites de ConsolaAlta. |
| [ConsolaServidorTests.cs](test/proyecto-integrador-final-tests/ConsolaServidorTests.md) | [test/proyecto-integrador-final-tests/ConsolaServidorTests.cs](../../test/proyecto-integrador-final-tests/ConsolaServidorTests.cs) | Escenarios, preparación, aserciones y límites de ConsolaServidor. |
| [ContratoFactAttribute.cs](test/proyecto-integrador-final-tests/ContratoFactAttribute.md) | [test/proyecto-integrador-final-tests/ContratoFactAttribute.cs](../../test/proyecto-integrador-final-tests/ContratoFactAttribute.cs) | Condiciones de ejecución de pruebas de procesos. |
| [CoordinacionSesionesTests.cs](test/proyecto-integrador-final-tests/CoordinacionSesionesTests.md) | [test/proyecto-integrador-final-tests/CoordinacionSesionesTests.cs](../../test/proyecto-integrador-final-tests/CoordinacionSesionesTests.cs) | Escenarios, preparación, aserciones y límites de CoordinacionSesionesTests. |
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
| [VistaPartidaTests.cs](test/proyecto-integrador-final-tests/VistaPartidaTests.md) | [test/proyecto-integrador-final-tests/VistaPartidaTests.cs](../../test/proyecto-integrador-final-tests/VistaPartidaTests.cs) | Escenarios, preparación, aserciones y límites de VistaPartidaTests. |
