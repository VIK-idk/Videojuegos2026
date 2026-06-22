using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// ====================
// HABILIDADES MANAGER
// ====================
public class HabilidadesManager : MonoBehaviour
{
    [Header("Habilidades")]
    [SerializeField] private HabilidadX2Peces habilidad1;
    [SerializeField] private HabilidadIman habilidad2;
    [SerializeField] private HabilidadQuitarStrike habilidad3;

    [Header("UI Slots")]
    [SerializeField] private HabilidadSlotUI slotHabilidad1;
    [SerializeField] private HabilidadSlotUI slotHabilidad2;
    [SerializeField] private HabilidadSlotUI slotHabilidad3;

    [Header("Mensaje")]
    [SerializeField] private Text textoMensaje;
    [SerializeField] private RectTransform textoMensajeRect;
    [SerializeField] private float duracionMensaje = 2f;

    [Header("Animacion mensaje")]
    [SerializeField] private float duracionEntradaMensaje = 0.18f;
    [SerializeField] private float duracionSalidaMensaje = 0.18f;
    [SerializeField] private float escalaInicialMensaje = 0.65f;
    [SerializeField] private float escalaVisibleMensaje = 1.15f;
    [SerializeField] private float escalaFinalMensaje = 0.65f;

    [Header("VFX Habilidades")]
    [SerializeField] private Transform puntoVFXHabilidades;
    [SerializeField] private TipoHabilidadVFX tipoVFXHabilidad1;
    [SerializeField] private TipoHabilidadVFX tipoVFXHabilidad2;
    [SerializeField] private TipoHabilidadVFX tipoVFXHabilidad3;

    [Header("Audio - Habilidades 2D")]
    [Tooltip("Fuente para el sonido comun que suena al activar cualquier habilidad.")]
    [SerializeField] private AudioSource audioSourceActivacion;

    [Tooltip("Fuente para el sonido especial de cada habilidad. En X2 e iman se usa en bucle.")]
    [SerializeField] private AudioSource audioSourceEfectoHabilidad;

    [SerializeField] private AudioMixerGroup grupoMixerHabilidades;

    [Header("Audio - Activacion comun")]
    [SerializeField] private AudioClip sonidoActivarHabilidad;
    [SerializeField, Range(0f, 1f)] private float volumenActivarHabilidad = 1f;

    [Header("Audio - X2 peces")]
    [SerializeField] private AudioClip sonidoX2Activo;
    [SerializeField, Range(0f, 1f)] private float volumenX2Activo = 0.75f;

    [Header("Audio - Iman")]
    [SerializeField] private AudioClip sonidoImanActivo;
    [SerializeField, Range(0f, 1f)] private float volumenImanActivo = 0.75f;

    [Header("Audio - Quitar strike")]
    [SerializeField] private AudioClip sonidoQuitarStrike;
    [SerializeField, Range(0f, 1f)] private float volumenQuitarStrike = 1f;

    private GameObject vfxActivoHabilidad1;
    private GameObject vfxActivoHabilidad2;
    private GameObject vfxActivoHabilidad3;

    private Coroutine rutinaMensaje;
    private Vector3 escalaOriginalMensaje = Vector3.one;
    private HabilidadBase habilidadConSonidoEnBucle;

    private void Awake()
    {
        ConfigurarAudioHabilidades();

        if (textoMensajeRect == null && textoMensaje != null)
            textoMensajeRect = textoMensaje.GetComponent<RectTransform>();

        if (textoMensajeRect != null)
            escalaOriginalMensaje = textoMensajeRect.localScale;
    }

    private void Start()
    {
        OcultarMensajeInstantaneo();
        ActualizarUI();
    }

    private void Update()
    {
        if (habilidad1 != null)
            habilidad1.Tick(this);

        if (habilidad2 != null)
            habilidad2.Tick(this);

        if (habilidad3 != null)
            habilidad3.Tick(this);

        GestionarInput();
        ActualizarUI();
    }

