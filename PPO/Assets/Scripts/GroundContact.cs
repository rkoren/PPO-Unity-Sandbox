using UnityEngine;

// Attach to a foot collider. Tag the ground plane "Ground".
public class GroundContact : MonoBehaviour
{
    public bool isGrounded;

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
