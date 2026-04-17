using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

public class BlockSpawner : MonoBehaviour
{

    [Header("Prefabs")]
    public GameObject redBlockPrefab;
    public GameObject blueBlockPrefab;

    [Header("Map Settings")]
    public string mapFileName = "EasyStandard.dat";
    public float bpm = 120f;

    [Header("Spawn Settings")]
    public float spawnZ = 20f;
    public float blockSpeed = 8f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadAndSpawn();
    }

    void LoadAndSpawn()
    {
        string path = Path.Combine(Application.streamingAssetsPath, mapFileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"Map file not found at:{path}");
            return;
        }

        string json = File.ReadAllText(path);
        BeatSaberMap map = JsonConvert.DeserializeObject<BeatSaberMap>(json);

        if (map == null || map._notes == null)
        {
            Debug.LogError("Failed to parse map file.");
            return; 
        }

        map._notes.Sort((a,b) => a._time.CompareTo(b._time));
        StartCoroutine(SpawnRoutine(map._notes));
    }

    IEnumerator SpawnRoutine(List<BeatSaberNote> notes)
    {
        float startTime = Time.time;
        float secondsPerBeat = 60f / bpm;

        foreach (var note in notes)
        {
            float spawnAt = note._time * secondsPerBeat;
            yield return new WaitUntil(() => Time.time - startTime >= spawnAt);

            SpawnBlock(note);
        }
    }

    void SpawnBlock(BeatSaberNote note)
    {
        if (note._type == 3) return;

        GameObject prefab = note._type == 0 ? redBlockPrefab : blueBlockPrefab;
        if (prefab == null) return;


        float x = Mathf.Lerp(-1.5f, 1.5f, note._lineIndex / 3f);
        float y = Mathf.Lerp(0.5f, 2.5f, note._lineLayer / 2f);

        GameObject block = Instantiate(prefab, new Vector3(x,y,spawnZ), Quaternion.identity);
        block.GetComponent<Block>().speed = blockSpeed;
    }
}

[System.Serializable]
public class BeatSaberMap
{
    public List<BeatSaberNote> _notes;
}

[System.Serializable]
public class BeatSaberNote
{
    public float _time;
    public int _lineIndex;
    public int _lineLayer;
    public int _type;
    public int _cutDirection; 
}


