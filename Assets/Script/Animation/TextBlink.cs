using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TextBlink : MonoBehaviour
{
    public Text fadingText;      // Đối tượng Text cần hiệu ứng mờ dần
    public float fadeDuration = 1.0f; // Thời gian để text mờ đi và hiện rõ lên

    private void Start()
    {
        if (fadingText != null)
        {
            StartCoroutine(FadeInOut());
        }
    }

    private IEnumerator FadeInOut()
    {
        while (true)
        {
            // Mờ dần từ 0 đến 1
            yield return StartCoroutine(FadeTextToFullAlpha());
            // Mờ dần từ 1 đến 0
            yield return StartCoroutine(FadeTextToZeroAlpha());
        }
    }

    private IEnumerator FadeTextToFullAlpha()
    {
        Color color = fadingText.color;
        for (float t = 0.01f; t < fadeDuration; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(0, 1, t / fadeDuration);
            fadingText.color = color;
            yield return null;
        }
        color.a = 1;
        fadingText.color = color;
    }

    private IEnumerator FadeTextToZeroAlpha()
    {
        Color color = fadingText.color;
        for (float t = 0.01f; t < fadeDuration; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(1, 0, t / fadeDuration);
            fadingText.color = color;
            yield return null;
        }
        color.a = 0;
        fadingText.color = color;
    }
}
