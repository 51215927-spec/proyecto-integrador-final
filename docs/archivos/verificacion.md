# Verificación documental e historia

## Auditoría del spec 006 — 2026-11-22

Referencia de código `f0f292b`; documentación separada en `docs/006-separacion-responsabilidades`. Inventario actual: **66 fuentes C# y 66 páginas**, incluyendo ocho nuevos archivos de servidor y tres nuevos archivos de pruebas, con retirada de las páginas de RepositorioJuego y TransicionesPartida. Las fuentes sin cambios conservan desarrollo contrastado; se actualizaron sus relaciones cuando cambió el componente responsable.

Las páginas de componentes nuevos/modificados describen sus miembros y el flujo real. Los bloques nuevos reproducen fragmentos o archivos completos del commit final; los de pruebas de integración se actualizaron sin retirar aserciones. El catálogo distingue EstadoPartida de dominio y Api/EstadoPartida de persistencia/contratos.

Evidencia funcional informada por la validación de implementación: build correcto y **190 pruebas aprobadas, cero fallidas/omitidas**, con tres DLL del mismo worktree; baseline histórico 179. CompatibilidadPersistenciaTests usa SQL/JSON generado por e18a050, reabre sin migración, recupera recibo y continúa sin alterar esquema. CoordinacionSesionesTests comprueba invalidación concurrente; VistaPartidaTests lectura/privacidad/pausa.

Controles documentales: comparación uno a uno de fuentes/páginas, revisión de miembros contra fuente, comprobación literal de bloques C# y enlaces relativos/anclas; el cierre global registra los resultados en el spec canónico. La auditoría histórica que sigue corresponde exclusivamente al spec 004 y sus cifras/restricciones antiguas no son estado vigente. La integración física de dos Windows del spec 002 sigue pendiente. No se acredita render visual del UML ni instalación WinGet.

## Auditoría histórica del spec 004 — 2026-11-22


### Referencia y alcance

