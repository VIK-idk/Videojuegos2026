using UnityEngine;
using UnityEngine.Audio;

public class GuppySonidos : MonoBehaviour
{
    [Header("Audio - Voz de Guppy 2D")]
    [SerializeField] private AudioSource audioSourceVoz;
    [SerializeField] private AudioMixerGroup grupoMixerPlayer;

    [Header("Encargo fallado")]
    [SerializeField] private AudioClip sonidoGuppyTriste;
    [SerializeField, Range(0f, 1f)] private float volumenGuppyTriste = 0.85f;
    [SerializeField] private bool reiniciarSiYaEstaSonando = false;

    private void Awake()
    {
        ConfigurarAudioSource();
    }

    private void ConfigurarAudioSource()
    {
        if (audioSourceVoz == null)
            audioSourceVoz = gameObject.AddComponent<AudioSource>();

        audioSourceVoz.playOnAwake = false;
        audioSourceVoz.loop = false;
        audioSourceVoz.spatialBlend = 0f;
        audioSourceVoz.dopplerLevel = 0f;

        if (grupoMixerPlayer != null)
            audioSourceVoz.outputAudioMixerGroup = grupoMixerPlayer;
    }

    public void ReproducirTristePorEncargoFallado()
    {
        ConfigurarAudioSource();

        if (audioSourceVoz == null || sonidoGuppyTriste == null)
            return;

        if (audioSourceVoz.isPlaying)
        {
            if (!reiniciarSiYaEstaSonando)
                return;

            audioSourceVoz.Stop();
        }

        audioSourceVoz.PlayOneShot(sonidoGuppyTriste, volumenGuppyTriste);
    }

    public void DetenerVoz()
    {
        if (audioSourceVoz != null)
            audioSourceVoz.Stop();
    }
}
