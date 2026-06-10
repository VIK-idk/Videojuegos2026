using UnityEngine;
using UnityEngine.Audio;

public class MorsaAnimacion : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private const string PARAM_REBOTAR = "Rebotar";

    [Header("Audio - Morsa / Trampolin 3D")]
    [SerializeField] private AudioSource audioSourceMorsa;
    [SerializeField] private AudioMixerGroup grupoMixerMorsa;

    [Header("Clips")]
    [SerializeField] private AudioClip[] sonidosQuejidoMorsa;
    [SerializeField] private AudioClip[] sonidosBoingTrampolin;

    [Header("Volumenes")]
    [SerializeField, Range(0f, 3f)] private float volumenQuejido = 1f;
    [SerializeField, Range(0f, 3f)] private float volumenBoing = 1f;

    [Header("Boost extra")]
    [Tooltip("Multiplicador extra solo para el quejido de la morsa. Sirve para subirlo sin tocar el boing.")]
    [SerializeField, Range(0f, 5f)] private float multiplicadorExtraQuejido = 2.2f;

    [Tooltip("Multiplicador extra solo para el boing. Normalmente dejarlo en 1.")]
    [SerializeField, Range(0f, 5f)] private float multiplicadorExtraBoing = 1f;

    [Header("Configuracion")]
    [SerializeField] private bool reproducirQuejido = true;
    [SerializeField] private bool reproducirBoing = true;
    [SerializeField] private bool evitarRepetirMismoQuejido = true;
    [SerializeField] private bool evitarRepetirMismoBoing = true;
    [SerializeField] private float tiempoMinimoEntreSonidos = 0.08f;
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
    [SerializeField] private float minDistance = 4f;
    [SerializeField] private float maxDistance = 28f;
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    private int ultimoQuejido = -1;
    private int ultimoBoing = -1;
    private float tiempoUltimoSonido = -999f;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        ConfigurarAudioSource();
    }

    private void OnEnable()
    {
        ConfigurarAudioSource();
    }

    public void ReproducirRebote()
    {
        if (animator != null)
        {
            animator.ResetTrigger(PARAM_REBOTAR);
            animator.SetTrigger(PARAM_REBOTAR);
        }

        ReproducirSonidosRebote();
    }

    private void ConfigurarAudioSource()
    {
        if (audioSourceMorsa == null)
        {
            audioSourceMorsa = GetComponent<AudioSource>();
        }

        if (audioSourceMorsa == null)
        {
            audioSourceMorsa = gameObject.AddComponent<AudioSource>();
        }

        audioSourceMorsa.playOnAwake = false;
        audioSourceMorsa.loop = false;
        audioSourceMorsa.spatialBlend = spatialBlend;
        audioSourceMorsa.minDistance = minDistance;
        audioSourceMorsa.maxDistance = maxDistance;
        audioSourceMorsa.rolloffMode = rolloffMode;
        audioSourceMorsa.dopplerLevel = 0f;

        if (grupoMixerMorsa != null)
        {
            audioSourceMorsa.outputAudioMixerGroup = grupoMixerMorsa;
        }
    }

    private void ReproducirSonidosRebote()
    {
        if (Time.time - tiempoUltimoSonido < tiempoMinimoEntreSonidos)
            return;

        tiempoUltimoSonido = Time.time;
        ConfigurarAudioSource();

        if (audioSourceMorsa == null)
            return;

        if (reproducirBoing)
        {
            AudioClip boing = ObtenerClipAleatorio(sonidosBoingTrampolin, evitarRepetirMismoBoing, ref ultimoBoing);

            if (boing != null)
            {
                float volumenFinalBoing = volumenBoing * multiplicadorExtraBoing;
                audioSourceMorsa.PlayOneShot(boing, volumenFinalBoing);
            }
        }

        if (reproducirQuejido)
        {
            AudioClip quejido = ObtenerClipAleatorio(sonidosQuejidoMorsa, evitarRepetirMismoQuejido, ref ultimoQuejido);

            if (quejido != null)
            {
                float volumenFinalQuejido = volumenQuejido * multiplicadorExtraQuejido;
                audioSourceMorsa.PlayOneShot(quejido, volumenFinalQuejido);
            }
        }
    }

    private AudioClip ObtenerClipAleatorio(AudioClip[] clips, bool evitarRepetir, ref int ultimoIndice)
    {
        if (clips == null || clips.Length == 0)
            return null;

        if (clips.Length == 1)
        {
            ultimoIndice = 0;
            return clips[0];
        }

        int indice = Random.Range(0, clips.Length);

        if (evitarRepetir)
        {
            int intentos = 0;

            while (indice == ultimoIndice && intentos < 10)
            {
                indice = Random.Range(0, clips.Length);
                intentos++;
            }
        }

        ultimoIndice = indice;
        return clips[indice];
    }
}
