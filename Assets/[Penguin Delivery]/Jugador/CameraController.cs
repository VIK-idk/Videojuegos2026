using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float sensitivity = 150f;
    [SerializeField] private Transform playerBody;

    private float xRotation;
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

        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
    }
}