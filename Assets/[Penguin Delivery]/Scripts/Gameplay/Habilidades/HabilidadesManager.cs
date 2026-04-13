using System.Collections;
using UnityEngine;
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
    [SerializeField] private float duracionMensaje = 2f;

    private Coroutine rutinaMensaje;

    private void Start()
    {
        if (textoMensaje != null)
            textoMensaje.enabled = false;

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

        yield return new WaitForSeconds(duracionMensaje);

        textoMensaje.enabled = false;
        rutinaMensaje = null;
    }

    private void ActualizarUI()
    {
        ActualizarSlot(slotHabilidad1, habilidad1);
        ActualizarSlot(slotHabilidad2, habilidad2);
        ActualizarSlot(slotHabilidad3, habilidad3);
    }

    private void ActualizarSlot(HabilidadSlotUI slot, HabilidadBase habilidad)
    {
        if (slot == null || habilidad == null)
            return;

        if (!habilidad.EstaAdquirida())
        {
            slot.MostrarVacio();
        }
        else if (habilidad.EstaUsada())
        {
            slot.MostrarUsada(habilidad.GetTitulo());
        }
        else if (habilidad.EstaActiva())
        {
            slot.MostrarActiva(habilidad.GetTitulo(), habilidad.GetTextoTecla(), habilidad.GetTiempoVisible());
        }
        else if (habilidad.EstaEnCooldown())
        {
            slot.MostrarCooldown(habilidad.GetTitulo(), habilidad.GetTextoTecla(), habilidad.GetTiempoVisible());
        }
        else if (HayOtraHabilidadActiva(habilidad))
        {
            slot.MostrarBloqueada(habilidad.GetTitulo(), habilidad.GetTextoTecla());
        }
        else
        {
            slot.MostrarDisponible(habilidad.GetTitulo(), habilidad.GetTextoTecla());
        }
    }
}