Revisión del 2026-11-22 para [main en 135daa7eb196b9bca4e40a1aef76e1131a9b24e8](https://github.com/51215927-spec/proyecto-integrador-final/tree/135daa7eb196b9bca4e40a1aef76e1131a9b24e8). Se trabaja en `docs/spec-004-documetacion-final`, en `/home/julian/.herdr/worktrees/proyecto-integrador-final/docs-spec-004-documetacion-final`. Los specs canónicos son locales, excluidos por Git y permanecen en `/home/julian/proyectos/escolares/proyecto-integrador-final/specs/`, rama main; no se copiaron a este worktree.

La auditoría abarca todos los archivos, no una muestra. Primero se leyó cada fuente, después se contrastaron sus tipos, datos y miembros con la explicación y se hizo una segunda lectura por miembro de las pruebas. Cliente y API tuvieron revisión cruzada entre responsables; la revisión de las pruebas detectó y corrigió atribuciones de cobertura excesivas (fase antes de opciones inválidas, ganador no asertado y EOF distinto de Enter). No se cambió código para ajustar la documentación.

### Inventario y cobertura por archivo

57 fuentes versionados → 57 páginas, sin faltantes, duplicados ni páginas obsoletas. Exclusiones: artefactos no versionados de bin/obj, resultados y dependencias externas. No hay fuentes generados deliberadamente versionados en esta referencia. Los tres Program conservan sus rutas y nombres. Catálogo y evidencia son guías, no páginas que representen otro fuente.

La tabla cuenta declaraciones explícitas obtenidas con el parser Roslyn del SDK 8.0.133: tipos; métodos/constructores explícitos/funciones locales; campos/propiedades/constantes de enum. Los parámetros de constructores primarios y records se revisaron en sus declaraciones/tablas, sin contarlos como propiedades explícitas nuevas. Getters/setters automáticos y miembros generados se explican con su tipo. Que un nombre aparezca en una página es un control mecánico de inventario, **no** sustituye la lectura de su explicación y condiciones.

| Fuente y página | Tipos | Métodos, constructores y locales | Datos explícitos | Revisión de profundidad |
|---|---:|---:|---:|---|
| [src/cliente/ClientePartidaHttp.cs](src/cliente/ClientePartidaHttp.md) | 2 | 9 | 6 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/cliente/ConfiguracionCliente.cs](src/cliente/ConfiguracionCliente.md) | 1 | 5 | 0 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/cliente/ContratosPartida.cs](src/cliente/ContratosPartida.md) | 8 | 0 | 0 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/cliente/DemostracionConsola.cs](src/cliente/DemostracionConsola.md) | 1 | 4 | 0 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/cliente/EntradaCliente.cs](src/cliente/EntradaCliente.md) | 1 | 3 | 2 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/cliente/EstadoCliente.cs](src/cliente/EstadoCliente.md) | 1 | 5 | 4 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/cliente/InterfazConsola.Red.cs](src/cliente/InterfazConsola.Red.md) | 1 | 4 | 0 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/cliente/InterfazConsola.cs](src/cliente/InterfazConsola.md) | 1 | 43 | 12 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/cliente/Program.cs](src/cliente/Program.md) | 1 | 2 | 0 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/cliente/SesionPartidaCliente.cs](src/cliente/SesionPartidaCliente.md) | 1 | 12 | 38 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/cliente/VistasConsola.cs](src/cliente/VistasConsola.md) | 6 | 0 | 3 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/comun/EntradaConsola.cs](src/comun/EntradaConsola.md) | 1 | 6 | 0 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/servidor/Api/BaseJuego.cs](src/servidor/Api/BaseJuego.md) | 3 | 1 | 22 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/servidor/Api/ConfiguracionServidor.cs](src/servidor/Api/ConfiguracionServidor.md) | 1 | 2 | 0 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/servidor/Api/ConsolaServidor.cs](src/servidor/Api/ConsolaServidor.md) | 1 | 3 | 1 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/servidor/Api/EstadoPartida.cs](src/servidor/Api/EstadoPartida.md) | 9 | 5 | 44 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/servidor/Api/LotePersonajes.cs](src/servidor/Api/LotePersonajes.md) | 1 | 4 | 0 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/servidor/Api/PersonajeCatalogo.cs](src/servidor/Api/PersonajeCatalogo.md) | 2 | 1 | 11 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/servidor/Api/PresentacionPartida.cs](src/servidor/Api/PresentacionPartida.md) | 4 | 1 | 0 | Aprobada: autor y revisión cruzada cliente/api. |
| `src/servidor/Api/RepositorioJuego.cs` (referencia histórica) | 4 | 21 | 5 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/servidor/Api/ServidorHttp.cs](src/servidor/Api/ServidorHttp.md) | 2 | 8 | 2 | Aprobada: autor y revisión cruzada cliente/api. |
| `src/servidor/Api/TransicionesPartida.cs` (referencia histórica) | 1 | 14 | 1 | Aprobada: autor y revisión cruzada cliente/api. |
| [src/servidor/Combate.cs](src/servidor/Combate.md) | 1 | 12 | 6 | Aprobada: lectura completa y contraste de fuente. |
| [src/servidor/Jugador.cs](src/servidor/Jugador.md) | 1 | 9 | 3 | Aprobada: lectura completa y contraste de fuente. |
| [src/servidor/Mazo.cs](src/servidor/Mazo.md) | 1 | 5 | 1 | Aprobada: lectura completa y contraste de fuente. |
| [src/servidor/Partida.cs](src/servidor/Partida.md) | 1 | 8 | 6 | Aprobada: lectura completa y contraste de fuente. |
| [src/servidor/Personaje.cs](src/servidor/Personaje.md) | 2 | 5 | 14 | Aprobada: lectura completa y contraste de fuente. |
| [src/servidor/Program.cs](src/servidor/Program.md) | 1 | 1 | 0 | Aprobada: lectura completa y contraste de fuente. |
| [src/servidor/Ronda.cs](src/servidor/Ronda.md) | 1 | 6 | 4 | Aprobada: lectura completa y contraste de fuente. |
| [src/subir-personajes/Program.cs](src/subir-personajes/Program.md) | 3 | 8 | 0 | Aprobada: lectura completa y contraste de fuente. |
| [test/proyecto-integrador-final-tests/ApiHttpTests.cs](test/proyecto-integrador-final-tests/ApiHttpTests.md) | 2 | 16 | 1 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/ArranqueRestTests.cs](test/proyecto-integrador-final-tests/ArranqueRestTests.md) | 1 | 2 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/CargaJsonTests.cs](test/proyecto-integrador-final-tests/CargaJsonTests.md) | 1 | 9 | 1 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/CartasDePrueba.cs](test/proyecto-integrador-final-tests/CartasDePrueba.md) | 1 | 1 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs](test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.md) | 1 | 7 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/ClienteProcesoRestTests.cs](test/proyecto-integrador-final-tests/ClienteProcesoRestTests.md) | 2 | 8 | 6 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/ClienteRestIntegracionTests.cs](test/proyecto-integrador-final-tests/ClienteRestIntegracionTests.md) | 1 | 2 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/CombateTests.cs](test/proyecto-integrador-final-tests/CombateTests.md) | 1 | 5 | 2 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/ConfiguracionClienteTests.cs](test/proyecto-integrador-final-tests/ConfiguracionClienteTests.md) | 1 | 2 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/ConfiguracionServidorTests.cs](test/proyecto-integrador-final-tests/ConfiguracionServidorTests.md) | 1 | 3 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/ConsolaAltaTests.cs](test/proyecto-integrador-final-tests/ConsolaAltaTests.md) | 3 | 7 | 2 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/ConsolaServidorTests.cs](test/proyecto-integrador-final-tests/ConsolaServidorTests.md) | 1 | 5 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/ContratoFactAttribute.cs](test/proyecto-integrador-final-tests/ContratoFactAttribute.md) | 1 | 1 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.cs](test/proyecto-integrador-final-tests/EntradaProcesosContratoTests.md) | 1 | 9 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/EstadoClienteTests.cs](test/proyecto-integrador-final-tests/EstadoClienteTests.md) | 1 | 5 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.cs](test/proyecto-integrador-final-tests/IntegracionRespuestaPerdidaTests.md) | 4 | 12 | 7 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/InterfazConsolaTests.cs](test/proyecto-integrador-final-tests/InterfazConsolaTests.md) | 1 | 9 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/JugadorTests.cs](test/proyecto-integrador-final-tests/JugadorTests.md) | 1 | 2 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/MazoTests.cs](test/proyecto-integrador-final-tests/MazoTests.md) | 1 | 2 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/PantallasConsolaTests.cs](test/proyecto-integrador-final-tests/PantallasConsolaTests.md) | 1 | 5 | 2 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/PartidaTests.cs](test/proyecto-integrador-final-tests/PartidaTests.md) | 1 | 1 | 2 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/PersonajeTests.cs](test/proyecto-integrador-final-tests/PersonajeTests.md) | 1 | 5 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/PresentacionRedTests.cs](test/proyecto-integrador-final-tests/PresentacionRedTests.md) | 1 | 1 | 0 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/RondaTests.cs](test/proyecto-integrador-final-tests/RondaTests.md) | 1 | 4 | 2 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/ServidorContratoTests.cs](test/proyecto-integrador-final-tests/ServidorContratoTests.md) | 4 | 27 | 8 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/SesionClienteTests.cs](test/proyecto-integrador-final-tests/SesionClienteTests.md) | 5 | 30 | 24 | Aprobada: autor y segunda lectura por miembro. |
| [test/proyecto-integrador-final-tests/TransporteClienteTests.cs](test/proyecto-integrador-final-tests/TransporteClienteTests.md) | 2 | 3 | 0 | Aprobada: autor y segunda lectura por miembro. |

