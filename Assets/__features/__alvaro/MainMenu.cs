using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string nombreEscena;
    public void Jugar() {
        SceneManager.LoadScene(nombreEscena);
    }

    public void Salir() {
        Debug.Log("Salir..."); 
        Application.Quit();
    }
}
