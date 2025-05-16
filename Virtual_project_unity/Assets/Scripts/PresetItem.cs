using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PresetItem : MonoBehaviour
{
    private MainMenuPreset mainMenu;
    public Preset preset;
    private Image background;
    private bool isSelected = false;

    void Awake()
    {
        background = GetComponent<Image>();
        if (background == null)
        {
            Debug.LogError("Image component �� ������!");
        }
    }

    public void Setup(Preset preset, MainMenuPreset controller)
    {
        if (preset == null)
        {
            Debug.LogError("Preset �� ����� ���� null!");
            return;
        }

        this.preset = preset;
        mainMenu = controller;

        var textComponent = GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = preset.name;
            Debug.Log($"��������� ��������: {preset.name}");
        }
        else
        {
            Debug.LogError("Text component �� ������!");
        }
    }

    public void OnSelect()
    {
        if (isSelected) return;

        isSelected = true;
        background.color = Color.gray;

        if (mainMenu != null)
        {
            mainMenu.OnPresetSelected(preset);
        }
    }

    public void OnDeselect()
    {
        if (!isSelected) return;

        isSelected = false;
        background.color = Color.white;

        if (mainMenu != null)
        {
            mainMenu.OnPresetSelected(null);
        }
    }
}