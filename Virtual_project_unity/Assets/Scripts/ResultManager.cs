using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResultWrapper
{
    public List<LaunchResult> results = new List<LaunchResult>();
    public List<DispersionResult> dispersionResults = new List<DispersionResult>();
}

[System.Serializable]
public class DispersionResult
{
    public string id = Guid.NewGuid().ToString();
    public string presetName;
    public DateTime timestamp;
    public long timestampTicks; // Для сериализации
    public int shotCount;
    public float averageX;
    public float averageZ;
    public float probableDeviationX;
    public float probableDeviationZ;
    public float relativeDispersionX;
    public float relativeDispersionZ;

    // Свойство для обратной совместимости
    public DateTime Timestamp
    {
        get { return new DateTime(1970, 1, 1).AddSeconds(timestampTicks); }
        set { timestampTicks = (long)(value - new DateTime(1970, 1, 1)).TotalSeconds; }
    }
}

public class ResultManager : MonoBehaviour
{
    public static ResultManager Instance;
    private const string SAVE_KEY = "launch_results";

    [SerializeField] private List<LaunchResult> results = new List<LaunchResult>();
    [SerializeField] private List<DispersionResult> dispersionResults = new List<DispersionResult>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadResults();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveDispersionResult(DispersionResult result)
    {
        if (dispersionResults.Exists(r => r.id == result.id))
        {
            Debug.LogWarning("Результат серии уже существует!");
            return;
        }
        dispersionResults.Add(result);
        SaveResults();
    }

    public void SaveResult(LaunchResult result)
    {
        if (results.Exists(r => r.id == result.id))
        {
            Debug.LogWarning("Результат уже существует!");
            return;
        }
        results.Add(result);
        SaveResults();
    }

    public void DeleteResult(LaunchResult result)
    {
        results.Remove(result);
        SaveResults();
    }

    public void DeleteDispersionResult(DispersionResult result)
    {
        dispersionResults.Remove(result);
        SaveResults();
    }

    private void SaveResults()
    {
        // Подготавливаем результаты для сохранения
        foreach (var result in results)
        {
            // Конвертируем timestamp в ticks для сериализации
            if (result.timestampTicks == 0 && result.timestamp != DateTime.MinValue)
            {
                result.timestampTicks = (long)(result.timestamp - new DateTime(1970, 1, 1)).TotalSeconds;
            }
        }

        foreach (var dispersion in dispersionResults)
        {
            // Конвертируем timestamp в ticks для сериализации
            if (dispersion.timestampTicks == 0 && dispersion.timestamp != DateTime.MinValue)
            {
                dispersion.timestampTicks = (long)(dispersion.timestamp - new DateTime(1970, 1, 1)).TotalSeconds;
            }
        }

        var wrapper = new ResultWrapper
        {
            results = results,
            dispersionResults = dispersionResults
        };

        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    private void LoadResults()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            var wrapper = JsonUtility.FromJson<ResultWrapper>(json);

            if (wrapper != null)
            {
                results = wrapper.results ?? new List<LaunchResult>();
                dispersionResults = wrapper.dispersionResults ?? new List<DispersionResult>();

                // Конвертируем ticks обратно в DateTime
                foreach (var result in results)
                {
                    if (result.timestampTicks != 0 && result.timestamp == DateTime.MinValue)
                    {
                        result.timestamp = new DateTime(1970, 1, 1).AddSeconds(result.timestampTicks);
                    }
                    else if (result.timestampTicks == 0 && result.timestamp == DateTime.MinValue)
                    {
                        result.timestamp = DateTime.Now;
                        result.timestampTicks = (long)(result.timestamp - new DateTime(1970, 1, 1)).TotalSeconds;
                    }
                }

                foreach (var dispersion in dispersionResults)
                {
                    if (dispersion.timestampTicks != 0 && dispersion.timestamp == DateTime.MinValue)
                    {
                        dispersion.timestamp = new DateTime(1970, 1, 1).AddSeconds(dispersion.timestampTicks);
                    }
                    else if (dispersion.timestampTicks == 0 && dispersion.timestamp == DateTime.MinValue)
                    {
                        dispersion.timestamp = DateTime.Now;
                        dispersion.timestampTicks = (long)(dispersion.timestamp - new DateTime(1970, 1, 1)).TotalSeconds;
                    }
                }
            }
        }
    }

    public List<LaunchResult> GetResults() => results;
    public List<DispersionResult> GetDispersionResults() => dispersionResults;
}