### Documentos generales y diagramas

Se revisaron todos los Markdown versionados de la referencia y sus diagramas. Las páginas nuevas y modelos se contrastaron con las fuentes del inventario anterior; esta tabla cubre los documentos preexistentes.

| Documento | Resultado de revisión |
|---|---|
| [.github/pull_request_template.md](../../.github/pull_request_template.md) | Vigente; PR conserva propósito, cambios, verificación y pendientes. |
| [CONTRIBUTING.md](../../CONTRIBUTING.md) | Vigente; rama docs/, cambios exclusivamente documentales, mensajes sin tildes y un archivo por commit. No se integra sin revisión. |
| [README.md](../../README.md) | Actualizado: estado implementado y acceso al catálogo. |
| [docs/arquitectura.md](../../docs/arquitectura.md) | Actualizada: carga JSON y separación entre coordinador local incompleto y REST persistido. |
| [docs/bases-de-datos.md](../../docs/bases-de-datos.md) | Actualizada: reemplazo atómico del catálogo sin borrar copias de partidas. |
| [docs/consigna.md](../../docs/consigna.md) | Vigente como fuente académica; preservada sin adaptar requisitos al código. |
| [docs/diagramas/juego-guerra-propuestas.html](../../docs/diagramas/juego-guerra-propuestas.html) | Vigente como comparador histórico de maquetas; 12 estados y propuestas con IDs locales. No se presenta como aplicación ni ejecución. |
| [docs/diagramas/uml.uxf](../../docs/diagramas/uml.uxf) | Actualizado: firmas del modelo real, límite local, responsabilidades HTTP/JSON/SQLite y relaciones. XML parseado; no se acredita render visual en UMLetino. |
| [docs/diseno-de-interfaz.md](../../docs/diseno-de-interfaz.md) | Actualizado: enlaza coordinación y renderizado implementados; mantiene maquetas como propuestas. |
| [docs/estructura-del-repositorio.md](../../docs/estructura-del-repositorio.md) | Actualizada: correspondencia inequívoca fuente/página y mantenimiento. |
| [docs/instalacion-y-uso.md](../../docs/instalacion-y-uso.md) | Actualizada: aclara cómo habilitar todos los contratos de procesos. Rutas y flujo JSON contrastados; WinGet no ejecutado. |
| [docs/modelos.md](../../docs/modelos.md) | Actualizado: desarrollo completo de los 57 archivos y sus tipos; mismos fragmentos/explicaciones de páginas individuales, con enlaces recalculados. |
| [docs/plan-de-pruebas.md](../../docs/plan-de-pruebas.md) | Actualizado: resultado real de build/suite, comando, directorio, versión y limitaciones; evidencia anterior preservada como histórica. |
| [docs/plan.md](../../docs/plan.md) | Actualizado: carga JSON sustituye formulario manual; ensayo físico sigue pendiente. |
| [docs/protocolo-de-partida.md](../../docs/protocolo-de-partida.md) | Actualizado: POST individual conservado, cargador usa resumen/PUT; recuperación ya implementada. |
| [docs/reglas-del-juego.md](../../docs/reglas-del-juego.md) | Vigentes: atributos, dos combates, quince rondas, alternancias, privacidad y catálogo JSON contrastados. Condición de dos Windows sigue sin acreditarse. |
| [docs/tecnologias.md](../../docs/tecnologias.md) | Actualizadas responsabilidades de HttpClient; dependencias verificadas en csproj. |
| [src/cliente/README.md](../../src/cliente/README.md) | Vigente: arranque REST, navegación, lectura, salida local y recuperación sin persistencia. |
| [src/servidor/README.md](../../src/servidor/README.md) | Vigente: API, catálogo, operador y SQLite; enlaces resuelven. |
| [src/subir-personajes/README.md](../../src/subir-personajes/README.md) | Vigente: ruta opcional, nueve campos, validación, resumen, sí/no, reemplazo e incertidumbre sin reintento. |
| [test/proyecto-integrador-final-tests/README.md](../../test/proyecto-integrador-final-tests/README.md) | Vigente: tres DLL del mismo worktree, entorno, contratos, cliente REST y carga JSON. |

