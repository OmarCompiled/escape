using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private InputActionMap  m_PlayerActionMap;
    private InputAction     m_MoveAction;
    private InputAction     m_SprintAction;
    private InputAction     m_InteractAction;

    private Rigidbody       m_Rigidbody;
    private Transform       m_CameraMount;
    private Transform       m_Orientation;

    private bool            m_IsGrounded = true;

    private float           m_Speed;

    void Start()
    {
        m_PlayerActionMap   = InputSystem.actions.FindActionMap("Player");
        m_MoveAction        = m_PlayerActionMap.FindAction("Move");
        m_SprintAction      = m_PlayerActionMap.FindAction("Sprint");
        m_Rigidbody         = GetComponent<Rigidbody>();
        m_CameraMount       = transform.Find("CameraMount");
        m_Orientation       = transform.Find("Orientation");

        m_MoveAction.Enable();
        m_SprintAction.Enable();
        m_Speed = 75.0f;
    }

    void FixedUpdate()
    {
        ProcessMovement();
        ProcessHeadBob();
    }

    void ProcessMovement()
    {
        Vector2 movement    = m_MoveAction.ReadValue<Vector2>();
        Vector3 velocity    = (movement.y * m_Orientation.forward) + (movement.x * m_Orientation.right);
        bool isSprinting    = m_SprintAction.IsPressed();
        float speed         = isSprinting ? m_Speed * 1.5f : m_Speed;

        velocity *= speed;
        m_Rigidbody.AddForce(velocity, ForceMode.Acceleration);
        m_Rigidbody.linearDamping = m_IsGrounded ? 5.0f : 1.0f;

    }

    void ProcessHeadBob()
    {
        return;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            m_IsGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            m_IsGrounded = false;
        }
    }
}
