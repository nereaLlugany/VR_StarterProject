using UnityEngine;

public class CubeExplode : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float explosionAutoDestroy = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Explode()
    {
        if (explosionPrefab != null)
        {
            GameObject ex = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(ex, explosionAutoDestroy);
        }

        Destroy(gameObject);
    }
}
    
