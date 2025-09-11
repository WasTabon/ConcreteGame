using System.Collections;
using UnityEngine;

public class EarthquakeManager : MonoBehaviour
{
    [Header("Earthquake Settings")]
    [SerializeField] private float duration = 3f; // Длительность землетрясения в секундах
    [SerializeField] private float amplitude = 0.5f; // Амплитуда тряски
    [SerializeField] private float frequency = 10f; // Частота тряски
    
    // Singleton instance
    private static EarthquakeManager _instance;
    public static EarthquakeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<EarthquakeManager>();
                
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("EarthquakeManager");
                    _instance = singletonObject.AddComponent<EarthquakeManager>();
                }
            }
            return _instance;
        }
    }
    
    // Ссылки на камеру и объект для тряски
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform targetObject; // Объект, который будет трястись (опционально)
    
    // Оригинальные позиции для восстановления
    private Vector3 originalCameraPosition;
    private Vector3 originalObjectPosition;
    
    // Статус землетрясения
    private bool isEarthquakeActive = false;
    private Coroutine earthquakeCoroutine;
    
    private void Awake()
    {
        // Проверяем, что это единственный экземпляр
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Автоматически находим камеру, если она не назначена
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindObjectOfType<Camera>();
            }
        }
        
        // Сохраняем оригинальные позиции
        if (targetCamera != null)
        {
            originalCameraPosition = targetCamera.transform.position;
        }
        
        if (targetObject != null)
        {
            originalObjectPosition = targetObject.position;
        }
    }
    
    /// <summary>
    /// Запускает землетрясение с настройками по умолчанию
    /// </summary>
    public void StartEarthquake()
    {
        StartEarthquake(duration, amplitude, frequency);
    }
    
    /// <summary>
    /// Запускает землетрясение с кастомными параметрами
    /// </summary>
    /// <param name="customDuration">Длительность землетрясения</param>
    /// <param name="customAmplitude">Амплитуда тряски</param>
    /// <param name="customFrequency">Частота тряски</param>
    public void StartEarthquake(float customDuration, float customAmplitude, float customFrequency)
    {
        if (isEarthquakeActive)
        {
            Debug.LogWarning("Earthquake is already active!");
            return;
        }
        
        if (targetCamera == null)
        {
            Debug.LogError("Target camera is not assigned!");
            return;
        }
        
        Debug.Log($"Starting earthquake - Duration: {customDuration}s, Amplitude: {customAmplitude}, Frequency: {customFrequency}");
        earthquakeCoroutine = StartCoroutine(EarthquakeCoroutine(customDuration, customAmplitude, customFrequency));
    }
    
    /// <summary>
    /// Останавливает землетрясение принудительно
    /// </summary>
    public void StopEarthquake()
    {
        if (earthquakeCoroutine != null)
        {
            StopCoroutine(earthquakeCoroutine);
            earthquakeCoroutine = null;
        }
        
        RestorePositions();
        isEarthquakeActive = false;
        
        Debug.Log("Earthquake stopped manually.");
    }
    
    private IEnumerator EarthquakeCoroutine(float shakeDuration, float shakeAmplitude, float shakeFrequency)
    {
        isEarthquakeActive = true;
        float elapsedTime = 0f;
    
        while (elapsedTime < shakeDuration)
        {
            // Вычисляем смещение по X на основе синусоидальной функции
            float offsetX = Mathf.Sin(elapsedTime * shakeFrequency) * shakeAmplitude;
        
            // Вычисляем смещение по Y на основе косинусоидальной функции с другой частотой
            float offsetY = Mathf.Cos(elapsedTime * shakeFrequency * 1.3f) * shakeAmplitude * 0.6f;
        
            // Добавляем случайные вариации для более реалистичной тряски во все стороны
            float randomX = Random.Range(-0.3f, 0.3f) * shakeAmplitude;
            float randomY = Random.Range(-0.2f, 0.2f) * shakeAmplitude;
        
            offsetX += randomX;
            offsetY += randomY;
        
            // Создаем вектор смещения
            Vector3 shakeOffset = new Vector3(offsetX, offsetY, 0);
        
            // Применяем тряску к камере
            if (targetCamera != null)
            {
                Vector3 newCameraPos = originalCameraPosition + shakeOffset;
                targetCamera.transform.position = newCameraPos;
            }
        
            // Применяем тряску к объекту (если назначен)
            if (targetObject != null)
            {
                Vector3 newObjectPos = originalObjectPosition + shakeOffset;
                targetObject.position = newObjectPos;
            }
        
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    
        // Восстанавливаем оригинальные позиции
        RestorePositions();
        isEarthquakeActive = false;
        earthquakeCoroutine = null;
    
        Debug.Log("Earthquake completed.");
    }
    
    /// <summary>
    /// Восстанавливает оригинальные позиции камеры и объекта
    /// </summary>
    private void RestorePositions()
    {
        if (targetCamera != null)
        {
            targetCamera.transform.position = originalCameraPosition;
        }
        
        if (targetObject != null)
        {
            targetObject.position = originalObjectPosition;
        }
    }
    
    /// <summary>
    /// Проверяет, активно ли сейчас землетрясение
    /// </summary>
    public bool IsEarthquakeActive()
    {
        return isEarthquakeActive;
    }
    
    /// <summary>
    /// Устанавливает камеру для тряски
    /// </summary>
    public void SetTargetCamera(Camera camera)
    {
        targetCamera = camera;
        if (camera != null && !isEarthquakeActive)
        {
            originalCameraPosition = camera.transform.position;
        }
    }
    
    /// <summary>
    /// Устанавливает объект для тряски
    /// </summary>
    public void SetTargetObject(Transform obj)
    {
        targetObject = obj;
        if (obj != null && !isEarthquakeActive)
        {
            originalObjectPosition = obj.position;
        }
    }
    
    /// <summary>
    /// Устанавливает длительность землетрясения по умолчанию
    /// </summary>
    public void SetDefaultDuration(float newDuration)
    {
        duration = newDuration;
    }
    
    /// <summary>
    /// Устанавливает амплитуду землетрясения по умолчанию
    /// </summary>
    public void SetDefaultAmplitude(float newAmplitude)
    {
        amplitude = newAmplitude;
    }
    
    /// <summary>
    /// Устанавливает частоту землетрясения по умолчанию
    /// </summary>
    public void SetDefaultFrequency(float newFrequency)
    {
        frequency = newFrequency;
    }
    
    /// <summary>
    /// Обновляет сохраненные оригинальные позиции (полезно, если объекты изменили позицию)
    /// </summary>
    public void UpdateOriginalPositions()
    {
        if (!isEarthquakeActive)
        {
            if (targetCamera != null)
            {
                originalCameraPosition = targetCamera.transform.position;
            }
            
            if (targetObject != null)
            {
                originalObjectPosition = targetObject.position;
            }
            
            Debug.Log("Original positions updated.");
        }
        else
        {
            Debug.LogWarning("Cannot update positions during earthquake!");
        }
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    // Методы для удобного использования в инспекторе или других скриптах
    
    /// <summary>
    /// Быстрый запуск слабого землетрясения
    /// </summary>
    [ContextMenu("Start Light Earthquake")]
    public void StartLightEarthquake()
    {
        StartEarthquake(2f, 0.15f, 18f);
    }
    
    /// <summary>
    /// Быстрый запуск среднего землетрясения
    /// </summary>
    [ContextMenu("Start Medium Earthquake")]
    public void StartMediumEarthquake()
    {
        StartEarthquake(3f, 0.4f, 12f);
    }
    
    /// <summary>
    /// Быстрый запуск сильного землетрясения
    /// </summary>
    [ContextMenu("Start Strong Earthquake")]
    public void StartStrongEarthquake()
    {
        StartEarthquake(4f, 0.8f, 10f);
    }
}