using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CubeSpawnManager : MonoBehaviour
{
    [Header("Spawn points")]
    public Transform[] spawnPoints;

    [Header("Prefabs")]
    public GameObject gunCubePrefab;
    public GameObject swordCubePrefab;

    [Header("Waves")]
    public Wave[] waves;
    private int currentWave = 0;
    private float waveTimer = 0f;
    private float totalTime = 0f;

    private float spawnTimer = 0f;
    private float nextSpawnInterval = 2f;

    [Header("UI Progress Bar")]
    public Image progressBarFill;
    public Image progressBarOutline;

    [Header("UI Text")]
    public TMP_Text textTime;
    public Color[] waveFillColors = new Color[] {
        new Color(0.2f, 0.8f, 0.2f), // green
        new Color(0.2f, 0.6f, 1f),   // blue
        new Color(1f, 0.6f, 0.2f),   // orange
        new Color(1f, 0.2f, 0.2f),   // red
        new Color(0.7f, 0.2f, 1f)    // purple
    };

    private float totalElapsed = 0f;
    private const float MAX_TOTAL_SECONDS = 300f;

    private List<GameObject> activeCubes = new List<GameObject>();

    void Start()
    {
        if (waves == null || waves.Length == 0)
        {
            waves = new Wave[5];

            waves[0] = new Wave
            {
                waveName = "Wave 1 - Intro",
                duration = 60f,
                minSpawnInterval = 3.0f,
                maxSpawnInterval = 4.0f,
                allowGun = true,
                allowSword = false,
                maxSimultaneous = 3,
                maxSpawnPerTick = 1,
                speedMultiplier = 0.8f,
                introduceSpecialCubes = false
            };
            waves[1] = new Wave
            {
                waveName = "Wave 2 - Combo",
                duration = 60f,
                minSpawnInterval = 2.0f,
                maxSpawnInterval = 3.0f,
                allowGun = true,
                allowSword = true,
                maxSimultaneous = 6,
                maxSpawnPerTick = 2,
                speedMultiplier = 1.0f,
                introduceSpecialCubes = false
            };
            waves[2] = new Wave
            {
                waveName = "Wave 3 - Faster + Specials",
                duration = 60f,
                minSpawnInterval = 1.2f,
                maxSpawnInterval = 2.0f,
                allowGun = true,
                allowSword = true,
                maxSimultaneous = 8,
                maxSpawnPerTick = 2,
                speedMultiplier = 1.25f,
                introduceSpecialCubes = true
            };
            waves[3] = new Wave
            {
                waveName = "Wave 4 - Intense",
                duration = 60f,
                minSpawnInterval = 0.9f,
                maxSpawnInterval = 1.6f,
                allowGun = true,
                allowSword = true,
                maxSimultaneous = 10,
                maxSpawnPerTick = 3,
                speedMultiplier = 1.5f,
                introduceSpecialCubes = true
            };
            waves[4] = new Wave
            {
                waveName = "Wave 5 - Finale",
                duration = 60f,
                minSpawnInterval = 0.6f,
                maxSpawnInterval = 1.2f,
                allowGun = true,
                allowSword = true,
                maxSimultaneous = 12,
                maxSpawnPerTick = 3,
                speedMultiplier = 1.8f,
                introduceSpecialCubes = true
            };
        }

        nextSpawnInterval = Random.Range(waves[currentWave].minSpawnInterval, waves[currentWave].maxSpawnInterval);

        if (progressBarFill != null)
        {
            progressBarFill.type = Image.Type.Filled;
            progressBarFill.fillMethod = Image.FillMethod.Horizontal;
            progressBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            progressBarFill.fillAmount = 1f;
            progressBarFill.color = GetColorForWave(currentWave);
        }

        if (progressBarOutline != null)
            progressBarOutline.color = DarkenColor(GetColorForWave(currentWave), 0.5f);

        if (textTime != null)
            textTime.text = "00:00:00";
    }

    void Update()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        Wave w = waves[currentWave];

        spawnTimer += Time.deltaTime;
        waveTimer += Time.deltaTime;

        if (totalElapsed < MAX_TOTAL_SECONDS)
            totalElapsed += Time.deltaTime;

        UpdateElapsedText(totalElapsed);

        float remaining = Mathf.Clamp01(1f - (waveTimer / Mathf.Max(0.0001f, w.duration)));
        UpdateWaveProgressUI(remaining);

        if (spawnTimer >= nextSpawnInterval)
        {
            spawnTimer = 0f;
            nextSpawnInterval = Random.Range(w.minSpawnInterval, w.maxSpawnInterval);

            int allowedToSpawn = Mathf.Max(0, w.maxSimultaneous - activeCubes.Count);
            int spawnThisTick = Mathf.Min(w.maxSpawnPerTick, allowedToSpawn);

            for (int i = 0; i < spawnThisTick; i++)
            {
                SpawnOne(w);
            }
        }

        if (waveTimer >= w.duration)
        {
            currentWave++;
            if (currentWave >= waves.Length)
                currentWave = waves.Length - 1;

            waveTimer = 0f;
            Wave nw = waves[currentWave];
            nextSpawnInterval = Random.Range(nw.minSpawnInterval, nw.maxSpawnInterval);

            Color newFillColor = GetColorForWave(currentWave);
            if (progressBarFill != null)
            {
                progressBarFill.color = newFillColor;
                progressBarFill.fillAmount = 1f;
            }
            if (progressBarOutline != null)
                progressBarOutline.color = DarkenColor(newFillColor, 0.5f);
        }
    }

    private void SpawnOne(Wave w)
    {
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];

        List<GameObject> candidates = new List<GameObject>();
        if (w.allowGun && gunCubePrefab != null) candidates.Add(gunCubePrefab);
        if (w.allowSword && swordCubePrefab != null) candidates.Add(swordCubePrefab);

        if (candidates.Count == 0) return;

        GameObject prefab = candidates[Random.Range(0, candidates.Count)];
        GameObject inst = Instantiate(prefab, sp.position, sp.rotation);

        activeCubes.Add(inst);

        CubePhysics cp = inst.GetComponent<CubePhysics>();
        if (cp != null)
        {
            cp.SetSpeedMultiplier(w.speedMultiplier);
            cp.cubeSpawnManager = this;
        }
    }

    public void NotifyCubeDestroyed(GameObject cube)
    {
        if (cube == null) return;
        activeCubes.Remove(cube);
    }

    public int GetActiveCubeCount()
    {
        return activeCubes.Count;
    }

    private void UpdateWaveProgressUI(float normalizedRemaining)
    {
        if (progressBarFill != null)
            progressBarFill.fillAmount = Mathf.Clamp01(normalizedRemaining);
    }

