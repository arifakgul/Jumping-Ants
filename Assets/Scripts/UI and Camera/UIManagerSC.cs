using UnityEngine;

public class UIManagerSC : MonoBehaviour
{
    // bitmedi skin sistemi yeniden tasarlanacak sistem unutma.
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
