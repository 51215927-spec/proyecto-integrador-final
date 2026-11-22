# CoordinadorPartidas.cs

Fuente: [src/servidor/Api/CoordinadorPartidas.cs](../../../../../src/servidor/Api/CoordinadorPartidas.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final.Api`. Versión: [catálogo común](../../../README.md#versión-documentada).

## Propósito y responsabilidad

Coordinación de acciones, acceso y recibos.

## Estado, miembros y ejecución

CrearPartida delega la creación. Iniciar conserva el orden observable fase, presencia, catálogo y dominio. Cancelar y Abandonar invocan Partida; Abandonar exige acceso. Elegir valida la estructura, normaliza atributo, exige acceso, recupera primero el recibo previo y solo después comprueba presencia y registra la elección. ConsultarRecibo exige acceso y busca por rol. Presentacion autentica y delega la proyección a VistaPartida con presencia calculada. Acceso autentica y exige sesión; ExigirPresencia conserva partidaPausada. Ejecutar mantiene el bloqueo compartido durante carga, acceso, acción y confirmación, y traduce ExcepcionPartida a ErrorApi.

## Errores, límites y efectos

Una operación idéntica devuelve el recibo aunque cambien revisión o turno; el mismo ID con otros datos falla. No contiene reglas de combate o reparto ni expone DbContext. La presentación no renueva presencia.

## Implementación contrastada

El bloque reproduce el archivo final completo para conservar las firmas, campos, constructores, auxiliares privados y lambdas descritos arriba; no es pseudocódigo ni una ejecución adicional.

```csharp
namespace Proyecto_Integrador_Final.Api;

internal sealed class CoordinadorPartidas(AlmacenPartidas almacen, AdministradorSesiones sesiones, CatalogoPersonajes catalogo)
{
    private readonly VistaPartida vista = new();
    internal PartidaCreada CrearPartida() => almacen.CrearPartida();

    internal void Iniciar(Guid id) => Ejecutar(id, true, unidad =>
    {
        if (unidad.Partida.Estado.Fase != "esperandoInicio")
            throw new ErrorApi(409, "operacionIncompatible", "La partida ya se inició o finalizó.");
        ExigirPresencia(id);
        unidad.Partida.Iniciar(unidad.CopiarCatalogoActivo(catalogo));
        return true;
    });

    internal void Cancelar(Guid id) => Ejecutar(id, true, unidad =>
    {
        unidad.Partida.Cancelar();
        return true;
    });

    internal Confirmacion Elegir(Guid id, string? credencial, Guid idSesion, Eleccion datos)
    {
        if (datos.IdOperacion == Guid.Empty || datos.IdSolicitud == Guid.Empty ||
            datos.RevisionEsperada is null or < 0 || datos.IdPersonaje is null)
            throw ErrorApi.Entrada();
        datos = datos with { Atributo = Partida.NormalizarAtributo(datos.Atributo) };
        return Ejecutar(id, true, unidad =>
        {
            var rol = Acceso(unidad, id, credencial, idSesion);
            if (unidad.Recuperar(rol, datos) is { } anterior) return anterior;
            if (unidad.Partida.Estado.Fase is "esperandoElector" or "esperandoRival") ExigirPresencia(id);
            unidad.Partida.RegistrarEleccion(rol, datos.IdSolicitud, datos.RevisionEsperada.Value,
                datos.IdPersonaje.Value, datos.Atributo);
            return unidad.Confirmar(rol, datos);
        });
    }

    internal Recibo ConsultarRecibo(Guid id, string? credencial, Guid idSesion, Guid idOperacion) =>
        Ejecutar(id, false, unidad => unidad.ConsultarRecibo(Acceso(unidad, id, credencial, idSesion), idOperacion));

    internal void Abandonar(Guid id, string? credencial, Guid idSesion) => Ejecutar(id, true, unidad =>
    {
        unidad.Partida.Abandonar(Acceso(unidad, id, credencial, idSesion));
        return true;
    });

    internal PresentacionVisible Presentacion(Guid id, string? credencial, Guid idSesion) =>
        Ejecutar(id, false, unidad => vista.Construir(unidad.Partida.Estado,
            Acceso(unidad, id, credencial, idSesion), sesiones.AmbosPresentes(id)));

    private string Acceso(UnidadPartida unidad, Guid id, string? credencial, Guid idSesion)
    {
        var rol = unidad.Autenticar(credencial);
        sesiones.ExigirSesion(id, rol, idSesion);
        return rol;
    }

    private void ExigirPresencia(Guid id)
    {
        if (!sesiones.AmbosPresentes(id))
            throw new ErrorApi(409, "partidaPausada", "Se necesitan los dos participantes presentes.");
    }

    private T Ejecutar<T>(Guid id, bool escritura, Func<UnidadPartida, T> accion)
    {
        lock (sesiones.Sincronizacion)
        {
            try { return almacen.Ejecutar(id, escritura, accion); }
            catch (ExcepcionPartida error)
            {
                throw new ErrorApi(error.Codigo == "eleccionInvalida" ? 422 : 409, error.Codigo, error.Message);
            }
        }
    }
}
```

## Relaciones y verificación

Véanse [arquitectura](../../../../arquitectura.md), [modelos](../../../../modelos.md) y [auditoría del spec 006](../../../verificacion.md#auditoría-del-spec-006--2026-11-22). La suite final registra 190 casos aprobados sin fallos ni omisiones; esa evidencia local no acredita la prueba física de dos Windows del spec 002.
