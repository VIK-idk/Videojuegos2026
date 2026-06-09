using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class PezPickupTest : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Pez pez;
    [SerializeField] private Animator burbujaAnimator;
    [SerializeField] private Animator pezAnimator;

    [Header("Animaciones")]
    [SerializeField] private string triggerExplotarBurbuja = "Explotar";
    [SerializeField] private string triggerRecolectarPez = "Recolectar";
    [SerializeField] private float duracionAntesDeRecoger = 0.6f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem vfxRecogerPez;

    [Header("Audio - Recogida Burbuja 3D")]
    [SerializeField] private AudioSource audioSourceRecogida;
    [SerializeField] private AudioMixerGroup grupoMixerPlayer;
    [SerializeField] private AudioClip[] sonidosRecogidaBurbuja;
    [SerializeField, Range(0f, 1f)] private float volumenRecogidaBurbuja = 1f;
    [SerializeField] private bool evitarRepetirMismoSonido = true;

    [Header("Configuracion Audio 3D")]
    [SerializeField] private bool usarSonidoTemporalParaNoCortar = true;
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 18f;
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    private bool recogido = false;
    private SphereCollider triggerCollider;
    private float radioBase = 0f;

    private PecesTestManager manager;
    private int ultimoIndiceSonido = -1;

    private void Awake()
    {
        triggerCollider = GetComponent<SphereCollider>();

        if (triggerCollider != null)
        {
            radioBase = triggerCollider.radius;
        }

        if (pez == null)
        {
            pez = GetComponentInParent<Pez>();
        }

        Transform raiz = pez != null ? pez.transform : transform.root;

        if (burbujaAnimator == null)
        {
            Transform burbuja = BuscarHijoPorNombre(raiz, "Burbuja");

            if (burbuja != null)
            {
                burbujaAnimator = burbuja.GetComponent<Animator>();
            }
        }

        if (pezAnimator == null)
        {
            Transform pezVisual = BuscarHijoPorNombre(raiz, "Pez");

            if (pezVisual != null)
            {
                pezAnimator = pezVisual.GetComponent<Animator>();
            }
        }

        if (vfxRecogerPez == null)
        {
            Transform vfxEncontrado = BuscarHijoPorNombre(transform.root, "VFX_RecogerPez");

            if (vfxEncontrado != null)
            {
                vfxRecogerPez = vfxEncontrado.GetComponent<ParticleSystem>();
            }
        }

        ConfigurarAudioSourceRecogida();

        manager = FindFirstObjectByType<PecesTestManager>();
    }

    private void OnEnable()
    {
        recogido = false;

        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<SphereCollider>();
        }

        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;

            if (radioBase <= 0f)
            {
                radioBase = triggerCollider.radius;
            }
        }

        ReiniciarAnimator(burbujaAnimator, triggerExplotarBurbuja);
        ReiniciarAnimator(pezAnimator, triggerRecolectarPez);

        if (pez != null)
        {
            AuraPezVisual aura = pez.GetComponentInChildren<AuraPezVisual>(true);

            if (aura != null)
            {
                aura.ReiniciarAura();
            }
        }

        ConfigurarAudioSourceRecogida();
    }

    public void SetMultiplicadorRecogida(float multiplicador)
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<SphereCollider>();
        }

        if (triggerCollider == null)
            return;

        if (radioBase <= 0f)
        {
            radioBase = triggerCollider.radius;
        }

        triggerCollider.radius = radioBase * multiplicador;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (recogido)
            return;

        Player player = other.GetComponentInParent<Player>();

        if (player == null)
            return;

        StartCoroutine(SecuenciaRecogerPez());
    }

    private IEnumerator SecuenciaRecogerPez()
    {
        recogido = true;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        ReproducirSonidoRecogidaBurbuja();
        ReproducirVFXRecogerPez();

        if (pez != null)
        {
            AuraPezVisual aura = pez.GetComponentInChildren<AuraPezVisual>(true);

            if (aura != null)
            {
                aura.Desvanecer(duracionAntesDeRecoger);
            }
        }

        if (burbujaAnimator != null)
        {
            burbujaAnimator.ResetTrigger(triggerExplotarBurbuja);
            burbujaAnimator.SetTrigger(triggerExplotarBurbuja);
        }

        if (pezAnimator != null)
        {
            pezAnimator.ResetTrigger(triggerRecolectarPez);
            pezAnimator.SetTrigger(triggerRecolectarPez);
        }

        yield return new WaitForSeconds(duracionAntesDeRecoger);

        if (manager == null)
        {
            manager = FindFirstObjectByType<PecesTestManager>();
        }

        if (pez == null)
        {
            pez = GetComponentInParent<Pez>();
        }

        if (pez != null && manager != null)
        {
            manager.ProcesarRecogida(pez);
        }
    }

    private void ConfigurarAudioSourceRecogida()
    {
        if (audioSourceRecogida == null)
        {
            audioSourceRecogida = GetComponentInParent<AudioSource>();
        }

        if (audioSourceRecogida == null)
            return;

        audioSourceRecogida.playOnAwake = false;
        audioSourceRecogida.loop = false;
        audioSourceRecogida.spatialBlend = spatialBlend;
        audioSourceRecogida.minDistance = minDistance;
        audioSourceRecogida.maxDistance = maxDistance;
        audioSourceRecogida.rolloffMode = rolloffMode;
        audioSourceRecogida.dopplerLevel = 0f;

        if (grupoMixerPlayer != null)
        {
            audioSourceRecogida.outputAudioMixerGroup = grupoMixerPlayer;
        }
    }

    private void ReproducirSonidoRecogidaBurbuja()
    {
        if (sonidosRecogidaBurbuja == null || sonidosRecogidaBurbuja.Length == 0)
            return;

        AudioClip clip = ObtenerClipAleatorioRecogida();

        if (clip == null)
            return;

        if (usarSonidoTemporalParaNoCortar)
        {
            ReproducirClipTemporal3D(clip);
            return;
        }

        if (audioSourceRecogida == null)
            return;

        ConfigurarAudioSourceRecogida();
        audioSourceRecogida.PlayOneShot(clip, volumenRecogidaBurbuja);
    }

    private AudioClip ObtenerClipAleatorioRecogida()
    {
        if (sonidosRecogidaBurbuja == null || sonidosRecogidaBurbuja.Length == 0)
            return null;

        int indice = Random.Range(0, sonidosRecogidaBurbuja.Length);

        if (evitarRepetirMismoSonido && sonidosRecogidaBurbuja.Length > 1)
        {
            int intentos = 0;

            while (indice == ultimoIndiceSonido && intentos < 10)
            {
                indice = Random.Range(0, sonidosRecogidaBurbuja.Length);
                intentos++;
            }
        }

        ultimoIndiceSonido = indice;
        return sonidosRecogidaBurbuja[indice];
    }

    private void ReproducirClipTemporal3D(AudioClip clip)
    {
        GameObject sonidoTemporal = new GameObject("SFX_RecogidaBurbuja_3D");
        sonidoTemporal.transform.position = transform.position;

        AudioSource source = sonidoTemporal.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = volumenRecogidaBurbuja;
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = spatialBlend;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = rolloffMode;
        source.dopplerLevel = 0f;

        if (grupoMixerPlayer != null)
        {
            source.outputAudioMixerGroup = grupoMixerPlayer;
        }
        else if (audioSourceRecogida != null && audioSourceRecogida.outputAudioMixerGroup != null)
        {
            source.outputAudioMixerGroup = audioSourceRecogida.outputAudioMixerGroup;
        }

        source.Play();

        float duracion = clip.length + 0.15f;
        Destroy(sonidoTemporal, duracion);
    }

    private void ReiniciarAnimator(Animator animator, string trigger)
    {
        if (animator == null)
            return;

        animator.ResetTrigger(trigger);
        animator.Rebind();
        animator.Update(0f);
    }

    private Transform BuscarHijoPorNombre(Transform padre, string nombre)
    {
        if (padre == null)
            return null;

        if (padre.name == nombre)
            return padre;

        for (int i = 0; i < padre.childCount; i++)
        {
            Transform resultado = BuscarHijoPorNombre(padre.GetChild(i), nombre);

            if (resultado != null)
                return resultado;
        }

        return null;
    }

    private void ReproducirVFXRecogerPez()
    {
        if (vfxRecogerPez == null)
            return;

        vfxRecogerPez.gameObject.SetActive(true);
        vfxRecogerPez.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        vfxRecogerPez.Play(true);
    }
}