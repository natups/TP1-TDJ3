using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemigoPrefab;
    public float tiempoRespawn = 5f;
    public int maxEnemigos = 6;
    public Transform[] puntosDeSpawn;

    public void EnemigoMurio()
    {
        if (MovimientoEnemigos.enemigosVivos < maxEnemigos)
            StartCoroutine(RespawnEnemigo());
    }

    IEnumerator RespawnEnemigo()
    {
        yield return new WaitForSeconds(tiempoRespawn);

        if (MovimientoEnemigos.enemigosVivos >= maxEnemigos) yield break;

        Transform punto = puntosDeSpawn[Random.Range(0, puntosDeSpawn.Length)];
        GameObject nuevoEnemigo = Instantiate(enemigoPrefab, punto.position, Quaternion.identity);

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