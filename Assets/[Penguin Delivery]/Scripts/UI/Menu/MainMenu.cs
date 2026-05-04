using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string nombreEscena = "HabilidadesTest";
    [SerializeField] private GameObject primerBotonMenu;

    void Start()
    {
        // 🎮 activar navegación mando desde el inicio
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(primerBotonMenu);
    }

    void Update()
    {
        // 🔥 mantener foco
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(primerBotonMenu);
        }
    }

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