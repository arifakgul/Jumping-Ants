using TMPro;
using UnityEngine;

public class PoinDisplayUI : MonoBehaviour
{
    private TextMeshProUGUI poinText;

    void Awake()
    {
        poinText = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        int poins = PlayerPrefs.GetInt("TotalPoins", 2);
        UpdatePoinText(poins);
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPoinChanged += UpdatePoinText;
            UpdatePoinText(GameManager.Instance.GetTotalPoin());
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPoinChanged -= UpdatePoinText;
        }
    }

    private void UpdatePoinText(int amount)
    {
        if (poinText != null)
        {
            poinText.text = amount.ToString();
        }
    }
}
