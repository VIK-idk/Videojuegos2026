using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class ReyMorsaAnimacion : MonoBehaviour
{
    private enum TipoVozRey
    {
        Ninguna,
        NarracionEncargo,
        Alegre,
        Enojado
    }

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Audio Sources 3D")]
    [SerializeField] private AudioSource audioSourceVoz;
    [SerializeField] private AudioSource audioSourceAplausos;
    [SerializeField] private AudioMixerGroup grupoMixerReyMorsa;

    [Header("Configuracion 3D")]
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
    [SerializeField] private float distanciaMinima = 4f;
    [SerializeField] private float distanciaMaxima = 30f;
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    [Header("Voz: narracion y alegria")]
    [Tooltip("Los 4 sonidos de voz/narracion del Rey Morsa. Se usa uno al aparecer un encargo y otro al ponerse alegre.")]
    [SerializeField] private AudioClip[] sonidosNarracion;
    [SerializeField, Range(0f, 1.5f)] private float volumenNarracion = 1f;
    [SerializeField, Range(0f, 1.5f)] private float volumenVozAlegre = 1f;

    [Header("Voz: enojado")]
    [Tooltip("Los 2 sonidos de voz enojada del Rey Morsa.")]
    [SerializeField] private AudioClip[] sonidosEnojado;
    [SerializeField, Range(0f, 1.5f)] private float volumenEnojado = 1f;

    [Header("Aplausos")]
    [Tooltip("Los 3 sonidos de aplauso. En cada golpe se elige uno al azar.")]
    [SerializeField] private AudioClip[] sonidosAplausos;
    [SerializeField, Min(1)] private int cantidadAplausos = 3;
    [SerializeField, Min(0f)] private float retrasoPrimerAplauso = 0.2f;
    [SerializeField, Min(0f)] private float intervaloEntreAplausos = 0.35f;
    [SerializeField, Range(0f, 1.5f)] private float volumenAplausos = 1f;

    [Header("Aleatoriedad")]
    [SerializeField] private bool evitarRepetirMismoSonido = true;

    private static readonly int TriggerAplaudir = Animator.StringToHash("Aplaudir");
    private static readonly int TriggerEnojar = Animator.StringToHash("Enojar");

    private int ultimoIndiceNarracion = -1;
    private int ultimoIndiceEnojado = -1;
    private int ultimoIndiceAplauso = -1;

    private TipoVozRey tipoVozActual = TipoVozRey.Ninguna;
    private Coroutine rutinaAplausos;
    private bool audioReyBloqueado = false;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        PrepararAudioSources();
    }

    private void OnDisable()
    {
        DetenerSecuenciaAplausos();
    }

    /// <summary>
    /// Se llama exactamente cuando aparece un nuevo encargo.
    /// Reproduce una de las voces de narracion al azar.
    /// </summary>
    public void NarrarEncargo()
    {
        if (audioReyBloqueado)
            return;

        DetenerSecuenciaAplausos();

        ReproducirVozAleatoria(
            sonidosNarracion,
            ref ultimoIndiceNarracion,
            volumenNarracion,
            TipoVozRey.NarracionEncargo,
            false);
    }

    /// <summary>
    /// Lanza la animacion alegre, una voz aleatoria y tres aplausos temporizados.
    /// </summary>
    public void Aplaudir()
    {
        if (audioReyBloqueado)
            return;

        if (animator != null)
        {
            animator.ResetTrigger(TriggerEnojar);
            animator.ResetTrigger(TriggerAplaudir);
            animator.SetTrigger(TriggerAplaudir);
        }

        ReproducirVozAleatoria(
            sonidosNarracion,
            ref ultimoIndiceNarracion,
            volumenVozAlegre,
            TipoVozRey.Alegre,
            false);

        IniciarSecuenciaAplausos();
    }

    /// <summary>
    /// Lanza la animacion enfadada y una de las dos voces enojadas al azar.
    /// Si el tutorial o el game over llaman varias veces mientras la misma voz
    /// sigue sonando, no la reinicia ni la solapa.
    /// </summary>
    public void Enojar()
    {
        if (audioReyBloqueado)
            return;

        if (animator != null)
        {
            animator.ResetTrigger(TriggerAplaudir);
            animator.ResetTrigger(TriggerEnojar);
            animator.SetTrigger(TriggerEnojar);
        }

        DetenerSecuenciaAplausos();

        ReproducirVozAleatoria(
            sonidosEnojado,
            ref ultimoIndiceEnojado,
            volumenEnojado,
            TipoVozRey.Enojado,
            true);
    }


    /// <summary>
    /// Detiene la voz y los aplausos del Rey Morsa y bloquea nuevos sonidos.
    /// Se usa cuando la pantalla de derrota ya está completamente visible.
    /// </summary>
    public void DetenerAudioPorDerrota()
    {
        audioReyBloqueado = true;

        DetenerSecuenciaAplausos();

        if (audioSourceVoz != null)
        {
            audioSourceVoz.Stop();
        }

        tipoVozActual = TipoVozRey.Ninguna;
    }

    /// <summary>
    /// Permite volver a activar el audio si se reutiliza el mismo Rey Morsa.
    /// </summary>
    public void ReactivarAudioRey()
    {
        audioReyBloqueado = false;
    }

    private void PrepararAudioSources()
    {
        if (audioSourceVoz == null)
        {
            audioSourceVoz = CrearAudioSourceHijo("Audio_Voz_ReyMorsa");
        }

        if (audioSourceAplausos == null)
        {
            audioSourceAplausos = CrearAudioSourceHijo("Audio_Aplausos_ReyMorsa");
        }

        ConfigurarAudioSource(audioSourceVoz);
        ConfigurarAudioSource(audioSourceAplausos);
    }

    private AudioSource CrearAudioSourceHijo(string nombreObjeto)
    {
        Transform hijoExistente = transform.Find(nombreObjeto);

        if (hijoExistente != null)
        {
            AudioSource sourceExistente = hijoExistente.GetComponent<AudioSource>();

            if (sourceExistente != null)
            {
                return sourceExistente;
            }
        }

        GameObject objetoAudio = new GameObject(nombreObjeto);
        objetoAudio.transform.SetParent(transform, false);

        return objetoAudio.AddComponent<AudioSource>();
    }

    private void ConfigurarAudioSource(AudioSource source)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = spatialBlend;
        source.minDistance = distanciaMinima;
        source.maxDistance = distanciaMaxima;
        source.rolloffMode = rolloffMode;
        source.dopplerLevel = 0f;

        if (grupoMixerReyMorsa != null)
        {
            source.outputAudioMixerGroup = grupoMixerReyMorsa;
        }
    }

    private void ReproducirVozAleatoria(
        AudioClip[] clips,
        ref int ultimoIndice,
        float volumen,
        TipoVozRey nuevoTipo,
        bool noReiniciarSiMismoTipoEstaSonando)
    {
        PrepararAudioSources();

        if (audioSourceVoz == null || clips == null || clips.Length == 0)
            return;

        if (noReiniciarSiMismoTipoEstaSonando &&
            tipoVozActual == nuevoTipo &&
            audioSourceVoz.isPlaying)
        {
            return;
        }

        AudioClip clip = ObtenerClipAleatorio(clips, ref ultimoIndice);

        if (clip == null)
            return;

        if (audioSourceVoz.isPlaying)
        {
            audioSourceVoz.Stop();
        }

        tipoVozActual = nuevoTipo;
        audioSourceVoz.PlayOneShot(clip, volumen);
    }

    private void IniciarSecuenciaAplausos()
    {
        DetenerSecuenciaAplausos();

        if (sonidosAplausos == null || sonidosAplausos.Length == 0 || cantidadAplausos <= 0)
            return;

        rutinaAplausos = StartCoroutine(SecuenciaAplausos());
    }

    private IEnumerator SecuenciaAplausos()
    {
        if (retrasoPrimerAplauso > 0f)
        {
            yield return new WaitForSeconds(retrasoPrimerAplauso);
        }

        for (int i = 0; i < cantidadAplausos; i++)
        {
            ReproducirUnAplauso();

            if (i < cantidadAplausos - 1 && intervaloEntreAplausos > 0f)
            {
                yield return new WaitForSeconds(intervaloEntreAplausos);
            }
        }

        rutinaAplausos = null;
    }

    private void ReproducirUnAplauso()
    {
        PrepararAudioSources();

        if (audioSourceAplausos == null || sonidosAplausos == null || sonidosAplausos.Length == 0)
            return;

        AudioClip clip = ObtenerClipAleatorio(sonidosAplausos, ref ultimoIndiceAplauso);

        if (clip != null)
        {
            audioSourceAplausos.PlayOneShot(clip, volumenAplausos);
        }
    }

    private void DetenerSecuenciaAplausos()
    {
        if (rutinaAplausos != null)
        {
            StopCoroutine(rutinaAplausos);
            rutinaAplausos = null;
        }

        if (audioSourceAplausos != null && audioSourceAplausos.isPlaying)
        {
            audioSourceAplausos.Stop();
        }
    }

    private AudioClip ObtenerClipAleatorio(AudioClip[] clips, ref int ultimoIndice)
    {
        if (clips == null || clips.Length == 0)
            return null;

        int indice = Random.Range(0, clips.Length);

        if (evitarRepetirMismoSonido && clips.Length > 1)
        {
            int intentos = 0;

            while (indice == ultimoIndice && intentos < 12)
            {
                indice = Random.Range(0, clips.Length);
                intentos++;
            }
        }

        ultimoIndice = indice;
        return clips[indice];
    }
}
