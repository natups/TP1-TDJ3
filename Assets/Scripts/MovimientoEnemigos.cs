using UnityEngine;

public class MovimientoEnemigos : MonoBehaviour
{
    public float speed = 2f;

    public LayerMask obstaculosLayer;
    public LayerMask bloquesLayer;

    private Vector2 direction;

    void Start()
    {
        ChooseNewDirection();
    }

    void Update()
    {
        Vector2 origin = transform.position;

        int mask = obstaculosLayer | bloquesLayer;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, 0.5f, mask);

        Debug.DrawRay(origin, direction * 0.5f, Color.red);

        if (hit.collider != null)
        {
            BloquesMoviles bloque = hit.collider.GetComponent<BloquesMoviles>();

            if (bloque != null)
            {
                // 💥 SOLO MUERE SI ESTÁ SIENDO EMPUJADO
                if (bloque.isMoving)
                {
                    Destroy(gameObject);
                    return;
                }

                // 🧱 si está quieto = pared
                ChooseNewDirection();
                return;
            }

            // obstáculo normal
            ChooseNewDirection();
            return;
        }

        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    void ChooseNewDirection()
    {
        int rand = Random.Range(0, 4);

        switch (rand)
        {
            case 0: direction = Vector2.up; break;
            case 1: direction = Vector2.down; break;
            case 2: direction = Vector2.left; break;
            case 3: direction = Vector2.right; break;
        }
    }
}