using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class PlayerSettings : MonoBehaviour
{
    [SerializeField] private Toggle pantallaCompleta;
    [SerializeField] private Slider volumenMaster;
    [SerializeField] private Slider volumenSFX;
    [SerializeField] private Dropdown resolucionDrop;
    [SerializeField] private Slider sensibilidadSlider;
    [SerializeField] private Text textoValorSensibilidad;
    [SerializeField] private AudioMixer audioMixer;

    [Header("UI Mando")]
    [SerializeField] private GameObject primerBotonOpciones;

    private readonly Vector2Int[] resoluciones = new Vector2Int[]
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080)
    };

    private void Start()
    {
        ConfigurarDropdownResolucion();
        ConfigurarSliderSensibilidad();
        ConfigurarSlidersAudio();
        CargarYAplicarSettings();

        if (EventSystem.current != null && primerBotonOpciones != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(primerBotonOpciones);
        }
    }

    private void Update()
    {
        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == null &&
            primerBotonOpciones != null)
        {
            EventSystem.current.SetSelectedGameObject(primerBotonOpciones);
        }
    }

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
        if (volumenMaster != null)
        {
            volumenMaster.minValue = -80f;
            volumenMaster.maxValue = 0f;
            volumenMaster.wholeNumbers = false;
        }

        if (volumenSFX != null)
        {
            volumenSFX.minValue = -80f;
            volumenSFX.maxValue = 0f;
            volumenSFX.wholeNumbers = false;
        }
    }

    private void CargarYAplicarSettings()
    {
        bool pantallaCompletaGuardada = PlayerPrefs.GetInt("PantallaCompleta", Screen.fullScreen ? 1 : 0) == 1;
        float volumenMasterGuardado = PlayerPrefs.GetFloat("VolumenMaster", 0f);
        float volumenSFXGuardado = PlayerPrefs.GetFloat("VolumenSFX", 0f);
        float sensibilidadGuardada = PlayerPrefs.GetFloat("Sensibilidad", 550f);

        int resolucionIndexGuardado = PlayerPrefs.GetInt("ResolucionIndex", BuscarIndiceResolucionActual());
        resolucionIndexGuardado = Mathf.Clamp(resolucionIndexGuardado, 0, resoluciones.Length - 1);

        AplicarResolucion(resolucionIndexGuardado, pantallaCompletaGuardada);

        if (audioMixer != null)
        {
            audioMixer.SetFloat("VolumenMaster", volumenMasterGuardado);
            audioMixer.SetFloat("VolumenSFX", volumenSFXGuardado);
        }

        if (pantallaCompleta != null)
            pantallaCompleta.SetIsOnWithoutNotify(pantallaCompletaGuardada);

        if (volumenMaster != null)
            volumenMaster.SetValueWithoutNotify(volumenMasterGuardado);

        if (volumenSFX != null)
            volumenSFX.SetValueWithoutNotify(volumenSFXGuardado);

        if (resolucionDrop != null)
        {
            resolucionDrop.SetValueWithoutNotify(resolucionIndexGuardado);
            resolucionDrop.RefreshShownValue();
        }

        if (sensibilidadSlider != null)
            sensibilidadSlider.SetValueWithoutNotify(sensibilidadGuardada);

        ActualizarTextoSensibilidad(sensibilidadGuardada);
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

        Screen.SetResolution(resoluciones[index].x, resoluciones[index].y, fullscreen);
    }

    private void ActualizarTextoSensibilidad(float valor)
    {
        if (textoValorSensibilidad != null)
            textoValorSensibilidad.text = Mathf.RoundToInt(valor).ToString();
    }

    public void SetPantallaCompletaPref(bool valor)
    {
        int indiceActual = PlayerPrefs.GetInt("ResolucionIndex", BuscarIndiceResolucionActual());
        indiceActual = Mathf.Clamp(indiceActual, 0, resoluciones.Length - 1);

        AplicarResolucion(indiceActual, valor);

        PlayerPrefs.SetInt("PantallaCompleta", valor ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMasterVolumePref(float valor)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("VolumenMaster", valor);

        PlayerPrefs.SetFloat("VolumenMaster", valor);
        PlayerPrefs.Save();
    }

    public void SetSFXVolumePref(float valor)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("VolumenSFX", valor);

        PlayerPrefs.SetFloat("VolumenSFX", valor);
        PlayerPrefs.Save();
    }

    public void SetResolucionPref(int index)
    {
        index = Mathf.Clamp(index, 0, resoluciones.Length - 1);

        bool fullscreenActual = PlayerPrefs.GetInt("PantallaCompleta", Screen.fullScreen ? 1 : 0) == 1;
        AplicarResolucion(index, fullscreenActual);

        PlayerPrefs.SetInt("ResolucionIndex", index);
        PlayerPrefs.Save();
    }

    public void SetSensibilidadPref(float valor)
    {
        PlayerPrefs.SetFloat("Sensibilidad", valor);
        PlayerPrefs.Save();

        ActualizarTextoSensibilidad(valor);
    }
}