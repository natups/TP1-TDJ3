using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MenuInicio : MonoBehaviour
{
    public TextMeshProUGUI enterText; // ← asigná el texto "PRESIONA ENTER PARA JUGAR"

    void Start()
    {
        StartCoroutine(Parpadear());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            SceneManager.LoadScene("Pengo_N1");
    }

    IEnumerator Parpadear()
    {
        while (true)
        {
            enterText.enabled = !enterText.enabled;
            yield return new WaitForSeconds(0.6f);
        }
    }
}