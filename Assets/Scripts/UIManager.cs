using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Referencias")]
    public MovimientoJugador player;

    public TextMeshProUGUI vidaText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI unrafText;

    [Header("Panel Final")]
    public GameObject panelFin;
    public TextMeshProUGUI textoResultado;
    public TextMeshProUGUI textoPuntajeFinal;

    [Header("Gameplay")]
    public int score = 0;
    public float tiempoRestante = 120f;

    private bool timerActivo = true;
    private bool juegoTerminado = false;

    void Awake()
    {
        // Singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(ParpadeaUnraf());

        if (panelFin != null)
            panelFin.SetActive(false);
    }

    void Update()
    {
        if (juegoTerminado) return;

        vidaText.text = "x" + player.vidas;
        scoreText.text = score.ToString("D6");

        if (timerActivo)
        {
            tiempoRestante -= Time.deltaTime;

            if (tiempoRestante <= 0)
            {
                tiempoRestante = 0;
                timerActivo = false;
                Perder();
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
            if (unrafText != null)
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

    // =========================
    // 🎯 FINAL DEL JUEGO
    // =========================

    public void Ganar()
    {
        if (juegoTerminado) return;

        juegoTerminado = true;
        timerActivo = false;

        if (panelFin != null) panelFin.SetActive(true);

        if (textoResultado != null)
            textoResultado.text = "GANASTE";

        if (textoPuntajeFinal != null)
            textoPuntajeFinal.text = "PUNTAJE: " + score.ToString("D6");

        Time.timeScale = 0f;
    }

    public void Perder()
    {
        if (juegoTerminado) return;

        juegoTerminado = true;
        timerActivo = false;

        if (panelFin != null) panelFin.SetActive(true);

        if (textoResultado != null)
            textoResultado.text = "PERDISTE";

        if (textoPuntajeFinal != null)
            textoPuntajeFinal.text = "PUNTAJE: " + score.ToString("D6");

        Time.timeScale = 0f;
    }
}