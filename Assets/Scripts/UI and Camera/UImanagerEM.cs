using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UImanagerEM : MonoBehaviour
{
    [SerializeField] private Image timerRingImage;

    [Header("Settings")]
    [SerializeField] private float timeToWait = 6f;

    private Coroutine countdownCoroutine;

    void OnEnable()
    {
        if (timeToWait <= 0) timeToWait = 6f;
        if (timerRingImage != null) timerRingImage.fillAmount = 1f;

        countdownCoroutine = StartCoroutine(CountDownAndLoadMenu());   
    }

    private IEnumerator CountDownAndLoadMenu()
    {
        float timer = timeToWait;
        while (timer > 0)
        {
            timer -= Time.unscaledDeltaTime;

            if (timerRingImage != null)
            {
                timerRingImage.fillAmount = timer / timeToWait;
            }

            yield return null;
        }

        TriggerRestart();
    }

    public void SkipClicked()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        TriggerRestart();
    }

    private void TriggerRestart()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}