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

        // Остальной код без изменений
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

    private void DeleteResult(LaunchResult result)
    {
        ResultManager.Instance.DeleteResult(result);
        ResultList.Instance.RefreshList();
    }
}