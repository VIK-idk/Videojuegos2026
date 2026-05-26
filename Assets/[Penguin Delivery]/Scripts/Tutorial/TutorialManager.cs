using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private enum PasoTutorial
    {
        Ninguno,
        RebotarEnMorsa,
        RecogerPrimerPez,
        ExplicarStrikes,
        CompletarPrimerEncargo,
        Finalizado
    }

    [Header("UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private RectTransform tutorialPanelRect;
    [SerializeField] private Text tutorialTitulo;
    [SerializeField] private Text tutorialTexto;
    [SerializeField] private Text tutorialSkipTexto;

    [Header("Posicion panel tutorial")]
    [SerializeField] private Vector2 posicionPanelArriba = new Vector2(0f, 300f);
    [SerializeField] private Vector2 posicionPanelAbajo = new Vector2(0f, -300f);

    [Header("Indicadores visuales opcionales")]
    [SerializeField] private GameObject indicadorMorsa;
    [SerializeField] private GameObject indicadorEncargo;
    [SerializeField] private GameObject indicadorStrikes;

    [Header("Configuracion")]
    [SerializeField] private KeyCode teclaSaltarTutorial = KeyCode.Tab;
    [SerializeField] private float tiempoMinimoLectura = 1.5f;
    [SerializeField] private float duracionPasoStrikes = 4f;
    [SerializeField] private float duracionMensajeFinal = 4f;

    [Header("Referencias")]
    [SerializeField] private StrikeManager strikeManager;
    [SerializeField] private GestorEncargosTest gestorEncargos;

    [Header("Rey Morsa")]
    [SerializeField] private ReyMorsaAnimacion reyMorsaAnimacion;
    [SerializeField] private float intervaloAplausoReyMorsa = 1.4f;

    [Header("Cinematica inicial")]
    [SerializeField] private Camera camaraJugador;
    [SerializeField] private Camera camaraTutorial;
    [SerializeField] private Transform puntoCamaraReyMorsa;
    [SerializeField] private Transform puntoCamaraPeces;
    [SerializeField] private float duracionMovimientoCamara = 2f;
    [SerializeField] private float duracionParadaCamara = 4f;
    [SerializeField] private float duracionParadaReyMorsa = 5.5f;
    [SerializeField] private Player jugador;
    [SerializeField] private Rigidbody rbJugador;
    [SerializeField] private CameraPivotController controladorCamaraJugador;

    [Header("SOLO TESTING")]
    [SerializeField] private bool forzarTutorialSiempre = false;
    [SerializeField] private KeyCode teclaResetTutorial = KeyCode.Y;

    private bool tutorialActivo = false;
    private bool tutorialGuardadoComoCompletado = false;
    private PasoTutorial pasoActual = PasoTutorial.Ninguno;

    private float bloqueoHasta = 0f;

    // ====================
    // ESTADO DEL TUTORIAL
    // ====================

    public bool DebeMostrarTutorial()
    {
        if (forzarTutorialSiempre)
            return true;

        return !TutorialEstado.EstaCompletado();
    }

    public bool TutorialActivo()
    {
        return tutorialActivo;
    }

    [ContextMenu("Resetear tutorial guardado")]
    public void ResetearTutorialGuardado()
    {
        TutorialEstado.Resetear();
    }

    public void MarcarTutorialComoCompletado()
    {
        if (tutorialGuardadoComoCompletado)
            return;

        tutorialGuardadoComoCompletado = true;
        tutorialActivo = false;

        TutorialEstado.MarcarCompletado();
    }

    // ====================
    // UNITY
    // ====================

    private void Awake()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);

            if (tutorialPanelRect == null)
                tutorialPanelRect = tutorialPanel.GetComponent<RectTransform>();
        }

        OcultarTodosLosIndicadores();

        if (camaraTutorial != null)
        {
            camaraTutorial.gameObject.SetActive(true);
            camaraTutorial.enabled = false;
        }
    }

    private void Start()
    {
        BuscarReferencias();
    }

    private void Update()
    {
        if (Input.GetKeyDown(teclaResetTutorial))
        {
            ResetearTutorialGuardado();
            Debug.Log("Tutorial reseteado");
        }

        if (!tutorialActivo)
            return;

        if (Input.GetKeyDown(teclaSaltarTutorial))
        {
            SaltarTutorial();
            return;
        }
    }

    // ====================
    // INICIO DEL TUTORIAL
    // ====================

    public void IniciarTutorial()
    {
        tutorialActivo = true;
        tutorialGuardadoComoCompletado = false;
        pasoActual = PasoTutorial.Ninguno;

        MostrarPanel();
        StartCoroutine(SecuenciaIntroduccionTutorial());
    }

    private IEnumerator SecuenciaIntroduccionTutorial()
    {
        BuscarReferencias();
        BloquearJugador();

        if (camaraJugador == null)
            camaraJugador = Camera.main;

        if (camaraJugador == null || camaraTutorial == null)
        {
            pasoActual = PasoTutorial.RebotarEnMorsa;
            MostrarPasoReboteMorsa();
            DesbloquearJugador();
            yield break;
        }

        Vector3 posicionInicial = camaraJugador.transform.position;
        Quaternion rotacionInicial = camaraJugador.transform.rotation;

        camaraTutorial.transform.position = posicionInicial;
        camaraTutorial.transform.rotation = rotacionInicial;

        if (camaraTutorial != null)
        {
            camaraTutorial.gameObject.SetActive(true);
            camaraTutorial.enabled = true;
        }

        if (camaraJugador != null)
        {
            camaraJugador.enabled = false;
        }

        // PRIMER DIALOGO: REY MORSA
        PonerPanelAbajo();

        MostrarDialogoTutorial(
            "Rey Morsa",
            "Gupy, desde ahora trabajarás para mí.\n\n" +
            "Completa encargos a tiempo, gana puntos y conviértete en el mejor repartidor."
        );

        if (puntoCamaraReyMorsa != null)
        {
            yield return StartCoroutine(MoverCamaraTutorial(puntoCamaraReyMorsa));
        }

        yield return StartCoroutine(AplaudirReyMorsaDurante(duracionParadaReyMorsa));

        // SEGUNDO DIALOGO: PECES
        PonerPanelArriba();

        MostrarDialogoTutorial(
            "Peces",
            "Los peces están sobre las morsas.\n\n" +
            "Recógelos para completar los encargos antes de que se acabe el tiempo."
        );

        if (puntoCamaraPeces != null)
        {
            yield return StartCoroutine(MoverCamaraTutorial(puntoCamaraPeces));
        }

        yield return new WaitForSeconds(duracionParadaCamara);

        // VOLVER AL JUGADOR
        yield return StartCoroutine(MoverCamaraTutorial(posicionInicial, rotacionInicial));

        if (camaraJugador != null)
        {
            camaraJugador.enabled = true;
        }

        if (camaraTutorial != null)
        {
            camaraTutorial.enabled = false;
        }

        pasoActual = PasoTutorial.RebotarEnMorsa;
        MostrarPasoReboteMorsa();

        DesbloquearJugador();
    }

    // ====================
    // EVENTOS DEL TUTORIAL
    // ====================

    public void NotificarReboteEnMorsa()
    {
        if (!tutorialActivo)
            return;

        if (pasoActual != PasoTutorial.RebotarEnMorsa)
            return;

        if (indicadorMorsa != null)
            indicadorMorsa.SetActive(false);

        pasoActual = PasoTutorial.ExplicarStrikes;
        StartCoroutine(SecuenciaExplicacionStrikes());
    }

    public void NotificarPrimerPezRecogido()
    {
        // Ya no usamos este paso.
        // Ahora el tutorial espera a que el jugador recoja los 5 peces del encargo.
    }

    public void OcultarIndicadoresTutorial()
    {
        OcultarTodosLosIndicadores();
    }

    // ====================
    // PASOS DEL TUTORIAL
    // ====================

    private void MostrarPasoReboteMorsa()
    {
        PonerPanelArriba();

        if (tutorialTitulo != null)
            tutorialTitulo.text = "Rebota sobre la morsa";

        if (tutorialTexto != null)
        {
            tutorialTexto.text =
                "Salta sobre una morsa para impulsarte y llegar hasta los peces.";
        }

        MostrarSoloIndicador(indicadorMorsa);
        ActualizarTextoSkip(true);
        BloquearLectura();
    }

    private void MostrarPasoRecogerPez()
    {
        PonerPanelArriba();

        if (tutorialTitulo != null)
            tutorialTitulo.text = "Completa el encargo";

        if (tutorialTexto != null)
        {
            tutorialTexto.text =
                "Recoge los 5 peces para completar el encargo antes de que acabe el tiempo";
        }

        MostrarSoloIndicador(indicadorEncargo);
        ActualizarTextoSkip(true);
        BloquearLectura();
    }

    private void MostrarPasoExplicarStrikes()
    {
        PonerPanelArriba();

        if (tutorialTitulo != null)
            tutorialTitulo.text = "Cuidado con los strikes";

        if (tutorialTexto != null)
        {
            tutorialTexto.text =
                "Si fallas un encargo, recibirás un strike.\n\n" +
                "Con 3 strikes irás a tu celda para descansar antes de intentarlo otra vez.";
        }

        MostrarIndicadores(indicadorStrikes, indicadorEncargo);
        ActualizarTextoSkip(true);
        BloquearLectura();
    }

   

    private IEnumerator SecuenciaExplicacionStrikes()
    {
        MostrarPasoExplicarStrikes();

        if (strikeManager != null)
            yield return StartCoroutine(strikeManager.ParpadearStrikeDemo(4f));
        else
            yield return new WaitForSeconds(2f);

        yield return new WaitForSeconds(duracionPasoStrikes);

        if (!tutorialActivo)
            yield break;

        OcultarTodosLosIndicadores();

        pasoActual = PasoTutorial.RecogerPrimerPez;
        MostrarPasoRecogerPez();
    }
 
    public IEnumerator MostrarMensajeFinalTutorial()
    {
        pasoActual = PasoTutorial.Finalizado;

        PonerPanelArriba();

        if (tutorialTitulo != null)
            tutorialTitulo.text = "Tutorial completado";

        if (tutorialTexto != null)
        {
            tutorialTexto.text =
                "Ya estás listo. Completa encargos, consigue puntos y evita acumular strikes.";
        }

        ActualizarTextoSkip(false);
        OcultarTodosLosIndicadores();
        MostrarPanel();

        yield return new WaitForSeconds(duracionMensajeFinal);

        tutorialActivo = false;
        OcultarPanel();
    }

    // ====================
    // SALTAR TUTORIAL
    // ====================

    private void SaltarTutorial()
    {
        StartCoroutine(SaltarTutorialCoroutine());
    }

    private IEnumerator SaltarTutorialCoroutine()
    {
        MarcarTutorialComoCompletado();
        OcultarTodosLosIndicadores();

        yield return StartCoroutine(MostrarMensajeFinalTutorial());

        if (gestorEncargos != null)
        {
            gestorEncargos.SaltarTutorialYEmpezarJuegoNormal();
        }
    }

    // ====================
    // UI
    // ====================

    private void MostrarPanel()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    private void OcultarPanel()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    private void MostrarDialogoTutorial(string titulo, string texto)
    {
        MostrarPanel();

        if (tutorialTitulo != null)
            tutorialTitulo.text = titulo;

        if (tutorialTexto != null)
            tutorialTexto.text = texto;

        ActualizarTextoSkip(true);
    }

    private void ActualizarTextoSkip(bool mostrar)
    {
        if (tutorialSkipTexto == null)
            return;

        if (mostrar)
            tutorialSkipTexto.text = "Pulsa " + teclaSaltarTutorial + " para saltar el tutorial";
        else
            tutorialSkipTexto.text = "";
    }

    private void PonerPanelArriba()
    {
        if (tutorialPanelRect != null)
            tutorialPanelRect.anchoredPosition = posicionPanelArriba;
    }

    private void PonerPanelAbajo()
    {
        if (tutorialPanelRect != null)
            tutorialPanelRect.anchoredPosition = posicionPanelAbajo;
    }

    // ====================
    // INDICADORES
    // ====================

    private void MostrarSoloIndicador(GameObject indicador)
    {
        OcultarTodosLosIndicadores();

        if (indicador != null)
            indicador.SetActive(true);
    }

    private void MostrarIndicadores(GameObject indicadorA, GameObject indicadorB)
    {
        OcultarTodosLosIndicadores();

        if (indicadorA != null)
            indicadorA.SetActive(true);

        if (indicadorB != null)
            indicadorB.SetActive(true);
    }

    private void OcultarTodosLosIndicadores()
    {
        if (indicadorMorsa != null)
            indicadorMorsa.SetActive(false);

        if (indicadorEncargo != null)
            indicadorEncargo.SetActive(false);

        if (indicadorStrikes != null)
            indicadorStrikes.SetActive(false);
    }

    // ====================
    // CINEMATICA
    // ====================

    private IEnumerator MoverCamaraTutorial(Transform destino)
    {
        if (destino == null)
            yield break;

        yield return StartCoroutine(MoverCamaraTutorial(destino.position, destino.rotation));
    }

    private IEnumerator MoverCamaraTutorial(Vector3 posicionFinal, Quaternion rotacionFinal)
    {
        if (camaraTutorial == null)
            yield break;

        Vector3 posicionInicial = camaraTutorial.transform.position;
        Quaternion rotacionInicial = camaraTutorial.transform.rotation;

        float tiempo = 0f;

        while (tiempo < duracionMovimientoCamara)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracionMovimientoCamara;
            t = Mathf.SmoothStep(0f, 1f, t);

            camaraTutorial.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, t);
            camaraTutorial.transform.rotation = Quaternion.Slerp(rotacionInicial, rotacionFinal, t);

            yield return null;
        }

        camaraTutorial.transform.position = posicionFinal;
        camaraTutorial.transform.rotation = rotacionFinal;
    }

    private IEnumerator AplaudirReyMorsaDurante(float duracion)
    {
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            if (reyMorsaAnimacion != null)
            {
                reyMorsaAnimacion.Aplaudir();
            }

            yield return new WaitForSeconds(intervaloAplausoReyMorsa);
            tiempo += intervaloAplausoReyMorsa;
        }
    }

    // ====================
    // BLOQUEO JUGADOR
    // ====================

    private void BloquearJugador()
    {
        BuscarReferencias();

        if (jugador != null)
        {
            jugador.enabled = false;
        }

        if (controladorCamaraJugador != null)
        {
            controladorCamaraJugador.enabled = false;
        }

        if (rbJugador != null)
        {
            rbJugador.linearVelocity = Vector3.zero;
            rbJugador.angularVelocity = Vector3.zero;
            rbJugador.isKinematic = true;
        }
    }

    private void DesbloquearJugador()
    {
        if (rbJugador != null)
        {
            rbJugador.isKinematic = false;
        }

        if (jugador != null)
        {
            jugador.enabled = true;
        }

        if (controladorCamaraJugador != null)
        {
            controladorCamaraJugador.enabled = true;
        }
    }

    // ====================
    // LECTURA
    // ====================

    private void BloquearLectura()
    {
        bloqueoHasta = Time.time + tiempoMinimoLectura;
    }

    private bool PuedeAvanzarPaso()
    {
        return Time.time >= bloqueoHasta;
    }

    // ====================
    // REFERENCIAS
    // ====================

    private void BuscarReferencias()
    {
        if (strikeManager == null)
            strikeManager = FindFirstObjectByType<StrikeManager>();

        if (gestorEncargos == null)
            gestorEncargos = FindFirstObjectByType<GestorEncargosTest>();

        if (camaraJugador == null)
            camaraJugador = Camera.main;

        if (jugador == null)
            jugador = FindFirstObjectByType<Player>();

        if (rbJugador == null && jugador != null)
            rbJugador = jugador.GetComponent<Rigidbody>();

        if (controladorCamaraJugador == null && camaraJugador != null)
            controladorCamaraJugador = camaraJugador.GetComponentInParent<CameraPivotController>();

        if (controladorCamaraJugador == null)
            controladorCamaraJugador = FindFirstObjectByType<CameraPivotController>();

        if (reyMorsaAnimacion == null)
            reyMorsaAnimacion = FindFirstObjectByType<ReyMorsaAnimacion>();

        if (camaraTutorial != null)
            camaraTutorial.gameObject.SetActive(true);
    }
}