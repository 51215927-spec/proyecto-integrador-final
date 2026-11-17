namespace Proyecto_Integrador_Final.Tests;

internal static class CartasDePrueba
{
    internal static Personaje Crear(int fuerza = 650)
    {
        return new Personaje("Carta", "", fuerza, 1000 - fuerza, 500, 500, 500, 500);
    }
}
