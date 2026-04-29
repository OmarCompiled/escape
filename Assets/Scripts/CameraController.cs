using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private float m_Pitch       = 0.0f; // don't need yaw as mouseDelta is good enough.
    private float m_Sensitivity = 30.0f;
    
    private Transform m_Player;

    void Start()
    {
        Cursor.lockState    = CursorLockMode.Locked;
        m_Player = transform.parent.transform;
    }

    void LateUpdate()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        m_Player?.Rotate(Vector3.up, mouseDelta.x * Time.deltaTime * m_Sensitivity);

        m_Pitch -= mouseDelta.y * Time.deltaTime * m_Sensitivity;
        m_Pitch = Mathf.Clamp(m_Pitch, -80.0f, 80.0f);
        transform.localEulerAngles = new Vector3(m_Pitch, transform.localEulerAngles.y, 0.0f);
    }
}
