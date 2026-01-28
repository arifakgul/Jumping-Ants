using System;
using UnityEngine;

public class UIManagerST : MonoBehaviour
{
    public static UIManagerST Instance {get; private set;}

    [Header("Panels")]
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private GameObject settingsMenuPanel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OpenSettings()
    {
        startMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        startMenuPanel.SetActive(true);
        settingsMenuPanel.SetActive(false);
    }
}