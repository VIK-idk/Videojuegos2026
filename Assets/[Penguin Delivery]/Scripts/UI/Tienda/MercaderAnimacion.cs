using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class MercaderAnimacion : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Duraciones")]
    [SerializeField] private float duracionDespedida = 0.45f;
    [SerializeField] private float duracionEnojado = 3f;

    [Header("Audio - Dialogos 2D")]
    [SerializeField] private AudioSource audioSourceDialogos;
    [SerializeField] private AudioMixerGroup grupoMixerDialogos;
    [SerializeField, Range(0f, 1f)] private float volumenDialogos = 1f;

    [Header("Clips Mercader")]
    [SerializeField] private AudioClip sonidoSaludoTienda;
    [SerializeField] private AudioClip sonidoCompraAlegre;
    [SerializeField] private AudioClip sonidoDespedida;
    [SerializeField] private AudioClip[] sonidosCompraSinDinero;

    private Coroutine rutinaEnojado;

    private const string TRIGGER_LLEGADA = "Llegada";
    private const string TRIGGER_ALEGRE = "Alegre";
    private const string TRIGGER_ENOJADO = "Enojado";
    private const string TRIGGER_DESPEDIDA = "Despedida";
    private const string BOOL_MANTENER_ENOJADO = "MantenerEnojado";

    private bool enojadoActivo = false;
    private float tiempoEnojadoRestante = 0f;
    private int ultimoIndiceEnojado = -1;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        PrepararAudioSourceDialogos(true);
    }

    private void OnEnable()
    {
        enojadoActivo = false;
        tiempoEnojadoRestante = 0f;

        if (animator != null)
        {
            animator.SetBool(BOOL_MANTENER_ENOJADO, false);
            animator.ResetTrigger(TRIGGER_LLEGADA);
            animator.ResetTrigger(TRIGGER_ALEGRE);
            animator.ResetTrigger(TRIGGER_ENOJADO);
            animator.ResetTrigger(TRIGGER_DESPEDIDA);
        }

        PrepararAudioSourceDialogos(true);
    }

    private void OnValidate()
    {
        PrepararAudioSourceDialogos(false);
    }

    public void ReproducirLlegada()
    {
        DetenerEnojado();

        if (animator != null)
            animator.SetTrigger(TRIGGER_LLEGADA);

        ReproducirSonidoSaludo();
    }

    public void PedirAlegre()
    {
        DetenerEnojado();

        if (animator != null)
            animator.SetTrigger(TRIGGER_ALEGRE);

        ReproducirSonidoAlegre();
    }

    public void PedirEnojado()
    {
        tiempoEnojadoRestante = duracionEnojado;

        if (animator != null)
            animator.SetBool(BOOL_MANTENER_ENOJADO, true);

        ReproducirSonidoEnojadoAleatorio();

        if (enojadoActivo)
        {
            // Si ya está enojado, NO reinicia la animación.
            // Solo vuelve a poner el contador en duracionEnojado segundos.
            return;
        }

        if (animator != null)
            animator.SetTrigger(TRIGGER_ENOJADO);

        rutinaEnojado = StartCoroutine(EnojadoTemporal());
    }

    private IEnumerator EnojadoTemporal()
    {
        enojadoActivo = true;

        while (tiempoEnojadoRestante > 0f)
        {
            tiempoEnojadoRestante -= Time.unscaledDeltaTime;
            yield return null;
        }

        enojadoActivo = false;
        rutinaEnojado = null;

        if (animator != null)
            animator.SetBool(BOOL_MANTENER_ENOJADO, false);
    }

    public IEnumerator ReproducirDespedida()
    {
        DetenerEnojado();

        if (animator != null)
            animator.SetTrigger(TRIGGER_DESPEDIDA);

        ReproducirSonidoDespedida();

        yield return new WaitForSecondsRealtime(duracionDespedida);
    }

    private void DetenerEnojado()
    {
        if (rutinaEnojado != null)
        {
            StopCoroutine(rutinaEnojado);
            rutinaEnojado = null;
        }

        enojadoActivo = false;
        tiempoEnojadoRestante = 0f;

        if (animator != null)
            animator.SetBool(BOOL_MANTENER_ENOJADO, false);
    }

    private void PrepararAudioSourceDialogos(bool crearSiFalta)
    {
        if (audioSourceDialogos == null)
            audioSourceDialogos = GetComponent<AudioSource>();

        if (audioSourceDialogos == null && crearSiFalta)
            audioSourceDialogos = gameObject.AddComponent<AudioSource>();

        if (audioSourceDialogos == null)
            return;

        audioSourceDialogos.playOnAwake = false;
        audioSourceDialogos.loop = false;

        // Importante para sonidos de UI/canvas: 0 = sonido 2D, no depende de distancia ni posición.
        audioSourceDialogos.spatialBlend = 0f;

        audioSourceDialogos.volume = volumenDialogos;

        if (grupoMixerDialogos != null)
            audioSourceDialogos.outputAudioMixerGroup = grupoMixerDialogos;
    }

    private void ReproducirSonidoSaludo()
    {
        ReproducirClipDialogo(sonidoSaludoTienda, true);
    }

    private void ReproducirSonidoAlegre()
    {
        ReproducirClipDialogo(sonidoCompraAlegre, true);
    }

    private void ReproducirSonidoDespedida()
    {
        ReproducirClipDialogo(sonidoDespedida, true);
    }

    private void ReproducirSonidoEnojadoAleatorio()
    {
        PrepararAudioSourceDialogos(true);

        // Para que no se amontonen las voces del mercader.
        // Si el sonido anterior no terminó, este click no reproduce otro audio.
        if (audioSourceDialogos != null && audioSourceDialogos.isPlaying)
            return;

        AudioClip clip = ObtenerClipEnojadoAleatorio();
        ReproducirClipDialogo(clip, false);
    }

    private AudioClip ObtenerClipEnojadoAleatorio()
    {
        if (sonidosCompraSinDinero == null || sonidosCompraSinDinero.Length == 0)
            return null;

        int clipsValidos = 0;

        for (int i = 0; i < sonidosCompraSinDinero.Length; i++)
        {
            if (sonidosCompraSinDinero[i] != null)
                clipsValidos++;
        }

        if (clipsValidos == 0)
            return null;

        int indiceElegido = -1;

        if (clipsValidos == 1)
        {
            for (int i = 0; i < sonidosCompraSinDinero.Length; i++)
            {
                if (sonidosCompraSinDinero[i] != null)
                {
                    indiceElegido = i;
                    break;
                }
            }
        }
        else
        {
            int intentos = 0;

            while (intentos < 20)
            {
                int indiceAleatorio = Random.Range(0, sonidosCompraSinDinero.Length);

                if (sonidosCompraSinDinero[indiceAleatorio] != null && indiceAleatorio != ultimoIndiceEnojado)
                {
                    indiceElegido = indiceAleatorio;
                    break;
                }

                intentos++;
            }

            if (indiceElegido == -1)
            {
                for (int i = 0; i < sonidosCompraSinDinero.Length; i++)
                {
                    if (sonidosCompraSinDinero[i] != null)
                    {
                        indiceElegido = i;
                        break;
                    }
                }
            }
        }

        ultimoIndiceEnojado = indiceElegido;
        return sonidosCompraSinDinero[indiceElegido];
    }

    private void ReproducirClipDialogo(AudioClip clip, bool cortarDialogoActual)
    {
        if (clip == null)
            return;

        PrepararAudioSourceDialogos(true);

        if (audioSourceDialogos == null)
            return;

        if (!cortarDialogoActual && audioSourceDialogos.isPlaying)
            return;

        if (cortarDialogoActual)
            audioSourceDialogos.Stop();

        audioSourceDialogos.clip = clip;
        audioSourceDialogos.volume = volumenDialogos;
        audioSourceDialogos.Play();
    }
}
