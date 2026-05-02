using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private float m_Pitch       = 0.0f; // don't need yaw as mouseDelta is good enough.
    private float m_Sensitivity = 30.0f;
    
    private Transform m_Player;
    private Transform m_CameraMount;
    private Transform m_PlayerOrientation;

    void Start()
    {
        Cursor.lockState    = CursorLockMode.Locked;
        m_Player = GameObject.Find("Player").transform;
        m_CameraMount = m_Player.Find("CameraMount");
        m_PlayerOrientation = m_Player.Find("Orientation");
        transform.position = m_CameraMount.position;
    }

    void LateUpdate()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        transform.Rotate(Vector3.up, mouseDelta.x * Time.deltaTime * m_Sensitivity);
        m_PlayerOrientation.localEulerAngles = new Vector3(0.0f, transform.localEulerAngles.y, 0.0f);
        m_Pitch -= mouseDelta.y * Time.deltaTime * m_Sensitivity;
        m_Pitch = Mathf.Clamp(m_Pitch, -80.0f, 80.0f);
        transform.localEulerAngles = new Vector3(m_Pitch, transform.localEulerAngles.y, 0.0f);
        transform.position = m_CameraMount.position;
    }
}