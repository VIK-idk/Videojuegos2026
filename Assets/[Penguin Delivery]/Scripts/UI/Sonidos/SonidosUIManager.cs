using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Gestor persistente de los sonidos generales de la interfaz.
/// Usa un único AudioSource 2D y evita que existan varios gestores entre escenas.
/// </summary>
[DisallowMultipleComponent]
public class SonidosUIManager : MonoBehaviour
{
    public static SonidosUIManager Instancia { get; private set; }

    [Header("AudioSource 2D")]
    [SerializeField] private AudioSource audioSourceUI;
    [SerializeField] private AudioMixerGroup grupoMixerUI;

    [Header("Clips botones generales")]
    [SerializeField] private AudioClip sonidoHoverSelected;
    [SerializeField] private AudioClip sonidoPulsar;

    [Header("Clip toggle opciones")]
    [SerializeField] private AudioClip sonidoToggle;

    [Header("Volumen botones generales")]
    [SerializeField, Range(0f, 1f)] private float volumenHoverSelected = 0.8f;
    [SerializeField, Range(0f, 1f)] private float volumenPulsar = 1f;

    [Header("Volumen toggle")]
    [SerializeField, Range(0f, 1f)] private float volumenToggle = 0.8f;

    [Header("Protección contra sonidos repetidos")]
    [SerializeField, Min(0f)] private float intervaloMinimoHover = 0.05f;

    private float ultimoHover = -999f;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;

        if (transform.parent != null)
            transform.SetParent(null);

        DontDestroyOnLoad(gameObject);
        ConfigurarAudioSource();
    }

    private void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }

    private void Reset()
    {
        ConfigurarAudioSource();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
            ConfigurarAudioSource();
    }

    private void ConfigurarAudioSource()
    {
        if (audioSourceUI == null)
            audioSourceUI = GetComponent<AudioSource>();

        if (audioSourceUI == null)
            audioSourceUI = gameObject.AddComponent<AudioSource>();

        audioSourceUI.playOnAwake = false;
        audioSourceUI.loop = false;
        audioSourceUI.spatialBlend = 0f;
        audioSourceUI.dopplerLevel = 0f;
        audioSourceUI.ignoreListenerPause = true;

        if (grupoMixerUI != null)
            audioSourceUI.outputAudioMixerGroup = grupoMixerUI;
    }

    public static void ReproducirHoverSelected()
    {
        if (Instancia == null)
            return;

        Instancia.ReproducirHoverInterno();
    }

    public static void ReproducirPulsar()
    {
        if (Instancia == null)
            return;

        Instancia.ReproducirPulsarInterno();
    }

    public static void ReproducirToggle()
    {
        if (Instancia == null)
            return;

        Instancia.ReproducirToggleInterno();
    }

    private void ReproducirHoverInterno()
    {
        if (audioSourceUI == null || sonidoHoverSelected == null)
            return;

        if (Time.unscaledTime - ultimoHover < intervaloMinimoHover)
            return;

        ultimoHover = Time.unscaledTime;
        audioSourceUI.PlayOneShot(sonidoHoverSelected, volumenHoverSelected);
    }

    private void ReproducirPulsarInterno()
    {
        if (audioSourceUI == null || sonidoPulsar == null)
            return;

        audioSourceUI.PlayOneShot(sonidoPulsar, volumenPulsar);
    }

    private void ReproducirToggleInterno()
    {
        if (audioSourceUI == null || sonidoToggle == null)
            return;

        audioSourceUI.PlayOneShot(sonidoToggle, volumenToggle);
    }
}
