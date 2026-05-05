using UnityEngine;

public class BloqueDiamante : MonoBehaviour
{
    public int golpesParaRomper = 5;
    private int golpesActuales = 0;

    public float tileSize = 1f;
    private UIManager ui;

    public Sprite[] spritesGolpe;
    private SpriteRenderer sr;

    public GameObject prefabReloj;
    public GameObject prefabBolaNieve;

    void Start()
    {
        ui = FindAnyObjectByType<UIManager>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void RecibirImpacto(Vector2 direccion, GameObject bloqueQueGolpea)
    {
        Vector2 nuevaPos = (Vector2)transform.position + direccion * tileSize;
        bool puedeMoverse = !Physics2D.OverlapPoint(nuevaPos);

        if (puedeMoverse)
            transform.position = nuevaPos;

        if (bloqueQueGolpea != null)
            Destroy(bloqueQueGolpea);

        golpesActuales++;
        ActualizarSprite();
        StartCoroutine(Golpecito());

        if (golpesActuales >= golpesParaRomper)
            Romper();
    }

    void ActualizarSprite()
    {
        int[] framesPorGolpe = { 0, 1, 1, 2, 3 };
        int index = Mathf.Clamp(golpesActuales - 1, 0, framesPorGolpe.Length - 1);
        int frame = framesPorGolpe[index];

        if (spritesGolpe != null && frame < spritesGolpe.Length)
            sr.sprite = spritesGolpe[frame];
    }

    void Romper()
    {
        StartCoroutine(MostrarRotura());
    }

    System.Collections.IEnumerator MostrarRotura()
    {
        if (spritesGolpe != null && spritesGolpe.Length > 3)
            sr.sprite = spritesGolpe[3];

        yield return new WaitForSeconds(0.2f);

        int rand = Random.Range(0, 3);
        Vector3 pos = transform.position;

        if (rand == 0)
        {
            if (ui != null) ui.SumarPuntos(200);
            MostrarTextoPuntos(pos);
        }
        else if (rand == 1)
        {
            if (prefabBolaNieve != null)
                Instantiate(prefabBolaNieve, pos, Quaternion.identity);
        }
        else
        {
            if (prefabReloj != null)
                Instantiate(prefabReloj, pos, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    void MostrarTextoPuntos(Vector3 pos)
    {
        GameObject textObj = new GameObject("TextoPuntos");
        textObj.transform.position = pos;

        TMPro.TextMeshPro tmp = textObj.AddComponent<TMPro.TextMeshPro>();
        tmp.text = "+200";
        tmp.fontSize = 4;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.yellow;
        tmp.sortingOrder = 10;

        // ← el objeto se anima solo, no depende del diamante
        textObj.AddComponent<TextoFlotante>();
    }

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