using UnityEngine;
using UnityEngine.UI;

public class SpecialCube : MonoBehaviour
{
    public int specialType = 0;
    public float effectDuration = 6f;

    private SpriteRenderer iconRenderer;
    private Image iconImage;
    private WaveManager waveManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Transform iconTf = transform.Find("Icon");
        if (iconTf != null)
        {
            iconRenderer = iconTf.GetComponent<SpriteRenderer>();
        }
    }

    void Awake()
    {
        FindIconComponents();
    }
    // Update is called once per frame
    void Update()
    {

    }

    private void FindIconComponents()
    {
        Transform iconTf = transform.Find("Icon");
        if (iconTf != null)
        {
            iconRenderer = iconTf.GetComponent<SpriteRenderer>();
            iconImage = iconTf.GetComponent<Image>();
        }
    }

    public void ConfigureSpecial(int type, Sprite iconSprite, WaveManager wm)
    {
        specialType = type;
        waveManager = wm;

        // set durations per type (keeps original values)
        if (specialType == 0) // slow
            effectDuration = 6f;
        else if (specialType == 1) // fast
            effectDuration = 6f;
        else // swap positions
            effectDuration = 5f;

        // Ensure we have references to the icon renderer/image
        if (iconRenderer == null && iconImage == null)
            FindIconComponents();

        // Hide whichever isn't used and show the one that is
        if (iconSprite != null)
        {
            if (iconRenderer != null)
            {
                iconRenderer.sprite = iconSprite;
                iconRenderer.enabled = true;
                if (iconImage != null) iconImage.enabled = false;
            }
            else if (iconImage != null)
            {
                iconImage.sprite = iconSprite;
                iconImage.enabled = true;
                if (iconRenderer != null) iconRenderer.enabled = false;
            }
        }
        else
        {
            // No sprite provided: hide both
            if (iconRenderer != null) iconRenderer.enabled = false;
            if (iconImage != null) iconImage.enabled = false;
        }
    }

    public void ClearVisual()
    {
        if (iconRenderer == null && iconImage == null)
            FindIconComponents();

        if (iconRenderer != null) iconRenderer.enabled = false;
        if (iconImage != null) iconImage.enabled = false;
    }

    public void Activate()
    {
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
            if (waveManager == null) return;
        }

        if (specialType == 0) // slow
        {
            waveManager.ApplyGlobalSpeedMultiplier(0.6f, effectDuration);
        }
        else if (specialType == 1) // fast
        {
            waveManager.ApplyGlobalSpeedMultiplier(1.6f, effectDuration);
        }
        else if (specialType == 2) // swap sides
        {
            waveManager.TriggerSwapSides(effectDuration);
        }

        Destroy(gameObject);
    }
}
