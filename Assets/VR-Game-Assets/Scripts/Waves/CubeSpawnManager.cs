using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CubeSpawnManager : MonoBehaviour
{
    [Header("Prefabs (només GunCube i SwordCube)")]
    public GameObject gunCubePrefab;
    public GameObject swordCubePrefab;

    [Header("References")]
    public WaveManager waveManager;

    [Header("UI Progress")]
    public Image progressBarFill;
    public Image progressBarOutline;
    public TMP_Text textTime;

    [Header("Wave colors (optional)")]
    public Color[] waveFillColors = new Color[] {
        new Color(0.2f, 0.8f, 0.2f), // green
        new Color(0.2f, 0.6f, 1f),   // blue
        new Color(1f, 0.6f, 0.2f),   // orange
        new Color(1f, 0.2f, 0.2f),   // red
        new Color(0.7f, 0.2f, 1f)    // purple
    };

    private float spawnTimer = 0f;
    private float nextSpawnInterval = 2f;

    private List<GameObject> activeCubes = new List<GameObject>();

    private float totalElapsed = 0f;
    private const float MAX_TOTAL_SECONDS = 300f;

    void Start()
    {
        if (waveManager == null)
        {
            Debug.LogError("CubeSpawnManager: waveManager no assignat a l'Inspector!");
            enabled = false;
            return;
        }

        Wave w = waveManager.GetCurrentWave();
        nextSpawnInterval = Random.Range(w.minSpawnInterval, w.maxSpawnInterval);

        if (progressBarFill != null)
        {
            progressBarFill.type = Image.Type.Filled;
            progressBarFill.fillMethod = Image.FillMethod.Horizontal;
            progressBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            progressBarFill.fillAmount = 1f;
            progressBarFill.color = GetColorForWave(waveManager.GetCurrentWaveIndex());
        }
        if (progressBarOutline != null)
            progressBarOutline.color = DarkenColor(GetColorForWave(waveManager.GetCurrentWaveIndex()), 0.5f);

        if (textTime != null) textTime.text = "00:00:000";
    }

    void Update()
    {
        if (waveManager == null) return;

        Wave w = waveManager.GetCurrentWave();

        spawnTimer += Time.deltaTime;
        if (totalElapsed < MAX_TOTAL_SECONDS) totalElapsed += Time.deltaTime;
        UpdateElapsedText(totalElapsed);

        float waveElapsed = waveManager.GetWaveElapsed();
        float waveDur = waveManager.GetWaveDuration();
        float remaining = waveDur > 0f ? Mathf.Clamp01(1f - (waveElapsed / waveDur)) : 0f;
        UpdateWaveProgressUI(remaining);

        if (progressBarFill != null)
        {
            progressBarFill.color = GetColorForWave(waveManager.GetCurrentWaveIndex());
        }
        if (progressBarOutline != null)
            progressBarOutline.color = DarkenColor(GetColorForWave(waveManager.GetCurrentWaveIndex()), 0.5f);

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
    }

    private void SpawnOne(Wave w)
    {
        bool makeSpecial = w.introduceSpecialCubes && Random.value <= w.specialChance;

        List<GameObject> candidates = new List<GameObject>();
        if (w.allowGun && gunCubePrefab != null) candidates.Add(gunCubePrefab);
        if (w.allowSword && swordCubePrefab != null) candidates.Add(swordCubePrefab);

        if (candidates.Count == 0) return;

        GameObject prefab = candidates[Random.Range(0, candidates.Count)];

        Transform spawnPoint = waveManager.ChooseSpawnPointForType(prefab == gunCubePrefab ? "gun" : "sword",
                                                                    prefab == gunCubePrefab ? w.gunSidePreference : w.swordSidePreference);

        Vector3 spawnPos = (spawnPoint != null) ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = (spawnPoint != null) ? spawnPoint.rotation : Quaternion.identity;

        GameObject inst = Instantiate(prefab, spawnPos, spawnRot);
        activeCubes.Add(inst);

        CubePhysics cp = inst.GetComponent<CubePhysics>();
        if (cp != null)
        {
            float globalMul = waveManager.GetGlobalSpeedMultiplier();
            cp.SetSpeedMultiplier(w.speedMultiplier * globalMul);
            cp.cubeSpawnManager = this;
        }

        if (makeSpecial)
        {
            SpecialCube sc = inst.GetComponent<SpecialCube>();
            if (sc == null) sc = inst.AddComponent<SpecialCube>();

            int specialType = Random.Range(0, 3);
            sc.ConfigureSpecial(specialType, waveManager.specialIcons != null && waveManager.specialIcons.Length > specialType ? waveManager.specialIcons[specialType] : null, waveManager);
        }
        else
        {
            SpecialCube sc = inst.GetComponent<SpecialCube>();
            if (sc != null)
            {
                sc.ClearVisual();
            }
        }
    }

    public void NotifyCubeDestroyed(GameObject cube)
    {
        if (cube == null) return;
        activeCubes.Remove(cube);
    }

    public int GetActiveCubeCount() { return activeCubes.Count; }

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
