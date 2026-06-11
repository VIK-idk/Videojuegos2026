using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Reproduce un ambiente 2D en bucle únicamente en la escena donde se coloca.
/// No usa DontDestroyOnLoad: al salir de Gameplay o Tutorial desaparece.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class AmbienteEscena : MonoBehaviour
{
    [Header("Audio ambiente")]
    [SerializeField] private AudioSource audioSourceAmbiente;
    [SerializeField] private AudioMixerGroup grupoMixerAmbiente;
    [SerializeField] private AudioClip sonidoAmbiente;
    [SerializeField, Range(0f, 1f)] private float volumenObjetivo = 0.65f;

    [Header("Reproducción")]
    [SerializeField] private bool reproducirAutomaticamente = true;
    [SerializeField] private bool empezarEnPuntoAleatorio = false;
    [SerializeField, Min(0f)] private float duracionFadeEntrada = 1f;

    private Coroutine rutinaFade;

    private void Awake()
    {
        ConfigurarAudioSource();
    }

    private void Start()
    {
        if (reproducirAutomaticamente)
            Reproducir();
    }

    private void OnDisable()
    {
        if (rutinaFade != null)
        {
            StopCoroutine(rutinaFade);
            rutinaFade = null;
        }

        if (audioSourceAmbiente != null)
            audioSourceAmbiente.Stop();
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
        if (audioSourceAmbiente == null)
            audioSourceAmbiente = GetComponent<AudioSource>();

        if (audioSourceAmbiente == null)
            return;

        audioSourceAmbiente.playOnAwake = false;
        audioSourceAmbiente.loop = true;
        audioSourceAmbiente.spatialBlend = 0f;
        audioSourceAmbiente.dopplerLevel = 0f;

        if (grupoMixerAmbiente != null)
            audioSourceAmbiente.outputAudioMixerGroup = grupoMixerAmbiente;
    }

    public void Reproducir()
    {
        ConfigurarAudioSource();

        if (audioSourceAmbiente == null || sonidoAmbiente == null)
            return;

        if (rutinaFade != null)
            StopCoroutine(rutinaFade);

        audioSourceAmbiente.Stop();
        audioSourceAmbiente.clip = sonidoAmbiente;
        audioSourceAmbiente.loop = true;

        if (empezarEnPuntoAleatorio && sonidoAmbiente.length > 0.05f)
            audioSourceAmbiente.time = Random.Range(0f, sonidoAmbiente.length - 0.01f);
        else
            audioSourceAmbiente.time = 0f;

        if (duracionFadeEntrada <= 0f)
        {
            audioSourceAmbiente.volume = volumenObjetivo;
            audioSourceAmbiente.Play();
            return;
        }

        audioSourceAmbiente.volume = 0f;
        audioSourceAmbiente.Play();
        rutinaFade = StartCoroutine(FadeEntrada());
    }

    public void Detener()
    {
        if (rutinaFade != null)
        {
            StopCoroutine(rutinaFade);
            rutinaFade = null;
        }

        if (audioSourceAmbiente != null)
            audioSourceAmbiente.Stop();
    }

    private IEnumerator FadeEntrada()
    {
        float tiempo = 0f;

        while (tiempo < duracionFadeEntrada)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracionFadeEntrada);

            if (audioSourceAmbiente != null)
                audioSourceAmbiente.volume = Mathf.Lerp(0f, volumenObjetivo, t);

            yield return null;
        }

        if (audioSourceAmbiente != null)
            audioSourceAmbiente.volume = volumenObjetivo;

        rutinaFade = null;
    }
}