    private void GestionarInput()
    {
        if (Input.GetButtonDown("Habilidad1") && habilidad1 != null)
        {
            habilidad1.IntentarActivar(this);
        }

        if (Input.GetButtonDown("Habilidad2") && habilidad2 != null)
        {
            habilidad2.IntentarActivar(this);
        }

        if (Input.GetButtonDown("Habilidad3") && habilidad3 != null)
        {
            habilidad3.IntentarActivar(this);
        }
    }

    public bool HayOtraHabilidadActiva(HabilidadBase habilidadActual)
    {
        if (habilidad1 != null && habilidad1 != habilidadActual && habilidad1.EstaActiva())
            return true;

        if (habilidad2 != null && habilidad2 != habilidadActual && habilidad2.EstaActiva())
            return true;

        if (habilidad3 != null && habilidad3 != habilidadActual && habilidad3.EstaActiva())
            return true;

        return false;
    }

    public int GetCantidadPecesPorRecogida()
    {
        if (habilidad1 != null)
            return habilidad1.GetCantidadPeces();

        return 1;
    }

    public void MostrarMensaje(string mensaje)
    {
        if (textoMensaje == null)
            return;

        if (rutinaMensaje != null)
            StopCoroutine(rutinaMensaje);

        rutinaMensaje = StartCoroutine(MostrarMensajeCoroutine(mensaje));
    }

    private IEnumerator MostrarMensajeCoroutine(string mensaje)
    {
        textoMensaje.text = mensaje;
        textoMensaje.enabled = true;

        Color colorBase = textoMensaje.color;
        colorBase.a = 1f;

        Color colorInicial = colorBase;
        colorInicial.a = 0f;
        textoMensaje.color = colorInicial;

        if (textoMensajeRect != null)
            textoMensajeRect.localScale = escalaOriginalMensaje * escalaInicialMensaje;

        yield return StartCoroutine(AnimarMensaje(
            0f,
            1f,
            escalaInicialMensaje,
            escalaVisibleMensaje,
            duracionEntradaMensaje,
            colorBase
        ));

        yield return new WaitForSeconds(duracionMensaje);

        yield return StartCoroutine(AnimarMensaje(
            1f,
            0f,
            escalaVisibleMensaje,
            escalaFinalMensaje,
            duracionSalidaMensaje,
            colorBase
        ));

        textoMensaje.enabled = false;

        if (textoMensajeRect != null)
            textoMensajeRect.localScale = escalaOriginalMensaje;

        rutinaMensaje = null;
    }

    private IEnumerator AnimarMensaje(
        float alphaInicial,
        float alphaFinal,
        float escalaInicial,
        float escalaFinal,
        float duracion,
        Color colorBase)
    {
        if (textoMensaje == null)
            yield break;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracion);
            t = Mathf.SmoothStep(0f, 1f, t);

            Color color = colorBase;
            color.a = Mathf.Lerp(alphaInicial, alphaFinal, t);
            textoMensaje.color = color;

            if (textoMensajeRect != null)
            {
                float escala = Mathf.Lerp(escalaInicial, escalaFinal, t);
                textoMensajeRect.localScale = escalaOriginalMensaje * escala;
            }

