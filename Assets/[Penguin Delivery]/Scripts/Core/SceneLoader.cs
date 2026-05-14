using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static string EscenaDestino { get; private set; }

    public static void CargarEscena(string nombreEscena)
    {
        Time.timeScale = 1f;

        EscenaDestino = nombreEscena;
        SceneManager.LoadScene("PantallaCarga");
    }
}