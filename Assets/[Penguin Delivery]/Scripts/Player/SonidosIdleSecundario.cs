using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Reproduce los sonidos del idle secundario (estado Fidget):
/// - sonido de rascarse la panza;
/// - apoyo del pie derecho;
/// - apoyo del pie izquierdo.
///
/// Los tiempos se cuentan desde que el Animator entra realmente en el estado Fidget.
/// Si la animación se interrumpe porque Guppy se mueve o salta, la secuencia se cancela.
/// </summary>
public class SonidosIdleSecundario : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform raizJugador;
    [SerializeField] private Transform puntoPieDerecho;
    [SerializeField] private Transform puntoPieIzquierdo;

    [Header("Estado del Animator")]
    [Tooltip("Nombre exacto del estado del idle secundario en el Animator.")]
    [SerializeField] private string nombreEstadoIdleSecundario = "Fidget";
    [SerializeField] private int capaAnimator = 0;

    [Header("Tiempo desde que empieza Fidget")]
    [Min(0f)]
    [SerializeField] private float retrasoSonidoRascarse = 1.20f;

    [Tooltip("Momento en el que el pie derecho vuelve a tocar el suelo.")]
    [Min(0f)]
    [SerializeField] private float retrasoApoyoPieDerecho = 0.35f;

    [Tooltip("Momento en el que el pie izquierdo vuelve a tocar el suelo.")]
    [Min(0f)]
    [SerializeField] private float retrasoApoyoPieIzquierdo = 0.70f;

    [Header("Sonido de rascarse")]
    [SerializeField] private AudioSource audioSourceRascarse;
    [SerializeField] private AudioMixerGroup grupoMixerPlayer;
    [SerializeField] private AudioClip sonidoRascarsePanza;
    [SerializeField, Range(0f, 1f)] private float volumenRascarse = 0.55f;

    [Header("Sonidos de apoyo de los pies")]
    [Tooltip("Usa SFX > Pasos.")]
    [SerializeField] private AudioMixerGroup grupoMixerPasos;
    [SerializeField] private AudioSource audioSourcePieDerecho;
    [SerializeField] private AudioSource audioSourcePieIzquierdo;
    [SerializeField, Range(0f, 1f)] private float volumenApoyoPies = 0.65f;
    [SerializeField, Range(0f, 0.5f)] private float variacionPitchPies = 0.04f;
    [SerializeField] private bool evitarRepetirMismoPaso = true;

    [Header("Detección del suelo actual")]
    [SerializeField] private LayerMask capasSuelo = ~0;
    [SerializeField] private float alturaOrigenDeteccion = 0.8f;
    [SerializeField] private float radioDeteccion = 0.25f;
    [SerializeField] private float distanciaDeteccion = 2f;

    [Header("Audio 2D / 3D")]
    [Tooltip("Para Guppy se recomienda 0, porque suele mantenerse a la misma distancia de la cámara.")]
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 0f;

    private bool idleSecundarioActivo;
    private bool rascarseReproducido;
    private bool pieDerechoReproducido;
    private bool pieIzquierdoReproducido;
    private float tiempoEnIdleSecundario;

    private int ultimoPasoDerecho = -1;
    private int ultimoPasoIzquierdo = -1;

    private void Awake()
    {
        BuscarReferencias();
        ConfigurarAudioSources();
    }

    private void OnEnable()
    {
        ReiniciarSecuencia();
    }

    private void OnDisable()
    {
        CancelarSecuencia();
    }

    private void Update()
    {
        if (animator == null)
            return;

        bool estaEnIdleSecundario = EstaEnEstadoIdleSecundario();

        if (estaEnIdleSecundario && !idleSecundarioActivo)
        {
            ComenzarSecuencia();
        }
        else if (!estaEnIdleSecundario && idleSecundarioActivo)
        {
            CancelarSecuencia();
        }

        if (!idleSecundarioActivo)
            return;

        tiempoEnIdleSecundario += Time.deltaTime;

        if (!pieDerechoReproducido && tiempoEnIdleSecundario >= retrasoApoyoPieDerecho)
        {
            pieDerechoReproducido = true;
            ReproducirApoyoPie(false);
        }

        if (!pieIzquierdoReproducido && tiempoEnIdleSecundario >= retrasoApoyoPieIzquierdo)
        {
            pieIzquierdoReproducido = true;
            ReproducirApoyoPie(true);
        }

        if (!rascarseReproducido && tiempoEnIdleSecundario >= retrasoSonidoRascarse)
        {
            rascarseReproducido = true;
            ReproducirRascarse();
        }
    }

    private void BuscarReferencias()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (animator == null)
                animator = GetComponentInParent<Animator>();
        }

        if (raizJugador == null)
        {
            Player player = GetComponentInParent<Player>();
            raizJugador = player != null ? player.transform : transform.root;
        }

        if (puntoPieDerecho == null)
            puntoPieDerecho = raizJugador;

        if (puntoPieIzquierdo == null)
            puntoPieIzquierdo = raizJugador;
    }

    private void ConfigurarAudioSources()
    {
        audioSourceRascarse = ConfigurarAudioSource(
            audioSourceRascarse,
            raizJugador,
            "Audio_Idle_Rascarse",
            grupoMixerPlayer
        );

        audioSourcePieDerecho = ConfigurarAudioSource(
            audioSourcePieDerecho,
            puntoPieDerecho,
            "Audio_Idle_Pie_Derecho",
            grupoMixerPasos
        );

        audioSourcePieIzquierdo = ConfigurarAudioSource(
            audioSourcePieIzquierdo,
            puntoPieIzquierdo,
            "Audio_Idle_Pie_Izquierdo",
            grupoMixerPasos
        );
    }

    private AudioSource ConfigurarAudioSource(
        AudioSource source,
        Transform padre,
        string nombre,
        AudioMixerGroup grupoMixer)
    {
        if (padre == null)
            padre = transform;

        if (source == null)
        {
            GameObject objetoAudio = new GameObject(nombre);
            objetoAudio.transform.SetParent(padre, false);
            objetoAudio.transform.localPosition = Vector3.zero;
            source = objetoAudio.AddComponent<AudioSource>();
        }

        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = spatialBlend;
        source.dopplerLevel = 0f;

        if (grupoMixer != null)
            source.outputAudioMixerGroup = grupoMixer;

        return source;
    }

    private bool EstaEnEstadoIdleSecundario()
    {
        if (animator.IsInTransition(capaAnimator))
        {
            AnimatorStateInfo siguiente = animator.GetNextAnimatorStateInfo(capaAnimator);

            if (EsEstadoIdleSecundario(siguiente))
                return true;
        }

        AnimatorStateInfo actual = animator.GetCurrentAnimatorStateInfo(capaAnimator);
        return EsEstadoIdleSecundario(actual);
    }

    private bool EsEstadoIdleSecundario(AnimatorStateInfo estado)
    {
        if (string.IsNullOrEmpty(nombreEstadoIdleSecundario))
            return false;

        return estado.IsName(nombreEstadoIdleSecundario) ||
               estado.IsName("Base Layer." + nombreEstadoIdleSecundario);
    }

    private void ComenzarSecuencia()
    {
        idleSecundarioActivo = true;
        tiempoEnIdleSecundario = 0f;
        rascarseReproducido = false;
        pieDerechoReproducido = false;
        pieIzquierdoReproducido = false;
    }

    private void CancelarSecuencia()
    {
        idleSecundarioActivo = false;
        tiempoEnIdleSecundario = 0f;

        if (audioSourceRascarse != null && audioSourceRascarse.isPlaying)
        {
            audioSourceRascarse.Stop();
            audioSourceRascarse.pitch = 1f;
        }
    }

    private void ReiniciarSecuencia()
    {
        idleSecundarioActivo = false;
        tiempoEnIdleSecundario = 0f;
        rascarseReproducido = false;
        pieDerechoReproducido = false;
        pieIzquierdoReproducido = false;
    }

    private void ReproducirRascarse()
    {
        if (audioSourceRascarse == null || sonidoRascarsePanza == null)
            return;

        audioSourceRascarse.pitch = 1f;
        audioSourceRascarse.PlayOneShot(sonidoRascarsePanza, volumenRascarse);
    }

    private void ReproducirApoyoPie(bool pieIzquierdo)
    {
        TipoSuelo tipoSuelo = DetectarTipoSueloActual();

        if (tipoSuelo == null)
            return;

        int ultimoIndice = pieIzquierdo ? ultimoPasoIzquierdo : ultimoPasoDerecho;
        int nuevoIndice;
        AudioClip clip = tipoSuelo.ObtenerSonidoPasoAleatorio(ultimoIndice, out nuevoIndice);

        if (clip == null)
            return;

        if (pieIzquierdo)
            ultimoPasoIzquierdo = nuevoIndice;
        else
            ultimoPasoDerecho = nuevoIndice;

        AudioSource source = pieIzquierdo ? audioSourcePieIzquierdo : audioSourcePieDerecho;

        if (source == null)
            return;

        source.pitch = Random.Range(
            1f - variacionPitchPies,
            1f + variacionPitchPies
        );

        source.PlayOneShot(clip, volumenApoyoPies);
    }

    private TipoSuelo DetectarTipoSueloActual()
    {
        Transform referencia = raizJugador != null ? raizJugador : transform;
        Vector3 origen = referencia.position + Vector3.up * alturaOrigenDeteccion;

        RaycastHit[] impactos = Physics.SphereCastAll(
            origen,
            radioDeteccion,
            Vector3.down,
            distanciaDeteccion,
            capasSuelo,
            QueryTriggerInteraction.Ignore
        );

        if (impactos == null || impactos.Length == 0)
            return null;

        System.Array.Sort(impactos, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < impactos.Length; i++)
        {
            Collider colliderDetectado = impactos[i].collider;

            if (colliderDetectado == null)
                continue;

            Player playerDetectado = colliderDetectado.GetComponentInParent<Player>();

            if (playerDetectado != null && raizJugador != null && playerDetectado.transform == raizJugador)
                continue;

            Suelo suelo = colliderDetectado.GetComponent<Suelo>();

            if (suelo == null)
                suelo = colliderDetectado.GetComponentInParent<Suelo>();

            if (suelo != null && suelo.tipo != null)
                return suelo.tipo;
        }

        return null;
    }
}
