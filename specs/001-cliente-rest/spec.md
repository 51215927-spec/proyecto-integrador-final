# Cliente REST

Estado: borrador.

- R1. Incorporar un jugador con URL, UUID de partida, credencial y nombre. CA1. Se muestra el rol y se espera la orden de inicio.
- R2. Mantener presencia y mostrar la mano propia, turno, marcador y resultados. CA2. La pausa y reanudación se reflejan incluso con la misma revisión y cada resultado se presenta una vez.
- R3. Enviar elecciones según la solicitud en curso. CA3. La entrada inválida se puede corregir, una solicitud desactualizada actualiza la vista y un reintento compatible recupera el mismo recibo.
- R4. Recuperar la sesión tras una interrupción. CA4. Se puede volver a la vista vigente y distinguir salida local, abandono, cancelación y victoria.

[Contrato REST](../../docs/wiki/Protocolo-de-partida.md)
