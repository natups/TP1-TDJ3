using UnityEngine;

public class BloqueDiamante : MonoBehaviour
{
    public int golpesParaRomper = 3;
    private int golpesActuales = 0;

    public float tileSize = 1f;

    private UIManager ui;

    void Start()
    {
        ui = FindAnyObjectByType<UIManager>();
    }

    // 👉 Se llama cuando un BloquesMoviles lo impacta
    public void RecibirImpacto(Vector2 direccion, GameObject bloqueQueGolpea)
    {
        Vector2 nuevaPos = (Vector2)transform.position + direccion * tileSize;

        // 🔵 intenta moverse 1 tile si hay espacio
        bool puedeMoverse = !Physics2D.OverlapPoint(nuevaPos);

        if (puedeMoverse)
        {
            transform.position = nuevaPos;
        }

        // 💥 el bloque que lo empuja se destruye SIEMPRE
        if (bloqueQueGolpea != null)
        {
            Destroy(bloqueQueGolpea);
        }

        // 🔨 el diamante recibe daño
        golpesActuales++;

        StartCoroutine(Golpecito());

        if (golpesActuales >= golpesParaRomper)
        {
            Romper();
        }
    }

    void Romper()
    {
        if (ui != null)
            ui.SumarPuntos(200);

        Destroy(gameObject);
    }

    // 💥 feedback visual de impacto
    System.Collections.IEnumerator Golpecito()
    {
        Vector3 original = transform.position;

        float t = 0f;

        while (t < 0.1f)
        {
            t += Time.deltaTime;

            transform.position = original + (Vector3)Random.insideUnitCircle * 0.05f;

            yield return null;
        }

        transform.position = original;
    }
}