using UnityEngine;

public class UIManagerSC : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject skinCardPanel;

    public void OpenCard()
    {
        if (skinCardPanel != null)
        {
            skinCardPanel.SetActive(true);
        }
    }

    public void CloseCard()
    {
        if (skinCardPanel != null)
        {
            skinCardPanel.SetActive(false);
        }
    }
}
