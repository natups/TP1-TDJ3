using UnityEngine;

public class EnemyMovimiento : MonoBehaviour
{
    public float speed = 2f;
    public LayerMask obstaculosLayer;
    public LayerMask bloquesLayer;
    public float checkDistance = 0.2f;

    private Vector2 direction;

    void Start()
    {
        ChooseNewDirection();
    }

    void Update()
    {
        Vector2 origin = transform.position;

        // chequeo antes de moverse
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, checkDistance, obstaculosLayer);

        if (hit.collider == null)
        {
            transform.position += (Vector3)direction * speed * Time.deltaTime;
        }
        else
        {
            ChooseNewDirection();
        }
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