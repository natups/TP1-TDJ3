using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BloquesMoviles : MonoBehaviour
{
    public LayerMask bloquesLayer;
    public LayerMask paredesLayer;
    public LayerMask bloqueRompibleLayer;

    public float slideSpeed = 8f;
    public float respawnTime = 8f;

    private Vector3 initialPosition;
    public bool isMoving = false;

    void Start()
    {
        initialPosition = transform.position;
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

        List<BloquesMoviles> cadena = ObtenerCadena(direccion);

        if (cadena == null || cadena.Count == 0) return;

        BloquesMoviles ultimo = cadena[cadena.Count - 1];

        Vector2 nextPos = (Vector2)ultimo.transform.position + direccion;

        Collider2D hitBloque = Physics2D.OverlapBox(nextPos, new Vector2(0.6f, 0.6f), 0f, bloquesLayer);
        Collider2D hitPared = Physics2D.OverlapBox(nextPos, new Vector2(0.6f, 0.6f), 0f, paredesLayer);

        if (hitBloque != null || hitPared != null)
        {
            StartCoroutine(ReboteEnCadena(cadena, direccion));
            return;
        }

        StartCoroutine(EmpujarConFeedback(cadena, direccion));
    }

    List<BloquesMoviles> ObtenerCadena(Vector2 direccion)
    {
        List<BloquesMoviles> lista = new List<BloquesMoviles>();

        Transform actual = transform;
        int seguridad = 20;

        while (seguridad > 0)
        {
            seguridad--;

            BloquesMoviles bloque = actual.GetComponent<BloquesMoviles>();

            if (bloque == null) break;
            if (bloque.isMoving) return null;

            lista.Add(bloque);

            Vector2 checkPos = (Vector2)actual.position + direccion;

            Collider2D hit = Physics2D.OverlapBox(checkPos, new Vector2(0.6f, 0.6f), 0f, bloquesLayer);

            if (hit == null)
                break;

            actual = hit.transform;
        }

        return lista;
    }

    IEnumerator EmpujarConFeedback(List<BloquesMoviles> cadena, Vector2 direccion)
    {
        foreach (var bloque in cadena)
        {
            bloque.StartCoroutine(bloque.PequenoEmpujon(direccion));
        }

        yield return new WaitForSeconds(0.05f);

        BloquesMoviles ultimo = cadena[cadena.Count - 1];
        ultimo.StartCoroutine(ultimo.Deslizar(direccion));
    }

    IEnumerator ReboteEnCadena(List<BloquesMoviles> cadena, Vector2 direccion)
    {
        foreach (var bloque in cadena)
        {
            bloque.StartCoroutine(bloque.PequenoEmpujon(direccion));
        }

        yield return null;
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

    public IEnumerator Deslizar(Vector2 direccion)
    {
        isMoving = true;

        Vector2 boxSize = new Vector2(0.6f, 0.6f);
        int rebotes = 0;

        Collider2D propioCollider = GetComponent<Collider2D>();

        while (true)
        {
            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                transform.position,
                boxSize,
                0f,
                direccion,
                1f
            );

            RaycastHit2D hitValido = new RaycastHit2D();
            bool encontro = false;

            foreach (var hit in hits)
            {
                if (hit.collider != propioCollider)
                {
                    hitValido = hit;
                    encontro = true;
                    break;
                }
            }

            if (encontro)
            {
                GameObject obj = hitValido.collider.gameObject;

                // 🔴 PARED
                if (((1 << obj.layer) & paredesLayer) != 0)
                {
                    if (rebotes < 1)
                    {
                        Vector2 reboteDir = ObtenerRebote(direccion);

                        if (reboteDir != Vector2.zero)
                        {
                            direccion = reboteDir;
                            rebotes++;
                            continue;
                        }
                    }

                    Destruir();
                    break;
                }

                // 🪑 BLOQUE ROMPIBLE
                if (((1 << obj.layer) & bloqueRompibleLayer) != 0)
                {
                    Destroy(obj);

                    FindObjectOfType<UIManager>().SumarPuntos(50); // ⭐ PUNTOS

                    Destruir();
                    break;
                }

                // 🔵 BLOQUE MOVIL
                if (((1 << obj.layer) & bloquesLayer) != 0)
                {
                    BloquesMoviles otro = obj.GetComponent<BloquesMoviles>();

                    if (otro != null && !otro.isMoving)
                    {
                        otro.StartCoroutine(otro.Deslizar(direccion));
                    }

                    break;
                }

                // 👾 ENEMIGO
                MovimientoEnemigos enemigo = obj.GetComponent<MovimientoEnemigos>();

                if (enemigo != null)
                {
                    Destroy(enemigo.gameObject);

                    FindObjectOfType<UIManager>().SumarPuntos(100); // ⭐ PUNTOS

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
        }

        isMoving = false;
    }

    Vector2 ObtenerRebote(Vector2 direccion)
    {
        Vector2 derecha = new Vector2(direccion.y, -direccion.x);
        Vector2 izquierda = new Vector2(-direccion.y, direccion.x);

        Vector2 pos = transform.position;
        Vector2 boxSize = new Vector2(0.6f, 0.6f);

        if (!Physics2D.OverlapBox(pos + derecha, boxSize, 0f, paredesLayer))
            return derecha;

        if (!Physics2D.OverlapBox(pos + izquierda, boxSize, 0f, paredesLayer))
            return izquierda;

        return Vector2.zero;
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