using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TiendaUIController : MonoBehaviour
{
    public static bool HayTiendaAbierta { get; private set; } = false;

    [Header("Panel")]
    [SerializeField] private GameObject panelTienda;

    [Header("Mercader")]
    [SerializeField] private MercaderAnimacion mercaderAnimacion;

    [Header("Habilidades")]
    [SerializeField] private CanvasGroup grupoHabilidades;
    [SerializeField] private PanelHabilidadUI panelHabilidadUI;

    [Header("Fade habilidades")]
    [SerializeField] private float duracionLlegadaMercader = 0.55f;
    [SerializeField] private float aparecerCuandoFalte = 0.1f;
    [SerializeField] private float duracionFadeHabilidades = 0.2f;

    [Header("Controladores")]
    [SerializeField] private MonoBehaviour controladorCamara;
    [SerializeField] private MonoBehaviour controladorJugador;

    [Header("UI Mando")]
    [SerializeField] private GameObject primerBotonTienda;

    private bool tiendaOcultaPorPausa = false;
    private bool cerrandoTienda = false;

    private Coroutine rutinaFadeHabilidades;

    public bool TiendaAbierta
    {
        get
        {
            return panelTienda != null && panelTienda.activeSelf;
        }
    }

    public bool TiendaOcultaPorPausa
    {
        get
        {
            return tiendaOcultaPorPausa;
        }
    }

    private void OnDisable()
    {
        HayTiendaAbierta = false;
    }

    private void Update()
    {
        if (tiendaOcultaPorPausa)
            return;

        if (!TiendaAbierta)
            return;

        if (cerrandoTienda)
            return;

        if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            CerrarTienda();
            return;
        }

        MantenerSeleccionTienda();
    }

    public void AbrirTienda()
    {
        if (cerrandoTienda)
            return;

        if (TiendaAbierta)
            return;

        HayTiendaAbierta = true;
        tiendaOcultaPorPausa = false;

        if (panelTienda != null)
            panelTienda.SetActive(true);

        PrepararHabilidadesOcultas();

        if (mercaderAnimacion != null)
            mercaderAnimacion.ReproducirLlegada();

        if (rutinaFadeHabilidades != null)
            StopCoroutine(rutinaFadeHabilidades);

        rutinaFadeHabilidades = StartCoroutine(AparecerHabilidadesTrasLlegada());

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (controladorCamara != null)
            controladorCamara.enabled = false;

        if (controladorJugador != null)
            controladorJugador.enabled = false;
    }

    public void CerrarTienda()
    {
        if (!TiendaAbierta)
            return;

        if (cerrandoTienda)
            return;

        StartCoroutine(CerrarTiendaCoroutine());
    }

    private IEnumerator CerrarTiendaCoroutine()
    {
        cerrandoTienda = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (panelHabilidadUI != null)
            panelHabilidadUI.LimpiarSeleccion();

        if (rutinaFadeHabilidades != null)
        {
            StopCoroutine(rutinaFadeHabilidades);
            rutinaFadeHabilidades = null;
        }

        Coroutine fadeSalida = null;

        if (grupoHabilidades != null)
            fadeSalida = StartCoroutine(FadeGrupoHabilidades(grupoHabilidades.alpha, 0f));

        if (mercaderAnimacion != null)
            yield return StartCoroutine(mercaderAnimacion.ReproducirDespedida());

        if (fadeSalida != null)
            yield return fadeSalida;

        if (panelTienda != null)
            panelTienda.SetActive(false);

        tiendaOcultaPorPausa = false;
        cerrandoTienda = false;
        HayTiendaAbierta = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (controladorCamara != null)
            controladorCamara.enabled = true;

        if (controladorJugador != null)
            controladorJugador.enabled = true;
    }

    public void OcultarInterfazPorPausa()
    {
        if (!TiendaAbierta)
            return;

        tiendaOcultaPorPausa = true;

        if (panelTienda != null)
            panelTienda.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void RestaurarInterfazTrasPausa()
    {
        if (!tiendaOcultaPorPausa)
            return;

        tiendaOcultaPorPausa = false;
        HayTiendaAbierta = true;

        if (panelTienda != null)
            panelTienda.SetActive(true);

        MostrarHabilidadesInstantaneo();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SeleccionarPrimerBotonTienda();
    }

    private void PrepararHabilidadesOcultas()
    {
        if (grupoHabilidades == null)
            return;

        grupoHabilidades.alpha = 0f;

        // IMPORTANTE:
        // No ponemos interactable = false porque eso activa el Disabled Sprite de los botones.
        grupoHabilidades.interactable = true;

        // Esto sí evita que se puedan pulsar mientras están invisibles.
        grupoHabilidades.blocksRaycasts = false;
    }

    private void MostrarHabilidadesInstantaneo()
    {
        if (grupoHabilidades == null)
            return;

        grupoHabilidades.alpha = 1f;

        // Siempre true para que los botones no se vean disabled/pressed.
        grupoHabilidades.interactable = true;

        // Ahora sí pueden recibir clicks.
        grupoHabilidades.blocksRaycasts = true;
    }

    private IEnumerator AparecerHabilidadesTrasLlegada()
    {
        float espera = Mathf.Max(0f, duracionLlegadaMercader - aparecerCuandoFalte);

        yield return new WaitForSecondsRealtime(espera);

        yield return StartCoroutine(FadeGrupoHabilidades(0f, 1f));

        rutinaFadeHabilidades = null;

        SeleccionarPrimerBotonTienda();
    }

    private IEnumerator FadeGrupoHabilidades(float alphaInicial, float alphaFinal)
    {
        if (grupoHabilidades == null)
            yield break;

        // IMPORTANTE:
        // Mantener interactable en true para que NO se activen los Disabled Sprite.
        grupoHabilidades.interactable = true;

        // Durante el fade bloqueamos clicks sin cambiar el estado visual.
        grupoHabilidades.blocksRaycasts = false;

        float tiempo = 0f;

        while (tiempo < duracionFadeHabilidades)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = tiempo / duracionFadeHabilidades;
            grupoHabilidades.alpha = Mathf.Lerp(alphaInicial, alphaFinal, t);

            yield return null;
        }

        grupoHabilidades.alpha = alphaFinal;

        bool visible = alphaFinal > 0.99f;

        // Mantener siempre true.
        grupoHabilidades.interactable = true;

        // Solo permitimos clicks cuando ya terminó de aparecer.
        grupoHabilidades.blocksRaycasts = visible;
    }

    private void MantenerSeleccionTienda()
    {
        if (EventSystem.current == null)
            return;

        if (EventSystem.current.currentSelectedGameObject != null)
            return;

        SeleccionarPrimerBotonTienda();
    }

    private void SeleccionarPrimerBotonTienda()
    {
        if (grupoHabilidades != null && !grupoHabilidades.interactable)
            return;

        if (EventSystem.current == null || primerBotonTienda == null)
            return;

        if (!primerBotonTienda.activeInHierarchy)
            return;

        Selectable selectable = primerBotonTienda.GetComponent<Selectable>();

        if (selectable != null && !selectable.interactable)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(primerBotonTienda);
    }
}