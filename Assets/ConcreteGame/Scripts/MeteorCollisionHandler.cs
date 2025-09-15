using UnityEngine;
using DG.Tweening;

public class MeteorCollisionHandler : MonoBehaviour
{
    [Header("Screen Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeStrength = 0.2f;
    [SerializeField] private float soundCooldown = 0.1f;

    public AudioClip sound;
    
    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    private bool hasCollided = false;
    private float lastSoundTime = 0f;

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

        gameObject.GetComponentInChildren<ParticleSystem>().gameObject.SetActive(false);
        
        if (collision.gameObject.CompareTag("Player"))
        {
            TriggerCameraShake();
            return;
        }

        GridMovement gridMovement = collision.gameObject.GetComponent<GridMovement>();
        if (gridMovement != null)
        {
            TriggerCameraShake();
        }
    }

    private void TriggerCameraShake()
    {
        if (mainCamera != null)
        {
            Debug.Log("Meteor hit! Triggering camera shake...");
            
            if (Time.time >= lastSoundTime + soundCooldown)
            {
                MusicController.Instance.PlaySpecificSound(sound);
                lastSoundTime = Time.time;
            }
            
            mainCamera.transform.DOKill();
            
            mainCamera.transform.DOShakePosition(
                duration: shakeDuration,
                strength: shakeStrength,
                vibrato: 10,
                randomness: 90f,
                snapping: false,
                fadeOut: true
            ).OnComplete(() => {
                mainCamera.transform.position = originalCameraPosition;
            });
        }
    }
}