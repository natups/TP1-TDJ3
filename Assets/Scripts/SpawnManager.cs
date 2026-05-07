using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemigoPrefab;
    public float tiempoRespawn = 5f;
    public int maxEnemigosEnPantalla = 6; 
    public int totalEnemigosPorNivel = 6; 
    public Transform[] puntosDeSpawn;

    [Header("Seguimiento")]
    public int enemigosYaGenerados = 3; 
    private int enemigosEnCamino = 0;

    public void EnemigoMurio()
    {
        Debug.Log("EnemigoMurio llamado! YaGenerados: " + enemigosYaGenerados + " Total: " + totalEnemigosPorNivel);
        
        if (enemigosYaGenerados < totalEnemigosPorNivel)
        {
            enemigosYaGenerados++;
            StartCoroutine(RespawnEnemigo());
        }
    }
    public bool HayEnemigosPendientes()
    {
        return enemigosEnCamino > 0 || enemigosYaGenerados < totalEnemigosPorNivel;
    }

    IEnumerator RespawnEnemigo()
    {
        enemigosEnCamino++;
        
        yield return new WaitForSeconds(tiempoRespawn);

        // Elegimos un punto al azar de la lista
        Transform punto = puntosDeSpawn[Random.Range(0, puntosDeSpawn.Length)];
        GameObject nuevoEnemigo = Instantiate(enemigoPrefab, punto.position, Quaternion.identity);

        enemigosEnCamino--;

        // --- Lógica visual de parpadeo al aparecer ---
        SpriteRenderer sr = nuevoEnemigo.GetComponent<SpriteRenderer>();
        Collider2D col = nuevoEnemigo.GetComponent<Collider2D>();
        
        if (col != null) col.enabled = false;
        
        for (int i = 0; i < 6; i++)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.2f);
        }
        
        if (sr != null) sr.enabled = true;
        if (col != null) col.enabled = true;
    }
}