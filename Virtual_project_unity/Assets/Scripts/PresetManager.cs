using System.Collections.Generic;
using UnityEngine;

public class PresetManager : MonoBehaviour
{
    public static PresetManager Instance;
    private const string SAVE_KEY = "projectile_presets";

    [SerializeField] private List<Preset> presets = new List<Preset>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadPresets();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnApplicationQuit()
    {
        SavePresets();
    }

    public void SavePresets()
    {
        var wrapper = new PresetWrapper { presets = presets };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public void LoadPresets()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            presets = JsonUtility.FromJson<PresetWrapper>(json).presets;
        }
    }

    public void UpdatePreset(Preset updatedPreset)
    {
        int index = presets.FindIndex(p => p.id == updatedPreset.id);
        if (index != -1)
        {
            presets[index] = updatedPreset;
            SavePresets();
        }
    }

    public void AddPreset(Preset preset)
    {
        presets.Add(preset);
        SavePresets();
    }

    public void RemovePreset(Preset preset)
    {
        presets.Remove(preset);
        SavePresets();
    }

    [System.Serializable]
    private class PresetWrapper
    {
        public List<Preset> presets;
    }

    public List<Preset> GetPresets()
    {
        return presets;
    }

    public void ClearPresets()
    {
        presets.Clear();
        SavePresets();
    }
}