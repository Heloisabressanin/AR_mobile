using UnityEngine;

public class ObjectVivant : MonoBehaviour
{
    [Header("Références")]
    public VivantConfiguration configuration;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public Rigidbody rb;

    [Header("Layers")]
    public LayerMask layerSol;
    public LayerMask layerVivant;

    private Vector3 _target;
    private float _targetTimer;
    private float _jumpTimer;

    // --- NOUVEAU : référence de la nourriture trouvée ---
    private Transform _foodTarget;

    void Start()
    {
        float random = Random.value;

        // Taille aléatoire
        float randomSize = Mathf.Lerp(configuration.tailleRandom.x, configuration.tailleRandom.y, random);
        transform.localScale = Vector3.one * randomSize;

        // Masse aléatoire
        rb.mass = Mathf.Lerp(configuration.masseRandom.x, configuration.masseRandom.y, random);

        // Matériau aléatoire
        meshRenderer.sharedMaterial =
            configuration.materiauxRandom[
                Random.Range(0, configuration.materiauxRandom.Count)
            ];

        // IMPORTANT : éviter la convergence vers le centre
        _target = transform.position;
        _targetTimer = 0f;

        ResetJumpTimer();
    }

    void ResetJumpTimer()
    {
        _jumpTimer = Random.Range(configuration.tempsEntreSauts.x, configuration.tempsEntreSauts.y);
    }

    // ---------------------------------------------------------
    //  NOUVEAU : Détection de nourriture
    // ---------------------------------------------------------
    bool TryFindFood(out Vector3 pos)
    {
        foreach (var col in Physics.OverlapSphere(transform.position, configuration.rayonNourriture))
        {
            if (!col.CompareTag("Nourriture"))
                continue;

            // On stocke la référence du transform
            _foodTarget = col.transform;

            pos = col.transform.position;
            return true;
        }

        pos = Vector3.zero;
        return false;
    }

    bool TryPickTarget(out Vector3 t)
    {
        for (int i = 0; i < 20; i++)
        {
            float distanceX = Random.Range(configuration.rayonMouvement.x, configuration.rayonMouvement.y) * (Random.value < 0.5f ? -1f : 1f);
            float distanceZ = Random.Range(configuration.rayonMouvement.x, configuration.rayonMouvement.y) * (Random.value < 0.5f ? -1f : 1f);

            Vector3 p = transform.position + new Vector3(distanceX, 2f, distanceZ);

            // Détection du sol
            if (!Physics.Raycast(p, Vector3.down, out RaycastHit hit, 10f, layerSol))
                continue;

            // Détection d’un autre vivant
            if (Physics.SphereCast(p, 0.4f, Vector3.down, out var hit2, 10f, layerVivant))
                continue;

            t = hit.point;
            return true;
        }

        t = Vector3.zero;
        return false;
    }

    void Update()
    {
        // ---------------------------------------------------------
        //  PRIORITÉ : nourriture
        // ---------------------------------------------------------
        if (TryFindFood(out _target))
        {
            // Si on a une nourriture, on vérifie si on est assez proche pour la manger
            if (_foodTarget != null)
            {
                float dist = Vector3.Distance(transform.position, _foodTarget.position);

                if (dist <= configuration.distanceManger)
                {
                    // On détruit le parent du collider (important)
                    Destroy(_foodTarget.parent.gameObject);

                    _foodTarget = null;
                }
            }

            return; // On ne fait rien d'autre
        }

        // -------- Target --------
        _targetTimer -= Time.deltaTime;

        if (_targetTimer <= 0f)
        {
            if (TryPickTarget(out _target))
            {
                _targetTimer = Random.Range(configuration.tempsAttente.x, configuration.tempsAttente.y);
            }
            else
            {
                _targetTimer = 0.1f;
            }
        }

        // -------- Saut --------
        _jumpTimer -= Time.deltaTime;

        if (_jumpTimer <= 0f)
        {
            float force = Random.Range(configuration.forceSaut.x, configuration.forceSaut.y);

            rb.AddForce(Vector3.up * force, ForceMode.Acceleration);
            ResetJumpTimer();
        }
    }

    void FixedUpdate()
    {
        Vector3 direction = _target - transform.position;
        direction.y = 0f;

        if (direction.magnitude <= configuration.distanceArret)
            return;

        rb.AddForce(direction.normalized * configuration.acceleration, ForceMode.Acceleration);

        Vector3 vitesseHorizontale = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (vitesseHorizontale.magnitude > configuration.vitesseMax)
        {
            Vector3 vitesseLimitee = vitesseHorizontale.normalized * configuration.vitesseMax;

            rb.linearVelocity = new Vector3(
                vitesseLimitee.x,
                rb.linearVelocity.y,
                vitesseLimitee.z
            );
        }
    }
}