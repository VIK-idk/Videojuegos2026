using UnityEngine;

public class CamaraMenuFlotante : MonoBehaviour
{
    [Header("Movimiento flotante")]
    [SerializeField] private float alturaMovimiento = 0.15f;
    [SerializeField] private float velocidadMovimiento = 0.8f;

    [Header("Rotacion suave")]
    [SerializeField] private float intensidadRotacion = 0.5f;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    private void Start()
    {
        posicionInicial = transform.localPosition;
        rotacionInicial = transform.localRotation;
    }

    private void Update()
    {
        float onda = Mathf.Sin(Time.time * velocidadMovimiento);

        float movimientoY = onda * alturaMovimiento;
        transform.localPosition = posicionInicial + new Vector3(0f, movimientoY, 0f);

        float rotacionZ = onda * intensidadRotacion;
        transform.localRotation = rotacionInicial * Quaternion.Euler(0f, 0f, rotacionZ);
    }
}