using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2 movement;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // evitar diagonales
        if (movement.x != 0)
            movement.y = 0;

        // parámetros de movimiento
        animator.SetFloat("MoveX", movement.x);
        animator.SetFloat("MoveY", movement.y);

        // velocidad (para saber si está quieto o no)
        animator.SetFloat("Speed", movement.sqrMagnitude);

        // 🔥 GUARDAR ÚLTIMA DIRECCIÓN
        if (movement != Vector2.zero)
        {
            animator.SetFloat("LastX", movement.x);
            animator.SetFloat("LastY", movement.y);
        }
    }

    void FixedUpdate()
    {
        transform.position += (Vector3)movement * moveSpeed * Time.fixedDeltaTime;
    }
}