using System;

[Serializable]
public class Preset
{
    public string id = Guid.NewGuid().ToString();
    public string name;
    public float speed;
    public float angle;
    public float drag;
    public float mass;
    public float caliber;
}