using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuPreset : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject addPresetPanel;
    public Transform presetContent;
    public GameObject presetItemPrefab;
    public Button insertButton;
    public Button deleteButton;

    [Header("Edit Panel")]
    public Button editButton;
    public TMP_InputField editNameInput;
    public TMP_InputField editSpeedInput;
    public TMP_InputField editAngleInput;
    public TMP_InputField editDragInput;
    public TMP_InputField editMassInput;
    public TMP_InputField editCaliberInput;
    public GameObject editPresetPanel;

    [Header("Add Panel")]
    [SerializeField] private MainMenuController mainMenuController;

    public TMP_InputField nameInput;
    public TMP_InputField speedInput;
    public TMP_InputField angleInput;
    public TMP_InputField dragCoeffInput;
    public TMP_InputField massInput;
    public TMP_InputField caliberInput;

    private Preset selectedPreset;
    public Preset preset;

    void Start()
    {
        LoadPresets();
        UpdateButtonsState();
    }

    public void ShowAddPresetPanel()
    {
        addPresetPanel.SetActive(true);
    }

    public void ShowEditPresetPanel()
    {
        if (selectedPreset == null) return;

        editNameInput.text = selectedPreset.name;
        editSpeedInput.text = selectedPreset.speed.ToString();
        editAngleInput.text = selectedPreset.angle.ToString();
        editDragInput.text = selectedPreset.drag.ToString();
        editMassInput.text = selectedPreset.mass.ToString();
        editCaliberInput.text = selectedPreset.caliber.ToString();

        editPresetPanel.SetActive(true);
    }

    public void HideAddPresetPanel()
    {
        addPresetPanel.SetActive(false);
    }

    public void HideEditPresetPanel()
    {
        editPresetPanel.SetActive(false);
    }

    public void AddPreset()
    {
        if (string.IsNullOrEmpty(nameInput.text))
        {
            Debug.LogError("Название не может быть пустым!");
            return;
        }

        Preset newPreset = new Preset
        {
            name = nameInput.text,
            speed = float.Parse(speedInput.text),
            angle = float.Parse(angleInput.text),
            drag = float.Parse(dragCoeffInput.text),
            mass = float.Parse(massInput.text),
            caliber = float.Parse(caliberInput.text),
        };

        Debug.Log($"Добавлен пресет: {newPreset.name}");

        PresetManager.Instance.GetPresets().Add(newPreset);
        LoadPresets();
        HideAddPresetPanel();
    }

    public void OnPresetSelected(Preset preset)
    {
        if (preset == selectedPreset) return;

        if (selectedPreset != null)
        {
            var oldItem = FindPresetItem(selectedPreset);
            if (oldItem != null)
            {
                oldItem.OnDeselect();
            }
        }

        selectedPreset = preset;

        if (selectedPreset != null)
        {
            var newItem = FindPresetItem(preset);
            if (newItem != null)
            {
                newItem.OnSelect();
            }
        }

        UpdateButtonsState();

        editButton.interactable = (selectedPreset != null);
    }

    private PresetItem FindPresetItem(Preset preset)
    {
        foreach (Transform child in presetContent)
        {
            var item = child.GetComponent<PresetItem>();
            if (item != null && item.preset == preset)
            {
                return item;
            }
        }
        return null;
    }

    private void UpdateButtonsState()
    {
        insertButton.interactable = selectedPreset != null;
        deleteButton.interactable = selectedPreset != null;
    }

    public void UpdatePreset()
    {
        if (selectedPreset == null || !ValidateEditInputs())
            return;

        selectedPreset.name = editNameInput.text;
        selectedPreset.speed = float.Parse(editSpeedInput.text);
        selectedPreset.angle = float.Parse(editAngleInput.text);
        selectedPreset.drag = float.Parse(editDragInput.text);
        selectedPreset.mass = float.Parse(editMassInput.text);
        selectedPreset.caliber = float.Parse(editCaliberInput.text);

        PresetManager.Instance.SavePresets();
        LoadPresets();
        editPresetPanel.SetActive(false);
    }

    private bool ValidateEditInputs()
    {
        if (string.IsNullOrEmpty(editNameInput.text))
        {
            Debug.LogError("Название не может быть пустым!");
            return false;
        }

        if (!float.TryParse(editSpeedInput.text, out float speed) || speed < 100 || speed > 2000)
        {
            Debug.LogError("Неверная скорость!");
            return false;
        }

        return true;
    }

    private void LoadPresets()
    {
        if (PresetManager.Instance == null ||
            PresetManager.Instance.GetPresets() == null ||
            presetContent == null ||
            presetItemPrefab == null)
        {
            Debug.LogError("Критическая ошибка: не все компоненты инициализированы!");
            return;
        }

        foreach (Transform child in presetContent)
            Destroy(child.gameObject);

        foreach (var preset in PresetManager.Instance.GetPresets())
        {
            GameObject item = Instantiate(presetItemPrefab, presetContent);
            PresetItem presetItem = item.GetComponent<PresetItem>();

            if (presetItem == null)
            {
                Debug.LogError("PresetItem component не найден на префабе!");
                continue;
            }

            presetItem.Setup(preset, this);
        }
    }

    public void InsertPreset()
    {
        if (selectedPreset != null)
        {
            mainMenuController.speedInput.text = selectedPreset.speed.ToString();
            mainMenuController.angleInput.text = selectedPreset.angle.ToString();
            mainMenuController.dragCoeffInput.text = selectedPreset.drag.ToString();
            mainMenuController.massInput.text = selectedPreset.mass.ToString();
            mainMenuController.caliberInput.text = selectedPreset.caliber.ToString();
        }
    }

    public void DeletePreset()
    {
        if (selectedPreset != null)
        {
            PresetManager.Instance.RemovePreset(selectedPreset);
            selectedPreset = null;
            LoadPresets();
        }
    }

    public void OnStartSimulation()
    {
        PresetManager.Instance.SavePresets();
        SceneManager.LoadScene("Simulation");
    }
}