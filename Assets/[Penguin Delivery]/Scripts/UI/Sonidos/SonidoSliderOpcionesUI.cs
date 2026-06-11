using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Sonido de hover/selected y sonido de prueba para un Slider de opciones.
/// El sonido de prueba se envía al grupo del AudioMixer asignado al slider.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Slider))]
public class SonidoSliderOpcionesUI : MonoBehaviour,
    IPointerEnterHandler,
    ISelectHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IDeselectHandler
{
    [Header("Slider")]
    [SerializeField] private Slider slider;

    [Header("Audio de prueba")]
    [SerializeField] private AudioSource audioSourcePrueba;
    [SerializeField] private AudioMixerGroup grupoSalida;
    [SerializeField] private AudioClip sonidoPrueba;
    [SerializeField, Range(0f, 1f)] private float volumenPrueba = 0.8f;

    [Header("Cuándo reproducir")]
    [SerializeField] private bool reproducirAlPulsar = false;
    [SerializeField] private bool reproducirAlSoltar = true;
    [SerializeField] private bool reproducirConTecladoMando = true;
    [SerializeField, Min(0f)] private float retardoTecladoMando = 0.14f;

    [Header("Hover / Selected")]
    [SerializeField] private bool reproducirHoverSelected = true;
    [SerializeField, Min(0f)] private float intervaloHoverLocal = 0.06f;

    private bool punteroPulsado;
    private bool listenerRegistrado;
    private float ultimoHover = -999f;
    private Coroutine rutinaPruebaPendiente;

    private void Awake()
    {
        BuscarReferencias();
        ConfigurarAudioSource();
        RegistrarListener();
    }

    private void OnEnable()
    {
        BuscarReferencias();
        ConfigurarAudioSource();
        RegistrarListener();
        ultimoHover = -999f;
        punteroPulsado = false;
    }

    private void OnDisable()
    {
        CancelarPruebaPendiente();
        punteroPulsado = false;
    }

    private void OnDestroy()
    {
        QuitarListener();
    }

    private void Reset()
    {
        BuscarReferencias();
        ConfigurarAudioSource();
    }

    private void BuscarReferencias()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (audioSourcePrueba == null)
            audioSourcePrueba = GetComponent<AudioSource>();

        if (audioSourcePrueba == null)
            audioSourcePrueba = gameObject.AddComponent<AudioSource>();
    }

    private void ConfigurarAudioSource()
    {
        if (audioSourcePrueba == null)
            return;

        audioSourcePrueba.playOnAwake = false;
        audioSourcePrueba.loop = false;
        audioSourcePrueba.spatialBlend = 0f;
        audioSourcePrueba.dopplerLevel = 0f;
        audioSourcePrueba.ignoreListenerPause = true;

        if (grupoSalida != null)
            audioSourcePrueba.outputAudioMixerGroup = grupoSalida;
    }

    private void RegistrarListener()
    {
        if (listenerRegistrado || slider == null)
            return;

        slider.onValueChanged.AddListener(AlCambiarValor);
        listenerRegistrado = true;
    }

    private void QuitarListener()
    {
        if (!listenerRegistrado || slider == null)
            return;

        slider.onValueChanged.RemoveListener(AlCambiarValor);
        listenerRegistrado = false;
    }

    private bool Disponible()
    {
        return slider != null &&
               slider.interactable &&
               gameObject.activeInHierarchy;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ReproducirHover();
    }

    public void OnSelect(BaseEventData eventData)
    {
        ReproducirHover();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !Disponible())
            return;

        punteroPulsado = true;
        CancelarPruebaPendiente();

        if (reproducirAlPulsar)
            ReproducirPrueba();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        punteroPulsado = false;
        CancelarPruebaPendiente();

        if (reproducirAlSoltar && Disponible())
            ReproducirPrueba();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        CancelarPruebaPendiente();
    }

    private void AlCambiarValor(float valor)
    {
        if (!reproducirConTecladoMando || punteroPulsado || !Disponible())
            return;

        if (EventSystem.current == null ||
            EventSystem.current.currentSelectedGameObject != gameObject)
        {
            return;
        }

        CancelarPruebaPendiente();
        rutinaPruebaPendiente = StartCoroutine(ReproducirPruebaTrasRetardo());
    }

    private IEnumerator ReproducirPruebaTrasRetardo()
    {
        if (retardoTecladoMando > 0f)
            yield return new WaitForSecondsRealtime(retardoTecladoMando);

        rutinaPruebaPendiente = null;
        ReproducirPrueba();
    }

    private void ReproducirHover()
    {
        if (!reproducirHoverSelected || !Disponible())
            return;

        if (Time.unscaledTime - ultimoHover < intervaloHoverLocal)
            return;

        ultimoHover = Time.unscaledTime;
        SonidosUIManager.ReproducirHoverSelected();
    }

    public void ReproducirPrueba()
    {
        if (!Disponible() || audioSourcePrueba == null || sonidoPrueba == null)
            return;

        ConfigurarAudioSource();

        // Stop + Play evita que varias pruebas largas se acumulen unas encima de otras.
        audioSourcePrueba.Stop();
        audioSourcePrueba.clip = sonidoPrueba;
        audioSourcePrueba.volume = volumenPrueba;
        audioSourcePrueba.Play();
    }

    private void CancelarPruebaPendiente()
    {
        if (rutinaPruebaPendiente == null)
            return;

        StopCoroutine(rutinaPruebaPendiente);
        rutinaPruebaPendiente = null;
    }

    public void Configurar(
        AudioClip nuevoSonido,
        AudioMixerGroup nuevoGrupo,
        float nuevoVolumen,
        bool usarHoverSelected,
        bool sonarAlPulsar,
        bool sonarAlSoltar,
        bool sonarConTecladoMando,
        float nuevoRetardoTecladoMando)
    {
        sonidoPrueba = nuevoSonido;
        grupoSalida = nuevoGrupo;
        volumenPrueba = Mathf.Clamp01(nuevoVolumen);
        reproducirHoverSelected = usarHoverSelected;
        reproducirAlPulsar = sonarAlPulsar;
        reproducirAlSoltar = sonarAlSoltar;
        reproducirConTecladoMando = sonarConTecladoMando;
        retardoTecladoMando = Mathf.Max(0f, nuevoRetardoTecladoMando);

        BuscarReferencias();
        ConfigurarAudioSource();
        RegistrarListener();
    }
}
