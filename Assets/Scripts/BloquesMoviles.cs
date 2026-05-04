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

        // 🧠 SI HAY PARED → EXPLOTA (NO REBOTA)
        if (HayAlgo(nextPos, paredesLayer))
        {
            Vector2 centro = transform.position;

            ExplosionEnCruz(centro);

            Destruir();
            return;
        }

        StartCoroutine(Deslizar(direccion));
    }

    // =========================
    public IEnumerator Deslizar(Vector2 direccion)
    {
        isMoving = true;

        while (true)
        {
            Vector2 nextPos = (Vector2)transform.position + direccion * tileSize;

            // 🧠 PARED → EXPLOSIÓN
            if (HayAlgo(nextPos, paredesLayer))
            {
                Vector2 centro = transform.position;

                ExplosionEnCruz(centro);

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

            // 🔵 BLOQUE
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

            // 🟢 MOVIMIENTO EN GRILLA
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
    void ExplosionEnCruz(Vector2 centro)
{
    Vector2[] dirs =
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
    };

    foreach (Vector2 dir in dirs)
    {
        Vector2 pos = centro + dir * tileSize;

        // 🔥 primero intentamos detectar exactamente en el tile
        Collider2D hit = Physics2D.OverlapPoint(pos);

        if (hit == null)
            continue;

        // 👾 ENEMIGOS
        MovimientoEnemigos enemigo = hit.GetComponent<MovimientoEnemigos>();
        if (enemigo != null)
        {
            Destroy(enemigo.gameObject);

            if (ui != null)
                ui.SumarPuntos(100);

            continue;
        }

        // 🔵 BLOQUES MOVILES
        BloquesMoviles bloque = hit.GetComponent<BloquesMoviles>();
        if (bloque != null)
        {
            bloque.Destruir();
            continue;
        }
    }
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