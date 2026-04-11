using UnityEngine;

[System.Serializable]
public class Encargo
{
    [Header("Peces requeridos")]
    public int pecesAmarillos;
    public int pecesRosas;
    public int pecesVerdes;

    [Header("Tiempo")]
    public float tiempoLimite;

    [Header("Estado")]
    public bool enProceso;
    public bool completado;
    public bool fallado;
}