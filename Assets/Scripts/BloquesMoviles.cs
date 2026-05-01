using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BloquesMoviles : MonoBehaviour
{
    public LayerMask bloquesLayer;
    public LayerMask paredesLayer;
    public LayerMask bloqueRompibleLayer;

    public float slideSpeed = 18f;
    public float respawnTime = 8f;

    private Vector3 initialPosition;

    public bool isMoving = false;

    void Start()
    {
        initialPosition = transform.position;
    }

    // =========================
    // EMPUJAR
    // =========================
    public void Empujar(Vector2 direccion)
    {
        if (isMoving) return;

        direccion = direccion.normalized;

        List<BloquesMoviles> cadena = ObtenerCadena(direccion);

        if (cadena == null || cadena.Count == 0) return;

        BloquesMoviles ultimo = cadena[cadena.Count - 1];

        Vector2 nextPos = (Vector2)ultimo.transform.position + direccion;

        // 🔍 DETECCIÓN REAL (sin grid falso)
        Collider2D hitBloque = Physics2D.OverlapBox(nextPos, new Vector2(0.6f, 0.6f), 0f, bloquesLayer);
        Collider2D hitPared = Physics2D.OverlapBox(nextPos, new Vector2(0.6f, 0.6f), 0f, paredesLayer);

        // 🚫 BLOQUE O PARED → SOLO REBOTE
        if (hitBloque != null || hitPared != null)
        {
            StartCoroutine(ReboteEnCadena(cadena, direccion));
            return;
        }

        // ✅ LIBRE → REBOTE + ÚLTIMO SE DESLIZA
        StartCoroutine(EmpujarConFeedback(cadena, direccion));
    }

    // =========================
    // OBTENER CADENA
    // =========================
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

    // =========================
    // EMPUJE CON GAME FEEL
    // =========================
    IEnumerator EmpujarConFeedback(List<BloquesMoviles> cadena, Vector2 direccion)
    {
        // 🔹 todos hacen rebote
        foreach (var bloque in cadena)
        {
            bloque.StartCoroutine(bloque.PequenoEmpujon(direccion));
        }

        // 🔹 pequeño delay para que se vea natural
        yield return new WaitForSeconds(0.05f);

        // 🔹 SOLO el último se desliza
        BloquesMoviles ultimo = cadena[cadena.Count - 1];
        ultimo.StartCoroutine(ultimo.Deslizar(direccion));
    }

    // =========================
    // REBOTE EN CADENA (cuando está bloqueado)
    // =========================
    IEnumerator ReboteEnCadena(List<BloquesMoviles> cadena, Vector2 direccion)
    {
        foreach (var bloque in cadena)
        {
            bloque.StartCoroutine(bloque.PequenoEmpujon(direccion));
        }

        yield return null;
    }

    // =========================
    // PEQUEÑO EMPUJÓN (feedback)
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
    // DESLIZAR 
    // =========================
    public IEnumerator Deslizar(Vector2 direccion)
    {
        isMoving = true;

        Vector2 boxSize = new Vector2(0.6f, 0.6f);

        while (true)
        {
            Vector2 nextPos = (Vector2)transform.position + direccion;

            // 1. PARED → destruir bloque
            if (Physics2D.OverlapBox(nextPos, boxSize, 0f, paredesLayer))
            {
                Destruir();
                break;
            }

            // 2. BLOQUE ROMPIBLE → destruir ambos
            Collider2D rompible = Physics2D.OverlapBox(nextPos, boxSize, 0f, bloqueRompibleLayer);
            if (rompible != null)
            {
                Destroy(rompible.gameObject); // rompe silla 2
                Destruir(); // se destruye silla 1
                break;
            }

            // 3. BLOQUE MOVIL → frena (NO atraviesa)
            if (Physics2D.OverlapBox(nextPos, boxSize, 0f, bloquesLayer))
            {
                break;
            }

            // 4. ENEMIGO → muere + bloque se destruye
            Collider2D hit = Physics2D.OverlapBox(nextPos, boxSize, 0f);
            if (hit != null)
            {
                MovimientoEnemigos enemigo = hit.GetComponent<MovimientoEnemigos>();

                if (enemigo != null)
                {
                    Destroy(enemigo.gameObject);
                    Destruir();
                    break;
                }
            }

            // 5. MOVER NORMAL
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
    public void Destruir()
    {
        StartCoroutine(Respawn());
    }

    // =========================
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