using UnityEngine;
using DG.Tweening;

public class MeteorCollisionHandler : MonoBehaviour
{
    [Header("Screen Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeStrength = 0.2f;

    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    private bool hasCollided = false;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasCollided) return;

        // Проверяем столкновение с объектом с тегом "Player"
        if (collision.gameObject.CompareTag("Player"))
        {
            TriggerCameraShake();
            //hasCollided = true;
            return;
        }

        // Проверяем столкновение с объектом с компонентом GridMovement
        GridMovement gridMovement = collision.gameObject.GetComponent<GridMovement>();
        if (gridMovement != null)
        {
            TriggerCameraShake();
            //hasCollided = true;
        }
    }

    private void TriggerCameraShake()
    {
        if (mainCamera != null)
        {
            Debug.Log("Meteor hit! Triggering camera shake...");
            
            // Останавливаем предыдущие анимации камеры
            mainCamera.transform.DOKill();
            
            // Применяем тряску камеры
            mainCamera.transform.DOShakePosition(
                duration: shakeDuration,
                strength: shakeStrength,
                vibrato: 10,
                randomness: 90f,
                snapping: false,
                fadeOut: true
            ).OnComplete(() => {
                // Возвращаем камеру в исходную позицию
                mainCamera.transform.position = originalCameraPosition;
            });
        }
    }
}