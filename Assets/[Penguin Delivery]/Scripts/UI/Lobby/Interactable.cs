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
            MostrarTexto(false);
            return;
        }

        MostrarTexto(true);

        if (Input.GetButtonDown("Interactuar"))
            Interactuar();
    }

    protected virtual void Interactuar()
    {
        Debug.LogWarning(
            "Este objeto tiene el script Interactable base. " +
            "Usa InteractuarUI para la tienda o InteractuarCambiarEscena para la puerta.",
            this
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!EsJugador(other))
            return;

        jugadorDentro = true;
        MostrarTexto(!TiendaUIController.HayTiendaAbierta);
    }

    private void OnTriggerStay(Collider other)
    {
        // Sirve si la zona se activa cuando el jugador ya estaba dentro.
        if (!jugadorDentro && EsJugador(other))
            jugadorDentro = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!EsJugador(other))
            return;

        jugadorDentro = false;
        MostrarTexto(false);
    }

    private void OnDisable()
    {
        jugadorDentro = false;
        MostrarTexto(false);
    }

    private bool EsJugador(Collider other)
    {
        if (other == null)
            return false;

        if (other.CompareTag("Player"))
            return true;

        return other.GetComponentInParent<Player>() != null;
    }

    private void MostrarTexto(bool mostrar)
    {
        if (textoInteractuar != null && textoInteractuar.activeSelf != mostrar)
            textoInteractuar.SetActive(mostrar);
    }
}
