using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public MovimientoJugador player;
    public TextMeshProUGUI vidaText;

    void Update()
    {
        vidaText.text = "Vidas: " + player.vidas;
    }
}