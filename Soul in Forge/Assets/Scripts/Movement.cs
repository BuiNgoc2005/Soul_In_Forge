using UnityEngine;

public class Movement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed = 5;
    public int facingDirection = 1;

    public Rigidbody2D rb;
    public Animator anim;
    // Update is called once per frame
    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (vertical > 0)
        {
            anim.Play("WalkUp");
        }
        else if (vertical < 0)
        {
            anim.Play("Walk");
        }
        else if (horizontal < 0)
        {
            anim.Play("WalkSideOposite");
}
        else if (horizontal > 0)
        {
            anim.Play("WalkSide");
}
        else
        {
            anim.Play("Idle");
        }

        rb.linearVelocity = new Vector2(horizontal, vertical) * speed;
    }
    
}