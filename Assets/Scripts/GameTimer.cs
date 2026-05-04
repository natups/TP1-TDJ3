using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public float timeLimit = 120f; // 2 minutos
    public TextMeshProUGUI timerText;

    private bool running = true;

    void Update()
    {
        if (!running) return;

        timeLimit -= Time.deltaTime;

        if (timeLimit <= 0)
        {
            timeLimit = 0;
            running = false;
            GameOver();
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(timeLimit / 60f);
        int seconds = Mathf.FloorToInt(timeLimit % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    void GameOver()
    {
        Debug.Log("TIEMPO AGOTADO - PERDISTE");
        Time.timeScale = 0f;
    }
}