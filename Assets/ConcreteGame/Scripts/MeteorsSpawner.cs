using System.Collections.Generic;
using UnityEngine;

public class MeteorsSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject prefabToSpawn; // Префаб для спавна
    [SerializeField] private float fixedY = 0f; // Фиксированная Y позиция
    [SerializeField] private Transform leftBound; // Левая граница для X
    [SerializeField] private Transform rightBound; // Правая граница для X
    [SerializeField] private int objectCount = 5; // Количество объектов для спавна
    
    [Header("Movement Settings")]
    [SerializeField] private float downForce = 5f; // Сила вниз
    [SerializeField] private float maxSideForce = 2f; // Максимальная боковая сила для отклонения
    
    // Singleton instance
    private static MeteorsSpawner _instance;
    public static MeteorsSpawner Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MeteorsSpawner>();
                
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("ObjectSpawner");
                    _instance = singletonObject.AddComponent<MeteorsSpawner>();
                }
            }
            return _instance;
        }
    }
    
    // Список заспавненных объектов
    private List<GameObject> spawnedObjects = new List<GameObject>();
    
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
    }
    
    /// <summary>
    /// Спавнит объекты в рандомных позициях
    /// </summary>
    public void SpawnObjects()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogError("Prefab to spawn is not assigned!");
            return;
        }
        
        if (leftBound == null || rightBound == null)
        {
            Debug.LogError("Left or Right bound is not assigned!");
            return;
        }
        
        ClearPreviousObjects();
        
        float leftX = leftBound.position.x;
        float rightX = rightBound.position.x;
        
        // Убеждаемся, что leftX меньше rightX
        if (leftX > rightX)
        {
            float temp = leftX;
            leftX = rightX;
            rightX = temp;
        }
        
        for (int i = 0; i < objectCount; i++)
        {
            // Генерируем рандомную X позицию между границами
            float randomX = Random.Range(leftX, rightX);
            Vector3 spawnPosition = new Vector3(randomX, fixedY, 0f);
            
            // Спавним объект
            GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            spawnedObjects.Add(spawnedObject);
            
            // Добавляем движение
            AddMovementToMeteor(spawnedObject);
        }
        
        Debug.Log($"Spawned {objectCount} objects between X: {leftX} and X: {rightX} at Y: {fixedY}");
    }
    
    /// <summary>
    /// Спавнит объекты с кастомными параметрами
    /// </summary>
    /// <param name="prefab">Префаб для спавна</param>
    /// <param name="count">Количество объектов</param>
    /// <param name="yPosition">Y позиция</param>
    /// <param name="minX">Минимальная X позиция</param>
    /// <param name="maxX">Максимальная X позиция</param>
    public void SpawnObjects(GameObject prefab, int count, float yPosition, float minX, float maxX)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is null!");
            return;
        }
        
        ClearPreviousObjects();
        
        for (int i = 0; i < count; i++)
        {
            float randomX = Random.Range(minX, maxX);
            Vector3 spawnPosition = new Vector3(randomX, yPosition, 0f);
            
            GameObject spawnedObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
            spawnedObjects.Add(spawnedObject);
            
            // Добавляем движение
            AddMovementToMeteor(spawnedObject);
        }
        
        Debug.Log($"Spawned {count} objects between X: {minX} and X: {maxX} at Y: {yPosition}");
    }
    
    /// <summary>
    /// Добавляет движение метеору
    /// </summary>
    private void AddMovementToMeteor(GameObject meteor)
    {
        Rigidbody2D rb = meteor.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Рандомное боковое отклонение
            float sideForce = Random.Range(-maxSideForce, maxSideForce);
            
            // Создаем вектор силы (всегда вниз + небольшое боковое отклонение)
            Vector2 force = new Vector2(sideForce, -downForce);
            
            // Применяем силу
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
    
    /// <summary>
    /// Очищает все заспавненные объекты
    /// </summary>
    public void ClearPreviousObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        spawnedObjects.Clear();
    }
    
    /// <summary>
    /// Получить все заспавненные объекты
    /// </summary>
    public List<GameObject> GetSpawnedObjects()
    {
        return new List<GameObject>(spawnedObjects);
    }
    
    /// <summary>
    /// Установить новые границы для спавна
    /// </summary>
    public void SetBounds(Transform left, Transform right)
    {
        leftBound = left;
        rightBound = right;
    }
    
    /// <summary>
    /// Установить фиксированную Y позицию
    /// </summary>
    public void SetFixedY(float y)
    {
        fixedY = y;
    }
    
    /// <summary>
    /// Установить количество объектов для спавна
    /// </summary>
    public void SetObjectCount(int count)
    {
        objectCount = count;
    }
    
    /// <summary>
    /// Установить силу падения
    /// </summary>
    public void SetDownForce(float force)
    {
        downForce = force;
    }
    
    /// <summary>
    /// Установить максимальную боковую силу
    /// </summary>
    public void SetMaxSideForce(float force)
    {
        maxSideForce = force;
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}