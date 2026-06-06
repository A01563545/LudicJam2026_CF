using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class RecordedTimestamp
{
    public float time;
    public string key;
}

[Serializable]
public class TimestampRecording
{
    public string songName;
    public List<RecordedTimestamp> timestamps = new List<RecordedTimestamp>();
}

public class TimestampRecorder : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private bool playMusicOnStart = true;

    [Header("Save File")]
    [SerializeField] private string songName = "My Song";
    [SerializeField] private string fileName = "timestamps.json";

    private readonly TimestampRecording recording = new TimestampRecording();

    private string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, fileName); }
    }

    private void Start()
    {
        recording.songName = songName;

        if (musicSource != null && playMusicOnStart)
        {
            musicSource.Play();
        }

        Debug.Log("Timestamp JSON will be saved here: " + SavePath);
    }

    private void Update()
    {
        if (musicSource == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RecordKey("Jump");
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            RecordKey("SwitchPhase1");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            RecordKey("SwitchPhase2");
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveToJson();
        }
    }

    private void RecordKey(string keyName)
    {
        RecordedTimestamp timestamp = new RecordedTimestamp
        {
            time = musicSource.time,
            key = keyName
        };

        recording.timestamps.Add(timestamp);
        Debug.Log("Recorded " + keyName + " at " + timestamp.time.ToString("F3") + " seconds");
    }

    public void SaveToJson()
    {
        string json = JsonUtility.ToJson(recording, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Saved timestamps to: " + SavePath);
    }
}
