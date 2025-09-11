using System.Collections;
using UnityEngine;

public class EarthquakeManager : MonoBehaviour
{
    [Header("Earthquake Settings")]
    [SerializeField] private float duration = 3f;     // Длительность землетрясения
    [SerializeField] private float amplitude = 0.5f;  // Амплитуда тряски
    [SerializeField] private float frequency = 10f;   // Частота тряски

    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Rigidbody2D targetRb; // Rigidbody платформы вместо Transform

    private Vector3 originalCameraPos;
    private Vector2 originalObjectPos;

    private bool isEarthquakeActive;
    private Coroutine earthquakeCoroutine;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
            originalCameraPos = targetCamera.transform.position;

        if (targetRb != null)
            originalObjectPos = targetRb.position;
    }

    public void StartEarthquake()
    {
        StartEarthquake(duration, amplitude, frequency);
    }

    public void StartEarthquake(float customDuration, float customAmplitude, float customFrequency)
    {
        if (isEarthquakeActive)
            return;

        earthquakeCoroutine = StartCoroutine(EarthquakeCoroutine(customDuration, customAmplitude, customFrequency));
    }

    public void StopEarthquake()
    {
        if (earthquakeCoroutine != null)
            StopCoroutine(earthquakeCoroutine);

        RestorePositions();
        isEarthquakeActive = false;
    }

    private IEnumerator EarthquakeCoroutine(float shakeDuration, float shakeAmplitude, float shakeFrequency)
    {
        isEarthquakeActive = true;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            // смещение по синусу/косинусу
            float offsetX = Mathf.Sin(elapsedTime * shakeFrequency) * shakeAmplitude;
            float offsetY = Mathf.Cos(elapsedTime * shakeFrequency * 1.3f) * shakeAmplitude * 0.6f;

            // рандом для реализма
            float randomX = Random.Range(-0.3f, 0.3f) * shakeAmplitude;
            float randomY = Random.Range(-0.2f, 0.2f) * shakeAmplitude;

            Vector2 shakeOffset = new Vector2(offsetX + randomX, offsetY + randomY);

            // трясём камеру
            if (targetCamera != null)
                targetCamera.transform.position = originalCameraPos + (Vector3)shakeOffset;

            // трясём платформу через Rigidbody2D
            if (targetRb != null)
                targetRb.MovePosition(originalObjectPos + shakeOffset);

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        RestorePositions();
        isEarthquakeActive = false;
    }

    private void RestorePositions()
    {
        if (targetCamera != null)
            targetCamera.transform.position = originalCameraPos;

        if (targetRb != null)
            targetRb.MovePosition(originalObjectPos);
    }

    // Для удобства: быстрые пресеты
    [ContextMenu("Light Quake")]
    private void StartLight() => StartEarthquake(3f, 0.05f, 3f);

    [ContextMenu("Medium Quake")]
    private void StartMedium() => StartEarthquake(3f, 0.4f, 12f);

    [ContextMenu("Strong Quake")]
    private void StartStrong() => StartEarthquake(4f, 0.8f, 10f);
}
