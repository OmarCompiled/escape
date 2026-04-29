using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private InputActionMap  m_PlayerActionMap;
    private InputAction     m_MoveAction;
    private InputAction     m_JumpAction;
    private InputAction     m_SprintAction;
    private InputAction     m_InteractAction;

    private Rigidbody       m_Rigidbody;

    private bool            m_IsGrounded = true;

    private float           m_JumpForce;
    private float           m_Speed;

    void Start()
    {
        m_PlayerActionMap   = InputSystem.actions.FindActionMap("Player");
        m_MoveAction        = m_PlayerActionMap.FindAction("Move");
        m_JumpAction        = m_PlayerActionMap.FindAction("Jump");

        m_Rigidbody         = GetComponent<Rigidbody>();

        m_MoveAction.Enable();
        m_JumpAction.Enable();
    }

    void FixedUpdate()
    {
        Vector2 movement = m_MoveAction.ReadValue<Vector2>();
        m_Rigidbody.AddForce(movement.y * Time.fixedDeltaTime * 25.0f * transform.forward, ForceMode.Impulse);
        if(m_JumpAction.IsPressed() && m_IsGrounded)
        {
            m_Rigidbody.AddForce(Time.fixedDeltaTime * 100.0f * Vector3.up, ForceMode.Impulse);
        }
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
