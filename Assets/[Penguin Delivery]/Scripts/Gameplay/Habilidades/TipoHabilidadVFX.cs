using UnityEngine;

[CreateAssetMenu(fileName = "NuevoTipoHabilidadVFX", menuName = "Penguin Delivery/Tipo Habilidad VFX")]
public class TipoHabilidadVFX : ScriptableObject
{
    [Header("Datos")]
    public string nombre;

    [Header("Audio")]
    public AudioClip efectoSonido;

    [Header("VFX")]
    public GameObject efectoVisual;

    [Header("Pulso")]
    public float duracionPulso = 0.6f;
}