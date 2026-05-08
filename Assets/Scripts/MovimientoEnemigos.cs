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
    public static int enemigosVivos = 0;
    private static bool appQuitting = false;
    
    private bool estaAturdido = false;
    private float stunTimer = 0f;
    private bool yaMurio = false; 

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

    void OnApplicationQuit() { appQuitting = true; }

    // Esta corrutina maneja la muerte visual y lógica
  IEnumerator Morir()
    {
        if (yaMurio) yield break;
        yaMurio = true;

        SpawnManager sm = FindAnyObjectByType<SpawnManager>();
        if (sm != null)
            sm.EnemigoMurio();

        enemigosVivos--;
        todos.Remove(this);
        isMoving = false;

        // usar SetInteger en vez de trigger
        if (animator != null)
            animator.SetInteger("Direccion", 4);

        yield return new WaitForSeconds(1.5f);

        if (enemigosVivos <= 0 && (sm == null || !sm.HayEnemigosPendientes()))
            if (UIManager.Instance != null)
                UIManager.Instance.Ganar();

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (appQuitting) return;
        
        // Seguro por si el objeto se destruye por otra causa ajena a Morir()
        if (!yaMurio)
        {
            enemigosVivos--;
            todos.Remove(this);
        }
    }

    IEnumerator MoverEnGrilla()
    {
        while (true)
        {
            if (estaAturdido)
            {
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0) estaAturdido = false;
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

    void AlinearAGrilla()
    {
        Vector3Int cellPos = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.GetCellCenterWorld(cellPos);
    }

    void ActualizarAnimacion(Vector2 dir)
    {
        if (animator == null) return;
        if (dir == Vector2.down)       animator.SetInteger("Direccion", 0);
        else if (dir == Vector2.up)    animator.SetInteger("Direccion", 1);
        else if (dir == Vector2.right) animator.SetInteger("Direccion", 2);
        else if (dir == Vector2.left)  animator.SetInteger("Direccion", 3);
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
        if (rand == 0) direction = Vector2.up;
        else if (rand == 1) direction = Vector2.down;
        else if (rand == 2) direction = Vector2.left;
        else direction = Vector2.right;
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
    public void MorirPorBloque()
    {
        if (yaMurio) return;
        StopAllCoroutines(); // ← frena el movimiento
        StartCoroutine(Morir()); // ← Morir() se encarga de setear yaMurio
    }
}