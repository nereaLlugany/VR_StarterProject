using UnityEngine;

public class CubePhysics : MonoBehaviour
{
    [Header("Movement")]
    public float baseSpeed = 2.5f;

    public float speedMultiplier = 1.0f;

    public CubeSpawnManager cubeSpawnManager;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning($"{name} no té Rigidbody - afegeix-ne un per moure el cub.");
        }
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        float globalMul = 1f;
        if (cubeSpawnManager != null)
        {
            var wmField = cubeSpawnManager.GetType().GetField("waveManager");
            WaveManager wm = null;
            try
            {
                wm = cubeSpawnManager.waveManager;
            }
            catch
            {
                wm = null;
            }

            if (wm != null)
            {
                globalMul = wm.GetGlobalSpeedMultiplier();
            }
        }

        Vector3 desiredVelocity = transform.forward * baseSpeed * speedMultiplier * globalMul;
        rb.linearVelocity = desiredVelocity;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CubeKiller"))
        {
            NotifyAndDestroy();
            return;
        }

        if (other.CompareTag("Player"))
        {
            SpecialCube sc = GetComponent<SpecialCube>();
            if (sc != null)
            {
                sc.Activate();
                return;
            }
        }

        if (other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);

            CubeExplode ce = GetComponent<CubeExplode>();
            if (ce != null)
            {
                ce.Explode();
            }
            else
            {
                NotifyAndDestroy();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Bullet"))
        {
            Destroy(collision.collider.gameObject);

            CubeExplode ce = GetComponent<CubeExplode>();
            if (ce != null)
                ce.Explode();
            else
                NotifyAndDestroy();
        }
    }

    public void SetSpeedMultiplier(float m)
    {
        speedMultiplier = m;
    }

    private void NotifyAndDestroy()
    {
        if (cubeSpawnManager != null)
        {
            cubeSpawnManager.NotifyCubeDestroyed(gameObject);
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (cubeSpawnManager != null)
        {
            cubeSpawnManager.NotifyCubeDestroyed(gameObject);
        }
    }
}
