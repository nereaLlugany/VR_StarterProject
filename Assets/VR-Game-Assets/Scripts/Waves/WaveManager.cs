using UnityEngine;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Waves (configured in Inspector or default created in Start)")]
    public Wave[] waves;

    [Header("Special icons (3): 0 = slow, 1 = fast, 2 = swap")]
    public Sprite[] specialIcons = new Sprite[3];

    [Header("Spawn sides (configure Transforms)")]
    public Transform[] leftSpawnPoints;
    public Transform[] rightSpawnPoints;

    private int currentWave = 0;
    private float waveTimer = 0f;

    private float globalSpeedMultiplier = 1f;
    private float globalSpeedTimer = 0f;

    private bool swapSidesActive = false;
    private float swapSidesTimer = 0f;

    void Start()
    {
        if (waves == null || waves.Length == 0)
        {
            waves = new Wave[5];

            waves[0] = new Wave {
                waveName = "Wave 1 - Introducció",
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

            waves[1] = new Wave {
                waveName = "Wave 2 - Combinació bàsica",
                duration = 60f,
                minSpawnInterval = 2.0f,
                maxSpawnInterval = 3.0f,
                allowGun = true,
                allowSword = true,
                maxSimultaneous = 6,
                maxSpawnPerTick = 1, 
                speedMultiplier = 1.0f,
                introduceSpecialCubes = false,
                gunSidePreference = 1,   
                swordSidePreference = 2  
            };

            waves[2] = new Wave {
                waveName = "Wave 3 - Combinació incrementada",
                duration = 60f,
                minSpawnInterval = 1.2f,
                maxSpawnInterval = 2.0f,
                allowGun = true,
                allowSword = true,
                maxSimultaneous = 8,
                maxSpawnPerTick = 2, 
                speedMultiplier = 1.25f,
                introduceSpecialCubes = false,
                gunSidePreference = 0,
                swordSidePreference = 0
            };

            waves[3] = new Wave {
                waveName = "Wave 4 - Increment i cubs nous",
                duration = 60f,
                minSpawnInterval = 0.9f,
                maxSpawnInterval = 1.6f,
                allowGun = true,
                allowSword = true,
                maxSimultaneous = 10,
                maxSpawnPerTick = 3,
                speedMultiplier = 1.5f,
                introduceSpecialCubes = true,
                specialChance = 0.15f 
            };

            waves[4] = new Wave {
                waveName = "Wave 5 - Caos (difícil)",
                duration = 90f,
                minSpawnInterval = 0.5f,
                maxSpawnInterval = 1.2f,
                allowGun = true,
                allowSword = true,
                maxSimultaneous = 14,
                maxSpawnPerTick = 4,
                speedMultiplier = 1.9f,
                introduceSpecialCubes = true,
                specialChance = 0.30f
            };
        }

        waveTimer = 0f;
    }

    void Update()
    {
        waveTimer += Time.deltaTime;

        if (globalSpeedTimer > 0f)
        {
            globalSpeedTimer -= Time.deltaTime;
            if (globalSpeedTimer <= 0f)
            {
                globalSpeedMultiplier = 1f;
            }
        }

        if (swapSidesTimer > 0f)
        {
            swapSidesTimer -= Time.deltaTime;
            if (swapSidesTimer <= 0f)
            {
                swapSidesActive = false;
            }
        }

        if (waveTimer >= waves[currentWave].duration)
        {
            waveTimer = 0f;
            currentWave++;
            if (currentWave >= waves.Length)
                currentWave = waves.Length - 1;
        }
    }

    public Wave GetCurrentWave()
    {
        return waves[currentWave];
    }

    public Transform ChooseSpawnPointForType(string type, int sidePreference)
    {
        int effectivePref = sidePreference;
        if (swapSidesActive)
        {
            if (sidePreference == 1) effectivePref = 2;
            else if (sidePreference == 2) effectivePref = 1;
        }

        if ((leftSpawnPoints == null || leftSpawnPoints.Length == 0) && (rightSpawnPoints == null || rightSpawnPoints.Length == 0))
            return null;

        if (effectivePref == 1 && leftSpawnPoints != null && leftSpawnPoints.Length > 0)
            return leftSpawnPoints[Random.Range(0, leftSpawnPoints.Length)];
        if (effectivePref == 2 && rightSpawnPoints != null && rightSpawnPoints.Length > 0)
            return rightSpawnPoints[Random.Range(0, rightSpawnPoints.Length)];

        List<Transform> pool = new List<Transform>();
        if (leftSpawnPoints != null && leftSpawnPoints.Length > 0) pool.AddRange(leftSpawnPoints);
        if (rightSpawnPoints != null && rightSpawnPoints.Length > 0) pool.AddRange(rightSpawnPoints);

        if (pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    public void ApplyGlobalSpeedMultiplier(float multiplier, float duration)
    {
        if (duration <= 0f) return;
        globalSpeedMultiplier = multiplier;
        globalSpeedTimer = duration;
    }

    public float GetGlobalSpeedMultiplier()
    {
        return globalSpeedMultiplier;
    }

    public void TriggerSwapSides(float duration)
    {
        if (duration <= 0f) return;
        swapSidesActive = true;
        swapSidesTimer = duration;
    }

    public int GetCurrentWaveIndex() { return currentWave; }
    public float GetWaveElapsed() { return waveTimer; }
    public float GetWaveDuration() { return waves != null && waves.Length > 0 ? waves[currentWave].duration : 0f; }
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
    [Range(0f, 1f)]
    public float specialChance = 0.0f;
    public int gunSidePreference = 0;
    public int swordSidePreference = 0;
}
