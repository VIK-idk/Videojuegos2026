using UnityEngine;
using UnityEngine.Audio;

public enum TipoBotonTienda
{
    Madera,
    Habilidad
}

/// <summary>
/// Centraliza todos los sonidos 2D propios de la tienda.
/// Colócalo en el objeto raíz Tienda, no en un botón individual.
/// </summary>
[DisallowMultipleComponent]
public class SonidosTiendaManager : MonoBehaviour
{
    [Header("AudioSource 2D")]
    [SerializeField] private AudioSource audioSourceTienda;
    [SerializeField] private AudioMixerGroup grupoMixerUI;

    [Header("Botones de madera")]
    [SerializeField] private AudioClip sonidoHoverMadera;
    [SerializeField] private AudioClip sonidoPulsarMadera;
    [SerializeField, Range(0f, 1f)] private float volumenHoverMadera = 0.8f;
    [SerializeField, Range(0f, 1f)] private float volumenPulsarMadera = 1f;

    [Header("Botones de habilidades")]
    [SerializeField] private AudioClip sonidoHoverHabilidad;
    [SerializeField] private AudioClip sonidoPulsarHabilidad;
    [SerializeField, Range(0f, 1f)] private float volumenHoverHabilidad = 0.8f;
    [SerializeField, Range(0f, 1f)] private float volumenPulsarHabilidad = 1f;

    [Header("Resultado de compra")]
    [SerializeField] private AudioClip sonidoCompraExitosa;
    [SerializeField] private AudioClip sonidoCompraSinDinero;
    [SerializeField, Range(0f, 1f)] private float volumenCompraExitosa = 1f;
    [SerializeField, Range(0f, 1f)] private float volumenCompraSinDinero = 1f;

    [Header("Protección hover")]
    [SerializeField, Min(0f)] private float intervaloMinimoHover = 0.05f;

    private float ultimoHover = -999f;

    private void Awake()
    {
        ConfigurarAudioSource(true);
    }

    private void OnValidate()
    {
        ConfigurarAudioSource(false);
    }

    private void Reset()
    {
        ConfigurarAudioSource(true);
    }

    private void ConfigurarAudioSource(bool crearSiFalta)
    {
        if (audioSourceTienda == null)
            audioSourceTienda = GetComponent<AudioSource>();

        if (audioSourceTienda == null && crearSiFalta)
            audioSourceTienda = gameObject.AddComponent<AudioSource>();

        if (audioSourceTienda == null)
            return;

        audioSourceTienda.playOnAwake = false;
        audioSourceTienda.loop = false;
        audioSourceTienda.spatialBlend = 0f;
        audioSourceTienda.dopplerLevel = 0f;
        audioSourceTienda.ignoreListenerPause = true;

        if (grupoMixerUI != null)
            audioSourceTienda.outputAudioMixerGroup = grupoMixerUI;
    }

    public void ReproducirHover(TipoBotonTienda tipoBoton)
    {
        if (Time.unscaledTime - ultimoHover < intervaloMinimoHover)
            return;

        ultimoHover = Time.unscaledTime;

        if (tipoBoton == TipoBotonTienda.Habilidad)
            Reproducir(sonidoHoverHabilidad, volumenHoverHabilidad);
        else
            Reproducir(sonidoHoverMadera, volumenHoverMadera);
    }

    public void ReproducirPulsar(TipoBotonTienda tipoBoton)
    {
        if (tipoBoton == TipoBotonTienda.Habilidad)
            Reproducir(sonidoPulsarHabilidad, volumenPulsarHabilidad);
        else
            Reproducir(sonidoPulsarMadera, volumenPulsarMadera);
    }

    public void ReproducirCompraExitosa()
    {
        Reproducir(sonidoCompraExitosa, volumenCompraExitosa);
    }

    public void ReproducirCompraSinDinero()
    {
        Reproducir(sonidoCompraSinDinero, volumenCompraSinDinero);
    }

    private void Reproducir(AudioClip clip, float volumen)
    {
        if (clip == null)
            return;

        ConfigurarAudioSource(true);

        if (audioSourceTienda == null)
            return;

        audioSourceTienda.PlayOneShot(clip, Mathf.Clamp01(volumen));
    }
}
