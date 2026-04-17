using UnityEngine;
using System.Collections;

public class BloquesMoviles : MonoBehaviour
{
    public float slideSpeed = 12f;
    public float respawnTime = 3f;

    public LayerMask obstaculosLayer;

    private Vector3 initialPosition;

    public bool isMoving = false;

    void Start()
    {
        initialPosition = transform.position;
    }

    // EMPUJAR 
    public void Empujar(Vector2 direccion)
    {
        if (isMoving) return;

        // 🔥 pequeño empujón visual
        StartCoroutine(PequenoEmpujon(direccion));

        BloquesMoviles ultimo = BuscarUltimoBloque(direccion);

        if (ultimo != null)
        {
            ultimo.StartCoroutine(ultimo.Deslizar(direccion));
        }
    }

    IEnumerator PequenoEmpujon(Vector2 direccion)
    {
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)direccion * 0.2f;

        float t = 0f;
        float duracion = 0.05f;

        while (t < duracion)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t / duracion);
            yield return null;
        }

        t = 0f;

        while (t < duracion)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(end, start, t / duracion);
            yield return null;
        }

        transform.position = start;
    }

    // BUSCA EL ÚLTIMO BLOQUE QUE SE PUEDE MOVER
    BloquesMoviles BuscarUltimoBloque(Vector2 direccion)
    {
        Transform actual = transform;
        BloquesMoviles ultimo = this;

        int seguridad = 20;

        while (seguridad > 0)
        {
            seguridad--;

            Vector2 checkPos = (Vector2)actual.position + direccion;

            Collider2D hit = Physics2D.OverlapCircle(
                checkPos,
                0.2f,
                obstaculosLayer
            );

            Debug.DrawLine(actual.position, checkPos, Color.yellow, 1f);

            if (hit == null)
            {
                // espacio libre
                return ultimo;
            }

            BloquesMoviles bloque = hit.GetComponent<BloquesMoviles>();

            if (bloque != null)
            {
                if (bloque.isMoving)
                    return null;

                ultimo = bloque;
                actual = bloque.transform;
            }
            else
            {
                // pared
                return null;
            }
        }

        return null;
    }

    // DESLIZAMIENTO CONTINUO
    public IEnumerator Deslizar(Vector2 direccion)
    {
        isMoving = true;

        while (true)
        {
            Vector2 nextPos = (Vector2)transform.position + direccion;

            Collider2D hit = Physics2D.OverlapCircle(
                nextPos,
                0.2f,
                obstaculosLayer
            );

            // 💥 si hay algo adelante
            if (hit != null)
            {
                MovimientoEnemigos enemigo = hit.GetComponent<MovimientoEnemigos>();

                if (enemigo != null)
                {
                    Destroy(enemigo.gameObject);

                    // 🔥 IMPORTANTE: seguimos avanzando
                }
                else
                {
                    // 🧱 pared o bloque → se rompe
                    Destruir();
                    break;
                }
            }

            // 🚀 movimiento en pasos de grid (CLAVE)
            Vector3 start = transform.position;
            Vector3 end = start + (Vector3)direccion;

            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime * slideSpeed;
                transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            transform.position = end;
        }

        isMoving = false;
    }

    public void Romper()
    {
        StartCoroutine(Respawn());
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