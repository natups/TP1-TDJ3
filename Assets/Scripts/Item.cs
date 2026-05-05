using UnityEngine;

public class Item : MonoBehaviour
{
    public enum TipoItem { Stun, TiempoExtra }
    public TipoItem tipo;

    public float tiempoStun = 8f;
    public float tiempoExtra = 20f;

    private UIManager ui;

    void Start()
    {
        ui = FindAnyObjectByType<UIManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<MovimientoJugador>() == null) return;

        if (tipo == TipoItem.Stun)
        {
            MovimientoEnemigos.StunAll(tiempoStun);
        }
        else if (tipo == TipoItem.TiempoExtra)
        {
            if (ui != null) ui.AgregarTiempo(tiempoExtra);
        }

        Destroy(gameObject);
    }
}