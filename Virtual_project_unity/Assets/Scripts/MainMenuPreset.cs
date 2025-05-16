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

    public void HideAddPresetPanel()
    {
        addPresetPanel.SetActive(false);
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

        // Выделяем новый элемент
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