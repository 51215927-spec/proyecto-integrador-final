# Bases de datos

El servidor crea el esquema con `EnsureCreated` al abrir el repositorio. La configuración `ConnectionStrings__Juego` permite seleccionar la cadena SQLite. Por defecto usa `Data Source=guerra.db;Default Timeout=1`. Una ruta relativa se interpreta desde el directorio de trabajo del servidor.

| Tabla | Clave primaria | Datos y relaciones |
|---|---|---|
| PersonajeCatalogo | Id | Nombre, descripción, ASCII, seis atributos y estado activo. El repositorio asigna el ID. |
| Partida | Id | Fase, revisión, ronda y combate actuales, solicitud activa, motivo de fin, ganador y abandono. |
| Participante | IdPartida, Rol | Referencia a Partida. Guarda nombre, hash de credencial y rondas ganadas. |
| PersonajePartida | IdPartida, IdPersonaje | Referencia a Partida. Copia nombre, descripción, ASCII y atributos. Guarda ubicación y orden en la partida. |
| Ronda | IdPartida, Numero | Referencia a Partida. Guarda mano inicial, victorias por plaza y ganador. |
| Combate | IdPartida, NumeroRonda, Numero | Referencia a Ronda. Guarda elector, cartas elegidas, atributo, puntajes y ganador. |
| Solicitud | Id | Referencia a Combate. Guarda plaza, tipo y estado de consumo. Tiene índice por IdPartida. |
| Operacion | IdPartida, Rol, IdOperacion | Referencias a Participante y Solicitud. Guarda elección, revisión esperada y confirmada para devolver el mismo recibo ante un reintento compatible. |

Las claves de la partida, solicitudes y operaciones son UUID. Los números de ronda, combate, personaje, orden, atributos y revisiones son enteros. Los resultados y elecciones pendientes admiten valores nulos según la etapa del juego. Las reglas de atributos se validan en el dominio. Las sesiones y sus vencimientos viven en memoria.

[Configuración de entidades](../src/servidor/Api/BaseJuego.cs) · [Entidades de partida](../src/servidor/Api/EstadoPartida.cs) · [Entidad del catálogo](../src/servidor/Api/PersonajeCatalogo.cs)
