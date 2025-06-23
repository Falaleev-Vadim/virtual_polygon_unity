using UnityEngine;
using UnityEngine.UI;

public class ResultList : MonoBehaviour
{
    public static ResultList Instance;

    [SerializeField] private GameObject resultItemPrefab;
    [SerializeField] private Transform contentPanel;
    [SerializeField] private GameObject resultListPanel;

    void Awake()
    {
        Instance = this;
    }

    public void ShowResults()
    {
        resultListPanel.SetActive(true);
        RefreshList();
    }

    public void ClosePanel()
    {
        resultListPanel.SetActive(false);
    }

    public void DeleteResult(LaunchResult result)
    {
        ResultManager.Instance.DeleteResult(result);
        RefreshList();
    }

    public int GetResultIndex(LaunchResult result)
    {
        return ResultManager.Instance.GetResults().IndexOf(result);
    }

    public void RefreshList()
    {
        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);

        var results = ResultManager.Instance.GetResults();
        for (int i = 0; i < results.Count; i++)
        {
            GameObject item = Instantiate(resultItemPrefab, contentPanel);
            var resultItem = item.GetComponent<ResultItem>();
            resultItem.Setup(results[i], i + 1);
        }
    }
}