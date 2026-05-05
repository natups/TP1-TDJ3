using UnityEngine;
using TMPro;

public class TextoFlotante : MonoBehaviour
{
    private float duracion = 1f;
    private float t = 0f;
    private Vector3 posInicial;
    private TextMeshPro tmp;

    void Start()
    {
        posInicial = transform.position;
        tmp = GetComponent<TextMeshPro>();
    }

    void Update()
    {
        t += Time.deltaTime;
        transform.position = posInicial + Vector3.up * t * 1.5f;
        tmp.color = new Color(1f, 1f, 0f, 1f - (t / duracion));

        if (t >= duracion)
            Destroy(gameObject);
    }
}