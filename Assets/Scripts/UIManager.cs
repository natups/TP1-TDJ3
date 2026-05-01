using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public MovimientoJugador player;
    public TextMeshProUGUI vidaText;
    public TextMeshProUGUI scoreText;
    public int score = 0;

    void Update()
    {
        vidaText.text = "Vidas: " + player.vidas;
        scoreText.text = "SCORE: " + score;
    }

    // 🔥 FUNCIÓN PARA SUMAR PUNTOS
    public void SumarPuntos(int puntos)
    {
        score += puntos;
    }
}