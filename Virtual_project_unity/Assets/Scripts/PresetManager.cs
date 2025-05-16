using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Preset
{
    public string name;
    public float speed;
    public float angle;
    public float drag;
    public float mass;
    public float caliber;
}

public class PresetManager : MonoBehaviour
{
    public static PresetManager Instance;

    public List<Preset> presets = new List<Preset>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}