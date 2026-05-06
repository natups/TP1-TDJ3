using UnityEngine;
using System.Collections;

public class SpawnEnemigos : MonoBehaviour
{
    public GameObject enemigoPrefab;
    public float tiempoRespawn = 5f;
    public int maxEnemigos = 6;

    private int enemigosSpawneados = 0;

    void OnEnable()
    {
        // suscribirse al evento de muerte del enemigo
    }

    public void EnemigoMurio()
    {
        if (enemigosSpawneados < maxEnemigos)
            StartCoroutine(RespawnEnemigo());
    }

    IEnumerator RespawnEnemigo()
    {
        yield return new WaitForSeconds(tiempoRespawn);

        if (enemigosSpawneados >= maxEnemigos) yield break;

        enemigosSpawneados++;

        // efecto de parpadeo antes de aparecer
        GameObject nuevoEnemigo = Instantiate(enemigoPrefab, transform.position, Quaternion.identity);
        SpriteRenderer sr = nuevoEnemigo.GetComponent<SpriteRenderer>();
        Collider2D col = nuevoEnemigo.GetComponent<Collider2D>();

        col.enabled = false;
        for (int i = 0; i < 6; i++)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.2f);
        }
        sr.enabled = true;
        col.enabled = true;
    }
}