using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private enum PasoTutorial
    {
        Ninguno,
        Movimiento,
        RebotarEnMorsa,
        CompletarPrimerEncargo,
        Finalizado
    }

    [Header("UI Dialogo")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private RectTransform tutorialPanelRect;
    [SerializeField] private CanvasGroup tutorialPanelCanvasGroup;
    [SerializeField] private Text tutorialTitulo;
    [SerializeField] private Text tutorialTexto;
    [SerializeField] private Text tutorialSkipTexto;

    [Header("UI Saltar Tutorial")]
    [SerializeField] private GameObject tutorialSaltarPanel;
    [SerializeField] private Text tutorialSaltarTexto;
    [SerializeField] private string textoSaltarTutorial = ": saltar tutorial";

    [Header("Posicion panel tutorial")]
    [SerializeField] private Vector2 posicionPanelArriba = new Vector2(0f, 300f);
    [SerializeField] private Vector2 posicionPanelAbajo = new Vector2(0f, -300f);

    [Header("Indicadores visuales opcionales")]
    [SerializeField] private GameObject indicadorMorsa;
    [SerializeField] private GameObject indicadorEncargo;
    [SerializeField] private GameObject indicadorStrikes;
    [SerializeField] private bool mantenerIndicadorEncargoVisibleDuranteTutorial = true;

    [Header("Configuracion")]
    [SerializeField] private KeyCode teclaSaltarTutorial = KeyCode.Tab;

    [Header("Saltar tutorial con mando - Input Manager antiguo")]
    [Tooltip("Debe ser un eje configurado como 7th axis en Project Settings > Input Manager.")]
    [SerializeField] private string ejeSaltarTutorialMando = "SaltarTutorialMando";
    [SerializeField, Range(0.1f, 1f)] private float umbralEjeSaltarTutorialMando = 0.6f;

    [SerializeField] private float tiempoMinimoLectura = 1.2f;
    [SerializeField] private float duracionMensajeFinal = 4f;
    [SerializeField] private float inputMinimoMovimiento = 0.1f;
    [SerializeField] private float esperaTrasDetectarAccionTutorial = 0.1f;

    [Header("Dialogo interactivo")]
    [SerializeField] private float velocidadEscrituraDialogo = 0.035f;
    [SerializeField] private float esperaAntesDePermitirAvanzarDialogo = 1.5f;
    [SerializeField] private float duracionFadeDialogo = 0.2f;
    [SerializeField] private string textoContinuarDialogo = "Pulsa cualquier tecla para continuar";

    [Header("Referencias")]
    [SerializeField] private StrikeManager strikeManager;
    [SerializeField] private GestorEncargosTest gestorEncargos;

    [Header("Rey Morsa")]
    [SerializeField] private ReyMorsaAnimacion reyMorsaAnimacion;
    [SerializeField] private float intervaloGestoReyMorsa = 1.4f;

    [Header("Cinematica")]
    [SerializeField] private Camera camaraJugador;
    [SerializeField] private Camera camaraTutorial;

    [SerializeField] private Transform puntoCamaraReyMorsa;
    [SerializeField] private Transform puntoCamaraPeces;
    [SerializeField] private Transform puntoCamaraGuppy;
    [SerializeField] private Transform puntoCamaraMorsa;

    [SerializeField] private float duracionMovimientoCamara = 1.4f;
    [SerializeField] private float pausaEntreDialogos = 0.4f;

    [Header("Jugador")]
    [SerializeField] private Player jugador;
    [SerializeField] private Rigidbody rbJugador;
    [SerializeField] private CameraPivotController controladorCamaraJugador;

    [Header("Animacion jugador")]
    [SerializeField] private Animator animatorJugador;
    [SerializeField] private string parametroVelocidadJugador = "Velocidad";
    [SerializeField] private string parametroMoviendoseJugador = "EstaMoviendose";
    [SerializeField] private string nombreEstadoIdleJugador = "Idle";
    [SerializeField] private bool usarCrossFadeIdleJugador = false;

    [Header("SOLO TESTING")]
    [SerializeField] private bool forzarTutorialSiempre = false;
    [SerializeField] private KeyCode teclaResetTutorial = KeyCode.Y;

    private bool tutorialActivo = false;
    private bool tutorialGuardadoComoCompletado = false;
    private bool saltandoTutorial = false;
    private bool encargoTutorialMostrado = false;

    private PasoTutorial pasoActual = PasoTutorial.Ninguno;

    private float bloqueoHasta = 0f;

    private Vector3 posicionInicialCamaraJugador;
    private Quaternion rotacionInicialCamaraJugador;

    private Coroutine rutinaGestoReyMorsa;
    private Coroutine rutinaTutorial;
    private Coroutine rutinaDialogoEvento;
    private Coroutine rutinaSaltarTutorial;

    private bool ejeSaltarTutorialMandoActivoAnterior = false;
    private bool saltoMandoSolicitadoEsteFrame = false;

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
        PrepararCanvasGroupTutorial();

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);

            if (tutorialPanelRect == null)
                tutorialPanelRect = tutorialPanel.GetComponent<RectTransform>();
        }

        MostrarPanelSaltarTutorial(false);
        OcultarIndicadoresSinApagarEncargoTutorial();

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
        saltoMandoSolicitadoEsteFrame = DetectarPulsacionEjeSaltarTutorialMando();

        if (Input.GetKeyDown(teclaResetTutorial))
        {
            ResetearTutorialGuardado();
            Debug.Log("Tutorial reseteado");
        }

        if (!tutorialActivo)
            return;

        bool saltoSolicitado =
            Input.GetKeyDown(teclaSaltarTutorial) ||
            saltoMandoSolicitadoEsteFrame;

        if (!saltandoTutorial && saltoSolicitado)
        {
            SaltarTutorial();
            return;
        }
    }

    private void OnDisable()
    {
        DetenerGestoReyMorsa();
        DetenerParpadeoStrikesTutorial();
    }

    // ====================
    // INICIO
    // ====================

    public void IniciarTutorial()
    {
        tutorialActivo = true;
        tutorialGuardadoComoCompletado = false;
        saltandoTutorial = false;
        encargoTutorialMostrado = false;
        pasoActual = PasoTutorial.Ninguno;

        ejeSaltarTutorialMandoActivoAnterior =
            Mathf.Abs(LeerEjeSeguro(ejeSaltarTutorialMando)) >= umbralEjeSaltarTutorialMando;
        saltoMandoSolicitadoEsteFrame = false;

        MostrarPanelSaltarTutorial(true);
        ActualizarTextoSkip(false);

        if (rutinaTutorial != null)
            StopCoroutine(rutinaTutorial);

        rutinaTutorial = StartCoroutine(SecuenciaTutorial());
    }

    private IEnumerator SecuenciaTutorial()
    {
        BuscarReferencias();
        BloquearJugador();

        PrepararCamaraTutorial();

        // ====================
        // 1. REY PRESENTA
        // ====================

        PonerPanelAbajo();

        if (puntoCamaraReyMorsa != null)
            yield return StartCoroutine(MoverCamaraTutorial(puntoCamaraReyMorsa));

        IniciarGestoReyMorsa();

        yield return StartCoroutine(MostrarDialogoYEsperar(
            "Rey Morsa",
            "Guppy..."
        ));

        yield return StartCoroutine(MostrarDialogoYEsperar(
            "Rey Morsa",
            "Desde hoy trabajas para mí."
        ));

        yield return StartCoroutine(MostrarDialogoYEsperar(
            "Rey Morsa",
            "Yo pido peces y tú los entregas."
        ));

        DetenerGestoReyMorsa();

        // ====================
        // 2. ENCARGO
        // ====================

        PonerPanelArriba();

        if (puntoCamaraPeces != null)
            yield return StartCoroutine(MoverCamaraTutorial(puntoCamaraPeces));

        // Creamos el encargo una sola vez, justo cuando ya toca enseñarlo.
        // Así entra deslizándose una vez y no se vuelve a apagar/activar en el cambio de texto.
        if (gestorEncargos != null && !encargoTutorialMostrado)
        {
            gestorEncargos.IniciarEncargoTutorial();
            encargoTutorialMostrado = true;
        }

        MostrarIndicadorEncargoSinReiniciar();

        yield return StartCoroutine(MostrarDialogoYEsperar(
            "Encargos",
            "Consigue los peces del encargo para completarlo.",
            false,
            true
        ));

        yield return StartCoroutine(MostrarDialogoYEsperar(
            "Encargos",
            "Completa el pedido antes de que acabe el tiempo.",
            false,
            false
        ));

        // ====================
        // 3. STRIKES
        // ====================

        if (puntoCamaraReyMorsa != null)
            yield return StartCoroutine(MoverCamaraTutorial(puntoCamaraReyMorsa));

        IniciarGestoReyMorsa();

        MostrarIndicadores(indicadorStrikes, indicadorEncargo);
        IniciarParpadeoStrikesTutorial();

        // En esta sección mantenemos el panel visible entre textos para que no quede un hueco vacío.
        yield return StartCoroutine(MostrarDialogoYEsperar(
            "Strikes",
            "Si fallas el encargo, ganas un strike.",
            false,
            false
        ));

        yield return StartCoroutine(MostrarDialogoYEsperar(
            "Strikes",
            "Con 3 strikes, volverás a tu celda.",
            false,
            false
        ));

        DetenerParpadeoStrikesTutorial();
        DetenerGestoReyMorsa();
        OcultarIndicadoresSinApagarEncargoTutorial();

        // ====================
        // 4. VOLVER A GUPPY
        // ====================

        PonerPanelArriba();

        if (puntoCamaraGuppy != null)
        {
            yield return StartCoroutine(MoverCamaraTutorial(puntoCamaraGuppy));
        }
        else
        {
            yield return StartCoroutine(MoverCamaraTutorial(
                posicionInicialCamaraJugador,
                rotacionInicialCamaraJugador
            ));
        }

        yield return StartCoroutine(MostrarDialogoSoloEscribir(
            "Guppy",
            "Ahora muévete y ve a por los peces.",
            false
        ));

        ActivarCamaraJugador();
        DesbloquearJugador();

        pasoActual = PasoTutorial.Movimiento;
        BloquearLectura();

        yield return StartCoroutine(EsperarMovimientoJugador());

        if (!tutorialActivo)
            yield break;

        // Este texto queda visible mientras se enseña la morsa y hasta que el jugador rebote.
        // No espera tecla: el propio rebote en la morsa lo sustituye por el diálogo de peces.
        yield return StartCoroutine(MostrarDialogoSoloEscribir(
            "Bien",
            "Salta sobre una morsa para alcanzar los peces.",
            false
        ));

        if (puntoCamaraMorsa != null)
        {
            BloquearJugador();
            PrepararCamaraTutorialDesdeJugador();

            yield return StartCoroutine(MoverCamaraTutorial(puntoCamaraMorsa));
            yield return new WaitForSeconds(1.2f);

            // Vuelve desde la morsa hasta la posición real de la cámara del jugador,
            // en vez de cortar directamente de una cámara a otra.
            if (camaraJugador != null)
            {
                yield return StartCoroutine(MoverCamaraTutorial(
                    camaraJugador.transform.position,
                    camaraJugador.transform.rotation
                ));
            }

            ActivarCamaraJugador();
            DesbloquearJugador();
        }

        MostrarSoloIndicador(indicadorMorsa);

        pasoActual = PasoTutorial.RebotarEnMorsa;
        BloquearLectura();

        rutinaTutorial = null;
    }

    // ====================
    // GESTO REY MORSA
    // ====================

    private void IniciarGestoReyMorsa()
    {
        if (reyMorsaAnimacion == null)
            return;

        if (rutinaGestoReyMorsa != null)
            StopCoroutine(rutinaGestoReyMorsa);

        rutinaGestoReyMorsa = StartCoroutine(GestoReyMorsaEnBucle());
    }

    private void DetenerGestoReyMorsa()
    {
        if (rutinaGestoReyMorsa != null)
        {
            StopCoroutine(rutinaGestoReyMorsa);
            rutinaGestoReyMorsa = null;
        }
    }

    private IEnumerator GestoReyMorsaEnBucle()
    {
        while (true)
        {
            if (reyMorsaAnimacion != null)
                reyMorsaAnimacion.Enojar();

            yield return new WaitForSeconds(intervaloGestoReyMorsa);
        }
    }

    // ====================
    // PARPADEO STRIKES TUTORIAL
    // ====================

    private void IniciarParpadeoStrikesTutorial()
    {
        if (strikeManager == null)
            return;

        strikeManager.IniciarParpadeoStrikeDemoContinuo();
    }

    private void DetenerParpadeoStrikesTutorial()
    {
        if (strikeManager == null)
            return;

        strikeManager.DetenerParpadeoStrikeDemo();
    }

    // ====================
    // EVENTOS DEL TUTORIAL
    // ====================

    public void NotificarReboteEnMorsa()
    {
        if (!tutorialActivo || saltandoTutorial)
            return;

        if (pasoActual != PasoTutorial.RebotarEnMorsa)
            return;

        if (!PuedeAvanzarPaso())
            return;

        pasoActual = PasoTutorial.CompletarPrimerEncargo;

        if (rutinaDialogoEvento != null)
            StopCoroutine(rutinaDialogoEvento);

        ActualizarTextoSkip(false);

        // Igual que con el paso de caminar: detecta la accion, espera un instante
        // y despues cambia al siguiente dialogo sin esperar otra tecla.
        rutinaDialogoEvento = StartCoroutine(CambiarADialogoPecesTrasRebote());
    }

    private IEnumerator CambiarADialogoPecesTrasRebote()
    {
        yield return new WaitForSeconds(esperaTrasDetectarAccionTutorial);

        if (!tutorialActivo || saltandoTutorial)
            yield break;

        if (indicadorMorsa != null)
            indicadorMorsa.SetActive(false);

        MostrarIndicadorEncargoSinReiniciar();

        // Sustituye el texto de la morsa y se queda visible hasta que se complete el tutorial.
        yield return StartCoroutine(MostrarDialogoPecesHastaCompletar());
    }

    private IEnumerator MostrarDialogoPecesHastaCompletar()
    {
        yield return StartCoroutine(MostrarDialogoSoloEscribir(
            "Peces",
            "Recolecta los peces para completar tu primer encargo.",
            false
        ));

        rutinaDialogoEvento = null;
    }

    public void NotificarPrimerPezRecogido()
    {
        if (!tutorialActivo)
            return;

        if (pasoActual != PasoTutorial.CompletarPrimerEncargo)
            return;

        // El final lo controla GestorEncargosTest cuando se completa el encargo.
    }

    public void OcultarIndicadoresTutorial()
    {
        OcultarIndicadoresSinApagarEncargoTutorial();
    }

    // ====================
    // MENSAJE FINAL
    // ====================

    public IEnumerator MostrarMensajeFinalTutorial()
    {
        yield return StartCoroutine(MostrarMensajeFinalTutorial(false));
    }

    public IEnumerator MostrarMensajeFinalTutorial(bool tutorialSaltado)
    {
        pasoActual = PasoTutorial.Finalizado;

        DetenerGestoReyMorsa();
        DetenerParpadeoStrikesTutorial();

        if (rutinaDialogoEvento != null)
        {
            StopCoroutine(rutinaDialogoEvento);
            rutinaDialogoEvento = null;
        }

        PonerPanelArriba();

        ActualizarTextoSkip(false);
        MostrarPanelSaltarTutorial(false);
        OcultarIndicadoresSinApagarEncargoTutorial();

        string tituloFinal = tutorialSaltado ? "Tutorial Saltado" : "Tutorial completado";

        yield return StartCoroutine(MostrarDialogoTemporizado(
            tituloFinal,
            "Has completado el tutorial, ahora ve a convertirte en el mejor repartidor.",
            duracionMensajeFinal,
            true,
            false
        ));

        tutorialActivo = false;
        OcultarPanelInstantaneo();
    }

    // ====================
    // SALTAR TUTORIAL
    // ====================

    public void PulsarBotonSaltarTutorial()
    {
        SaltarTutorial();
    }

    private void SaltarTutorial()
    {
        if (saltandoTutorial)
            return;

        saltandoTutorial = true;

        // Corta cualquier diálogo/fade/cámara que estuviera en marcha.
        // Esto evita que una rutina antigua oculte el panel justo cuando aparece el final.
        StopAllCoroutines();

        rutinaTutorial = null;
        rutinaDialogoEvento = null;
        rutinaGestoReyMorsa = null;
        rutinaSaltarTutorial = null;

        rutinaSaltarTutorial = StartCoroutine(SaltarTutorialCoroutine());
    }

    private IEnumerator SaltarTutorialCoroutine()
    {
        DetenerGestoReyMorsa();
        DetenerParpadeoStrikesTutorial();
        OcultarTodosLosIndicadores();
        ActualizarTextoSkip(false);
        MostrarPanelSaltarTutorial(false);

        // Si TAB se pulsa durante una cinematica, no cortamos de golpe:
        // la camara tutorial vuelve suavemente hacia Guppy antes de reactivar la camara del jugador.
        yield return StartCoroutine(VolverCamaraAlJugadorSiHaceFalta());

        ActivarCamaraJugador();
        DesbloquearJugador();

        yield return StartCoroutine(MostrarMensajeFinalTutorial(true));

        MarcarTutorialComoCompletado();

        if (gestorEncargos != null)
            gestorEncargos.SaltarTutorialYEmpezarJuegoNormal();

        rutinaSaltarTutorial = null;
    }

    // ====================
    // UI / DIALOGO
    // ====================

    private IEnumerator MostrarDialogoYEsperar(
        string titulo,
        string texto,
        bool ocultarAlFinal = true,
        bool fundirSalidaInicial = true,
        bool permitirDuranteSalto = false)
    {

        yield return StartCoroutine(PrepararEntradaDialogo(titulo, fundirSalidaInicial));
        yield return StartCoroutine(EscribirTextoDialogo(texto, permitirDuranteSalto));

        if (saltandoTutorial && !permitirDuranteSalto)
            yield break;

        yield return new WaitForSeconds(esperaAntesDePermitirAvanzarDialogo);

        ActualizarTextoSkip(true);

        yield return StartCoroutine(EsperarCualquierTeclaParaContinuar());

        ActualizarTextoSkip(false);

        if (ocultarAlFinal)
            yield return StartCoroutine(FadePanelTutorial(0f, false));

    }

    private IEnumerator MostrarDialogoSoloEscribir(
        string titulo,
        string texto,
        bool fundirSalidaInicial = true,
        bool permitirDuranteSalto = false)
    {

        yield return StartCoroutine(PrepararEntradaDialogo(titulo, fundirSalidaInicial));
        yield return StartCoroutine(EscribirTextoDialogo(texto, permitirDuranteSalto));

    }

    private IEnumerator MostrarDialogoTemporizado(
        string titulo,
        string texto,
        float duracionVisible,
        bool permitirDuranteSalto = false,
        bool fundirSalidaInicial = true)
    {

        yield return StartCoroutine(PrepararEntradaDialogo(titulo, fundirSalidaInicial));
        yield return StartCoroutine(EscribirTextoDialogo(texto, permitirDuranteSalto));

        ActualizarTextoSkip(false);

        yield return new WaitForSeconds(duracionVisible);
        yield return StartCoroutine(FadePanelTutorial(0f, false));

    }

    private IEnumerator PrepararEntradaDialogo(string titulo, bool fundirSalidaInicial)
    {
        PrepararCanvasGroupTutorial();

        // Si NO queremos fundir la salida inicial, mantenemos/activamos el panel
        // al 100% y solo cambiamos el texto. Así no hay huecos vacíos entre diálogos.
        if (!fundirSalidaInicial)
        {
            MostrarPanelInstantaneoConAlpha(1f);

            if (tutorialTitulo != null)
                tutorialTitulo.text = titulo;

            if (tutorialTexto != null)
                tutorialTexto.text = "";

            ActualizarTextoSkip(false);
            ActualizarTextoSaltarTutorial();

            // Evita que la tecla usada antes complete este texto al instante.
            yield return null;
            yield break;
        }

        bool panelVisible = tutorialPanel != null &&
                            tutorialPanel.activeSelf &&
                            tutorialPanelCanvasGroup != null &&
                            tutorialPanelCanvasGroup.alpha > 0f;

        if (panelVisible)
            yield return StartCoroutine(FadePanelTutorial(0f, false));

        MostrarPanelInstantaneoConAlpha(0f);

        if (tutorialTitulo != null)
            tutorialTitulo.text = titulo;

        if (tutorialTexto != null)
            tutorialTexto.text = "";

        ActualizarTextoSkip(false);
        ActualizarTextoSaltarTutorial();

        yield return StartCoroutine(FadePanelTutorial(1f, true));

        // Evita que la tecla usada para avanzar el diálogo anterior complete este texto al instante.
        yield return null;
    }

    private IEnumerator EscribirTextoDialogo(string textoCompleto, bool permitirDuranteSalto = false)
    {
        if (tutorialTexto == null)
            yield break;

        tutorialTexto.text = "";

        // Evita capturar el Input.anyKeyDown de la misma pulsación que abrió este texto.
        yield return null;

        for (int i = 0; i < textoCompleto.Length; i++)
        {
            if (saltandoTutorial && !permitirDuranteSalto)
                yield break;

            tutorialTexto.text += textoCompleto[i];

            float tiempo = 0f;

            while (tiempo < velocidadEscrituraDialogo)
            {
                tiempo += Time.deltaTime;

                if (!saltandoTutorial && InputParaCompletarTexto())
                {
                    tutorialTexto.text = textoCompleto;

                    // Consumimos un frame para que la misma tecla no afecte al siguiente paso.
                    yield return null;
                    yield break;
                }

                yield return null;
            }
        }

        tutorialTexto.text = textoCompleto;
    }

    private IEnumerator EsperarCualquierTeclaParaContinuar()
    {
        // Evita que una tecla mantenida o el input del typewriter avance inmediatamente.
        yield return null;

        while (!saltandoTutorial)
        {
            if (InputParaContinuarDialogo())
                yield break;

            yield return null;
        }
    }

    private bool InputParaCompletarTexto()
    {
        // TAB y el 7th axis están reservados exclusivamente para saltar el tutorial.
        if (Input.GetKeyDown(teclaSaltarTutorial) || saltoMandoSolicitadoEsteFrame)
            return false;

        return Input.anyKeyDown || Input.GetButtonDown("Submit") || Input.GetButtonDown("Cancel");
    }

    private bool InputParaContinuarDialogo()
    {
        // TAB y el 7th axis no avanzan diálogos: saltan directamente al final.
        if (Input.GetKeyDown(teclaSaltarTutorial) || saltoMandoSolicitadoEsteFrame)
            return false;

        return Input.anyKeyDown || Input.GetButtonDown("Submit") || Input.GetButtonDown("Cancel");
    }

    private bool DetectarPulsacionEjeSaltarTutorialMando()
    {
        float valor = Mathf.Abs(LeerEjeSeguro(ejeSaltarTutorialMando));
        bool activoAhora = valor >= umbralEjeSaltarTutorialMando;
        bool pulsadoEsteFrame = activoAhora && !ejeSaltarTutorialMandoActivoAnterior;

        ejeSaltarTutorialMandoActivoAnterior = activoAhora;
        return pulsadoEsteFrame;
    }

    private float LeerEjeSeguro(string nombreEje)
    {
        if (string.IsNullOrWhiteSpace(nombreEje))
            return 0f;

        try
        {
            return Input.GetAxisRaw(nombreEje);
        }
        catch
        {
            return 0f;
        }
    }

    private void PrepararCanvasGroupTutorial()
    {
        if (tutorialPanel == null)
            return;

        if (tutorialPanelCanvasGroup == null)
            tutorialPanelCanvasGroup = tutorialPanel.GetComponent<CanvasGroup>();

        if (tutorialPanelCanvasGroup == null)
            tutorialPanelCanvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
    }

    private void MostrarPanelInstantaneoConAlpha(float alpha)
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        PrepararCanvasGroupTutorial();

        if (tutorialPanelCanvasGroup != null)
        {
            tutorialPanelCanvasGroup.alpha = alpha;
            tutorialPanelCanvasGroup.interactable = alpha > 0f;
            tutorialPanelCanvasGroup.blocksRaycasts = alpha > 0f;
        }
    }

    private void OcultarPanelInstantaneo()
    {
        if (tutorialPanelCanvasGroup != null)
        {
            tutorialPanelCanvasGroup.alpha = 0f;
            tutorialPanelCanvasGroup.interactable = false;
            tutorialPanelCanvasGroup.blocksRaycasts = false;
        }

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    private IEnumerator FadePanelTutorial(float alphaFinal, bool mantenerActivo)
    {
        PrepararCanvasGroupTutorial();

        if (tutorialPanel == null || tutorialPanelCanvasGroup == null)
            yield break;

        tutorialPanel.SetActive(true);

        float alphaInicial = tutorialPanelCanvasGroup.alpha;
        float tiempo = 0f;
        float duracion = Mathf.Max(0.01f, duracionFadeDialogo);

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            t = Mathf.SmoothStep(0f, 1f, t);

            tutorialPanelCanvasGroup.alpha = Mathf.Lerp(alphaInicial, alphaFinal, t);
            yield return null;
        }

        tutorialPanelCanvasGroup.alpha = alphaFinal;
        tutorialPanelCanvasGroup.interactable = alphaFinal > 0f;
        tutorialPanelCanvasGroup.blocksRaycasts = alphaFinal > 0f;

        if (!mantenerActivo && alphaFinal <= 0f)
            tutorialPanel.SetActive(false);
    }

    private void ActualizarTextoSkip(bool mostrar)
    {
        if (tutorialSkipTexto == null)
            return;

        if (mostrar)
        {
            tutorialSkipTexto.text = textoContinuarDialogo;
            tutorialSkipTexto.enabled = true;
        }
        else
        {
            tutorialSkipTexto.text = "";
            tutorialSkipTexto.enabled = false;
        }
    }

    private void ActualizarTextoSaltarTutorial()
    {
        if (tutorialSaltarTexto == null)
            return;

        tutorialSaltarTexto.text = textoSaltarTutorial;
        tutorialSaltarTexto.enabled = tutorialActivo && !saltandoTutorial && pasoActual != PasoTutorial.Finalizado;
    }

    private void MostrarPanelSaltarTutorial(bool mostrar)
    {
        if (tutorialSaltarPanel != null)
            tutorialSaltarPanel.SetActive(mostrar);

        if (tutorialSaltarTexto != null)
            tutorialSaltarTexto.enabled = mostrar;

        if (mostrar)
            ActualizarTextoSaltarTutorial();
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

    private void MostrarIndicadorEncargoSinReiniciar()
    {
        // Importante: no apagamos el indicadorEncargo si ya está activo.
        // Si este campo apunta al panel real del encargo, apagarlo y encenderlo produce
        // el parpadeo de milisegundos que se veía en el tutorial.
        if (indicadorMorsa != null)
            indicadorMorsa.SetActive(false);

        if (indicadorStrikes != null)
            indicadorStrikes.SetActive(false);

        if (indicadorEncargo != null && !indicadorEncargo.activeSelf)
            indicadorEncargo.SetActive(true);
    }

    private void MostrarSoloIndicador(GameObject indicador)
    {
        OcultarIndicadoresSinApagarEncargoTutorial();

        if (indicador != null && !indicador.activeSelf)
            indicador.SetActive(true);
    }

    private void MostrarIndicadores(GameObject indicadorA, GameObject indicadorB)
    {
        OcultarIndicadoresSinApagarEncargoTutorial();

        if (indicadorA != null)
            indicadorA.SetActive(true);

        if (indicadorB != null)
            indicadorB.SetActive(true);
    }

    private void OcultarIndicadoresSinApagarEncargoTutorial()
    {
        if (indicadorMorsa != null)
            indicadorMorsa.SetActive(false);

        if (indicadorStrikes != null)
            indicadorStrikes.SetActive(false);

        if (indicadorEncargo != null && !DebeMantenerIndicadorEncargoVisible())
            indicadorEncargo.SetActive(false);
    }

    private bool DebeMantenerIndicadorEncargoVisible()
    {
        return mantenerIndicadorEncargoVisibleDuranteTutorial &&
               encargoTutorialMostrado &&
               !saltandoTutorial;
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

    private void PrepararCamaraTutorial()
    {
        if (camaraJugador == null)
            camaraJugador = Camera.main;

        if (camaraJugador != null)
        {
            posicionInicialCamaraJugador = camaraJugador.transform.position;
            rotacionInicialCamaraJugador = camaraJugador.transform.rotation;
        }

        if (camaraJugador == null || camaraTutorial == null)
            return;

        camaraTutorial.transform.position = camaraJugador.transform.position;
        camaraTutorial.transform.rotation = camaraJugador.transform.rotation;

        camaraTutorial.gameObject.SetActive(true);
        camaraTutorial.enabled = true;

        camaraJugador.enabled = false;
    }

    private void PrepararCamaraTutorialDesdeJugador()
    {
        if (camaraJugador == null)
            camaraJugador = Camera.main;

        if (camaraJugador == null || camaraTutorial == null)
            return;

        camaraTutorial.transform.position = camaraJugador.transform.position;
        camaraTutorial.transform.rotation = camaraJugador.transform.rotation;

        camaraTutorial.gameObject.SetActive(true);
        camaraTutorial.enabled = true;

        camaraJugador.enabled = false;
    }

    private IEnumerator VolverCamaraAlJugadorSiHaceFalta()
    {
        if (camaraTutorial == null || camaraJugador == null)
            yield break;

        if (!camaraTutorial.enabled)
            yield break;

        Transform destino = puntoCamaraGuppy != null ? puntoCamaraGuppy : camaraJugador.transform;

        yield return StartCoroutine(MoverCamaraTutorial(destino.position, destino.rotation));
    }

    private void ActivarCamaraJugador()
    {
        if (camaraJugador != null)
            camaraJugador.enabled = true;

        if (camaraTutorial != null)
            camaraTutorial.enabled = false;
    }

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
        float duracion = Mathf.Max(0.01f, duracionMovimientoCamara);

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracion);
            t = Mathf.SmoothStep(0f, 1f, t);

            camaraTutorial.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, t);
            camaraTutorial.transform.rotation = Quaternion.Slerp(rotacionInicial, rotacionFinal, t);

            yield return null;
        }

        camaraTutorial.transform.position = posicionFinal;
        camaraTutorial.transform.rotation = rotacionFinal;

        if (pausaEntreDialogos > 0f)
            yield return new WaitForSeconds(pausaEntreDialogos);
    }

    // ====================
    // JUGADOR
    // ====================

    private IEnumerator EsperarMovimientoJugador()
    {
        while (tutorialActivo && pasoActual == PasoTutorial.Movimiento && !saltandoTutorial)
        {
            if (PuedeAvanzarPaso() && JugadorHaEmpezadoAMoverse())
            {
                yield return new WaitForSeconds(esperaTrasDetectarAccionTutorial);
                yield break;
            }

            yield return null;
        }
    }

    private bool JugadorHaEmpezadoAMoverse()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector2 inputMovimiento = new Vector2(inputX, inputZ);

        bool seMovio = inputMovimiento.magnitude > inputMinimoMovimiento;
        bool salto = Input.GetButtonDown("Saltar");

        return seMovio || salto;
    }

    private void BloquearJugador()
    {
        BuscarReferencias();

        DetenerMovimientoFisicoJugador();
        PonerJugadorEnIdle();

        if (jugador != null)
            jugador.enabled = false;

        if (controladorCamaraJugador != null)
            controladorCamaraJugador.enabled = false;

        if (rbJugador != null)
            rbJugador.isKinematic = true;
    }

    private void DetenerMovimientoFisicoJugador()
    {
        if (rbJugador == null)
            return;

        rbJugador.linearVelocity = Vector3.zero;
        rbJugador.angularVelocity = Vector3.zero;
    }

    private void PonerJugadorEnIdle()
    {
        if (animatorJugador == null)
            return;

        SetFloatSiExiste(animatorJugador, parametroVelocidadJugador, 0f);
        SetBoolSiExiste(animatorJugador, parametroMoviendoseJugador, false);
        SetBoolSiExiste(animatorJugador, "Caminando", false);

        if (usarCrossFadeIdleJugador && !string.IsNullOrEmpty(nombreEstadoIdleJugador))
        {
            animatorJugador.CrossFade(nombreEstadoIdleJugador, 0.05f);
        }

        animatorJugador.Update(0f);
    }

    private void SetFloatSiExiste(Animator animator, string nombreParametro, float valor)
    {
        if (animator == null || string.IsNullOrEmpty(nombreParametro))
            return;

        for (int i = 0; i < animator.parameters.Length; i++)
        {
            AnimatorControllerParameter parametro = animator.parameters[i];

            if (parametro.name == nombreParametro &&
                parametro.type == AnimatorControllerParameterType.Float)
            {
                animator.SetFloat(nombreParametro, valor);
                return;
            }
        }
    }

    private void SetBoolSiExiste(Animator animator, string nombreParametro, bool valor)
    {
        if (animator == null || string.IsNullOrEmpty(nombreParametro))
            return;

        for (int i = 0; i < animator.parameters.Length; i++)
        {
            AnimatorControllerParameter parametro = animator.parameters[i];

            if (parametro.name == nombreParametro &&
                parametro.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(nombreParametro, valor);
                return;
            }
        }
    }

    private void DesbloquearJugador()
    {
        if (rbJugador != null)
            rbJugador.isKinematic = false;

        if (jugador != null)
            jugador.enabled = true;

        if (controladorCamaraJugador != null)
            controladorCamaraJugador.enabled = true;
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

        if (animatorJugador == null && jugador != null)
            animatorJugador = jugador.GetComponentInChildren<Animator>();

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
