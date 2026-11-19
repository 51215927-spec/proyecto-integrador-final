# Código de las pruebas

Los atributos de xUnit identifican los casos. Las pruebas de dominio construyen objetos directamente. Las de consola capturan texto. Las de API envían HTTP local y las de contrato lanzan ejecutables separados.

## `CartasDePrueba.cs`

[Ver `test/proyecto-integrador-final-tests/CartasDePrueba.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/CartasDePrueba.cs)

- `Crear` fabrica un personaje válido con fuerza configurable y ajusta otro puntaje para conservar el total.

## `ContratoFactAttribute.cs`

[Ver `test/proyecto-integrador-final-tests/ContratoFactAttribute.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/ContratoFactAttribute.cs)

- El constructor de `ContratoFactAttribute` lee la variable de ruta de DLL y establece `Skip` si falta. Así los casos por proceso se descubren aunque sus binarios no estén configurados.

## `PersonajeTests.cs`

[Ver `test/proyecto-integrador-final-tests/PersonajeTests.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/PersonajeTests.cs)

- `ConservaNombreAsciiYLosSeisAtributos` construye un personaje y compara cada getter con su entrada.
- `AceptaUmbralesInclusivosYAsciiVacio` usa valores límite y una cadena de dibujo vacía.
- `RechazaAtributosQueIncumplenUnaRegla` inyecta varios conjuntos con `InlineData` y espera una excepción de validación.
- `RechazaNombreSinTexto` parametriza nombres nulos o en blanco. `RechazaAsciiNulo` prueba por separado el dibujo nulo.

## `MazoTests.cs`

[Ver `test/proyecto-integrador-final-tests/MazoTests.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/MazoTests.cs)

- `CopiaLaColeccionReparteEnOrdenYDevuelveAlFinal` modifica la lista original, reparte por tramos y verifica orden y devolución.
- `MezclarConservaTodasLasCartas` mezcla mazos de distintos tamaños y verifica que cada carta permanezca sin imponer un orden aleatorio concreto.

## `JugadorTests.cs`

[Ver `test/proyecto-integrador-final-tests/JugadorTests.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/JugadorTests.cs)

- `IniciaConNombreManoVaciaYSinVictorias` inspecciona los campos observables del jugador recién construido.
- `AcumulaCartasQuitaUnaYEntregaElResto` combina varias recepciones, prueba la copia defensiva, retira una carta y comprueba el vaciado y el marcador.

## `CombateTests.cs`

[Ver `test/proyecto-integrador-final-tests/CombateTests.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/CombateTests.cs)

- `ValidaParticipantesYDevuelveUnaCopiaEnOrden` prueba constructores inválidos y modifica un arreglo devuelto para comprobar que los participantes se conservan.
- `RechazaConsultasPrematurasYAtributosInvalidosORepetidos` consulta antes de elegir y comprueba las excepciones y la inmutabilidad de la primera elección.
- `RechazaCartasInvalidasSinRetirarlasYRegistraUnaPorJugador` mezcla cartas ajenas, jugadores inválidos y elecciones repetidas. Comprueba qué cartas se retiraron y cuáles quedan.
- `FaltaCompletarElCombateConUnaSolaCartaAunqueYaSeEligioElAtributo` alterna el participante que entrega la única carta y exige que resolver falle.
- `ResuelveSegunElAtributoElegidoYDaElEmpateAlRival` parametriza mayor, menor e igualdad y compara la instancia ganadora.

## `RondaTests.cs`

[Ver `test/proyecto-integrador-final-tests/RondaTests.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/RondaTests.cs)

- `EmpiezaSinCombatesYEligeElJugadorMano` confirma el estado inicial y la excepción al consultar ganador.
- `DosVictoriasSeguidasCierranLaRonda` parametriza al vencedor y prueba cierre temprano.
- `ElTercerCombateDefineLaRondaCuandoVanUnoAUno` genera un empate parcial y completa el tercer combate con cada posible ganador.
- `ResolverCombate` prepara cartas y atributo para producir el ganador que necesita el escenario, luego devuelve el combate ya resuelto.

## `PartidaTests.cs`

[Ver `test/proyecto-integrador-final-tests/PartidaTests.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/PartidaTests.cs)

