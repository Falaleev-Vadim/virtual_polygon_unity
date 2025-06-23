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
        detailsLabel.text =
            $"Дата: {result.timestamp:dd.MM.yy HH:mm}\n" +
            $"Скорость: {result.initialSpeed} м/с\n" +
            $"Угол: {result.angle}°\n" +
            $"Дальность: {result.maxDistance:F1} м";

        deleteButton.onClick.AddListener(() => DeleteResult(result));
    }

    private void DeleteResult(LaunchResult result)
    {
        ResultManager.Instance.DeleteResult(result);
        ResultList.Instance.RefreshList();
    }
}