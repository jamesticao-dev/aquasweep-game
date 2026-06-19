using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns Trash and Fish continuously throughout the round on independent timers,
/// at random positions within a defined spawn area. Call StartSpawning() / StopSpawning()
/// from GameManager when the round starts/ends.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject trashPrefab;
    public GameObject fishPrefab;

    [Header("Spawn Area")]
    public Vector2 spawnAreaCenter = Vector2.zero;
    public Vector2 spawnAreaSize = new Vector2(16f, 9f);

    [Header("Trash Spawning")]
    public float trashSpawnIntervalMin = 0.8f;
    public float trashSpawnIntervalMax = 1.8f;
    public int maxTrashOnScreen = 25;

    [Header("Fish Spawning")]
    public float fishSpawnIntervalMin = 2f;
    public float fishSpawnIntervalMax = 4f;
    public int maxFishOnScreen = 10;

    [Header("Initial Batch (optional head start so the level isn't empty)")]
    public int initialTrashCount = 6;
    public int initialFishCount = 3;

    private float trashTimer;
    private float fishTimer;
    private bool isSpawning = false;

    private List<GameObject> activeTrash = new List<GameObject>();
    private List<GameObject> activeFish = new List<GameObject>();

    public void StartSpawning()
    {
        isSpawning = true;
        ClearAll();

        for (int i = 0; i < initialTrashCount; i++) SpawnTrash();
        for (int i = 0; i < initialFishCount; i++) SpawnFish();

        ResetTrashTimer();
        ResetFishTimer();
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    private void Update()
    {
        if (!isSpawning) return;

        // Clean up null entries from lists (destroyed when shot)
        activeTrash.RemoveAll(item => item == null);
        activeFish.RemoveAll(item => item == null);

        trashTimer -= Time.deltaTime;
        if (trashTimer <= 0f)
        {
            if (activeTrash.Count < maxTrashOnScreen) SpawnTrash();
            ResetTrashTimer();
        }

        fishTimer -= Time.deltaTime;
        if (fishTimer <= 0f)
        {
            if (activeFish.Count < maxFishOnScreen) SpawnFish();
            ResetFishTimer();
        }
    }

    private void ResetTrashTimer()
    {
        trashTimer = Random.Range(trashSpawnIntervalMin, trashSpawnIntervalMax);
    }

    private void ResetFishTimer()
    {
        fishTimer = Random.Range(fishSpawnIntervalMin, fishSpawnIntervalMax);
    }

    private void SpawnTrash()
    {
        if (trashPrefab == null) return;
        Vector2 pos = GetRandomSpawnPosition();
        GameObject obj = Instantiate(trashPrefab, pos, Quaternion.identity);
        activeTrash.Add(obj);
    }

    private void SpawnFish()
    {
        if (fishPrefab == null) return;
        Vector2 pos = GetRandomSpawnPosition();
        GameObject obj = Instantiate(fishPrefab, pos, Quaternion.identity);
        activeFish.Add(obj);
    }

    private Vector2 GetRandomSpawnPosition()
    {
        float x = spawnAreaCenter.x + Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
        float y = spawnAreaCenter.y + Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
        return new Vector2(x, y);
    }

    private void ClearAll()
    {
        foreach (var t in activeTrash) if (t != null) Destroy(t);
        foreach (var f in activeFish) if (f != null) Destroy(f);
        activeTrash.Clear();
        activeFish.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
    }
}
