using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BloquesMoviles : MonoBehaviour
{
    public LayerMask bloquesLayer;
    public LayerMask paredesLayer;
    public LayerMask bloqueRompibleLayer;
    public LayerMask obstaculosLayer;
    public LayerMask diamanteLayer;

    public float slideSpeed = 6f;
    public float tileSize = 1f;

    public bool isMoving = false;

    private UIManager ui;

    void Start()
    {
        ui = FindAnyObjectByType<UIManager>();
    }

    Vector2 DireccionCardinal(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return new Vector2(Mathf.Sign(dir.x), 0);
        else
            return new Vector2(0, Mathf.Sign(dir.y));
    }

    public void Empujar(Vector2 direccion)
    {
        if (isMoving) return;

        direccion = DireccionCardinal(direccion);
        Vector2 nextPos = (Vector2)transform.position + direccion * tileSize;

        if (HayAlgo(nextPos, paredesLayer))
        {
            ExplosionEnCruz(transform.position);
            Destruir();
            return;
        }

        StartCoroutine(Deslizar(direccion, esUltimo: true));
    }

    public IEnumerator Deslizar(Vector2 direccion, bool esUltimo = true)
    {
        isMoving = true;
        int rebotes = 0;

        while (true)
        {
            Vector2 nextPos = (Vector2)transform.position + direccion * tileSize;

            // 🧱 PARED
            // 🧱 PARED U OBSTÁCULO
            if (HayAlgo(nextPos, paredesLayer) || HayAlgo(nextPos, obstaculosLayer))
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

                if (esUltimo)
                    ExplosionEnCruz(transform.position);

                Destruir();
                break;
            }

            // 🪑 ROMPIBLE
            Collider2D rompible = GetCollider(nextPos, bloqueRompibleLayer);
            if (rompible != null)
            {
                Destroy(rompible.gameObject);
                if (ui != null) ui.SumarPuntos(50);
                if (esUltimo)
                    ExplosionEnCruz(transform.position);
                Destruir();
                break;
            }

            // 💎 DIAMANTE
            Collider2D diamanteCol = GetCollider(nextPos, diamanteLayer);
            if (diamanteCol != null)
            {
                BloqueDiamante diamante = diamanteCol.GetComponent<BloqueDiamante>();
                if (diamante != null)
                {
                    diamante.RecibirImpacto(direccion, gameObject);
                    isMoving = false;
                    yield break;
                }
            }

            // 🔵 BLOQUE MOVIL
            Collider2D bloque = GetCollider(nextPos, bloquesLayer);
            if (bloque != null)
            {
                BloquesMoviles otro = bloque.GetComponent<BloquesMoviles>();
                if (otro != null && !otro.isMoving)
                    otro.StartCoroutine(otro.Deslizar(direccion, esUltimo: true));
                break;
            }

            // 👾 ENEMIGO
            Collider2D hit = Physics2D.OverlapPoint(nextPos);
            if (hit != null && hit.GetComponent<MovimientoJugador>() == null)
            {
                MovimientoEnemigos enemigo = hit.GetComponent<MovimientoEnemigos>();
                if (enemigo != null)
                {
                    enemigo.MorirPorBloque();
                    if (ui != null) ui.SumarPuntos(100);
                    Destruir();
                    break;
                }
            }

            // 🟢 MOVER
            Vector3 start = transform.position;
            Vector3 end = start + (Vector3)direccion * tileSize;
            float lerp = 0f;

            while (lerp < 1f)
            {
                lerp += Time.deltaTime * slideSpeed;
                transform.position = Vector3.Lerp(start, end, lerp);
                yield return null;
            }

            transform.position = end;
        }

        isMoving = false;
    }

    Vector2 ObtenerRebote(Vector2 direccion)
    {
        Vector2 derecha = new Vector2(direccion.y, -direccion.x);
        Vector2 izquierda = new Vector2(-direccion.y, direccion.x);
        Vector2 pos = transform.position;

        bool chequearDerechaPrimero = Random.value > 0.5f;
        Vector2 primero = chequearDerechaPrimero ? derecha : izquierda;
        Vector2 segundo = chequearDerechaPrimero ? izquierda : derecha;

        if (!HayAlgo(pos + primero * tileSize, paredesLayer)) return primero;
        if (!HayAlgo(pos + segundo * tileSize, paredesLayer)) return segundo;

        return Vector2.zero;
    }

    void ExplosionEnCruz(Vector2 centro)
    {
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        foreach (Vector2 dir in dirs)
        {
            Vector2 pos = centro + dir * tileSize;
            Collider2D hit = Physics2D.OverlapPoint(pos);
            if (hit == null) continue;

            MovimientoEnemigos enemigo = hit.GetComponent<MovimientoEnemigos>();
            if (enemigo != null)
            {
                enemigo.MorirPorBloque();
                if (ui != null) ui.SumarPuntos(100);
                continue;
            }

            BloquesMoviles bloque = hit.GetComponent<BloquesMoviles>();
            if (bloque != null)
            {
                bloque.Destruir();
                continue;
            }
        }
    }

    bool HayAlgo(Vector2 pos, LayerMask layer)
    {
        return Physics2D.OverlapPoint(pos, layer) != null;
    }

    Collider2D GetCollider(Vector2 pos, LayerMask layer)
    {
        return Physics2D.OverlapPoint(pos, layer);
    }

    public void Destruir()
    {
        Destroy(gameObject);
    }
}