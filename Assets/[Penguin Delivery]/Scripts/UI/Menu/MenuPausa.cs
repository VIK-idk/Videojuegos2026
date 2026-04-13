using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public GameObject menuPausa;
    public GameObject menuOpciones;

    [SerializeField] private string escenaPrincipal;
    [SerializeField] private TiendaUIController tiendaUIController;

    private bool isGamePaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            isGamePaused = !isGamePaused;
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isGamePaused)
        {
            Time.timeScale = 0;
            menuPausa.SetActive(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1;
            menuPausa.SetActive(false);
            menuOpciones.SetActive(false);

            bool tiendaAbierta = tiendaUIController != null && tiendaUIController.TiendaAbierta;

            if (tiendaAbierta)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void Continuar()
    {
        isGamePaused = false;
        PauseGame();
    }

    public void Salir()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(escenaPrincipal);
    }
}