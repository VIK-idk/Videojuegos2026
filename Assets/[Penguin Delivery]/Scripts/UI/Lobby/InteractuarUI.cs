using UnityEngine;

public class InteractuarUI : Interactable
{
    [SerializeField] private TiendaUIController tiendaUIController;

    protected override void Interactuar()
    {
        if (tiendaUIController != null)
        {
            tiendaUIController.AbrirTienda();
        }
    }
}