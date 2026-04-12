using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string nombreEscena = "HabilidadesTest";

    public void Jugar()
    {
        SesionPartida.ReiniciarSesion();
        SceneManager.LoadScene(nombreEscena);
    }

    public void Salir()
    {
        Debug.Log("Salir...");
        Application.Quit();
    }

    private void OnApplicationQuit()
    {
        SesionPartida.ReiniciarSesion();
    }
}