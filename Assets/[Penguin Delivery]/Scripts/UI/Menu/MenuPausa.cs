using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuPausa : MonoBehaviour
{
    public GameObject menuPausa;
    public GameObject menuOpciones;

    [Header("UI Mando")]
    [SerializeField] private GameObject primerBotonPausa;

    [SerializeField] private string escenaPrincipal;
    [SerializeField] private TiendaUIController tiendaUIController;

    private bool isGamePaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.JoystickButton9) ||
            Input.GetKeyDown(KeyCode.P) ||
            Input.GetKeyDown(KeyCode.Escape))
        {
            isGamePaused = !isGamePaused;
            PauseGame();
        }


        if (isGamePaused && menuPausa.activeSelf)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(primerBotonPausa);
            }
        }
    }

    public void PauseGame()
    {
        if (isGamePaused)
        {
            Time.timeScale = 0;

            menuPausa.SetActive(true);
            menuOpciones.SetActive(false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(primerBotonPausa);
        }
        else
        {
            Time.timeScale = 1;

            menuPausa.SetActive(false);
            menuOpciones.SetActive(false);

            bool tiendaAbierta = tiendaUIController != null && tiendaUIController.TiendaAbierta;

            if (!tiendaAbierta)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            EventSystem.current.SetSelectedGameObject(null);
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