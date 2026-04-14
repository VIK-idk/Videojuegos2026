using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractuarCambiarEscena : Interactable
{
    [SerializeField] private string nombreEscena;

    protected override void Interactuar()
    {
        SceneManager.LoadScene(nombreEscena);
    }
}