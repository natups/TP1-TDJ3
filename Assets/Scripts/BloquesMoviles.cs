using UnityEngine;
using System.Collections;

public class BloquesMoviles : MonoBehaviour
{
    public float slideSpeed = 12f;
    public float respawnTime = 3f;

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

    // BUSCA EL ÚLTIMO BLOQUE
    BloquesMoviles BuscarUltimoBloque(Vector2 direccion)
    {
        Transform actual = transform;
        BloquesMoviles ultimo = this;

        int seguridad = 20;

        while (seguridad > 0)
        {
            seguridad--;

            Vector2 checkPos = (Vector2)actual.position + direccion;

            Collider2D hit = Physics2D.OverlapBox(
                checkPos,
                new Vector2(0.8f, 0.8f),
                0f
            );

            if (hit == null)
                return ultimo;

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
                return null;
            }
        }

        return null;
    }

    // DESLIZAMIENTO
    public IEnumerator Deslizar(Vector2 direccion)
    {
        isMoving = true;

        while (true)
        {
            Vector2 nextPos = (Vector2)transform.position + direccion;

            Collider2D hit = Physics2D.OverlapBox(
                nextPos,
                new Vector2(0.8f, 0.8f),
                0f
            );

            if (hit != null)
            {
                MovimientoEnemigos enemigo = hit.GetComponent<MovimientoEnemigos>();

                if (enemigo != null)
                {
                    Destroy(enemigo.gameObject);
                }
                else
                {
                    Destruir();
                    break;
                }
            }

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

            // SNAP A GRID
            transform.position = new Vector3(
                Mathf.Round(transform.position.x),
                Mathf.Round(transform.position.y),
                transform.position.z
            );
        }

        isMoving = false;
    }

    public void Romper()
    {
        StartCoroutine(Respawn());
    }

    public void Destruir()
    {
        GenerarExpansion();
        StartCoroutine(Respawn());
    }

    void GenerarExpansion()
    {
        EvaluarDireccion(Vector2.up);
        EvaluarDireccion(Vector2.down);
        EvaluarDireccion(Vector2.left);
        EvaluarDireccion(Vector2.right);
    }

    void EvaluarDireccion(Vector2 direccion)
    {
        Vector2 checkPos = (Vector2)transform.position + direccion;

        Collider2D hit = Physics2D.OverlapBox(
            checkPos,
            new Vector2(0.8f, 0.8f),
            0f
        );

        if (hit != null)
        {
            MovimientoEnemigos enemigo = hit.GetComponent<MovimientoEnemigos>();
            if (enemigo != null)
            {
                Destroy(enemigo.gameObject);
                return;
            }

            BloquesMoviles bloque = hit.GetComponent<BloquesMoviles>();
            if (bloque != null)
            {
                bloque.StartCoroutine(bloque.MoverUnaCelda(direccion));
                return;
            }

            return;
        }
    }

    public IEnumerator MoverUnaCelda(Vector2 direccion)
    {
        if (isMoving) yield break;

        isMoving = true;

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

        transform.position = new Vector3(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y),
            transform.position.z
        );

        isMoving = false;
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