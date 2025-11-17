using System.Collections;
using UnityEngine;

/// <summary>
/// 星星升級動畫控制
/// 掛載於 StarPrefab 預製物上
/// </summary>
public class StarUpgradeAnimation : MonoBehaviour
{
    [Header("動畫設定")]
    [Tooltip("旋轉速度，度/秒")]
    public float rotationSpeed = 360f;

    [Tooltip("移動到目標位置所需時間")]
    public float moveDuration = 1f;

    [Tooltip("縮放目標大小")]
    public Vector3 targetScale = Vector3.one;

    private Vector3 startScale;
    private Transform targetTransform;

    /// <summary>
    /// 初始化星星動畫
    /// </summary>
    /// <param name="target">星星最終停放的等級位置 Transform</param>
    public void PlayUpgradeAnimation(Transform target)
    {
        targetTransform = target;
        startScale = transform.localScale;

        // 開始動畫協程
        StartCoroutine(AnimateStar());
    }

    private IEnumerator AnimateStar()
    {
        // 第1階段：自轉（360度） 同時保持初始大小
        float rotationTime = 1f; // 可以自行調整旋轉時間
        float elapsed = 0f;

        while (elapsed < rotationTime)
        {
            float delta = Time.deltaTime;
            transform.Rotate(0f, rotationSpeed * delta, 0f, Space.Self);
            elapsed += delta;
            yield return null;
        }

        // 第2階段：移動到目標位置並縮放
        elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            transform.position = Vector3.Lerp(startPos, targetTransform.position, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // 最終修正位置與縮放
        transform.position = targetTransform.position;
        transform.localScale = targetScale;
    }
}