- `RechazaUnMazoInsuficienteAntesDeRepartir` llama a `Jugar` con tamaños menores a seis, espera la excepción de inicio y comprueba que el mazo y las manos conservan su estado.

## `InterfazConsolaTests.cs`

[Ver `test/proyecto-integrador-final-tests/InterfazConsolaTests.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/InterfazConsolaTests.cs)

- `Carta` crea una vista de carta para alimentar la interfaz.
- `PedirCarta_ReintentaEntradasInvalidasYConservaLaMano` alimenta líneas inválidas y válidas y comprueba la selección sin mutar la mano.
- `ManoVacia_SePuedeMostrarPeroNoSolicitar` comprueba el mensaje de mano vacía y la excepción de selección.
- `PedirAtributo_OfreceLosSeisAtributos` parametriza el número de desplazamientos. `PedirAtributo_RespetaLasOpcionesDeLaSolicitud` aporta una lista restringida y verifica la selección.
- `FinDeEntrada_TerminaLaSolicitud` provoca fin de lectura y espera la excepción de entrada.
- `MostrarMano_AdaptaLasFichasAlAncho` y `MostrarCombate_PresentaElResultadoConfirmado` usan anchos determinados y verifican las fichas y el contenido del resultado. `PresentaMarcadorEstadosYCierresConLosNombresRecibidos` comprueba los nombres y mensajes en la salida capturada.

## `PantallasConsolaTests.cs`

[Ver `test/proyecto-integrador-final-tests/PantallasConsolaTests.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/PantallasConsolaTests.cs)

- `PreparacionEsperaInicialEInicioDeRonda_TienenSusPaneles` llama a tres pantallas y busca información específica de cada una.
- `Apertura_UsaMesaConFichasEncabezadoYPie` ejecuta la apertura a diferentes anchos.
- `EsperaYRespuesta_MuestranManoPropiaSinPedirAtributo` usa la misma mesa para revisar espera y respuesta.
- `EnvioSuspensionYCierre_DistinguenLosEstadosConfirmados` comprueba los mensajes de envío, suspensión e interrupción.
- `ResultadoYRondas_UsanDueloVictoriaYPlanilla` comprueba los paneles y textos del combate, ronda y partida.

## `ConsolaAltaTests.cs`

[Ver `test/proyecto-integrador-final-tests/ConsolaAltaTests.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/ConsolaAltaTests.cs)

- `CargaDosPersonajesManualmenteYConfirmaSusIds` alimenta dos formularios por texto, envía las dos altas a un servidor local con SQLite en memoria y comprueba los IDs impresos.
- `UnAltaRechazadaNoSeAnunciaComoConfirmada` provoca una respuesta no autorizada y comprueba el mensaje y código de salida.

## `ClienteConsolaContratoTests.cs`

[Ver `test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/ClienteConsolaContratoTests.cs)

- `Demo_MuestraElFocoLosCombatesYLaVictoriaEnQuince` verifica que el ejecutable emita pantallas y marcadores en orden.
- `Demo_EntradaRedirigidaPermiteMoverElFocoYConfirmarLaCarta` cambia el foco con texto y observa qué se confirmó.
- `Demo_SuspensionConservaElMarcadorConfirmadoYPermiteSalir` arranca en modo pausa y comprueba que el marcador no cambie.
- `Demo_DistingueAbandonoCancelacionYSalidaLocal` ejecuta los tres modos y compara sus mensajes.
- `Demo_FinDeEntradaTerminaLaSolicitudSinConfirmarUnaJugada` cierra la entrada y busca el cierre local.
- `EnOrden` busca fragmentos sucesivos desde la última coincidencia. `EjecutarDemo` lanza la DLL con entrada y salida redirigidas, escribe el guion de teclas y devuelve el texto del proceso.

## `ApiHttpTests.cs`

