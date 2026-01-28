using UnityEngine;

public class UIManagerAntsMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private GameObject skinsMenuPanel;

    public void OpenSettings()
    {
        startMenuPanel.SetActive(false);
        skinsMenuPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        startMenuPanel.SetActive(true);
        skinsMenuPanel.SetActive(false);
    }
}