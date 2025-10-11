using UnityEngine;

public class CubePhysics : MonoBehaviour
{
    [Header("Movement")]
    public float baseSpeed = 2.5f;

    public float speedMultiplier = 1.0f;
    public CubeSpawnManager cubeSpawnManager;
    Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        Vector3 desiredVelocity = transform.forward * baseSpeed * speedMultiplier;
        rb.velocity = desiredVelocity;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CubeKiller"))
        {
            // Notificar manager i destruir
            if (cubeSpawnManager != null) cubeSpawnManager.NotifyCubeDestroyed(gameObject);
            Destroy(gameObject);
            return;
        }
        if (other.CompareTag("Bullet"))
        {
            // destruir la bala i reproduir l'explosió
            Destroy(other.gameObject);

            CubeExplode ce = GetComponent<CubeExplode>();
            if (ce != null)
            {
                ce.Explode();
            }
            else
            {
                // si no hi ha CubeExplode, fem una destrucció bàsica
                if (cubeSpawnManager != null) cubeSpawnManager.NotifyCubeDestroyed(gameObject);
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Bullet"))
        {
            Destroy(collision.collider.gameObject);
            CubeExplode ce = GetComponent<CubeExplode>();
            if (ce != null) ce.Explode();
            else
            {
                if (cubeSpawnManager != null) cubeSpawnManager.NotifyCubeDestroyed(gameObject);
                Destroy(gameObject);
            }
        }
    }

    public void SetSpeedMultiplier(float m)
    {
        speedMultiplier = m;
    }

    void OnDestroy()
    {
        if (cubeSpawnManager != null)
            cubeSpawnManager.NotifyCubeDestroyed(gameObject);
    }
}
