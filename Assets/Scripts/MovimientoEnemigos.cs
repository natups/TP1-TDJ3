using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class MovimientoEnemigos : MonoBehaviour
{
    public float speed = 4f;

    public LayerMask obstaculosLayer;
    public LayerMask bloquesLayer;
    public Tilemap tilemap; // ← asigná Suelo_Tilemap en el Inspector

    private Vector2 direction;
    private bool isMoving = false;

    private Animator animator;

    // 🔵 LISTA GLOBAL
    private static List<MovimientoEnemigos> todos = new List<MovimientoEnemigos>();

    // 🧊 STUN
    private bool estaAturdido = false;
    private float stunTimer = 0f;

    public static int enemigosVivos = 0;

    void Start()
    {
        enemigosVivos++;
        todos.Add(this);
        animator = GetComponent<Animator>();

        // alinear a la grilla al inicio
        AlinearAGrilla();
        ChooseNewDirection();
        StartCoroutine(MoverEnGrilla());
    }

    void AlinearAGrilla()
    {
        Vector3Int cellPos = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.GetCellCenterWorld(cellPos);
    }

    void OnDestroy()
    {
        enemigosVivos--;
        todos.Remove(this);

        if (enemigosVivos <= 0)
        {
            Debug.Log("GANASTE");
            Time.timeScale = 0f;
        }
    }

    IEnumerator MoverEnGrilla()
    {
        while (true)
        {
            // 🧊 STUN
            if (estaAturdido)
            {
                animator.SetFloat("MoveX", 0);
                animator.SetFloat("MoveY", 0);
                yield return null;
                continue;
            }

            Vector2 nextPos = (Vector2)transform.position + direction;
            int mask = obstaculosLayer | bloquesLayer;

            // chequear si hay obstáculo
            if (Physics2D.OverlapPoint(nextPos, mask))
            {
                ChooseNewDirection();
                yield return null;
                continue;
            }

            // chequear si hay bloque moviéndose
            Collider2D bloqueCol = Physics2D.OverlapPoint(nextPos, bloquesLayer);
            if (bloqueCol != null)
            {
                BloquesMoviles bloque = bloqueCol.GetComponent<BloquesMoviles>();
                if (bloque != null && bloque.isMoving)
                {
                    StartCoroutine(Morir());
                    yield break;
                }
                ChooseNewDirection();
                yield return null;
                continue;
            }

            // 🟢 MOVER tile por tile
            isMoving = true;
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);

            Vector3 start = transform.position;
            Vector3 end = start + (Vector3)direction;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime * speed;
                transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            transform.position = end;
            isMoving = false;

            // cambiar dirección aleatoriamente de vez en cuando
            if (Random.value < 0.3f)
                ChooseNewDirection();
        }
    }

    IEnumerator Morir()
    {
        if (animator != null)
            animator.SetTrigger("Morir");

        // esperar que termine la animación de muerte
        yield return new WaitForSeconds(0.5f);

        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        MovimientoJugador player = collision.gameObject.GetComponent<MovimientoJugador>();
        if (player != null)
            player.RecibirDanio();
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

    public void Stun(float duration)
    {
        estaAturdido = true;
        stunTimer = duration;
    }

    public static void StunAll(float duration)
    {
        foreach (var e in todos)
        {
            if (e != null)
                e.Stun(duration);
        }
    }
}