using UnityEngine;
using UnityEngine.UI;

// ====================
// SLOT HABILIDAD UI
// Cambia el visual del slot según el estado de la habilidad
// ====================
public class HabilidadSlotUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RectTransform slotRect;
    [SerializeField] private Image fondoSlot;
    [SerializeField] private Image iconoHabilidad;
    [SerializeField] private Sprite iconoSlotVacio;

    [Header("Sprites del slot")]
    [SerializeField] private Sprite spriteNormal;
    [SerializeField] private Sprite spriteActivo;
    [SerializeField] private Sprite spriteDesactivado;
    [SerializeField] private Sprite spriteVacio;

    [Header("Textos e Inputs")]
    [SerializeField] private Text textoTitulo;
    [SerializeField] private Text textoTecla;
    [SerializeField] private Text textoTiempo;
    // === NUEVA REFERENCIA A LA IMAGEN DEL BOTÓN ===
    [SerializeField] private Image imagenBotonEntrada; 

    [Header("Temporizador")]
    [SerializeField] private GameObject tiempoPanel;

    [Header("Movimiento")]
    [SerializeField] private bool usarPosicionInicialComoVisible = true;
    [SerializeField] private Vector2 posicionVisibleManual = Vector2.zero;
    [SerializeField] private Vector2 desplazamientoEscondido = new Vector2(120f, 0f);
    [SerializeField] private float velocidadMovimiento = 12f;

    [Header("Colores de icono")]
    [SerializeField] private Color colorIconoNormal = Color.white;
    [SerializeField] private Color colorIconoApagado = new Color(1f, 1f, 1f, 0.45f);

    [Header("Colores de texto")]
    [SerializeField] private Color colorTextoNormal = Color.white;
    [SerializeField] private Color colorTextoApagado = new Color(1f, 1f, 1f, 0.5f);

    private Vector2 posicionVisible;
    private Vector2 posicionEscondida;
    private Vector2 posicionObjetivo;

    // === VARIABLES PARA EL SISTEMA DINÁMICO ===
    private bool estadoAnteriorMando;
    private bool estadoActualApagado;
    private Sprite spriteTecladoActual;
    private Sprite spriteMandoActual;

    private enum EstadoVisual
    {
        Vacio, Disponible, Activa, Cooldown, Bloqueada, Usada
    }

    private void Awake()
    {
        if (slotRect == null) slotRect = GetComponent<RectTransform>();
        if (slotRect != null)
        {
            if (usarPosicionInicialComoVisible) posicionVisible = slotRect.anchoredPosition;
            else posicionVisible = posicionVisibleManual;

            posicionEscondida = posicionVisible + desplazamientoEscondido;
            posicionObjetivo = posicionVisible;
        }
    }

    private void Start()
    {
        estadoAnteriorMando = InputDetector.usandoMando;
        ActualizarIconoEntrada();
    }

    private void Update()
    {
        // Detecta si se cambia de mando a teclado o viceversa
        if (estadoAnteriorMando != InputDetector.usandoMando)
        {
            estadoAnteriorMando = InputDetector.usandoMando;
            ActualizarIconoEntrada();
        }

        if (slotRect == null) return;

        slotRect.anchoredPosition = Vector2.Lerp(
            slotRect.anchoredPosition, posicionObjetivo, velocidadMovimiento * Time.deltaTime
        );
    }

    // ====================
    // ESTADOS (Ahora piden los iconos de teclado y mando)
    // ====================

    public void MostrarVacio()
    {
        AplicarEstado(EstadoVisual.Vacio, "Vacío", "", iconoSlotVacio, null, null, false, 0f);
    }

    public void MostrarDisponible(string titulo, string tecla, Sprite icono, Sprite iconoTeclado, Sprite iconoMando)
    {
        AplicarEstado(EstadoVisual.Disponible, titulo, tecla, icono, iconoTeclado, iconoMando, false, 0f);
    }

    public void MostrarActiva(string titulo, string tecla, float tiempo, Sprite icono, Sprite iconoTeclado, Sprite iconoMando)
    {
        AplicarEstado(EstadoVisual.Activa, titulo, tecla, icono, iconoTeclado, iconoMando, true, tiempo);
    }

    public void MostrarCooldown(string titulo, string tecla, float tiempo, Sprite icono, Sprite iconoTeclado, Sprite iconoMando)
    {
        AplicarEstado(EstadoVisual.Cooldown, titulo, tecla, icono, iconoTeclado, iconoMando, true, tiempo);
    }

    public void MostrarBloqueada(string titulo, string tecla, Sprite icono, Sprite iconoTeclado, Sprite iconoMando)
    {
        AplicarEstado(EstadoVisual.Bloqueada, titulo, tecla, icono, iconoTeclado, iconoMando, false, 0f);
    }

    public void MostrarUsada(string titulo, Sprite icono, Sprite iconoTeclado, Sprite iconoMando)
    {
        AplicarEstado(EstadoVisual.Usada, titulo, "Usada", icono, iconoTeclado, iconoMando, false, 0f);
    }

    // ====================
    // APLICAR VISUAL
    // ====================

    private void AplicarEstado(EstadoVisual estado, string titulo, string tecla, Sprite icono, Sprite iconoTeclado, Sprite iconoMando, bool mostrarTiempo, float tiempo)
    {
        estadoActualApagado = EsEstadoApagado(estado);
        posicionObjetivo = DebeEstarEscondido(estado) ? posicionEscondida : posicionVisible;

        // Guardamos las imágenes de la habilidad actual
        spriteTecladoActual = iconoTeclado;
        spriteMandoActual = iconoMando;

        AplicarFondo(estado);
        AplicarIcono(icono, estadoActualApagado);
        AplicarTextos(titulo, tecla, estadoActualApagado);
        AplicarTemporizador(mostrarTiempo, tiempo);
        
        ActualizarIconoEntrada();
    }

    private void ActualizarIconoEntrada()
    {
        if (imagenBotonEntrada == null) return;

        Sprite spriteAMostrar = InputDetector.usandoMando ? spriteMandoActual : spriteTecladoActual;
        
        imagenBotonEntrada.sprite = spriteAMostrar;
        imagenBotonEntrada.enabled = spriteAMostrar != null; 
        imagenBotonEntrada.color = estadoActualApagado ? colorIconoApagado : colorIconoNormal;
    }

    private bool EsEstadoApagado(EstadoVisual estado)
    {
        return estado == EstadoVisual.Vacio || estado == EstadoVisual.Cooldown || estado == EstadoVisual.Bloqueada || estado == EstadoVisual.Usada;
    }

    private bool DebeEstarEscondido(EstadoVisual estado)
    {
        return estado == EstadoVisual.Vacio || estado == EstadoVisual.Usada;
    }

    private void AplicarFondo(EstadoVisual estado)
    {
        if (fondoSlot == null) return;

        if (estado == EstadoVisual.Activa) fondoSlot.sprite = spriteActivo != null ? spriteActivo : spriteNormal;
        else if (estado == EstadoVisual.Vacio) fondoSlot.sprite = spriteVacio != null ? spriteVacio : spriteDesactivado;
        else if (estado == EstadoVisual.Cooldown || estado == EstadoVisual.Bloqueada || estado == EstadoVisual.Usada) fondoSlot.sprite = spriteDesactivado != null ? spriteDesactivado : spriteNormal;
        else fondoSlot.sprite = spriteNormal;

        fondoSlot.color = Color.white;
    }

    private void AplicarIcono(Sprite icono, bool apagado)
    {
        if (iconoHabilidad == null) return;
        iconoHabilidad.sprite = icono;
        iconoHabilidad.enabled = icono != null;
        iconoHabilidad.color = apagado ? colorIconoApagado : colorIconoNormal;
    }

    private void AplicarTextos(string titulo, string tecla, bool apagado)
    {
        Color colorTexto = apagado ? colorTextoApagado : colorTextoNormal;
        if (textoTitulo != null) { textoTitulo.text = titulo; textoTitulo.color = colorTexto; }
        if (textoTecla != null) { textoTecla.text = tecla; textoTecla.color = colorTexto; }
    }

    private void AplicarTemporizador(bool mostrarTiempo, float tiempo)
    {
        if (tiempoPanel != null) tiempoPanel.SetActive(mostrarTiempo);
        if (textoTiempo != null)
        {
            textoTiempo.gameObject.SetActive(mostrarTiempo);
            if (mostrarTiempo) textoTiempo.text = Mathf.CeilToInt(Mathf.Max(tiempo, 0f)).ToString();
        }
    }
}