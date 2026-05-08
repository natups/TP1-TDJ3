using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

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
    public TextMeshProUGUI textoInstrucciones;

    [Header("Gameplay")]
    public int score = 0;
    public float tiempoRestante = 120f;

    private bool timerActivo = true;
    private bool juegoTerminado = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f; 
        StartCoroutine(ParpadeaUnraf());

        if (panelFin != null)
            panelFin.SetActive(false);
    }

    void Update()
    {
        if (juegoTerminado)
        {
            if (Input.GetKeyDown(KeyCode.R))
                Reintentar();

            if (Input.GetKeyDown(KeyCode.Return))
                VolverAlMenu();

            return;
        }

        if (player != null)
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

            yield return new WaitForSecondsRealtime(0.6f);
        }
    }

    public void SumarPuntos(int puntos)
    {
        score += puntos;
    }

    // --- FUNCIÓN RESTAURADA PARA EL ITEM.CS ---
    public void AgregarTiempo(float segundos)
    {
        tiempoRestante += segundos;
    }

    public void Ganar()
    {
        if (juegoTerminado) return;
        FinalizarPartida("GANASTE");
    }

    public void Perder()
    {
        if (juegoTerminado) return;
        FinalizarPartida("PERDISTE");
    }

    private void FinalizarPartida(string mensaje)
    {
        juegoTerminado = true;
        timerActivo = false;

        if (panelFin != null)
        {
            panelFin.SetActive(true);
            textoResultado.text = mensaje;
            textoPuntajeFinal.text = "PUNTAJE: " + score.ToString("D6");
            textoInstrucciones.text = "R: Reintentar\nEnter: Volver al menú";
        }

        Time.timeScale = 0f; 
    }

    void Reintentar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuInicio"); 
    }
}