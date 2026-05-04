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

        // 🎮 foco inicial mando
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(primerBotonOpciones);
    }

    private void Update()
    {
        // 🔥 mantener foco UI
        if (EventSystem.current.currentSelectedGameObject == null && primerBotonOpciones != null)
        {
            EventSystem.current.SetSelectedGameObject(primerBotonOpciones);
        }
    }

    private void ConfigurarDropdownResolucion()
    {
        if (resolucionDrop == null) return;

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
        if (sensibilidadSlider == null) return;

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
        }

        if (volumenSFX != null)
        {
            volumenSFX.minValue = -80f;
            volumenSFX.maxValue = 0f;
        }
    }

    private void CargarYAplicarSettings()
    {
        float sensibilidadGuardada = PlayerPrefs.GetFloat("Sensibilidad", 550f);

        if (sensibilidadSlider != null)
            sensibilidadSlider.SetValueWithoutNotify(sensibilidadGuardada);

        ActualizarTextoSensibilidad(sensibilidadGuardada);
    }

    private void ActualizarTextoSensibilidad(float valor)
    {
        if (textoValorSensibilidad != null)
            textoValorSensibilidad.text = Mathf.RoundToInt(valor).ToString();
    }

    public void SetSensibilidadPref(float valor)
    {
        PlayerPrefs.SetFloat("Sensibilidad", valor);
        PlayerPrefs.Save();

        ActualizarTextoSensibilidad(valor);
    }
}