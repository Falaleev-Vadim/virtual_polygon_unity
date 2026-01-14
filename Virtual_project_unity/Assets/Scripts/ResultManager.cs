using System;
using System.Collections.Generic;
using UnityEngine;

public class ResultManager : MonoBehaviour
{
    public static ResultManager Instance;
    private const string SAVE_KEY = "launch_results";

    [SerializeField] private List<LaunchResult> results = new List<LaunchResult>();

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

    private void SaveResults()
    {
        // Перед сохранением убедитесь, что все результаты имеют корректный timestampTicks
        foreach (var result in results)
        {
            // Если timestampTicks не установлен, но есть timestamp - конвертируем
            if (result.timestampTicks == 0 && result.timestamp != DateTime.MinValue)
            {
                result.timestampTicks = (long)(result.timestamp - new DateTime(1970, 1, 1)).TotalSeconds;
            }
        }

        var wrapper = new ResultWrapper { results = results };
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

            if (wrapper != null && wrapper.results != null)
            {
                results = wrapper.results;

                // Конвертируем timestampTicks в DateTime для обратной совместимости
                foreach (var result in results)
                {
                    // Если timestampTicks установлен, но timestamp не установлен - конвертируем
                    if (result.timestampTicks != 0 && result.timestamp == DateTime.MinValue)
                    {
                        result.timestamp = new DateTime(1970, 1, 1).AddSeconds(result.timestampTicks);
                    }
                    // Если ни timestampTicks, ни timestamp не установлены - используем текущее время
                    else if (result.timestampTicks == 0 && result.timestamp == DateTime.MinValue)
                    {
                        result.timestamp = DateTime.Now;
                        result.timestampTicks = (long)(result.timestamp - new DateTime(1970, 1, 1)).TotalSeconds;
                    }
                }
            }
        }
    }

    public List<LaunchResult> GetResults() => results;

    [System.Serializable]
    private class ResultWrapper
    {
        public List<LaunchResult> results;
    }
}