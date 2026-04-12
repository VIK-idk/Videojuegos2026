using UnityEngine;

public class CameraPivotController : MonoBehaviour
{
    [SerializeField] private float sensibilidad = 150f;
    [SerializeField] private Transform playerBody;

    private float xRotation = 0f;
    private bool ignorarPrimerFrame = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        float anguloInicial = transform.localEulerAngles.x;

        if (anguloInicial > 180f)
            anguloInicial -= 360f;

        xRotation = anguloInicial;
    }

    void Update()
    {
        if (ignorarPrimerFrame)
        {
            ignorarPrimerFrame = false;
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensibilidad * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidad * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -45f, 60f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}