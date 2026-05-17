using UnityEngine;

public class AguaAnimadaGrupo : MonoBehaviour
{
    [Header("Movimiento textura")]
    [SerializeField] private Vector2 velocidadTextura = new Vector2(0.03f, 0.01f);
    [SerializeField] private Vector2 tilingTextura = Vector2.one;

    [Header("Movimiento suave del lago completo")]
    [SerializeField] private bool moverLagoCompletoArribaAbajo = false;
    [SerializeField] private float alturaMovimiento = 0.03f;
    [SerializeField] private float velocidadMovimiento = 0.7f;

    private Renderer[] renderersAgua;
    private MaterialPropertyBlock propertyBlock;
    private Vector2 offsetActual;
    private Vector3 posicionInicialLago;

    private void Start()
    {
        renderersAgua = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        posicionInicialLago = transform.localPosition;
    }

    private void Update()
    {
        AnimarTextura();
        AnimarLagoCompleto();
    }

    private void AnimarTextura()
    {
        offsetActual += velocidadTextura * Time.deltaTime;

        Vector4 texturaST = new Vector4(
            tilingTextura.x,
            tilingTextura.y,
            offsetActual.x,
            offsetActual.y
        );

        for (int i = 0; i < renderersAgua.Length; i++)
        {
            Renderer rendererAgua = renderersAgua[i];

            if (rendererAgua == null)
                continue;

            rendererAgua.GetPropertyBlock(propertyBlock);

            if (rendererAgua.sharedMaterial != null && rendererAgua.sharedMaterial.HasProperty("_BaseMap"))
            {
                propertyBlock.SetVector("_BaseMap_ST", texturaST);
            }
            else
            {
                propertyBlock.SetVector("_MainTex_ST", texturaST);
            }

            rendererAgua.SetPropertyBlock(propertyBlock);
        }
    }

    private void AnimarLagoCompleto()
    {
        if (!moverLagoCompletoArribaAbajo)
            return;

        float movimientoY = Mathf.Sin(Time.time * velocidadMovimiento) * alturaMovimiento;

        transform.localPosition = posicionInicialLago + new Vector3(0f, movimientoY, 0f);
    }
}