using UnityEngine;

public class SpecialCube : MonoBehaviour
{
     public int specialType = 0;
    public float effectDuration = 6f;

    private SpriteRenderer iconRenderer;
    private WaveManager waveManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Transform iconTf = transform.Find("Icon");
        if (iconTf != null)
            iconRenderer = iconTf.GetComponent<SpriteRenderer>();        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void ConfigureSpecial(int type, Sprite iconSprite, WaveManager wm)
    {
        specialType = type;
        waveManager = wm;

        if (specialType == 0) // frenar
            effectDuration = 6f;
        else if (specialType == 1) // accelerar
            effectDuration = 6f;
        else // swap posicions
            effectDuration = 5f;

        if (iconSprite != null)
        {
            if (iconRenderer == null)
            {
                Transform iconTf = transform.Find("Icon");
                if (iconTf != null) iconRenderer = iconTf.GetComponent<SpriteRenderer>();
            }
            if (iconRenderer != null)
            {
                iconRenderer.sprite = iconSprite;
                iconRenderer.enabled = true;
            }
        }
    }

    public void ClearVisual()
    {
        if (iconRenderer == null)
        {
            Transform iconTf = transform.Find("Icon");
            if (iconTf != null) iconRenderer = iconTf.GetComponent<SpriteRenderer>();
        }
        if (iconRenderer != null) iconRenderer.enabled = false;
    }

    public void Activate()
    {
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
            if (waveManager == null) return;
        }

        if (specialType == 0)
        {
            waveManager.ApplyGlobalSpeedMultiplier(0.6f, effectDuration);
        }
        else if (specialType == 1)
        {
            waveManager.ApplyGlobalSpeedMultiplier(1.6f, effectDuration);
        }
        else if (specialType == 2)
        {
            waveManager.TriggerSwapSides(effectDuration);
        }

        Destroy(gameObject);
    }
}
