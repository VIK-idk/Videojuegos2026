using UnityEngine;

public static class TutorialEstado
{
    private const string CLAVE_TUTORIAL_COMPLETADO = "TUTORIAL_COMPLETADO";

    public static bool EstaCompletado()
    {
        return PlayerPrefs.GetInt(CLAVE_TUTORIAL_COMPLETADO, 0) == 1;
    }

    public static void MarcarCompletado()
    {
        PlayerPrefs.SetInt(CLAVE_TUTORIAL_COMPLETADO, 1);
        PlayerPrefs.Save();
    }

    public static void Resetear()
    {
        PlayerPrefs.DeleteKey(CLAVE_TUTORIAL_COMPLETADO);
        PlayerPrefs.Save();
    }
}