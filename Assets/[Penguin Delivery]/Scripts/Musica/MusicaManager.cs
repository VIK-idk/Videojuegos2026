using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicaManager : MonoBehaviour
{
    public static MusicaManager Instancia { get; private set; }

    [System.Serializable]
    public class MusicaPorEscena
    {
        [Header("Escena")]
        public string nombreEscena;

        [Header("Musica normal")]
        public AudioClip musicaNormal;

        [Header("Intro + loop")]
        public bool usarIntroYLoop;
        public AudioClip intro;
        public AudioClip loop;

        [Header("Volumen y fades")]
        [Range(0f, 1f)] public float volumen = 1f;
        public float fadeEntrada = 1f;
        public float fadeSalida = 1f;
    }

    [Header("Audio Sources 2D")]
    [SerializeField] private AudioSource audioSourceA;
    [SerializeField] private AudioSource audioSourceB;

    [Tooltip("Necesario para hacer crossfade limpio cuando una musica usa Intro + Loop y ocupa las dos primeras fuentes.")]
    [SerializeField] private AudioSource audioSourceC;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup grupoMixerMusica;

    [Header("Musicas por escena")]
    [SerializeField] private List<MusicaPorEscena> musicasPorEscena = new List<MusicaPorEscena>();

    [Header("Configuracion general")]
    [SerializeField] private bool reproducirMusicaDeLaEscenaActualAlIniciar = true;
    [SerializeField] private float fadeDefecto = 1f;
    [SerializeField] private float volumenDefecto = 1f;

    private AudioSource fuenteActual;
    private Coroutine rutinaCambioMusica;
    private Coroutine rutinaIntroLoop;
    private string escenaMusicaActual = "";

    // Fuentes que pertenecen a la musica actual.
    // Importante: aqui tambien guardamos fuentes programadas con PlayScheduled aunque todavia no esten sonando.
    private readonly List<AudioSource> fuentesMusicaActual = new List<AudioSource>();

    private const string ESCENA_PANTALLA_CARGA = "PantallaCarga";
    private const double RETRASO_DSP = 0.08d;
    private const double MARGEN_PROGRAMACION_DSP = 0.12d;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;

        // Si lo has creado dentro de la camara, lo separamos para que DontDestroyOnLoad funcione bien.
        if (transform.parent != null)
            transform.SetParent(null);

        DontDestroyOnLoad(gameObject);

        ConfigurarAudioSources();
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void Start()
    {
        if (!reproducirMusicaDeLaEscenaActualAlIniciar)
            return;

        string escenaActual = SceneManager.GetActiveScene().name;

        if (escenaActual != ESCENA_PANTALLA_CARGA)
            ReproducirMusicaDeEscena(escenaActual, false);
    }

    private void OnDestroy()
    {
        if (Instancia == this)
            SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name == ESCENA_PANTALLA_CARGA)
            return;

        // Esto sirve por si entras a una escena directamente desde el editor.
        // Si SceneLoader ya habia empezado la musica antes de la pantalla de carga, no se reinicia.
        ReproducirMusicaDeEscena(escena.name, true);
    }

    private void ConfigurarAudioSources()
    {
        AudioSource[] fuentes = GetComponents<AudioSource>();

        if (audioSourceA == null)
        {
            if (fuentes.Length > 0)
                audioSourceA = fuentes[0];
            else
                audioSourceA = gameObject.AddComponent<AudioSource>();
        }

        if (audioSourceB == null)
        {
            if (fuentes.Length > 1)
                audioSourceB = fuentes[1];
            else
                audioSourceB = gameObject.AddComponent<AudioSource>();
        }

        if (audioSourceC == null)
        {
            if (fuentes.Length > 2)
                audioSourceC = fuentes[2];
            else
                audioSourceC = gameObject.AddComponent<AudioSource>();
        }

        ConfigurarAudioSource(audioSourceA);
        ConfigurarAudioSource(audioSourceB);
        ConfigurarAudioSource(audioSourceC);
    }

    private void ConfigurarAudioSource(AudioSource fuente)
    {
        if (fuente == null)
            return;

        fuente.playOnAwake = false;
        fuente.loop = false;
        fuente.spatialBlend = 0f; // Musica 2D.
        fuente.volume = 0f;
        fuente.outputAudioMixerGroup = grupoMixerMusica;
    }

    // Llamalo antes de cargar la pantalla de carga para que empiece el crossfade ya.
    public void PrepararMusicaParaCambioDeEscena(string nombreEscenaDestino)
    {
        ReproducirMusicaDeEscena(nombreEscenaDestino, true);
    }

    public void ReproducirMusicaDeEscena(string nombreEscena, bool conFade)
    {
        if (string.IsNullOrEmpty(nombreEscena))
            return;

        MusicaPorEscena config = BuscarMusica(nombreEscena);

        if (config == null)
        {
            DetenerMusica(conFade);
            escenaMusicaActual = "";
            return;
        }

        if (escenaMusicaActual == nombreEscena && HayMusicaActualActiva())
            return;

        escenaMusicaActual = nombreEscena;

        if (rutinaCambioMusica != null)
        {
            StopCoroutine(rutinaCambioMusica);
            rutinaCambioMusica = null;
        }

        if (rutinaIntroLoop != null)
        {
            StopCoroutine(rutinaIntroLoop);
            rutinaIntroLoop = null;
        }

        rutinaCambioMusica = StartCoroutine(CambiarMusica(config, conFade));
    }

    public void DetenerMusica(bool conFade)
    {
        if (rutinaCambioMusica != null)
        {
            StopCoroutine(rutinaCambioMusica);
            rutinaCambioMusica = null;
        }

        if (rutinaIntroLoop != null)
        {
            StopCoroutine(rutinaIntroLoop);
            rutinaIntroLoop = null;
        }

        float fadeSalida = conFade ? fadeDefecto : 0f;
        rutinaCambioMusica = StartCoroutine(FadeOutYPararTodasLasFuentes(fadeSalida));
    }

    /// <summary>
    /// Baja suavemente toda la musica que pertenece a la escena actual y la detiene.
    /// Sirve para secuencias especiales, como la derrota, sin cargar otra escena todavía.
    /// </summary>
    public void DesvanecerMusicaActual(float duracion)
    {
        if (rutinaCambioMusica != null)
        {
            StopCoroutine(rutinaCambioMusica);
            rutinaCambioMusica = null;
        }

        if (rutinaIntroLoop != null)
        {
            StopCoroutine(rutinaIntroLoop);
            rutinaIntroLoop = null;
        }

        duracion = Mathf.Max(0f, duracion);
        rutinaCambioMusica = StartCoroutine(DesvanecerMusicaActualRutina(duracion));
    }

    private IEnumerator DesvanecerMusicaActualRutina(float duracion)
    {
        yield return StartCoroutine(FadeOutYPararTodasLasFuentes(duracion));

        fuenteActual = null;
        escenaMusicaActual = "";
        rutinaCambioMusica = null;
    }

    private IEnumerator CambiarMusica(MusicaPorEscena config, bool conFade)
    {
        if (config.usarIntroYLoop && config.intro != null && config.loop != null)
        {
            yield return StartCoroutine(CambiarAIntroYLoop(config, conFade));
        }
        else
        {
            AudioClip clip = config.usarIntroYLoop && config.loop != null ? config.loop : config.musicaNormal;
            yield return StartCoroutine(CambiarAClipNormal(config, clip, conFade));
        }

        rutinaCambioMusica = null;
    }

    private IEnumerator CambiarAClipNormal(MusicaPorEscena config, AudioClip clip, bool conFade)
    {
        if (clip == null)
        {
            yield return StartCoroutine(FadeOutYPararTodasLasFuentes(ObtenerFadeSalida(config, conFade)));
            yield break;
        }

        List<AudioSource> fuentesViejas = ObtenerFuentesActualesYSonando();
        AudioSource fuenteNueva = ObtenerFuenteDisponible(fuentesViejas);

        // Si no queda ninguna libre, bajamos la musica anterior y luego reutilizamos una fuente.
        // Con 3 AudioSources normalmente esto no pasara, salvo cambios muy raros entre dos canciones con intro+loop.
        if (fuenteNueva == null)
        {
            yield return StartCoroutine(FadeOutYPararFuentes(fuentesViejas, ObtenerFadeSalida(config, conFade)));
            fuentesViejas.Clear();
            fuenteNueva = audioSourceA;
        }

        float volumenObjetivo = ObtenerVolumen(config);
        float fadeEntrada = ObtenerFadeEntrada(config, conFade);
        float fadeSalida = ObtenerFadeSalida(config, conFade);

        PrepararFuente(fuenteNueva, clip, true, 0f);
        fuentesMusicaActual.Clear();
        fuentesMusicaActual.Add(fuenteNueva);
        fuenteNueva.Play();

        yield return StartCoroutine(FadeCruzado(fuentesViejas, fuenteNueva, volumenObjetivo, fadeSalida, fadeEntrada));

        PararFuentes(fuentesViejas);
        fuenteNueva.volume = volumenObjetivo;
        fuenteActual = fuenteNueva;
    }

    private IEnumerator CambiarAIntroYLoop(MusicaPorEscena config, bool conFade)
    {
        List<AudioSource> fuentesViejas = ObtenerFuentesActualesYSonando();

        AudioSource fuenteIntro = ObtenerFuenteDisponible(fuentesViejas);

        List<AudioSource> noUsarParaLoop = new List<AudioSource>(fuentesViejas);
        if (fuenteIntro != null)
            noUsarParaLoop.Add(fuenteIntro);

        AudioSource fuenteLoop = ObtenerFuenteDisponible(noUsarParaLoop);

        // Para hacer crossfade hacia una musica con intro+loop necesitamos dos fuentes libres.
        // Si no hay dos libres, primero apagamos la musica vieja suavemente y luego empezamos intro+loop.
        if (fuenteIntro == null || fuenteLoop == null)
        {
            yield return StartCoroutine(FadeOutYPararFuentes(fuentesViejas, ObtenerFadeSalida(config, conFade)));
            fuentesViejas.Clear();

            fuenteIntro = audioSourceA;
            fuenteLoop = audioSourceB;
        }

        float volumenObjetivo = ObtenerVolumen(config);
        float fadeEntrada = ObtenerFadeEntrada(config, conFade);
        float fadeSalida = ObtenerFadeSalida(config, conFade);

        double inicioIntroDsp = AudioSettings.dspTime + RETRASO_DSP;
        double inicioLoopDsp = inicioIntroDsp + config.intro.length;

        PrepararFuente(fuenteIntro, config.intro, false, 0f);
        PrepararFuente(fuenteLoop, config.loop, true, volumenObjetivo);

        fuentesMusicaActual.Clear();
        fuentesMusicaActual.Add(fuenteIntro);
        fuentesMusicaActual.Add(fuenteLoop);

        fuenteIntro.PlayScheduled(inicioIntroDsp);
        fuenteLoop.PlayScheduled(inicioLoopDsp);

        yield return StartCoroutine(FadeCruzado(fuentesViejas, fuenteIntro, volumenObjetivo, fadeSalida, fadeEntrada));

        PararFuentes(fuentesViejas);

        rutinaIntroLoop = StartCoroutine(EsperarFinIntroYDejarLoop(fuenteIntro, fuenteLoop, inicioLoopDsp));
        fuenteActual = fuenteLoop;
    }

    private IEnumerator EsperarFinIntroYDejarLoop(AudioSource fuenteIntro, AudioSource fuenteLoop, double inicioLoopDsp)
    {
        while (AudioSettings.dspTime < inicioLoopDsp + 0.05d)
            yield return null;

        if (fuenteIntro != null)
        {
            fuenteIntro.Stop();
            fuenteIntro.clip = null;
            fuenteIntro.volume = 0f;
        }

        fuentesMusicaActual.Remove(fuenteIntro);

        if (fuenteLoop != null && !fuentesMusicaActual.Contains(fuenteLoop))
            fuentesMusicaActual.Add(fuenteLoop);

        fuenteActual = fuenteLoop;
        rutinaIntroLoop = null;
    }

    private IEnumerator FadeCruzado(List<AudioSource> fuentesViejas, AudioSource fuenteNueva, float volumenNuevo, float fadeSalida, float fadeEntrada)
    {
        Dictionary<AudioSource, float> volumenesIniciales = new Dictionary<AudioSource, float>();

        for (int i = 0; i < fuentesViejas.Count; i++)
        {
            if (fuentesViejas[i] != null)
                volumenesIniciales[fuentesViejas[i]] = fuentesViejas[i].volume;
        }

        if (fadeEntrada <= 0f && fuenteNueva != null)
            fuenteNueva.volume = volumenNuevo;

        if (fadeSalida <= 0f)
        {
            for (int i = 0; i < fuentesViejas.Count; i++)
            {
                if (fuentesViejas[i] != null)
                    fuentesViejas[i].volume = 0f;
            }
        }

        float duracion = Mathf.Max(fadeSalida, fadeEntrada);
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;

            if (fadeEntrada > 0f && fuenteNueva != null)
            {
                float tEntrada = Mathf.Clamp01(tiempo / fadeEntrada);
                fuenteNueva.volume = Mathf.Lerp(0f, volumenNuevo, tEntrada);
            }

            if (fadeSalida > 0f)
            {
                for (int i = 0; i < fuentesViejas.Count; i++)
                {
                    AudioSource fuenteVieja = fuentesViejas[i];

                    if (fuenteVieja == null)
                        continue;

                    float volumenInicial = volumenesIniciales.ContainsKey(fuenteVieja) ? volumenesIniciales[fuenteVieja] : fuenteVieja.volume;
                    float tSalida = Mathf.Clamp01(tiempo / fadeSalida);
                    fuenteVieja.volume = Mathf.Lerp(volumenInicial, 0f, tSalida);
                }
            }

            yield return null;
        }

        if (fuenteNueva != null)
            fuenteNueva.volume = volumenNuevo;

        for (int i = 0; i < fuentesViejas.Count; i++)
        {
            if (fuentesViejas[i] != null)
                fuentesViejas[i].volume = 0f;
        }
    }

    private IEnumerator FadeOutYPararTodasLasFuentes(float fadeSalida)
    {
        List<AudioSource> fuentes = ObtenerFuentesActualesYSonando();
        yield return StartCoroutine(FadeOutYPararFuentes(fuentes, fadeSalida));
        fuentesMusicaActual.Clear();
    }

    private IEnumerator FadeOutYPararFuentes(List<AudioSource> fuentes, float fadeSalida)
    {
        LimpiarListaFuentes(fuentes);

        if (fadeSalida <= 0f)
        {
            PararFuentes(fuentes);
            yield break;
        }

        Dictionary<AudioSource, float> volumenesIniciales = new Dictionary<AudioSource, float>();

        for (int i = 0; i < fuentes.Count; i++)
        {
            if (fuentes[i] != null)
                volumenesIniciales[fuentes[i]] = fuentes[i].volume;
        }

        float tiempo = 0f;

        while (tiempo < fadeSalida)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / fadeSalida);

            for (int i = 0; i < fuentes.Count; i++)
            {
                AudioSource fuente = fuentes[i];

                if (fuente == null)
                    continue;

                float volumenInicial = volumenesIniciales.ContainsKey(fuente) ? volumenesIniciales[fuente] : fuente.volume;
                fuente.volume = Mathf.Lerp(volumenInicial, 0f, t);
            }

            yield return null;
        }

        PararFuentes(fuentes);
    }

    private void PrepararFuente(AudioSource fuente, AudioClip clip, bool loop, float volumen)
    {
        if (fuente == null)
            return;

        fuente.Stop();
        fuente.clip = clip;
        fuente.loop = loop;
        fuente.volume = volumen;
        fuente.spatialBlend = 0f;
        fuente.outputAudioMixerGroup = grupoMixerMusica;
        fuente.playOnAwake = false;
    }

    private void PararFuentes(List<AudioSource> fuentes)
    {
        for (int i = 0; i < fuentes.Count; i++)
        {
            PararFuente(fuentes[i]);
        }
    }

    private void PararFuente(AudioSource fuente)
    {
        if (fuente == null)
            return;

        fuente.Stop();
        fuente.clip = null;
        fuente.volume = 0f;
        fuente.loop = false;
    }

    private AudioSource ObtenerFuenteDisponible(List<AudioSource> fuentesNoUsar)
    {
        List<AudioSource> fuentes = ObtenerTodasLasFuentes();

        for (int i = 0; i < fuentes.Count; i++)
        {
            AudioSource fuente = fuentes[i];

            if (fuente == null)
                continue;

            if (fuentesNoUsar != null && fuentesNoUsar.Contains(fuente))
                continue;

            if (!fuente.isPlaying && fuente.clip == null)
                return fuente;
        }

        for (int i = 0; i < fuentes.Count; i++)
        {
            AudioSource fuente = fuentes[i];

            if (fuente == null)
                continue;

            if (fuentesNoUsar != null && fuentesNoUsar.Contains(fuente))
                continue;

            if (!fuente.isPlaying)
                return fuente;
        }

        return null;
    }

    private List<AudioSource> ObtenerFuentesActualesYSonando()
    {
        List<AudioSource> fuentes = new List<AudioSource>();

        for (int i = 0; i < fuentesMusicaActual.Count; i++)
        {
            if (fuentesMusicaActual[i] != null && !fuentes.Contains(fuentesMusicaActual[i]))
                fuentes.Add(fuentesMusicaActual[i]);
        }

        List<AudioSource> todas = ObtenerTodasLasFuentes();

        for (int i = 0; i < todas.Count; i++)
        {
            AudioSource fuente = todas[i];

            if (fuente != null && fuente.isPlaying && !fuentes.Contains(fuente))
                fuentes.Add(fuente);
        }

        LimpiarListaFuentes(fuentes);
        return fuentes;
    }

    private List<AudioSource> ObtenerTodasLasFuentes()
    {
        List<AudioSource> fuentes = new List<AudioSource>();

        if (audioSourceA != null)
            fuentes.Add(audioSourceA);

        if (audioSourceB != null && !fuentes.Contains(audioSourceB))
            fuentes.Add(audioSourceB);

        if (audioSourceC != null && !fuentes.Contains(audioSourceC))
            fuentes.Add(audioSourceC);

        return fuentes;
    }

    private void LimpiarListaFuentes(List<AudioSource> fuentes)
    {
        if (fuentes == null)
            return;

        for (int i = fuentes.Count - 1; i >= 0; i--)
        {
            if (fuentes[i] == null)
            {
                fuentes.RemoveAt(i);
                continue;
            }

            for (int j = 0; j < i; j++)
            {
                if (fuentes[j] == fuentes[i])
                {
                    fuentes.RemoveAt(i);
                    break;
                }
            }
        }
    }

    private bool HayMusicaActualActiva()
    {
        for (int i = 0; i < fuentesMusicaActual.Count; i++)
        {
            AudioSource fuente = fuentesMusicaActual[i];

            if (fuente != null && fuente.clip != null)
                return true;
        }

        return false;
    }

    private MusicaPorEscena BuscarMusica(string nombreEscena)
    {
        for (int i = 0; i < musicasPorEscena.Count; i++)
        {
            MusicaPorEscena config = musicasPorEscena[i];

            if (config == null)
                continue;

            if (config.nombreEscena == nombreEscena)
                return config;
        }

        return null;
    }

    private float ObtenerVolumen(MusicaPorEscena config)
    {
        if (config == null)
            return Mathf.Clamp01(volumenDefecto);

        return Mathf.Clamp01(config.volumen);
    }

    private float ObtenerFadeEntrada(MusicaPorEscena config, bool conFade)
    {
        if (!conFade)
            return 0f;

        if (config == null || config.fadeEntrada < 0f)
            return fadeDefecto;

        return config.fadeEntrada;
    }

    private float ObtenerFadeSalida(MusicaPorEscena config, bool conFade)
    {
        if (!conFade)
            return 0f;

        if (config == null || config.fadeSalida < 0f)
            return fadeDefecto;

        return config.fadeSalida;
    }
}
