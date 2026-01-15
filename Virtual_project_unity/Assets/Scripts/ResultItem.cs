using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultItem : MonoBehaviour
{
    public TextMeshProUGUI titleLabel;
    public TextMeshProUGUI detailsLabel;
    public Button deleteButton;

    public void Setup(LaunchResult result, int number)
    {
        titleLabel.text = $"Попытка {number}";

        // Форматируем дату и время
        string dateTimeStr = result.timestamp.ToString("dd.MM.yy HH:mm");

        // Форматируем погодные условия
        string weatherStr = $"Погода:\n" +
                           $"Ветер: {result.windSpeed:F1} м/с, {result.windDirection:F0}°\n" +
                           $"Темп.: {result.temperature:F1}°C\n" +
                           $"Высота: {result.altitude:F0} м\n" +
                           $"Турбулентность: {result.turbulenceLevel}";

        detailsLabel.text = $"Дата: {dateTimeStr}\n" +
                           $"Скорость: {result.initialSpeed} м/с\n" +
                           $"Угол: {result.angle}°\n" +
                           $"Дальность: {result.maxDistance:F1} м\n" +
                           $"{weatherStr}";

        deleteButton.onClick.AddListener(() => DeleteResult(result));
    }

    public void SetupSeriesItem(DispersionResult result, int number)
    {
        titleLabel.text = $"Серия #{number}";

        string dateTimeStr = result.timestamp.ToString("dd.MM.yy HH:mm");

        detailsLabel.text = $"Дата: {dateTimeStr}\n" +
                           $"Выстрелов: {result.shotCount}\n" +
                           $"Средняя точка: X={result.averageX:F1}, Z={result.averageZ:F1}\n" +
                           $"Вер. откл.: X=±{result.probableDeviationX:F1}, Z=±{result.probableDeviationZ:F1}\n" +
                           $"Отн. кучность: X={result.relativeDispersionX:P1}, Z={result.relativeDispersionZ:P1}";

        deleteButton.onClick.AddListener(() => DeleteDispersionResult(result));
    }

    private void DeleteResult(LaunchResult result)
    {
        if (ResultManager.Instance != null)
        {
            ResultManager.Instance.DeleteResult(result);
            ResultList.Instance.RefreshList();
        }
    }

    private void DeleteDispersionResult(DispersionResult result)
    {
        if (ResultManager.Instance != null)
        {
            ResultManager.Instance.DeleteDispersionResult(result);
            ResultList.Instance.RefreshList();
        }
    }
}