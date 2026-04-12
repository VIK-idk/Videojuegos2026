using System.Collections.Generic;
using UnityEngine;

// ====================
// DATOS PERSISTENTES
// ====================
[System.Serializable]
public class DatosJugadorGuardados
{
    public List<int> puntuacionesIntentos = new List<int>();
}

// ====================
// PROGRESO JUGADOR
// ====================
public static class ProgresoJugador
{
    private const string SAVE_KEY = "PROGRESO_JUGADOR";
    private static DatosJugadorGuardados datos;

    private static void AsegurarDatos()
    {
        if (datos != null)
            return;

        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            datos = JsonUtility.FromJson<DatosJugadorGuardados>(json);
        }
        else
        {
            datos = new DatosJugadorGuardados();
        }

        if (datos == null)
            datos = new DatosJugadorGuardados();

        if (datos.puntuacionesIntentos == null)
            datos.puntuacionesIntentos = new List<int>();
    }

    private static void Guardar()
    {
        AsegurarDatos();

        string json = JsonUtility.ToJson(datos);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    // ====================
    // PUNTUACIONES
    // ====================
    public static void RegistrarPuntuacionIntento(int puntos)
    {
        AsegurarDatos();
        datos.puntuacionesIntentos.Add(puntos);
        Guardar();
    }

    public static int GetMejorPuntuacion()
    {
        AsegurarDatos();

        if (datos.puntuacionesIntentos.Count == 0)
            return 0;

        int mejor = datos.puntuacionesIntentos[0];

        for (int i = 1; i < datos.puntuacionesIntentos.Count; i++)
        {
            if (datos.puntuacionesIntentos[i] > mejor)
                mejor = datos.puntuacionesIntentos[i];
        }

        return mejor;
    }
}