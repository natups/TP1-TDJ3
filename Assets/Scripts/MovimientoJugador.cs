using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2 movement;
    private Animator animator;

    // interacción
    public float interactDistance = 1.5f;
    public LayerMask bloqueLayer;

    private Vector2 lastDirection = Vector2.down;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // INPUT MOVIMIENTO
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // evitar diagonales
        if (movement.x != 0)
            movement.y = 0;

        // animación movimiento
        animator.SetFloat("MoveX", movement.x);
        animator.SetFloat("MoveY", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

        // guardar última dirección válida
        if (movement != Vector2.zero)
        {
            lastDirection = movement.normalized;

            animator.SetFloat("LastX", lastDirection.x);
            animator.SetFloat("LastY", lastDirection.y);
        }

        // INPUT ACCIONES
        if (Input.GetKeyDown(KeyCode.J))
        {
            IntentarEmpujar();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            IntentarRomper();
        }
    }

    void FixedUpdate()
    {
        transform.position += (Vector3)movement * moveSpeed * Time.fixedDeltaTime;
    }

    // ------------------------
    // INTERACCIÓN
    // ------------------------

    void IntentarEmpujar()
    {
        Vector2 direccion = lastDirection;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direccion,
            interactDistance,
            bloqueLayer
        );

        Debug.Log("Hit: " + hit.collider);

        if (hit.collider != null)
        {
            BloquesMoviles bloque = hit.collider.GetComponent<BloquesMoviles>();

            if (bloque != null)
            {
                bloque.Empujar(direccion);
            }
        }

        Debug.DrawRay(transform.position, direccion * interactDistance, Color.red, 1f);
    }

    void IntentarRomper()
    {
        Vector2 direccion = lastDirection;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direccion,
            interactDistance,
            bloqueLayer
        );

        if (hit.collider != null)
        {
            BloquesMoviles bloque = hit.collider.GetComponent<BloquesMoviles>();

            if (bloque != null)
            {
                bloque.Romper();
            }
        }
    }
}