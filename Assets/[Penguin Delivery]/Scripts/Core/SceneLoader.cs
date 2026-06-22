using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static string EscenaDestino { get; private set; }

    public static void CargarEscena(string nombreEscena)
    {
        Time.timeScale = 1f;

        EscenaDestino = nombreEscena;

        // Empieza el fade de musica antes de entrar a PantallaCarga.
        // Asi la musica antigua baja mientras la nueva escena empieza a preparar su musica.
        if (MusicaManager.Instancia != null)
            MusicaManager.Instancia.PrepararMusicaParaCambioDeEscena(nombreEscena);

        SceneManager.LoadScene("PantallaCarga");
    }
}
