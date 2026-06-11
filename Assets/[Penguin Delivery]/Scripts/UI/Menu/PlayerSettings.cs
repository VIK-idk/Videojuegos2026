using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PlayerSettings : MonoBehaviour
{
    // ====================
    // REFERENCIAS UI
    // ====================
    [Header("Pantalla")]
    [SerializeField] private Toggle pantallaCompleta;
    [SerializeField] private Dropdown resolucionDrop;

    [Header("Audio")]
    [SerializeField] private Slider volumenMaster;
    [SerializeField] private Slider volumenMusica;
    [SerializeField] private Slider volumenSFX;
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sensibilidad")]
    [SerializeField] private Slider sensibilidadSlider;
    [SerializeField] private Text textoValorSensibilidad;

    [Header("UI Mando")]
    [SerializeField] private GameObject primerBotonOpciones;

    // ====================
    // SONIDOS DE OPCIONES
    // ====================
    [Header("Sonidos UI de opciones")]
    [SerializeField] private bool configurarSonidosAutomaticamente = true;
    [SerializeField] private bool reproducirHoverEnControles = true;

    [Header("Audio de prueba - Master")]
    [SerializeField] private AudioClip sonidoPruebaMaster;
    [SerializeField] private AudioMixerGroup grupoPruebaMaster;
    [SerializeField, Range(0f, 1f)] private float volumenPruebaMaster = 0.8f;

    [Header("Audio de prueba - Musica")]
    [SerializeField] private AudioClip sonidoPruebaMusica;
    [SerializeField] private AudioMixerGroup grupoPruebaMusica;
    [SerializeField, Range(0f, 1f)] private float volumenPruebaMusica = 0.8f;

    [Header("Audio de prueba - SFX")]
    [SerializeField] private AudioClip sonidoPruebaSFX;
    [SerializeField] private AudioMixerGroup grupoPruebaSFX;
    [SerializeField, Range(0f, 1f)] private float volumenPruebaSFX = 0.8f;

    [Header("Audio de prueba - Sensibilidad")]
    [SerializeField] private AudioClip sonidoPruebaSensibilidad;
    [SerializeField] private AudioMixerGroup grupoPruebaSensibilidad;
    [SerializeField, Range(0f, 1f)] private float volumenPruebaSensibilidad = 0.8f;

    [Header("Comportamiento pruebas sliders")]
    [Tooltip("Normalmente se deja desactivado para no oir dos veces el mismo sonido.")]
    [SerializeField] private bool reproducirPruebaAlPulsar = false;
    [SerializeField] private bool reproducirPruebaAlSoltar = true;
    [SerializeField] private bool reproducirPruebaConTecladoMando = true;
    [SerializeField, Min(0f)] private float retardoPruebaTecladoMando = 0.14f;

    // ====================
    // PARAMETROS AUDIOMIXER
    // Tienen que llamarse exactamente igual que en Exposed Parameters
    // ====================
    private const string PARAM_MASTER = "VolumenMaster";
    private const string PARAM_MUSICA = "VolumenMusica";
    private const string PARAM_SFX = "VolumenSFX";

    // ====================
    // PLAYER PREFS
    // ====================
    private const string PREF_PANTALLA_COMPLETA = "PantallaCompleta";
    private const string PREF_RESOLUCION_INDEX = "ResolucionIndex";
    private const string PREF_SENSIBILIDAD = "Sensibilidad";

    private const string PREF_VOLUMEN_MASTER = "VolumenMaster";
    private const string PREF_VOLUMEN_MUSICA = "VolumenMusica";
    private const string PREF_VOLUMEN_SFX = "VolumenSFX";

    private const string PREF_AUDIO_VERSION = "AudioSettingsVersion";
    private const int AUDIO_VERSION_ACTUAL = 2;

    // ====================
    // AUDIO
    // ====================
    private const float VOLUMEN_MINIMO_LINEAL = 0.0001f;
    private const float DECIBELIOS_MINIMOS = -80f;

    // ====================
    // RESOLUCIONES
    // ====================
    private readonly Vector2Int[] resoluciones = new Vector2Int[]
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080)
    };

    // ====================
    // UNITY
    // ====================
    private void Start()
    {
        ConfigurarDropdownResolucion();
        ConfigurarSliderSensibilidad();
        ConfigurarSlidersAudio();

        CargarYAplicarSettings();
        RegistrarEventosSliders();
        ConfigurarSonidosOpciones();
    }

    private void OnDestroy()
    {
        QuitarEventosSliders();
    }

    // ====================
    // CONFIGURACION INICIAL
    // ====================
    private void ConfigurarDropdownResolucion()
    {
        if (resolucionDrop == null)
            return;

        resolucionDrop.ClearOptions();

        List<string> opciones = new List<string>();

        for (int i = 0; i < resoluciones.Length; i++)
        {
            opciones.Add(resoluciones[i].x + "x" + resoluciones[i].y);
        }

        resolucionDrop.AddOptions(opciones);
    }

    private void ConfigurarSliderSensibilidad()
    {
        if (sensibilidadSlider == null)
            return;

        sensibilidadSlider.minValue = 200f;
        sensibilidadSlider.maxValue = 1000f;
        sensibilidadSlider.wholeNumbers = true;
    }

    private void ConfigurarSlidersAudio()
    {
        ConfigurarSliderAudio(volumenMaster);
        ConfigurarSliderAudio(volumenMusica);
        ConfigurarSliderAudio(volumenSFX);
    }

    private void ConfigurarSliderAudio(Slider slider)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void RegistrarEventosSliders()
    {
        if (volumenMaster != null)
            volumenMaster.onValueChanged.AddListener(SetMasterVolumePref);

        if (volumenMusica != null)
            volumenMusica.onValueChanged.AddListener(SetMusicaVolumePref);

        if (volumenSFX != null)
            volumenSFX.onValueChanged.AddListener(SetSFXVolumePref);

        if (pantallaCompleta != null)
            pantallaCompleta.onValueChanged.AddListener(SetPantallaCompletaPref);

        if (resolucionDrop != null)
            resolucionDrop.onValueChanged.AddListener(SetResolucionPref);

        if (sensibilidadSlider != null)
            sensibilidadSlider.onValueChanged.AddListener(SetSensibilidadPref);
    }

    private void QuitarEventosSliders()
    {
        if (volumenMaster != null)
            volumenMaster.onValueChanged.RemoveListener(SetMasterVolumePref);

        if (volumenMusica != null)
            volumenMusica.onValueChanged.RemoveListener(SetMusicaVolumePref);

        if (volumenSFX != null)
            volumenSFX.onValueChanged.RemoveListener(SetSFXVolumePref);

        if (pantallaCompleta != null)
            pantallaCompleta.onValueChanged.RemoveListener(SetPantallaCompletaPref);

        if (resolucionDrop != null)
            resolucionDrop.onValueChanged.RemoveListener(SetResolucionPref);

        if (sensibilidadSlider != null)
            sensibilidadSlider.onValueChanged.RemoveListener(SetSensibilidadPref);
    }

    // ====================
    // SONIDOS DE OPCIONES
    // ====================
    private void ConfigurarSonidosOpciones()
    {
        if (!configurarSonidosAutomaticamente)
            return;

        // Toggle y desplegable usan el mismo hover/selected que los botones del menú.
        ConfigurarHoverControl(pantallaCompleta != null ? pantallaCompleta.gameObject : null);
        ConfigurarHoverControl(resolucionDrop != null ? resolucionDrop.gameObject : null);

        // Cada slider recibe su propio clip y su propio grupo del AudioMixer.
        ConfigurarSonidoSlider(
            volumenMaster,
            sonidoPruebaMaster,
            grupoPruebaMaster,
            volumenPruebaMaster
        );

        ConfigurarSonidoSlider(
            volumenMusica,
            sonidoPruebaMusica,
            grupoPruebaMusica,
            volumenPruebaMusica
        );

        ConfigurarSonidoSlider(
            volumenSFX,
            sonidoPruebaSFX,
            grupoPruebaSFX,
            volumenPruebaSFX
        );

        ConfigurarSonidoSlider(
            sensibilidadSlider,
            sonidoPruebaSensibilidad,
            grupoPruebaSensibilidad,
            volumenPruebaSensibilidad
        );
    }

    private void ConfigurarHoverControl(GameObject objeto)
    {
        if (objeto == null)
            return;

        SonidoControlOpcionesUI sonidoControl =
            objeto.GetComponent<SonidoControlOpcionesUI>();

        if (sonidoControl == null)
            sonidoControl = objeto.AddComponent<SonidoControlOpcionesUI>();

        sonidoControl.Configurar(reproducirHoverEnControles);
    }

    private void ConfigurarSonidoSlider(
        Slider slider,
        AudioClip clip,
        AudioMixerGroup grupo,
        float volumen)
    {
        if (slider == null)
            return;

        SonidoSliderOpcionesUI sonidoSlider =
            slider.GetComponent<SonidoSliderOpcionesUI>();

        if (sonidoSlider == null)
            sonidoSlider = slider.gameObject.AddComponent<SonidoSliderOpcionesUI>();

        sonidoSlider.Configurar(
            clip,
            grupo,
            volumen,
            reproducirHoverEnControles,
            reproducirPruebaAlPulsar,
            reproducirPruebaAlSoltar,
            reproducirPruebaConTecladoMando,
            retardoPruebaTecladoMando
        );
    }

    // ====================
    // CARGAR SETTINGS
    // ====================
    private void CargarYAplicarSettings()
    {
        bool pantallaCompletaGuardada = PlayerPrefs.GetInt(
            PREF_PANTALLA_COMPLETA,
            Screen.fullScreen ? 1 : 0
        ) == 1;

        int resolucionIndexGuardado = PlayerPrefs.GetInt(
            PREF_RESOLUCION_INDEX,
            BuscarIndiceResolucionActual()
        );

        resolucionIndexGuardado = Mathf.Clamp(resolucionIndexGuardado, 0, resoluciones.Length - 1);

        float sensibilidadGuardada = PlayerPrefs.GetFloat(PREF_SENSIBILIDAD, 550f);

        bool necesitaMigrarAudio = PlayerPrefs.GetInt(PREF_AUDIO_VERSION, 1) < AUDIO_VERSION_ACTUAL;

        float volumenMasterGuardado = CargarVolumenLineal(PREF_VOLUMEN_MASTER, necesitaMigrarAudio);
        float volumenMusicaGuardado = CargarVolumenLineal(PREF_VOLUMEN_MUSICA, necesitaMigrarAudio);
        float volumenSFXGuardado = CargarVolumenLineal(PREF_VOLUMEN_SFX, necesitaMigrarAudio);

        AplicarResolucion(resolucionIndexGuardado, pantallaCompletaGuardada);

        AplicarVolumenAudioMixer(PARAM_MASTER, volumenMasterGuardado);
        AplicarVolumenAudioMixer(PARAM_MUSICA, volumenMusicaGuardado);
        AplicarVolumenAudioMixer(PARAM_SFX, volumenSFXGuardado);

        if (pantallaCompleta != null)
            pantallaCompleta.SetIsOnWithoutNotify(pantallaCompletaGuardada);

        if (resolucionDrop != null)
        {
            resolucionDrop.SetValueWithoutNotify(resolucionIndexGuardado);
            resolucionDrop.RefreshShownValue();
        }

        if (sensibilidadSlider != null)
            sensibilidadSlider.SetValueWithoutNotify(sensibilidadGuardada);

        if (volumenMaster != null)
            volumenMaster.SetValueWithoutNotify(volumenMasterGuardado);

        if (volumenMusica != null)
            volumenMusica.SetValueWithoutNotify(volumenMusicaGuardado);

        if (volumenSFX != null)
            volumenSFX.SetValueWithoutNotify(volumenSFXGuardado);

        ActualizarTextoSensibilidad(sensibilidadGuardada);

        if (necesitaMigrarAudio)
        {
            GuardarVolumenesAudio(
                volumenMasterGuardado,
                volumenMusicaGuardado,
                volumenSFXGuardado
            );
        }
    }

    private float CargarVolumenLineal(string prefKey, bool necesitaMigrarAudio)
    {
        if (necesitaMigrarAudio)
        {
            // Antes se guardaban decibelios entre -80 y 0.
            // 0 dB equivale a volumen 1.
            // -6 dB equivale aprox. a volumen 0.5.
            float valorAntiguoEnDecibelios = PlayerPrefs.GetFloat(prefKey, 0f);
            return DecibeliosALineal(valorAntiguoEnDecibelios);
        }

        float valorLineal = PlayerPrefs.GetFloat(prefKey, 1f);
        return Mathf.Clamp01(valorLineal);
    }

    private void GuardarVolumenesAudio(float master, float musica, float sfx)
    {
        PlayerPrefs.SetFloat(PREF_VOLUMEN_MASTER, Mathf.Clamp01(master));
        PlayerPrefs.SetFloat(PREF_VOLUMEN_MUSICA, Mathf.Clamp01(musica));
        PlayerPrefs.SetFloat(PREF_VOLUMEN_SFX, Mathf.Clamp01(sfx));
        PlayerPrefs.SetInt(PREF_AUDIO_VERSION, AUDIO_VERSION_ACTUAL);
        PlayerPrefs.Save();
    }

    // ====================
    // AUDIO
    // ====================
    public void SetMasterVolumePref(float valor)
    {
        valor = Mathf.Clamp01(valor);

        AplicarVolumenAudioMixer(PARAM_MASTER, valor);

        PlayerPrefs.SetFloat(PREF_VOLUMEN_MASTER, valor);
        PlayerPrefs.SetInt(PREF_AUDIO_VERSION, AUDIO_VERSION_ACTUAL);
        PlayerPrefs.Save();
    }

    public void SetMusicaVolumePref(float valor)
    {
        valor = Mathf.Clamp01(valor);

        AplicarVolumenAudioMixer(PARAM_MUSICA, valor);

        PlayerPrefs.SetFloat(PREF_VOLUMEN_MUSICA, valor);
        PlayerPrefs.SetInt(PREF_AUDIO_VERSION, AUDIO_VERSION_ACTUAL);
        PlayerPrefs.Save();
    }

    public void SetSFXVolumePref(float valor)
    {
        valor = Mathf.Clamp01(valor);

        AplicarVolumenAudioMixer(PARAM_SFX, valor);

        PlayerPrefs.SetFloat(PREF_VOLUMEN_SFX, valor);
        PlayerPrefs.SetInt(PREF_AUDIO_VERSION, AUDIO_VERSION_ACTUAL);
        PlayerPrefs.Save();
    }

    private void AplicarVolumenAudioMixer(string parametro, float valorLineal)
    {
        if (audioMixer == null)
            return;

        float decibelios = LinealADecibelios(valorLineal);
        audioMixer.SetFloat(parametro, decibelios);
    }

    private float LinealADecibelios(float valorLineal)
    {
        valorLineal = Mathf.Clamp01(valorLineal);

        if (valorLineal <= VOLUMEN_MINIMO_LINEAL)
            return DECIBELIOS_MINIMOS;

        return Mathf.Log10(valorLineal) * 20f;
    }

    private float DecibeliosALineal(float decibelios)
    {
        if (decibelios <= DECIBELIOS_MINIMOS)
            return 0f;

        return Mathf.Clamp01(Mathf.Pow(10f, decibelios / 20f));
    }

    // ====================
    // PANTALLA
    // ====================
    public void SetPantallaCompletaPref(bool valor)
    {
        int indiceActual = PlayerPrefs.GetInt(
            PREF_RESOLUCION_INDEX,
            BuscarIndiceResolucionActual()
        );

        indiceActual = Mathf.Clamp(indiceActual, 0, resoluciones.Length - 1);

        AplicarResolucion(indiceActual, valor);

        PlayerPrefs.SetInt(PREF_PANTALLA_COMPLETA, valor ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetResolucionPref(int index)
    {
        index = Mathf.Clamp(index, 0, resoluciones.Length - 1);

        bool fullscreenActual = PlayerPrefs.GetInt(
            PREF_PANTALLA_COMPLETA,
            Screen.fullScreen ? 1 : 0
        ) == 1;

        AplicarResolucion(index, fullscreenActual);

        PlayerPrefs.SetInt(PREF_RESOLUCION_INDEX, index);
        PlayerPrefs.Save();
    }

    private int BuscarIndiceResolucionActual()
    {
        for (int i = 0; i < resoluciones.Length; i++)
        {
            if (resoluciones[i].x == Screen.width && resoluciones[i].y == Screen.height)
                return i;
        }

        return 0;
    }

    private void AplicarResolucion(int index, bool fullscreen)
    {
        if (index < 0 || index >= resoluciones.Length)
            return;

        Screen.SetResolution(
            resoluciones[index].x,
            resoluciones[index].y,
            fullscreen
        );
    }

    // ====================
    // SENSIBILIDAD
    // ====================
    public void SetSensibilidadPref(float valor)
    {
        PlayerPrefs.SetFloat(PREF_SENSIBILIDAD, valor);
        PlayerPrefs.Save();

        ActualizarTextoSensibilidad(valor);
    }

    private void ActualizarTextoSensibilidad(float valor)
    {
        if (textoValorSensibilidad != null)
            textoValorSensibilidad.text = Mathf.RoundToInt(valor).ToString();
    }
}