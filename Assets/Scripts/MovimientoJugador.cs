using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class MovimientoJugador : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2 movement;
    private Animator animator;

    public float interactDistance = 1.5f;
    public LayerMask bloqueLayer;
    public LayerMask diamanteLayer;
    public int vidas = 3;

    public Tilemap tilemap;

    public GameObject bloquePrefab;
    public float cooldownColocar = 2f;
    private float timerColocar = 0f;
    public LayerMask todasLasCapas;

    private Vector2 lastDirection = Vector2.down;
    private bool congelado = false;
    private bool esInvencible = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void RecibirDanio()
    {
        if (esInvencible || vidas <= 0) return;

        vidas--;

        if (vidas <= 0)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.Perder();
        }
        else
        {
            StartCoroutine(Parpadear());
        }
    }

    IEnumerator Parpadear()
    {
        esInvencible = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Parpadeo más rápido para evitar bugs en la Build (1.2 segundos total)
        for (int i = 0; i < 6; i++)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.2f);
        }

        if (sr != null) sr.enabled = true;
        esInvencible = false;
    }

    void Update()
    {
        if (congelado || Time.timeScale == 0f) return;

        timerColocar -= Time.deltaTime;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.x != 0) movement.y = 0;

        animator.SetFloat("MoveX", movement.x);
        animator.SetFloat("MoveY", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

        if (movement != Vector2.zero)
        {
            lastDirection = movement.normalized;
            animator.SetFloat("LastX", lastDirection.x);
            animator.SetFloat("LastY", lastDirection.y);
        }

        if (Input.GetKeyDown(KeyCode.J))
            IntentarEmpujar();

        if (Input.GetKeyDown(KeyCode.K))
            IntentarColocarBloque();
    }

    void FixedUpdate()
    {
        if (congelado || Time.timeScale == 0f) return;
        transform.position += (Vector3)movement * moveSpeed * Time.fixedDeltaTime;
    }

    void IntentarEmpujar()
    {
        Vector2 direccion = lastDirection;
        int combinado = bloqueLayer | diamanteLayer;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direccion, interactDistance, combinado);

        if (hit.collider == null) return;

        BloquesMoviles bloque = hit.collider.GetComponent<BloquesMoviles>();
        if (bloque != null)
        {
            bloque.Empujar(direccion);
            return;
        }

        BloqueDiamante diamante = hit.collider.GetComponent<BloqueDiamante>();
        if (diamante != null)
        {
            diamante.RecibirImpacto(direccion, null);
            return;
        }
    }

    void IntentarColocarBloque()
    {
        if (timerColocar > 0) return;

        Vector3Int cellPos = tilemap.WorldToCell(transform.position);
        Vector3 posCentrada = tilemap.GetCellCenterWorld(cellPos);

        if (Physics2D.OverlapPoint(posCentrada, todasLasCapas)) return;

        GameObject nuevo = Instantiate(bloquePrefab, posCentrada, Quaternion.identity);
        StartCoroutine(AnimacionAparicion(nuevo));
        timerColocar = cooldownColocar;
    }

    IEnumerator AnimacionAparicion(GameObject nuevoBloque)
    {
        SpriteRenderer sr = nuevoBloque.GetComponent<SpriteRenderer>();
        Collider2D col = nuevoBloque.GetComponent<Collider2D>();
        col.enabled = false;

        float tiempoTotal = 1.5f; // Reducido para agilizar gameplay
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoTotal)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.15f);
            tiempoTranscurrido += 0.15f;
        }

        if (sr != null) sr.enabled = true;
        if (col != null) col.enabled = true;

        if (Vector2.Distance(transform.position, nuevoBloque.transform.position) < 0.5f)
            StartCoroutine(CongelarJugador(1.0f));
    }

    IEnumerator CongelarJugador(float duracion)
    {
        congelado = true;
        animator.SetFloat("Speed", 0f);
        yield return new WaitForSeconds(duracion);
        congelado = false;
    }
}