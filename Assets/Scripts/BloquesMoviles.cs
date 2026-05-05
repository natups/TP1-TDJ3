using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BloquesMoviles : MonoBehaviour
{
    public LayerMask bloquesLayer;
    public LayerMask paredesLayer;
    public LayerMask bloqueRompibleLayer;

    public float slideSpeed = 6f;
    public float respawnTime = 8f;
    public float tileSize = 1f;

    private Vector3 initialPosition;
    public bool isMoving = false;

    private UIManager ui;

    void Start()
    {
        ui = FindAnyObjectByType<UIManager>();
        initialPosition = transform.position;
    }

    // =========================
    Vector2 DireccionCardinal(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return new Vector2(Mathf.Sign(dir.x), 0);
        else
            return new Vector2(0, Mathf.Sign(dir.y));
    }

    // =========================
    public void Empujar(Vector2 direccion)
    {
        if (isMoving) return;

        direccion = DireccionCardinal(direccion);

        Vector2 nextPos = (Vector2)transform.position + direccion * tileSize;

        // 🔴 SI HAY PARED → HACER MINI REBOTE (NO DESLIZA)
        if (HayAlgo(nextPos, paredesLayer))
        {
            StartCoroutine(PequenoEmpujon(direccion));
            return;
        }

        // 🟢 SI NO → DESLIZA NORMAL
        StartCoroutine(Deslizar(direccion));
    }

    // =========================
    IEnumerator PequenoEmpujon(Vector2 direccion)
    {
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)direccion * (tileSize * 0.2f);

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
    public IEnumerator Deslizar(Vector2 direccion)
    {
        isMoving = true;

        int rebotes = 0;

        while (true)
        {
            Vector2 nextPos = (Vector2)transform.position + direccion * tileSize;

            // 🔴 PARED (REBOTE REAL)
            if (HayAlgo(nextPos, paredesLayer))
            {
                if (rebotes < 1)
                {
                    Vector2 nuevaDir = ObtenerRebote(direccion);

                    if (nuevaDir != Vector2.zero)
                    {
                        direccion = nuevaDir;
                        rebotes++;
                        continue;
                    }
                }

                Destruir();
                break;
            }

            // 🪑 ROMPIBLE
            Collider2D rompible = GetCollider(nextPos, bloqueRompibleLayer);
            if (rompible != null)
            {
                Destroy(rompible.gameObject);

                if (ui != null)
                    ui.SumarPuntos(50);

                Destruir();
                break;
            }

            // 🔵 BLOQUE (EMPUJA SOLO AL ÚLTIMO)
            Collider2D bloque = GetCollider(nextPos, bloquesLayer);
            if (bloque != null)
            {
                BloquesMoviles otro = bloque.GetComponent<BloquesMoviles>();

                if (otro != null && !otro.isMoving)
                {
                    otro.StartCoroutine(otro.Deslizar(direccion));
                }

                break;
            }

            // 👾 ENEMIGO
            Collider2D hit = Physics2D.OverlapPoint(nextPos);
            if (hit != null)
            {
                MovimientoEnemigos enemigo = hit.GetComponent<MovimientoEnemigos>();

                if (enemigo != null)
                {
                    Destroy(enemigo.gameObject);

                    if (ui != null)
                        ui.SumarPuntos(100);

                    Destruir();
                    break;
                }
            }

            // 🟢 MOVIMIENTO PERFECTO EN GRILLA
            Vector3 start = transform.position;
            Vector3 end = start + (Vector3)direccion * tileSize;

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
    bool HayAlgo(Vector2 pos, LayerMask layer)
    {
        return Physics2D.OverlapPoint(pos, layer) != null;
    }

    Collider2D GetCollider(Vector2 pos, LayerMask layer)
    {
        return Physics2D.OverlapPoint(pos, layer);
    }

    // =========================
    Vector2 ObtenerRebote(Vector2 direccion)
    {
        Vector2 derecha = new Vector2(direccion.y, -direccion.x);
        Vector2 izquierda = new Vector2(-direccion.y, direccion.x);

        Vector2 pos = transform.position;

        if (!HayAlgo(pos + derecha * tileSize, paredesLayer))
            return derecha;

        if (!HayAlgo(pos + izquierda * tileSize, paredesLayer))
            return izquierda;

        return Vector2.zero;
    }

    // =========================
    public void Destruir()
    {
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(respawnTime);

        transform.position = initialPosition;

        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
    }
}