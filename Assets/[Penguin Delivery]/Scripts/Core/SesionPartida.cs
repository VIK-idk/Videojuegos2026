using UnityEngine;

// ====================
// SESION PARTIDA
// ====================
public static class SesionPartida
{
    public static int monedas = 0;
    public static int monedasGastadas = 0;

    public static bool habilidadX2Comprada = false;
    public static bool habilidadImanComprada = false;
    public static bool habilidadQuitarStrikeComprada = false;

    public static void ReiniciarSesion()
    {
        monedas = 0;
        monedasGastadas = 0;

        habilidadX2Comprada = false;
        habilidadImanComprada = false;
        habilidadQuitarStrikeComprada = false;
    }
}