using UnityEngine;

public class MovimientoCamara : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    private float xRotation = 0f;

    public Canvas inventario;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Oculta el cursor y lo bloquea en el centro de la pantalla
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limita la rotación vertical a 90 grados

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);

        if (inventario.enabled)
        {
            Cursor.lockState = CursorLockMode.None; // Oculta el cursor y lo bloquea en el centro de la pantalla
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // Oculta el cursor y lo bloquea en el centro de la pantalla
        }
    }
}
