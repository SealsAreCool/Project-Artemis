using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 move;
    private Animator animator;
    private Vector2 lastMove; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision){
        Debug.Log("Is it working?");
    }
    void Update()
    {

        move.x = Input.GetAxisRaw("Horizontal");
        move.y = Input.GetAxisRaw("Vertical");
        move = move.normalized;


        animator.SetFloat("Horizontal", move.x);
        animator.SetFloat("Vertical", move.y);
        animator.SetFloat("Speed", move.sqrMagnitude);

        if (move.sqrMagnitude > 0)
        {

            if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
                lastMove = new Vector2(Mathf.Sign(move.x), 0);
            else
                lastMove = new Vector2(0, Mathf.Sign(move.y));

            animator.SetFloat("LastHorizontal", lastMove.x);
            animator.SetFloat("LastVertical", lastMove.y);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);
    }

    void Die()
    {
        Debug.Log("Player Died!");
        Destroy(gameObject);
    }
}
