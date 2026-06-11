using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string nombreEscena = "Gameplay";
    [SerializeField] private string nombreEscenaTutorial = "Tutorial";

    [Header("Paneles")]
    [SerializeField] private GameObject panelMenuPrincipal;
    [SerializeField] private GameObject panelOpciones;

    [Header("Fondo blur")]
    [SerializeField] private GameObject fondoBlurMenu;

    [Header("Primeros botones")]
    [SerializeField] private GameObject primerBotonMenu;
    [SerializeField] private GameObject primerBotonOpciones;

    private void Start()
    {
        // Entrar en la escena del menú principal inicia una sesión nueva.
        // Esto reinicia monedas, habilidades y dificultad temporal.
        SesionPartida.ReiniciarSesion();
        InicializarMenu();
    }

    private void Update()
    {
        ActualizarSeleccionSegunEntrada();
    }

    private void InicializarMenu()
    {
        Time.timeScale = 1f;

        if (panelMenuPrincipal != null)
            panelMenuPrincipal.SetActive(true);

        if (panelOpciones != null)
            panelOpciones.SetActive(false);

        if (fondoBlurMenu != null)
            fondoBlurMenu.SetActive(false);

        ResetearVisuales(panelMenuPrincipal);
        ResetearVisuales(panelOpciones);

        PrepararSeleccion(primerBotonMenu);
    }

    public void Jugar()
    {
        if (TutorialEstado.EstaCompletado())
            SceneLoader.CargarEscena(nombreEscena);
        else
            SceneLoader.CargarEscena(nombreEscenaTutorial);
    }

    public void AbrirOpciones()
    {
        ResetearVisuales(panelMenuPrincipal);
        ResetearVisuales(panelOpciones);

        if (panelMenuPrincipal != null)
            panelMenuPrincipal.SetActive(false);

        if (fondoBlurMenu != null)
            fondoBlurMenu.SetActive(true);

        if (panelOpciones != null)
            panelOpciones.SetActive(true);

        PrepararSeleccion(primerBotonOpciones);
    }

    public void VolverMenuPrincipal()
    {
        ResetearVisuales(panelOpciones);
        ResetearVisuales(panelMenuPrincipal);

        if (panelOpciones != null)
            panelOpciones.SetActive(false);

        if (fondoBlurMenu != null)
            fondoBlurMenu.SetActive(false);

        if (panelMenuPrincipal != null)
            panelMenuPrincipal.SetActive(true);

        PrepararSeleccion(primerBotonMenu);
    }

    public void Salir()
    {
        Debug.Log("Salir...");
        Application.Quit();
    }

    private void ActualizarSeleccionSegunEntrada()
    {
        if (EventSystem.current == null)
            return;

        if (!InputDetector.DebeMostrarSeleccionUI)
        {
            if (EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);

            return;
        }

        if (EventSystem.current.currentSelectedGameObject != null)
            return;

        if (panelOpciones != null && panelOpciones.activeInHierarchy)
            SeleccionarObjeto(primerBotonOpciones);
        else if (panelMenuPrincipal != null && panelMenuPrincipal.activeInHierarchy)
            SeleccionarObjeto(primerBotonMenu);
    }

    private void PrepararSeleccion(GameObject objeto)
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (InputDetector.DebeMostrarSeleccionUI)
            StartCoroutine(SeleccionarAlFinalDelFrame(objeto));
    }

    private void SeleccionarObjeto(GameObject objeto)
    {
        if (!InputDetector.DebeMostrarSeleccionUI)
            return;

        if (EventSystem.current == null || objeto == null)
            return;

        if (!objeto.activeInHierarchy)
            return;

        Selectable selectable = objeto.GetComponent<Selectable>();

        if (selectable != null && !selectable.interactable)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(objeto);
    }

    private IEnumerator SeleccionarAlFinalDelFrame(GameObject objeto)
    {
        yield return null;
        SeleccionarObjeto(objeto);
    }

    private void ResetearVisuales(GameObject panel)
    {
        if (panel == null)
            return;

        BotonMenuAnimado[] botones = panel.GetComponentsInChildren<BotonMenuAnimado>(true);

        for (int i = 0; i < botones.Length; i++)
            botones[i].ResetearVisualCompleto();
    }

    private void OnApplicationQuit()
    {
        SesionPartida.ReiniciarSesion();
    }
}
