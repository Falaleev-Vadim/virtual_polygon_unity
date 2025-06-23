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
            results = JsonUtility.FromJson<ResultWrapper>(json).results;
        }
    }

    public List<LaunchResult> GetResults() => results;

    [System.Serializable]
    private class ResultWrapper
    {
        public List<LaunchResult> results;
    }
}