[Ver `test/proyecto-integrador-final-tests/ApiHttpTests.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/ApiHttpTests.cs)

- `PresentacionDePartidaDesconocidaDevuelveErrorDelProtocolo` consulta un UUID inexistente y comprueba estado, código e ID de seguimiento.
- `IncorporacionReservaLaPlazaYPermiteRenovarLaMismaSesion` compara la incorporación original, su repetición y la colisión con otro UUID.
- `AltaExigeCredencialAdministrativaYValidaAtributos` contrasta alta sin credencial, alta inválida y alta válida con ID.
- `CombateMantieneLasEleccionesPrivadasYConfirmaUnaSolaVez` envía elecciones concurrentes, consulta el recibo propio y ajeno y luego verifica cuándo aparece el combate completo.
- `ReinicioRecuperaSolicitudYRecibosYExigeSesionesNuevas` vuelve a abrir la misma base después de cerrar el host y comprueba que los datos persisten mientras las sesiones se pierden.
- `UnaEleccionConFalloDeEscrituraNoDejaCambiosParciales` induce un fallo de almacenamiento, comprueba que no haya recibo ni avance y vuelve a enviar.
- `PresenciaRenuevaElPlazoSinCambiarLaRevision` controla el reloj para observar vigencia y revisión por separado.
- `ReemplazarCredencialRevocaAccesoYCancelarConservaLaPlaza` rota la credencial, vuelve a incorporar y consulta el estado cancelado.
- `PartidaTerminaConQuinceRondasGanadasYSinRepartirOtra` recorre los turnos por HTTP hasta el marcador final. `AltaRechazaPuntajesRepresentadosComoTexto` manda tipos JSON incorrectos y comprueba el error.
- `Error` comprueba cuerpo y estado HTTP. `GetUtcNow` devuelve el reloj controlado y `Avanzar` le suma segundos. `DatosEleccion` arma el cuerpo a partir de revisión y solicitud. `Incorporar` prepara un cliente autorizado y `Vista` lee su presentación.

## `ServidorContratoTests.cs`

[Ver `test/proyecto-integrador-final-tests/ServidorContratoTests.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/ServidorContratoTests.cs)

- `Incorporacion_ValidaCredencialYSesionYNoCambiaRevisionConPresencia` compara incorporación, renovación y estados de error vía HTTP.
- `Elecciones_ProtegenManosYDevuelvenElMismoReciboAlReintentar` toma una vista por plaza y repite la elección para comprobar privacidad y el recibo original.
- `EleccionesConcurrentes_ConsumenUnaSolaSolicitud` dispara dos operaciones sobre la misma solicitud y verifica que avance una vez.
- `SesionAusente_PausaYRecuperaLaMismaSolicitudSinCambiarRevision` cierra una sesión y luego la reincorpora para observar la misma solicitud.
- `Partida_AlternaElElectorEnEmpatesYFinalizaAlLlegarAQuinceRondas` conduce los turnos hasta el marcador final y compara la alternancia y los empates.
- `Reinicio_InvalidaSesionesYConservaEstadoYReciboConfirmados` reinicia el proceso sobre el archivo SQLite anterior y verifica estado y autenticación.
- `Abandono_RepetirloConservaMarcadorYRevisionFinal` envía dos veces el abandono y compara ambas presentaciones finales.
- `Revision` y `Mano` extraen datos de una vista JSON. `Elegir` construye una elección desde las opciones visibles. `Leer` verifica estado y deserializa. `Error` comprueba los campos de un error.
- Los records privados `Eleccion` y `Jugador` conservan los parámetros que usa el banco de prueba HTTP. El constructor de `Servidor` configura la instancia de prueba. `Lanzar` inicia el proceso con su base aislada. `Reiniciar` detiene y vuelve a levantar el proceso sobre la misma base. `Crear` envía la orden de operador y lee partida y credenciales. `Iniciar` envía la orden de comenzar.
- `Renovar`, `CerrarSesion` e `Incorporar` controlan presencia mediante HTTP o consola del operador. `EsperarVista` repite la consulta hasta cumplir una condición. `Vista` obtiene JSON, `Post` envía una elección y `Enviar` prepara cabeceras y cuerpo.
- `LeerDato` busca la línea cuyo prefijo espera al leer la consola. `Detener` finaliza el proceso y `DisposeAsync` libera el cliente y elimina el directorio temporal.

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
