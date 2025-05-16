using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public TMP_InputField speedInput;
    public TMP_InputField angleInput;
    public TMP_InputField dragCoeffInput;
    public TMP_InputField massInput;
    public TMP_InputField caliberInput;
    public TMP_InputField targetDistanceInput;
    public TMP_InputField startToPolygonInput;

    public void OnStartSimulation()
    {
        if (ValidateInputs())
        {
            SimulationParameters parameters = new SimulationParameters
            {
                initialSpeed = float.Parse(speedInput.text),
                angleDegrees = float.Parse(angleInput.text),
                dragCoefficient = float.Parse(dragCoeffInput.text),
                mass = float.Parse(massInput.text),
                caliberMm = float.Parse(caliberInput.text),
                targetDistanceKm = float.Parse(targetDistanceInput.text),
                startToPolygonDistance = float.Parse(startToPolygonInput.text)
            };

            SimulationData.Parameters = parameters;
            SceneManager.LoadScene("Simulation", LoadSceneMode.Single);
        }
    }

    private bool ValidateInputs()
    {
        if (!ValidateRange(speedInput, 100, 2000)) return false;
        if (!ValidateRange(angleInput, 0, 90)) return false;
        if (!ValidateRange(dragCoeffInput, 0.1f, 2)) return false;
        if (!ValidateRange(massInput, 0.1f, 1000)) return false;
        if (!ValidateRange(caliberInput, 1, 500)) return false;
        if (!ValidateRange(targetDistanceInput, 0.1f, 50)) return false;
        if (!ValidateRange(startToPolygonInput, 0.1f, 50)) return false;
        return true;
    }

    private bool ValidateRange(TMP_InputField input, float min, float max)
    {
        if (!float.TryParse(input.text, out float value))
        {
            Debug.LogError("Некорректный ввод");
            return false;
        }
        if (value < min || value > max)
        {
            Debug.LogError($"Значение {input.name} должно быть между {min} и {max}");
            return false;
        }
        return true;
    }
}