using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public MovimientoJugador player;
    public TextMeshProUGUI vidaText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI unrafText; // ← el texto que parpadea
    public int score = 0;

    public float tiempoRestante = 120f;
    private bool timerActivo = true;

    void Start()
    {
        StartCoroutine(ParpadeaUnraf());
    }

    void Update()
    {
        vidaText.text = "x" + player.vidas;
        scoreText.text = score.ToString("D6"); // muestra 06300 estilo arcade

        if (timerActivo)
        {
            tiempoRestante -= Time.deltaTime;

            if (tiempoRestante <= 0)
            {
                tiempoRestante = 0;
                timerActivo = false;
                GameOver();
            }

            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    IEnumerator ParpadeaUnraf()
    {
        while (true)
        {
            unrafText.enabled = !unrafText.enabled;
            yield return new WaitForSeconds(0.6f);
        }
    }

    public void SumarPuntos(int puntos)
    {
        score += puntos;
    }

    public void AgregarTiempo(float segundos)
    {
        tiempoRestante += segundos;
    }

    void GameOver()
    {
        Debug.Log("TIEMPO AGOTADO - GAME OVER");
        Time.timeScale = 0f;
    }
}