También se leyeron todos los documentos locales de specs: índice; spec/plan/tasks de 001, 002, 003 y 005; spec/plan/tasks de 004. Se mantuvieron requisitos e historia; se aclaró la vigencia histórica de 003, se corrigió el orden de ruta/URL/credencial del plan 002 y se enlazó el nuevo seguimiento 004. El estado de red física no se cambió a aprobado. La revisión documental del UML de esta rama no se presenta como integración ya publicada de otro spec.

### Controles y ejecución

- `git ls-files '*.cs'`: inventario de 57 fuentes propios; comparación uno a uno con sus páginas.
- Parser Roslyn del SDK instalado: inventario de declaraciones explícitas y contraste por nombre; revisión manual adicional por miembro, sobrecarga, función local, lambda relevante y flujo.
- Script Python local de revisión: cada bloque csharp de las páginas es subcadena literal de su fuente en la referencia; los bloques del desarrollo en modelos se comparan con los de su página. Sin anonimización ni pseudocódigo. El contexto omitido se indica fuera de los bloques.
- Enlaces Markdown relativos y anclas de encabezados/IDs HTML: comprobación de todos los documentos versionados, páginas nuevas y specs canónicos; no se exige conexión externa para navegar el catálogo. El HTML de propuestas se revisó como maqueta estática; los ejemplos allí no sustituyen pruebas del ejecutable.
- Parseo XML del UML y contraste de cajas/relaciones con fuente: correcto; no se afirma un render manual en UMLetino.
- `git diff --check`: control de formato del diff. No se introducen cambios en src/ ni test/.
- [Build y suite](../plan-de-pruebas.md#verificación-de-la-versión-documentada-spec-004): cero errores/advertencias y 179 aprobadas, cero fallidas/omitidas. Linux, DLL del mismo worktree y datos de ensayo aislados; no acredita dos Windows ni instalaciones WinGet. Los ejemplos escritos por lectura no se presentan como ejecuciones adicionales.

### Criterios de aceptación del spec 004

| Criterio | Evidencia y resultado |
|---|---|
| CA1 | Aprobado: inventario 57/57, rutas y nombres conservados; exclusiones justificadas. |
| CA2 | Aprobado: contraste por archivo de tipos, datos, todos los miembros y decisiones; privados, partial y auxiliares incluidos. |
| CA3 | Aprobado: fragmentos reales explicados por miembro/flujo y cotejo literal completo; sin discrepancias de versión. |
| CA4 | Aprobado: recorridos normales y alternativos/errores con datos; ausencia de éxito en Partida local justificada por su fallo real. |
| CA5 | Aprobado: 27 páginas de pruebas/auxiliares; preparación, parámetros, acción, aserciones, dobles, limpieza, DLL y condiciones de omisión. |
| CA6 | Aprobado: todos los documentos y diagramas enumerados, revisados y actualizados cuando correspondía; consigna e historia preservadas. |
| CA7 | Aprobado: catálogo completo accesible desde README raíz, relaciones y enlaces/anclas locales resueltos. |
| CA8 | Aprobado: referencia común, comandos/resultados reales y límites explícitos; datos de ejemplo ficticios, sin credenciales reales. |
| CA9 | Aprobado: inventario, revisión individual y controles por criterio, sin discrepancias documentales abiertas. La revisión de compañero del PR sigue el flujo normal del repositorio. |
| CA10 | Aprobado: modelos contiene los 57 desarrollos completos, tipos/miembros, fragmentos, recorridos y evidencia, con enlaces a páginas individuales y comparación de su contenido. |

### Límites conservados

La documentación explica el estado existente: el bucle local de Partida está incompleto; no lo repara. La prueba física en dos Windows corresponde al spec 002 y permanece pendiente. No se repitieron manualmente todas las pantallas en terminal real ni instalaciones de herramientas. Esas limitaciones están diferenciadas de la suite local y de los recorridos deducidos por lectura; no hay una funcionalidad ausente presentada como implementada.
