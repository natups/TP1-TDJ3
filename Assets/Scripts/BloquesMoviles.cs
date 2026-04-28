using UnityEngine;
using System.Collections;

public class BloquesMoviles : MonoBehaviour
{
    public LayerMask bloquesLayer;
    public LayerMask paredesLayer;

    public float slideSpeed = 12f;
    public float respawnTime = 3f;

    private Vector3 initialPosition;

    public bool isMoving = false;

    void Start()
    {
        initialPosition = transform.position;
    }

    // =========================
    // 🔷 HELPERS GRID (CLAVE)
    // =========================

    Vector2 GetGridPos(Vector3 pos)
    {
        return new Vector2(
            Mathf.Round(pos.x),
            Mathf.Round(pos.y)
        );
    }

    Vector3 ToWorld(Vector2 gridPos, float z)
    {
        return new Vector3(gridPos.x, gridPos.y, z);
    }

    // =========================
    // EMPUJAR 
    // =========================
    public void Empujar(Vector2 direccion)
{
    if (isMoving) return;

    direccion = direccion.normalized;

    BloquesMoviles ultimo = BuscarUltimoBloque(direccion);
    if (ultimo == null) return;

    Vector2 nextPos = GetGridPos(ultimo.transform.position) + direccion;

    // 🔍 DEBUG VISUAL (línea en escena)
    Debug.DrawLine(ultimo.transform.position, nextPos, Color.red, 1f);

    // 🔍 DEBUG BLOQUES
    Collider2D hitBloque = Physics2D.OverlapPoint(nextPos, bloquesLayer);
    if (hitBloque != null)
    {
        Debug.Log("Detectó BLOQUE: " + hitBloque.name);
    }

    // 🔍 DEBUG PAREDES
    Collider2D hitPared = Physics2D.OverlapPoint(nextPos, paredesLayer);
    if (hitPared != null)
    {
        Debug.Log("Detectó PARED: " + hitPared.name);
    }

    // 🚫 BLOQUE ADELANTE → rebote
    if (hitBloque != null)
    {
        StartCoroutine(PequenoEmpujon(direccion));
        return;
    }

    // 🚫 PARED ADELANTE → rebote
    if (hitPared != null)
    {
        StartCoroutine(PequenoEmpujon(direccion));
        return;
    }

    // ✅ SI ESTA LIBRE → empuja normal
    StartCoroutine(PequenoEmpujon(direccion));
    ultimo.StartCoroutine(ultimo.Deslizar(direccion));
}

    // =========================
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

    // =========================
    // BUSCAR ÚLTIMO BLOQUE
    // =========================
    BloquesMoviles BuscarUltimoBloque(Vector2 direccion)
    {
        Transform actual = transform;
        BloquesMoviles ultimo = this;

        int seguridad = 20;

        while (seguridad > 0)
        {
            seguridad--;

            Vector2 checkPos = GetGridPos(actual.position) + direccion;

            Collider2D hit = Physics2D.OverlapPoint(checkPos, bloquesLayer);

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
                return ultimo;
            }
        }

        return ultimo;
    }

    // =========================
    // DESLIZAR (🔥 SOLO CAMBIO ACÁ)
    // =========================
    public IEnumerator Deslizar(Vector2 direccion)
    {
        isMoving = true;

        Vector2 boxSize = new Vector2(0.6f, 0.6f);

        while (true)
        {
            Vector2 nextPos = (Vector2)transform.position + direccion;

            Collider2D hit = Physics2D.OverlapBox(nextPos, boxSize, 0f);

            if (hit != null)
            {
                MovimientoEnemigos enemigo = hit.GetComponent<MovimientoEnemigos>();

                // 🔴 SI ES ENEMIGO
                if (enemigo != null)
                {
                    Destroy(enemigo.gameObject);
                    Destruir(); // 🔥 ahora también se destruye el bloque
                    break;
                }

                // 🔴 SI ES CUALQUIER OTRA COSA (pared / obstáculo)
                Destruir();
                break;
            }

            // 👉 movimiento normal
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

    // =========================
    public void Romper()
    {
        StartCoroutine(Respawn());
    }

    public void Destruir()
    {
        GenerarExpansion();
        StartCoroutine(Respawn());
    }

    // =========================
    void GenerarExpansion()
    {
        EvaluarDireccion(Vector2.up);
        EvaluarDireccion(Vector2.down);
        EvaluarDireccion(Vector2.left);
        EvaluarDireccion(Vector2.right);
    }

    void EvaluarDireccion(Vector2 direccion)
    {
        Vector2 checkPos = GetGridPos(transform.position) + direccion;

        Collider2D bloque = Physics2D.OverlapPoint(checkPos, bloquesLayer);
        Collider2D wall = Physics2D.OverlapPoint(checkPos, paredesLayer);

        if (wall != null) return;

        if (bloque != null)
        {
            BloquesMoviles b = bloque.GetComponent<BloquesMoviles>();
            if (b != null)
            {
                b.StartCoroutine(b.MoverUnaCelda(direccion));
            }
        }
    }

    public IEnumerator MoverUnaCelda(Vector2 direccion)
    {
        if (isMoving) yield break;

        isMoving = true;

        Vector2 currentGrid = GetGridPos(transform.position);
        Vector2 nextPos = currentGrid + direccion;

        Vector3 start = transform.position;
        Vector3 end = ToWorld(nextPos, transform.position.z);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * slideSpeed;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;

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