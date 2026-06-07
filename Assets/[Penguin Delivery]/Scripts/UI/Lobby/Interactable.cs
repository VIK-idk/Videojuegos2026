using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] protected GameObject textoInteractuar;

    protected bool jugadorDentro = false;

    protected virtual void Update()
    {
        if (!jugadorDentro)
            return;

        if (TiendaUIController.HayTiendaAbierta)
        {
            if (textoInteractuar != null)
                textoInteractuar.SetActive(false);

            return;
        }

        if (textoInteractuar != null)
            textoInteractuar.SetActive(true);

        if (Input.GetButtonDown("Interactuar"))
        {
            Interactuar();
        }
    }

    protected virtual void Interactuar()
    {
        Debug.Log("Interaccion base");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = true;

            if (textoInteractuar != null && !TiendaUIController.HayTiendaAbierta)
                textoInteractuar.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;

            if (textoInteractuar != null)
                textoInteractuar.SetActive(false);
        }
    }
}