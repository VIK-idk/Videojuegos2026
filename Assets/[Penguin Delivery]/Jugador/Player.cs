using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private bool estaEnSuelo = true;
    [SerializeField] private float jumpTrampolin = 40f;
    [SerializeField] private float multiplicadorCaida = 2.5f;
    [SerializeField] private float multiplicadorSaltoBajo = 2f;
    private GestorEncargos gestor;
    private bool haEmpezado = false;
    private Rigidbody rb;
    private GameManager gm;

    private float inputX;
    private float inputZ;

    void Start()
    {
        gestor = FindObjectOfType<GestorEncargos>();
        rb = GetComponent<Rigidbody>();
        gm = FindObjectOfType<GameManager>();

        rb.freezeRotation = true; 
    }

    void Update()
    {

        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");
                if (!haEmpezado && (
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.D)))
        {
            if (gestor != null)
                gestor.IniciarSistema();

            haEmpezado = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) && estaEnSuelo)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            estaEnSuelo = false;
        }

    }

    void FixedUpdate()
    {

        Vector3 direccion = transform.forward * inputZ + transform.right * inputX;
        Vector3 velocidad = direccion * speed;
        velocidad.y = rb.linearVelocity.y;

        rb.linearVelocity = velocidad;


        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (multiplicadorCaida - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (multiplicadorSaltoBajo - 1) * Time.fixedDeltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            estaEnSuelo = true;
        }

        if (collision.gameObject.CompareTag("Trampolin"))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpTrampolin, ForceMode.Impulse);
        }
    }

private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Amarillo"))
    {
        other.tag = "Untagged";
        if (gm != null)
    gm.SumarPez();

        if (gestor != null)
        {
            gestor.SumarPez("Amarillo"); 
        }

        Destroy(other.gameObject);
    }
}
}