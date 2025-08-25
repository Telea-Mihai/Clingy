using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HitFeedbackManager : MonoBehaviour
{
    [Header("Hitmarker")]
    public GameObject hitmarkerPrefab;
    public float hitmarkerDuration = 0.3f;
    public Color hitmarkerColor = Color.white;
    public Color killmarkerColor = Color.red;
    public float hitmarkerSize = 20f;
    [Header("Score Popup")]
    public GameObject scorePopupPrefab;
    public Transform scorePopupParent;
    public float scorePopupDuration = 1.5f;
    public float scorePopupSpeed = 100f;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip killSound;
    public AudioClip headshotSound;
    [Header("Screen Effects")]
    public Image damageIndicator;
    public float damageIndicatorDuration = 0.3f;
    public AnimationCurve damageIndicatorCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Header("Crosshair")]
    public Image crosshair;
    public Color crosshairHitColor = Color.red;
    public Color crosshairNormalColor = Color.white;
    public float crosshairFeedbackDuration = 0.1f;
    [Header("Kill Streak")]
    public Text killStreakText;
    public float killStreakDisplayTime = 3f;
    private int currentKillStreak = 0;
    private Coroutine killStreakCoroutine;
    private Camera playerCamera;
    private Canvas uiCanvas;
    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();
        uiCanvas = FindFirstObjectByType<Canvas>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (damageIndicator != null)
        {
            Color color = damageIndicator.color;
            color.a = 0;
            damageIndicator.color = color;
        }
    }
    public void ShowHitFeedback(Vector3 worldPosition, int damage, bool isKill = false, bool isHeadshot = false)
    {
        ShowHitmarker(isKill, isHeadshot);
        int scoreValue = CalculateScore(damage, isKill, isHeadshot);
        ShowScorePopup(worldPosition, scoreValue, isKill, isHeadshot);
        PlayHitAudio(isKill, isHeadshot);
        StartCoroutine(CrosshairFeedback());
        if (isKill)
        {
            UpdateKillStreak();
        }
    }
    void ShowHitmarker(bool isKill, bool isHeadshot)
    {
        if (hitmarkerPrefab == null || uiCanvas == null) return;
        GameObject hitmarker = Instantiate(hitmarkerPrefab, uiCanvas.transform);
        RectTransform rect = hitmarker.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.one * hitmarkerSize;
        }
        Image image = hitmarker.GetComponent<Image>();
        if (image != null)
        {
            if (isKill)
                image.color = killmarkerColor;
            else
                image.color = hitmarkerColor;
        }
        StartCoroutine(AnimateHitmarker(hitmarker));
    }
    IEnumerator AnimateHitmarker(GameObject hitmarker)
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 1.2f;
        Vector3 endScale = Vector3.one;
        RectTransform rect = hitmarker.GetComponent<RectTransform>();
        rect.localPosition = new Vector2(rect.position.x + Random.Range(-3f, 3f), rect.position.y + Random.Range(-3f, 3f));
        Image image = hitmarker.GetComponent<Image>();
        while (elapsed < hitmarkerDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hitmarkerDuration;
            if (rect != null)
                rect.localScale = Vector3.Lerp(startScale, endScale, t);
            if (image != null)
            {
                Color color = image.color;
                color.a = 1f - t;
                image.color = color;
            }
            yield return null;
        }
        if (hitmarker != null)
            Destroy(hitmarker);
    }
    void ShowScorePopup(Vector3 worldPosition, int score, bool isKill, bool isHeadshot)
    {
        if (scorePopupPrefab == null || playerCamera == null || uiCanvas == null) return;
        Vector3 screenPos = playerCamera.WorldToScreenPoint(worldPosition);
        GameObject popup = Instantiate(scorePopupPrefab, uiCanvas.transform);
        RectTransform rect = popup.GetComponent<RectTransform>();
        Text text = popup.GetComponent<Text>();
        if (rect != null)
        {
            Vector2 uiPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                uiCanvas.GetComponent<RectTransform>(),
                screenPos,
                uiCanvas.worldCamera,
                out uiPos
            );
            rect.anchoredPosition = uiPos;
        }
        if (text != null)
        {
            string scoreText = "+" + score.ToString();
            if (isHeadshot)
                scoreText += " HEADSHOT!";
            else if (isKill)
                scoreText += " KILL!";
            text.text = scoreText;
            if (isHeadshot)
                text.color = Color.yellow;
            else if (isKill)
                text.color = Color.red;
            else
                text.color = Color.white;
        }
        StartCoroutine(AnimateScorePopup(popup));
    }
    IEnumerator AnimateScorePopup(GameObject popup)
    {
        float elapsed = 0f;
        RectTransform rect = popup.GetComponent<RectTransform>();
        Text text = popup.GetComponent<Text>();
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * scorePopupSpeed;
        while (elapsed < scorePopupDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scorePopupDuration;
            if (rect != null)
                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            if (text != null)
            {
                Color color = text.color;
                color.a = 1f - t;
                text.color = color;
            }
            yield return null;
        }
        if (popup != null)
            Destroy(popup);
    }
    int CalculateScore(int damage, bool isKill, bool isHeadshot)
    {
        int score = damage * 2;
        if (isKill)
            score += 100;
        if (isHeadshot)
            score *= 2;
        if (isKill && currentKillStreak > 1)
            score += currentKillStreak * 25;
        return score;
    }
    void PlayHitAudio(bool isKill, bool isHeadshot)
    {
        if (audioSource == null) return;
        AudioClip clipToPlay = hitSound;
        if (isHeadshot && headshotSound != null)
            clipToPlay = headshotSound;
        else if (isKill && killSound != null)
            clipToPlay = killSound;
        if (clipToPlay != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clipToPlay);
        }
    }
    IEnumerator CrosshairFeedback()
    {
        if (crosshair == null) yield return null;
        crosshair.color = crosshairHitColor;
        yield return new WaitForSeconds(crosshairFeedbackDuration);

        crosshair.color = crosshairNormalColor;
    }
    void UpdateKillStreak()
    {
        currentKillStreak++;
        if (killStreakText != null && currentKillStreak > 1)
        {
            killStreakText.text = GetKillStreakText(currentKillStreak);
            killStreakText.gameObject.SetActive(true);
            if (killStreakCoroutine != null)
                StopCoroutine(killStreakCoroutine);
            killStreakCoroutine = StartCoroutine(HideKillStreakAfterDelay());
        }
    }
    string GetKillStreakText(int streak)
    {
        switch (streak)
        {
            case 2: return "DOUBLE KILL!";
            case 3: return "TRIPLE KILL!";
            case 4: return "MULTI KILL!";
            case 5: return "RAMPAGE!";
            case 10: return "UNSTOPPABLE!";
            case 15: return "LEGENDARY!";
            default: return $"{streak} KILL STREAK!";
        }
    }
    IEnumerator HideKillStreakAfterDelay()
    {
        yield return new WaitForSeconds(killStreakDisplayTime);
        if (killStreakText != null)
            killStreakText.gameObject.SetActive(false);
    }
    public void ResetKillStreak()
    {
        currentKillStreak = 0;
        if (killStreakText != null)
            killStreakText.gameObject.SetActive(false);
    }
    public void ShowDamageIndicator()
    {
        if (damageIndicator != null)
            StartCoroutine(AnimateDamageIndicator());
    }
    IEnumerator AnimateDamageIndicator()
    {
        float elapsed = 0f;
        while (elapsed < damageIndicatorDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / damageIndicatorDuration;
            float alpha = damageIndicatorCurve.Evaluate(t);
            Color color = damageIndicator.color;
            color.a = alpha * 0.3f;
            damageIndicator.color = color;
            yield return null;
        }
        Color finalColor = damageIndicator.color;
        finalColor.a = 0;
        damageIndicator.color = finalColor;
    }
}