private void UpdateElapsedText(float elapsedSeconds)
{
    if (textTime == null) return;

    float clampedSeconds = Mathf.Clamp(elapsedSeconds, 0f, MAX_TOTAL_SECONDS);

    int minutes = (int)(clampedSeconds / 60f);
    int seconds = (int)(clampedSeconds % 60f);

    float fractional = clampedSeconds - Mathf.Floor(clampedSeconds);
    int milliseconds = Mathf.FloorToInt(fractional * 1000f);

    textTime.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
}

    private Color GetColorForWave(int waveIndex)
    {
        if (waveFillColors == null || waveFillColors.Length == 0)
            return Color.white;
        return waveFillColors[waveIndex % waveFillColors.Length];
    }

    private Color DarkenColor(Color c, float factor)
    {
        factor = Mathf.Clamp01(factor);
        return new Color(c.r * (1f - factor), c.g * (1f - factor), c.b * (1f - factor), c.a);
    }
}

public class Wave
{
    public string waveName = "Wave";
    public float duration = 20f;
    public float minSpawnInterval = 1.0f;
    public float maxSpawnInterval = 3.0f;
    public bool allowGun = true;
    public bool allowSword = false;
    public int maxSimultaneous = 5;
    public int maxSpawnPerTick = 1;
    public float speedMultiplier = 1.0f;
    public bool introduceSpecialCubes = false;
}