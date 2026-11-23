# Pruebas automatizadas

Usa xUnit y referencia los proyectos de servidor, cliente y carga. Verifica reglas del dominio, contratos HTTP y comportamiento de consola. Desde la raíz ejecutá `dotnet test proyecto-integrador-final.sln`.

[Arquitectura](../../docs/arquitectura.md) · [Instalación y uso](../../docs/instalacion-y-uso.md) · [Pruebas](../../docs/plan-de-pruebas.md)

Las pruebas de contrato que arrancan procesos requieren compilar primero la solución y definir `GUERRA_CLIENTE_DLL`, `GUERRA_SERVIDOR_DLL` y `GUERRA_CARGA_DLL` con las rutas absolutas a `src/cliente/bin/Debug/net8.0/cliente.dll`, `src/servidor/bin/Debug/net8.0/servidor.dll` y `src/subir-personajes/bin/Debug/net8.0/subir-personajes.dll`, respectivamente. Con otra configuración de compilación, ajustá el segmento `Debug`.

Desde PowerShell, después de compilar:

```powershell
$env:GUERRA_CLIENTE_DLL = (Resolve-Path src/cliente/bin/Debug/net8.0/cliente.dll).Path
$env:GUERRA_SERVIDOR_DLL = (Resolve-Path src/servidor/bin/Debug/net8.0/servidor.dll).Path
$env:GUERRA_CARGA_DLL = (Resolve-Path src/subir-personajes/bin/Debug/net8.0/subir-personajes.dll).Path
dotnet test proyecto-integrador-final.sln
```

Estas variables son del arnés de pruebas; los programas se operan mediante preguntas y no necesitan variables de usuario. Las pruebas de procesos envían las respuestas por stdin. [Resultados del spec 003](../../docs/plan-de-pruebas.md#evidencia-del-spec-003)

## Cliente REST

Las pruebas del spec 001 cubren configuración, transporte con HttpMessageHandler controlado, coordinación con TimeProvider sustituible, privacidad y recibos contra servidor local con SQLite temporal y contratos del ejecutable. Las demostraciones anteriores se prueban directamente como auxiliar interno; no se ofrecen desde Program. Las tres DLL de contrato deben apuntar al mismo worktree que se compila y prueba. [Evidencia y límites](../../docs/plan-de-pruebas.md#cliente-rest-spec-001)

## Carga JSON

`CargaJsonTests` verifica formato y dominio, endpoints autenticados, reemplazo exacto al reabrir y rollback real de SQLite mediante un trigger. `ConsolaAltaTests` cubre rutas, confirmación/cancelación y errores sin reintentos. El recorrido de `EntradaProcesosContratoTests` carga el JSON de seis ejemplos desde el ejecutable, reemplaza, repite, cancela y rechaza datos inválidos. Requiere `GUERRA_CARGA_DLL` del mismo worktree para localizar tanto el binario como su archivo de ejemplos. [Resultados del spec 005](../../docs/plan-de-pruebas.md#sistema-de-carga-json-spec-005).

## Separación del servidor

`PartidaTests` verifica acciones sucesivas, reconstrucción, reglas y partida completa hasta quince rondas; `VistaPartidaTests` comprueba privacidad y lectura sin efectos. `CoordinacionSesionesTests` usa una barrera en el reloj para comprobar elección frente a cierre o rotación de sesión. `CompatibilidadPersistenciaTests` carga la fixture SQL de una base creada con el servidor anterior, recupera el recibo, continúa el combate y compara el esquema sin migración. La fixture se copia a la salida de pruebas por el proyecto.

La suite del spec 006 aprobó **190 pruebas, 0 fallidas y 0 omitidas**, con las tres DLL del mismo worktree. Los contratos HTTP/procesos, rollback con trigger y respuesta perdida siguen incluidos. [Referencia y límites](../../docs/plan-de-pruebas.md#separación-de-responsabilidades-spec-006).

## Inicio automático

Los casos v2 cubren invitado persistente, cola/sala/Listo, presencia, cancelación frente a inicio, rollback de asignaciones, reinicio y avisos SSE autenticados. AccesoLocalTests comprueba permisos Unix y corrupción; AislamientoInvitadoTests verifica que cambiar de servidor no envíe el secreto anterior y que el reemplazo exija confirmación. IntegracionRespuestaPerdidaTests también ejecuta el coordinador de juego con avisos y acceso automático.

InicioAutomaticoProcesoTests y AvisosSalaProcesoTests arrancan clientes reales y capturan treinta segundos sin polling en juego y sala. En Unix aíslan usuarios mediante XDG_DATA_HOME. En Windows se omiten de forma explícita para no tocar el perfil real: comprobar con perfiles distintos del mismo usuario o en dos equipos. Las pruebas manuales históricas ingresan por Recuperar acceso antiguo. [Matriz y límites](../../docs/plan-de-pruebas.md#inicio-automático-de-partidas-spec-007).

PerfilesTests verifica migración del acceso único, altas concurrentes, reemplazo aislado y disponibilidad sin reserva/renovación. SeleccionPerfilesTests cubre elección automática/manual, todos ocupados, creación explícita y carrera por el perfil. MismaCuentaProcesoTests ejecuta dos clientes con la misma carpeta de datos: crean perfiles diferentes, juegan y reabren usando automáticamente el único libre. El aislamiento temporal con XDG_DATA_HOME evita tocar los perfiles reales del usuario.
