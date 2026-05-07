using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class MovimientoEnemigos : MonoBehaviour
{
    public float speed = 2f;
    public LayerMask obstaculosLayer;
    public LayerMask bloquesLayer;
    public Tilemap tilemap;

    private Vector2 direction;
    public bool isMoving = false;
    private Animator animator;

    private static List<MovimientoEnemigos> todos = new List<MovimientoEnemigos>();
    private bool estaAturdido = false;
    private float stunTimer = 0f;
    public static int enemigosVivos = 0;

    void Start()
    {
        enemigosVivos++;
        todos.Add(this);
        animator = GetComponent<Animator>();

        if (tilemap == null)
            tilemap = GameObject.Find("Suelo_Tilemap").GetComponent<Tilemap>();

        AlinearAGrilla();
        ChooseNewDirection();
        StartCoroutine(MoverEnGrilla());
    }

    void OnDestroy()
    {
        enemigosVivos--;
        todos.Remove(this);

        SpawnManager sm = FindAnyObjectByType<SpawnManager>();
        if (sm != null)
            sm.EnemigoMurio();

        if (enemigosVivos <= 0)
        {
            Debug.Log("GANASTE");
            Time.timeScale = 0f;
        }
    }

    void AlinearAGrilla()
    {
        Vector3Int cellPos = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.GetCellCenterWorld(cellPos);
    }

    void ActualizarAnimacion(Vector2 dir)
    {
        if (dir == Vector2.down)       animator.SetInteger("Direccion", 0);
        else if (dir == Vector2.up)    animator.SetInteger("Direccion", 1);
        else if (dir == Vector2.right) animator.SetInteger("Direccion", 2);
        else if (dir == Vector2.left)  animator.SetInteger("Direccion", 3);
    }

    IEnumerator MoverEnGrilla()
    {
        while (true)
        {
            if (estaAturdido)
            {
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0)
                    estaAturdido = false;

                yield return null;
                continue;
            }

            Vector2 nextPos = (Vector2)transform.position + direction;
            int mask = obstaculosLayer | bloquesLayer;

            if (Physics2D.OverlapPoint(nextPos, mask))
            {
                ChooseNewDirection();
                ActualizarAnimacion(direction);
                yield return null;
                continue;
            }

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
                ActualizarAnimacion(direction);
                yield return null;
                continue;
            }

            isMoving = true;
            ActualizarAnimacion(direction);

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

            if (Random.value < 0.1f)
            {
                ChooseNewDirection();
                ActualizarAnimacion(direction);
            }
        }
    }

    IEnumerator Morir()
    {
        if (animator != null)
            animator.SetTrigger("Morir");

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        MovimientoJugador player = other.GetComponent<MovimientoJugador>();
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