            yield return null;
        }

        Color colorFinal = colorBase;
        colorFinal.a = alphaFinal;
        textoMensaje.color = colorFinal;

        if (textoMensajeRect != null)
            textoMensajeRect.localScale = escalaOriginalMensaje * escalaFinal;
    }

    private void OcultarMensajeInstantaneo()
    {
        if (textoMensaje != null)
        {
            Color color = textoMensaje.color;
            color.a = 0f;
            textoMensaje.color = color;
            textoMensaje.enabled = false;
        }

        if (textoMensajeRect != null)
            textoMensajeRect.localScale = escalaOriginalMensaje;
    }

    private void ActualizarUI()
    {
        ActualizarSlot(slotHabilidad1, habilidad1);
        ActualizarSlot(slotHabilidad2, habilidad2);
        ActualizarSlot(slotHabilidad3, habilidad3);
    }

    private void ActualizarSlot(HabilidadSlotUI slot, HabilidadBase habilidad)
    {
        if (slot == null)
            return;

        if (habilidad == null)
        {
            slot.MostrarVacio();
            return;
        }

        if (!habilidad.EstaAdquirida())
        {
            slot.MostrarVacio();
        }
        else if (habilidad.EstaUsada())
        {
            // AHORA PASA LOS ICONOS DE TECLADO Y MANDO
            slot.MostrarUsada(
                habilidad.GetTitulo(),
                habilidad.GetIcono(),
                habilidad.GetIconoTeclado(),
                habilidad.GetIconoMando()
            );
        }
        else if (habilidad.EstaActiva())
        {
            // AHORA PASA LOS ICONOS DE TECLADO Y MANDO
            slot.MostrarActiva(
                habilidad.GetTitulo(),
                habilidad.GetTextoTecla(),
                habilidad.GetTiempoVisible(),
                habilidad.GetIcono(),
                habilidad.GetIconoTeclado(),
                habilidad.GetIconoMando()
            );
        }
        else if (habilidad.EstaEnCooldown())
        {
            // AHORA PASA LOS ICONOS DE TECLADO Y MANDO
            slot.MostrarCooldown(
                habilidad.GetTitulo(),
                habilidad.GetTextoTecla(),
                habilidad.GetTiempoVisible(),
                habilidad.GetIcono(),
                habilidad.GetIconoTeclado(),
                habilidad.GetIconoMando()
            );
        }
        else if (HayOtraHabilidadActiva(habilidad))
        {
            // AHORA PASA LOS ICONOS DE TECLADO Y MANDO
            slot.MostrarBloqueada(
                habilidad.GetTitulo(),
                habilidad.GetTextoTecla(),
                habilidad.GetIcono(),
                habilidad.GetIconoTeclado(),
                habilidad.GetIconoMando()
            );
        }
        else
        {
            // AHORA PASA LOS ICONOS DE TECLADO Y MANDO
            slot.MostrarDisponible(
                habilidad.GetTitulo(),
                habilidad.GetTextoTecla(),
                habilidad.GetIcono(),
                habilidad.GetIconoTeclado(),
                habilidad.GetIconoMando()
            );
        }
    }

    // ====================
    // AUDIO HABILIDADES
    // ====================

    public void NotificarHabilidadActivada(HabilidadBase habilidad)
    {
        ConfigurarAudioHabilidades();

        ReproducirOneShot(audioSourceActivacion, sonidoActivarHabilidad, volumenActivarHabilidad);

        if (habilidad == habilidad1)
        {
            IniciarSonidoBucleHabilidad(habilidad, sonidoX2Activo, volumenX2Activo);
        }
        else if (habilidad == habilidad2)
        {
            IniciarSonidoBucleHabilidad(habilidad, sonidoImanActivo, volumenImanActivo);
        }
        else if (habilidad == habilidad3)
        {
            DetenerSonidoBucleHabilidad();
            ReproducirOneShot(audioSourceEfectoHabilidad, sonidoQuitarStrike, volumenQuitarStrike);
        }
    }

    public void NotificarHabilidadTerminada(HabilidadBase habilidad)
    {
        if (habilidadConSonidoEnBucle == habilidad)
            DetenerSonidoBucleHabilidad();
    }

    private void ConfigurarAudioHabilidades()
    {
        if (audioSourceActivacion == null)
            audioSourceActivacion = gameObject.AddComponent<AudioSource>();

        if (audioSourceEfectoHabilidad == null)
            audioSourceEfectoHabilidad = gameObject.AddComponent<AudioSource>();

        ConfigurarAudioSource2D(audioSourceActivacion, false);
        ConfigurarAudioSource2D(audioSourceEfectoHabilidad, false);
    }

    private void ConfigurarAudioSource2D(AudioSource source, bool loop)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;

        if (grupoMixerHabilidades != null)
            source.outputAudioMixerGroup = grupoMixerHabilidades;
    }

    private void ReproducirOneShot(AudioSource source, AudioClip clip, float volumen)
    {
        if (source == null || clip == null)
            return;

        source.PlayOneShot(clip, volumen);
    }

    private void IniciarSonidoBucleHabilidad(HabilidadBase habilidad, AudioClip clip, float volumen)
    {
        DetenerSonidoBucleHabilidad();

        if (audioSourceEfectoHabilidad == null || clip == null)
            return;

        habilidadConSonidoEnBucle = habilidad;
        audioSourceEfectoHabilidad.clip = clip;
        audioSourceEfectoHabilidad.volume = volumen;
        audioSourceEfectoHabilidad.loop = true;
        audioSourceEfectoHabilidad.Play();
    }

    private void DetenerSonidoBucleHabilidad()
    {
        habilidadConSonidoEnBucle = null;

        if (audioSourceEfectoHabilidad == null)
            return;

        audioSourceEfectoHabilidad.Stop();
        audioSourceEfectoHabilidad.clip = null;
        audioSourceEfectoHabilidad.loop = false;
    }

    private void OnDisable()
    {
        DetenerSonidoBucleHabilidad();

        if (audioSourceActivacion != null)
            audioSourceActivacion.Stop();
    }

    // ====================
    // VFX HABILIDADES
    // ====================

    public void ReproducirVFXHabilidad(HabilidadBase habilidad, bool esPulso)
    {
        TipoHabilidadVFX tipoVFX = ObtenerTipoVFX(habilidad);

        if (tipoVFX == null || tipoVFX.efectoVisual == null)
            return;

        GameObject nuevoVFX = CrearVFX(tipoVFX);

        if (nuevoVFX == null)
            return;

        if (esPulso)
        {
            StartCoroutine(DestruirVFXDespues(nuevoVFX, tipoVFX.duracionPulso));
        }
        else
        {
            GuardarVFXActivo(habilidad, nuevoVFX);
        }
    }

    public void DetenerVFXHabilidad(HabilidadBase habilidad)
    {
        GameObject vfx = ObtenerVFXActivo(habilidad);

        if (vfx != null)
        {
            Destroy(vfx);
        }

        GuardarVFXActivo(habilidad, null);
    }

    private TipoHabilidadVFX ObtenerTipoVFX(HabilidadBase habilidad)
    {
        if (habilidad == habilidad1)
            return tipoVFXHabilidad1;

        if (habilidad == habilidad2)
            return tipoVFXHabilidad2;

        if (habilidad == habilidad3)
            return tipoVFXHabilidad3;

        return null;
    }

    private GameObject CrearVFX(TipoHabilidadVFX tipoVFX)
    {
        Transform punto = puntoVFXHabilidades != null ? puntoVFXHabilidades : transform;

        GameObject nuevoVFX = Instantiate(
            tipoVFX.efectoVisual,
            punto.position,
            punto.rotation,
            punto
        );

        nuevoVFX.transform.localPosition = Vector3.zero;
        nuevoVFX.transform.localRotation = Quaternion.identity;

        ParticleSystem[] particulas = nuevoVFX.GetComponentsInChildren<ParticleSystem>();

        for (int i = 0; i < particulas.Length; i++)
        {
            particulas[i].Play();
        }

        return nuevoVFX;
    }

    private IEnumerator DestruirVFXDespues(GameObject vfx, float duracion)
    {
        if (duracion <= 0f)
            duracion = 0.6f;

        yield return new WaitForSeconds(duracion);

        if (vfx != null)
        {
            Destroy(vfx);
        }
    }

    private GameObject ObtenerVFXActivo(HabilidadBase habilidad)
    {
        if (habilidad == habilidad1)
            return vfxActivoHabilidad1;

        if (habilidad == habilidad2)
            return vfxActivoHabilidad2;

        if (habilidad == habilidad3)
            return vfxActivoHabilidad3;

        return null;
    }

    private void GuardarVFXActivo(HabilidadBase habilidad, GameObject vfx)
    {
        if (habilidad == habilidad1)
        {
            vfxActivoHabilidad1 = vfx;
            return;
        }

        if (habilidad == habilidad2)
        {
            vfxActivoHabilidad2 = vfx;
            return;
        }

        if (habilidad == habilidad3)
        {
            vfxActivoHabilidad3 = vfx;
            return;
        }
    }
}