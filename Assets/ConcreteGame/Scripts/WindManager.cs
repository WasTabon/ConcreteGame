using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WindManager : MonoBehaviour
{
    [Header("Wind Settings")]
    [SerializeField] private float duration = 3f;
    [SerializeField] private float windForce = 5f;
    [SerializeField] private Vector2 windDirection = Vector2.left;
    
    [Header("Target Settings")]
    [SerializeField] private string targetTag = "Building";
    [SerializeField] private LayerMask targetLayers = -1;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip sound;
    
    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem windParticles;
    [SerializeField] private float particleOffsetFromCamera = 2f;
    [SerializeField] private float particleMoveDuration = 2f;
    
    private static WindManager _instance;
    public static WindManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<WindManager>();
                
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("WindManager");
                    _instance = singletonObject.AddComponent<WindManager>();
                }
            }
            return _instance;
        }
    }
    
    private bool isWindActive;
    private Coroutine windCoroutine;
    private List<Rigidbody2D> affectedObjects = new List<Rigidbody2D>();
    private Camera mainCamera;
    private List<ParticleSystem> spawnedParticles = new List<ParticleSystem>();
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        mainCamera = Camera.main;
    }
    
    public void StartWind()
    {
        StartWind(duration, windForce);
    }
    
    public void StartWind(float customDuration, float customForce)
    {
        StartWind(customDuration, customForce, windDirection);
    }
    
    public void StartWind(float customDuration, float customForce, Vector2 customDirection)
    {
        if (isWindActive)
        {
            StopWind();
        }
        
        PlayWindSound();
        StartWindParticles();
        
        windCoroutine = StartCoroutine(WindCoroutine(customDuration, customForce, customDirection.normalized));
    }
    
    public void StopWind()
    {
        if (windCoroutine != null)
        {
            StopCoroutine(windCoroutine);
            windCoroutine = null;
        }
        
        isWindActive = false;
        affectedObjects.Clear();
        StopWindParticles();
    }
    
    private void PlayWindSound()
    {
        if (sound != null && MusicController.Instance != null)
        {
            MusicController.Instance.PlaySpecificSound(sound);
        }
    }
    
    private void StartWindParticles()
    {
        if (windParticles != null && mainCamera != null)
        {
            ClearPreviousParticles();
            
            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;
            Vector3 cameraPos = mainCamera.transform.position;
            
            float startX = cameraPos.x + (cameraWidth / 2f) + particleOffsetFromCamera;
            float endX = cameraPos.x - (cameraWidth / 2f) - particleOffsetFromCamera;
            
            for (int i = 0; i < 3; i++)
            {
                float randomY = Random.Range(cameraPos.y - cameraHeight / 2f, cameraPos.y + cameraHeight / 2f);
                
                ParticleSystem particle = Instantiate(windParticles);
                particle.transform.position = new Vector3(startX, randomY, cameraPos.z);
                
                particle.Play();
                spawnedParticles.Add(particle);
                
                particle.transform.DOMoveX(endX, particleMoveDuration)
                    .SetEase(Ease.Linear)
                    .OnComplete(() => {
                        if (particle != null)
                        {
                            particle.Stop();
                            Destroy(particle.gameObject, 1f);
                        }
                    });
            }
        }
    }
    
    private void StopWindParticles()
    {
        ClearPreviousParticles();
    }
    
    private void ClearPreviousParticles()
    {
        foreach (ParticleSystem particle in spawnedParticles)
        {
            if (particle != null)
            {
                particle.transform.DOKill();
                particle.Stop();
                Destroy(particle.gameObject);
            }
        }
        spawnedParticles.Clear();
    }
    
    private IEnumerator WindCoroutine(float windDuration, float force, Vector2 direction)
    {
        isWindActive = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < windDuration)
        {
            FindAffectedObjects();
            ApplyWindForce(force, direction);
            
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        
        isWindActive = false;
        affectedObjects.Clear();
        StopWindParticles();
    }
    
    private void FindAffectedObjects()
    {
        affectedObjects.Clear();
        
        Rigidbody2D[] allRigidbodies = FindObjectsOfType<Rigidbody2D>();
        
        foreach (Rigidbody2D rb in allRigidbodies)
        {
            if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic)
                continue;
                
            if (!string.IsNullOrEmpty(targetTag) && !rb.gameObject.CompareTag(targetTag))
                continue;
                
            if (((1 << rb.gameObject.layer) & targetLayers) == 0)
                continue;
                
            affectedObjects.Add(rb);
        }
    }
    
    private void ApplyWindForce(float force, Vector2 direction)
    {
        foreach (Rigidbody2D rb in affectedObjects)
        {
            if (rb == null)
                continue;
                
            Vector2 windForceVector = direction * force;
            rb.AddForce(windForceVector, ForceMode2D.Force);
        }
    }
    
    public bool IsWindActive()
    {
        return isWindActive;
    }
    
    [ContextMenu("Test Wind")]
    private void TestWind()
    {
        StartWind();
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}