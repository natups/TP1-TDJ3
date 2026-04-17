using UnityEngine;
using System.Collections;

public class BloquesMoviles : MonoBehaviour
{
    public float moveDistance = 1f;
    public float moveSpeed = 5f;

    public float respawnTime = 3f;
    public LayerMask obstaculosLayer;
    private Vector3 initialPosition;

    public bool isMoving = false;

    void Start()
    {
        initialPosition = transform.position;
    }

    public void Empujar(Vector2 direccion)
    {
        if (isMoving) return;

        // 💥 chequeo de colisión antes de moverse
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direccion,
            moveDistance,
            obstaculosLayer
        );

        if (hit.collider != null)
        {
            // hay algo adelante → no se mueve
            return;
        }

        StartCoroutine(Mover(direccion));
    }

    public void Romper()
    {
        StartCoroutine(Respawn());
    }

    private IEnumerator Mover(Vector2 direccion)
    {
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)direccion * moveDistance;

        float t = 0f;
        float duration = 0.2f;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }

        transform.position = end;

        isMoving = false;
    }

    public void Destruir()
    {
    StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(respawnTime);

        transform.position = initialPosition;

        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
    }
}