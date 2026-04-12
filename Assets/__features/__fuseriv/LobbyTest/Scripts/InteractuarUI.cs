using UnityEngine;

public class InteractuarUI : Interactable
{
    [SerializeField] private GameObject interfaz;

    protected override void Interactuar()
    {
        if (interfaz != null)
        {
            interfaz.SetActive(true);
        }
    }
}