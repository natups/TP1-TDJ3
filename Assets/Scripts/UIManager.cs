using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public MovimientoJugador player;
    public TextMeshProUGUI vidaText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public int score = 0;

    public float tiempoRestante = 120f; // 2 minutos, ajustalo como quieras
    private bool timerActivo = true;

    void Update()
    {
        vidaText.text = "Vidas: " + player.vidas;
        scoreText.text = "SCORE: " + score;

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