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
        titleLabel.text = $"������� {number}";
        detailsLabel.text =
            $"����: {result.timestamp:dd.MM.yy HH:mm}\n" +
            $"��������: {result.initialSpeed} �/�\n" +
            $"����: {result.angle}�\n" +
            $"���������: {result.maxDistance:F1} �";

        deleteButton.onClick.AddListener(() => DeleteResult(result));
    }

    private void DeleteResult(LaunchResult result)
    {
        ResultManager.Instance.DeleteResult(result);
        ResultList.Instance.RefreshList();
    }
}