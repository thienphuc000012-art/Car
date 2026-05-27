using System.Collections;
using UnityEngine;

public class UIPanelPopEffect : MonoBehaviour
{
    public float duration = 0.2f;
    public Vector3 startScale = new Vector3(0.85f, 0.85f, 0.85f);
    public Vector3 endScale = Vector3.one;

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float timer = 0f;
        transform.localScale = startScale;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        transform.localScale = endScale;
    }
}
