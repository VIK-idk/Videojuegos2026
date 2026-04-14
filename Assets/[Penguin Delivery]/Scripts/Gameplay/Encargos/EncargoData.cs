using UnityEngine;

// ====================
// ENCARGO DATA
// ====================
[System.Serializable]
public class EncargoData
{
    public int pecesRosas;
    public int pecesAmarillos;
    public int pecesVerdes;

    public float tiempoLimite;

    public bool enProceso;
    public bool completado;
    public bool fallado;
}