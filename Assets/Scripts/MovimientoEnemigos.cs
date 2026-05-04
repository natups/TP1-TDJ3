using UnityEngine;
using System.Collections.Generic;

public class MovimientoEnemigos : MonoBehaviour
{
    public float speed = 0.5f;

    public LayerMask obstaculosLayer;
    public LayerMask bloquesLayer;

    private Vector2 direction;

    // 🔵 LISTA GLOBAL DE ENEMIGOS
    private static List<MovimientoEnemigos> todos = new List<MovimientoEnemigos>();

    // 🧊 STUN
    private bool estaAturdido = false;
    private float stunTimer = 0f;

    public static int enemigosVivos = 0;

    void Start()
    {
        enemigosVivos++;
        todos.Add(this);

        ChooseNewDirection();
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

    void Update()
    {
        // 🧊 STUN LOGIC
        if (estaAturdido)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0)
                estaAturdido = false;

            return; // no se mueve
        }

        Vector2 origin = transform.position;

        int mask = obstaculosLayer | bloquesLayer;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, 0.5f, mask);

        Debug.DrawRay(origin, direction * 0.5f, Color.red);

        if (hit.collider != null)
        {
            BloquesMoviles bloque = hit.collider.GetComponent<BloquesMoviles>();

            if (bloque != null)
            {
                if (bloque.isMoving)
                {
                    Destroy(gameObject);
                    bloque.Destruir();
                    return;
                }

                ChooseNewDirection();
                return;
            }

            ChooseNewDirection();
            return;
        }

        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        MovimientoJugador player = collision.gameObject.GetComponent<MovimientoJugador>();

        if (player != null)
        {
            player.RecibirDanio();
        }
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

    // 🧊 STUN INDIVIDUAL
    public void Stun(float duration)
    {
        estaAturdido = true;
        stunTimer = duration;
    }

    // ❄ STUN GLOBAL (llamado por el diamante)
    public static void StunAll(float duration)
    {
        foreach (var e in todos)
        {
            if (e != null)
                e.Stun(duration);
        }
    }
}