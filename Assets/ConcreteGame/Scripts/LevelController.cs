using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Random = UnityEngine.Random;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    [SerializeField] private RectTransform _buildingsContent;
    
    [SerializeField] private RectTransform _menuButton;
    [SerializeField] private RectTransform _panel1;
    [SerializeField] private RectTransform _panel2;
    
    [SerializeField] private int _neededBuldings;
    private int _buildingsCount;

    [SerializeField] private TextMeshProUGUI _buildingsCountText;
    [SerializeField] private RectTransform _startGameButton;
    
    [SerializeField] private GridBoundaryController grid;

    [Header("Meteors Animation")]
    [SerializeField] private RectTransform _meteorsPanel;
    [SerializeField] private RectTransform _meteorIcon;
    [SerializeField] private RectTransform _meteorsBackground;

    [Header("Earthquake Animation")]
    [SerializeField] private RectTransform _earthquakePanel;
    [SerializeField] private RectTransform _earthquakeIcon;
    [SerializeField] private RectTransform _earthquakeBackground;

    [Header("Wind Animation")]
    [SerializeField] private RectTransform _windPanel;
    [SerializeField] private RectTransform _windIcon;
    [SerializeField] private RectTransform _windBackground;

    [Header("References")]
    [SerializeField] private EarthquakeManager _earthquakeManager;
    
    [Header("Stars System")]
    private int totalStars = 0;
    private List<GridMovement> gridMovements = new List<GridMovement>();
    private List<Vector3> initialPositions = new List<Vector3>();
    private List<Vector3> initialRotations = new List<Vector3>();

    private GameObject _currentObject;
    private GameObject _currentButton;

    // Запоминаем изначальные позиции для анимации
    private Vector3 _meteorIconInitialPos;
    private Vector3 _meteorsBackgroundInitialPos;
    private Vector3 _earthquakeIconInitialPos;
    private Vector3 _earthquakeBackgroundInitialPos;
    private Vector3 _windIconInitialPos;
    private Vector3 _windBackgroundInitialPos;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _neededBuldings = _buildingsContent.transform.childCount;
        
        _startGameButton.DOScale(Vector3.zero, 0f);
        _buildingsCountText.text = $"0/{_neededBuldings}";
        
        // Запоминаем изначальные позиции элементов
        _meteorIconInitialPos = _meteorIcon.anchoredPosition;
        _meteorsBackgroundInitialPos = _meteorsBackground.anchoredPosition;
        _earthquakeIconInitialPos = _earthquakeIcon.anchoredPosition;
        _earthquakeBackgroundInitialPos = _earthquakeBackground.anchoredPosition;
        _windIconInitialPos = _windIcon.anchoredPosition;
        _windBackgroundInitialPos = _windBackground.anchoredPosition;
        
        // Правильно скрываем элементы в начале
        InitializeMeteorElements();
        InitializeEarthquakeElements();
        InitializeWindElements();
    }

    private void InitializeMeteorElements()
    {
        // Скрываем панель метеоритов в начале
        _meteorsPanel.gameObject.SetActive(false);
        _meteorsPanel.localScale = Vector3.zero;
        
        // Прячем элементы за пределы экрана
        _meteorIcon.anchoredPosition = _meteorIconInitialPos + Vector3.right * 2000f; // Прячем справа
        _meteorsBackground.anchoredPosition = _meteorsBackgroundInitialPos + Vector3.left * 2000f; // Прячем слева
    }

    private void InitializeEarthquakeElements()
    {
        // Скрываем панель землетрясения в начале
        _earthquakePanel.gameObject.SetActive(false);
        _earthquakePanel.localScale = Vector3.zero;
        
        // Прячем элементы за пределы экрана
        _earthquakeIcon.anchoredPosition = _earthquakeIconInitialPos + Vector3.right * 2000f; // Прячем справа
        _earthquakeBackground.anchoredPosition = _earthquakeBackgroundInitialPos + Vector3.left * 2000f; // Прячем слева
    }

    private void InitializeWindElements()
    {
        // Скрываем панель ветра в начале
        _windPanel.gameObject.SetActive(false);
        _windPanel.localScale = Vector3.zero;
        
        // Прячем элементы за пределы экрана
        _windIcon.anchoredPosition = _windIconInitialPos + Vector3.right * 2000f; // Прячем справа
        _windBackground.anchoredPosition = _windBackgroundInitialPos + Vector3.left * 2000f; // Прячем слева
    }

    public void DenyBuild()
    {
        _currentObject.SetActive(false);
        _currentObject = null;
    }

    public void AllowBuild()
    {
        _currentButton.SetActive(false);
        _buildingsCount++;
        _buildingsCountText.text = $"{_buildingsCount}/{_neededBuldings}";

        if (_buildingsCount >= _neededBuldings)
        {
            ShowStartGameButton();
        }
        _currentObject = null;
    }

    private void ShowStartGameButton()
    {
        _buildingsCountText.rectTransform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete((() =>
            {
                _startGameButton.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.InBack);
            }));
    }

    public void StartGame()
    {
        _startGameButton.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack);
        
        _menuButton.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack);
        _panel1.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack);
        _panel2.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete((() =>
            {
                GridBoundaryController.Instance.HideGrid();
        
                GridMovement[] gridMovements = FindObjectsOfType<GridMovement>();

                foreach (var gm in gridMovements)
                {
                    gm.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                    gm.GetComponent<Rigidbody2D>().mass = 5f;
                    SaveInitialTransforms(gridMovements);
                }
                
                // Запускаем анимацию метеоритов
                StartMeteorAnimation();
            }));
    }

    private void SaveInitialTransforms(GridMovement[] gridMovements)
{
    this.gridMovements.Clear();
    initialPositions.Clear();
    initialRotations.Clear();
    
    foreach (var gm in gridMovements)
    {
        this.gridMovements.Add(gm);
        initialPositions.Add(gm.transform.position);
        initialRotations.Add(gm.transform.eulerAngles);
    }
    
    Debug.Log($"Saved initial transforms for {gridMovements.Length} GridMovement objects");
}

private void CheckObjectsStability(string testName)
{
    bool allObjectsStable = true;
    
    for (int i = 0; i < gridMovements.Count; i++)
    {
        if (gridMovements[i] == null) continue;
        
        Vector3 currentPos = gridMovements[i].transform.position;
        Vector3 currentRot = gridMovements[i].transform.eulerAngles;
        Vector3 initialPos = initialPositions[i];
        Vector3 initialRot = initialRotations[i];
        
        // Проверяем отклонение по Y позиции (20%)
        float posYDeviation = Mathf.Abs(currentPos.y - initialPos.y) / Mathf.Abs(initialPos.y);
        if (posYDeviation > 0.2f)
        {
            allObjectsStable = false;
            Debug.Log($"Object {i} failed Y position check: deviation {posYDeviation:P1}");
            break;
        }
        
        // Проверяем отклонение ротации по всем осям (20%)
        float rotXDeviation = Mathf.Abs(Mathf.DeltaAngle(currentRot.x, initialRot.x)) / 360f;
        float rotYDeviation = Mathf.Abs(Mathf.DeltaAngle(currentRot.y, initialRot.y)) / 360f;
        float rotZDeviation = Mathf.Abs(Mathf.DeltaAngle(currentRot.z, initialRot.z)) / 360f;
        
        if (rotXDeviation > 0.2f || rotYDeviation > 0.2f || rotZDeviation > 0.2f)
        {
            allObjectsStable = false;
            Debug.Log($"Object {i} failed rotation check: X={rotXDeviation:P1}, Y={rotYDeviation:P1}, Z={rotZDeviation:P1}");
            break;
        }
    }
    
    if (allObjectsStable)
    {
        totalStars++;
        Debug.Log($"★ {testName} passed! Total stars: {totalStars}/3");
    }
    else
    {
        Debug.Log($"✗ {testName} failed. Total stars: {totalStars}/3");
    }
}
    
    private void StartMeteorAnimation()
    {
        // Показываем панель метеоритов
        _meteorsPanel.gameObject.SetActive(true);
        
        // Анимируем появление панели
        _meteorsPanel.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // Создаем последовательность анимаций
                Sequence meteorSequence = DOTween.Sequence();
                
                // 1. Анимация MeteorIcon - въезжает справа налево
                meteorSequence.Append(_meteorIcon.DOAnchorPos(_meteorIconInitialPos, 0.6f)
                    .SetEase(Ease.OutQuart));
                
                // 2. Анимация MeteorsBackground - въезжает слева направо (с небольшой задержкой)
                meteorSequence.Insert(0.2f, _meteorsBackground.DOAnchorPos(_meteorsBackgroundInitialPos, 0.6f)
                    .SetEase(Ease.OutQuart));
                
                // 3. Пауза на позициях (0.5 секунды после завершения въезда)
                meteorSequence.AppendInterval(0.5f);
                
                // 4. Прячем элементы обратно
                meteorSequence.AppendCallback(() => HideMeteorElements());
            });
    }

    private void HideMeteorElements()
    {
        Sequence hideSequence = DOTween.Sequence();
        
        // Прячем MeteorIcon вправо за пределы экрана (влетает вправо)
        hideSequence.Append(_meteorIcon.DOAnchorPos(_meteorIconInitialPos + Vector3.right * 2000f, 0.4f)
            .SetEase(Ease.InQuart));
        
        // Прячем MeteorsBackground влево за пределы экрана (влетает влево) - параллельно
        hideSequence.Join(_meteorsBackground.DOAnchorPos(_meteorsBackgroundInitialPos + Vector3.left * 2000f, 0.4f)
            .SetEase(Ease.InQuart));
        
        // После того как элементы уехали за пределы экрана, просто отключаем панель
        hideSequence.OnComplete(() =>
        {
            _meteorsPanel.gameObject.SetActive(false);
            
            // Настраиваем ObjectSpawner и вызываем спавн метеоритов
            SpawnMeteors();
        });
    }

    private void SpawnMeteors()
    {
        // Просто вызываем спавн с настройками из MeteorsSpawner
        MeteorsSpawner.Instance.SpawnObjects();
        
        Debug.Log("Meteors spawned!");
        
        // Начинаем отслеживать падение метеоритов
        StartCoroutine(WaitForMeteorsToFall());
    }

    private IEnumerator WaitForMeteorsToFall()
    {
        // Ждем пока все 5 метеоритов заспавнятся
        while (MeteorsSpawner.Instance.IsSpawning() || MeteorsSpawner.Instance.GetSpawnedObjects().Count < 5)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("All 5 meteors spawned! Starting 3-second timer...");
        
        // Ждем 3 секунды после спавна 5-го метеорита
        yield return new WaitForSeconds(3f);
        
        // Выключаем все метеориты
        DestroyAllMeteors();
        
        CheckObjectsStability("Meteors Test");
        
        // Запускаем анимацию землетрясения
        StartEarthquakeAnimation();
    }

    private void DestroyAllMeteors()
    {
        MeteorsSpawner.Instance.ClearPreviousObjects();
        Debug.Log("All meteors destroyed!");
    }

    private void StartEarthquakeAnimation()
    {
        // Показываем панель землетрясения
        _earthquakePanel.gameObject.SetActive(true);
        
        // Анимируем появление панели
        _earthquakePanel.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // Создаем последовательность анимаций
                Sequence earthquakeSequence = DOTween.Sequence();
                
                // 1. Анимация EarthquakeIcon - въезжает справа налево
                earthquakeSequence.Append(_earthquakeIcon.DOAnchorPos(_earthquakeIconInitialPos, 0.6f)
                    .SetEase(Ease.OutQuart));
                
                // 2. Анимация EarthquakeBackground - въезжает слева направо (с небольшой задержкой)
                earthquakeSequence.Insert(0.2f, _earthquakeBackground.DOAnchorPos(_earthquakeBackgroundInitialPos, 0.6f)
                    .SetEase(Ease.OutQuart));
                
                // 3. Пауза на позициях (0.5 секунды после завершения въезда)
                earthquakeSequence.AppendInterval(0.5f);
                
                // 4. Прячем элементы обратно и запускаем землетрясение
                earthquakeSequence.AppendCallback(() => HideEarthquakeElementsAndStartQuake());
            });
    }

    private void HideEarthquakeElementsAndStartQuake()
    {
        Sequence hideSequence = DOTween.Sequence();
        
        // Прячем EarthquakeIcon вправо за пределы экрана (влетает вправо)
        hideSequence.Append(_earthquakeIcon.DOAnchorPos(_earthquakeIconInitialPos + Vector3.right * 2000f, 0.4f)
            .SetEase(Ease.InQuart));
        
        // Прячем EarthquakeBackground влево за пределы экрана (влетает влево) - параллельно
        hideSequence.Join(_earthquakeBackground.DOAnchorPos(_earthquakeBackgroundInitialPos + Vector3.left * 2000f, 0.4f)
            .SetEase(Ease.InQuart));
        
        // После того как элементы уехали за пределы экрана, отключаем панель и запускаем землетрясение
        hideSequence.OnComplete(() =>
        {
            _earthquakePanel.gameObject.SetActive(false);
            
            // Запускаем землетрясение с указанными параметрами
            if (_earthquakeManager != null)
            {
                _earthquakeManager.StartEarthquake(3f, 0.05f, 3f);
                Debug.Log("Earthquake started!");
                
                // Ждем окончания землетрясения и запускаем анимацию ветра
                StartCoroutine(WaitForEarthquakeToEnd());
            }
            else
            {
                Debug.LogError("EarthquakeManager reference is missing!");
            }
        });
    }

    private IEnumerator WaitForEarthquakeToEnd()
    {
        // Ждем 3 секунды (длительность землетрясения)
        yield return new WaitForSeconds(3f);
        
        Debug.Log("Earthquake ended! Starting wind animation...");
        
        CheckObjectsStability("Earthquake Test");
        
        // Запускаем анимацию ветра
        StartWindAnimation();
    }

    private void StartWindAnimation()
    {
        // Показываем панель ветра
        _windPanel.gameObject.SetActive(true);
        
        // Анимируем появление панели
        _windPanel.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // Создаем последовательность анимаций
                Sequence windSequence = DOTween.Sequence();
                
                // 1. Анимация WindIcon - въезжает справа налево
                windSequence.Append(_windIcon.DOAnchorPos(_windIconInitialPos, 0.6f)
                    .SetEase(Ease.OutQuart));
                
                // 2. Анимация WindBackground - въезжает слева направо (с небольшой задержкой)
                windSequence.Insert(0.2f, _windBackground.DOAnchorPos(_windBackgroundInitialPos, 0.6f)
                    .SetEase(Ease.OutQuart));
                
                // 3. Пауза на позициях (0.5 секунды после завершения въезда)
                windSequence.AppendInterval(0.5f);
                
                // 4. Прячем элементы обратно и запускаем ветер
                windSequence.AppendCallback(() => HideWindElementsAndStartWind());
            });
    }

    private void HideWindElementsAndStartWind()
    {
        Sequence hideSequence = DOTween.Sequence();
        
        // Прячем WindIcon вправо за пределы экрана (влетает вправо)
        hideSequence.Append(_windIcon.DOAnchorPos(_windIconInitialPos + Vector3.right * 2000f, 0.4f)
            .SetEase(Ease.InQuart));
        
        // Прячем WindBackground влево за пределы экрана (влетает влево) - параллельно
        hideSequence.Join(_windBackground.DOAnchorPos(_windBackgroundInitialPos + Vector3.left * 2000f, 0.4f)
            .SetEase(Ease.InQuart));
        
        // После того как элементы уехали за пределы экрана, отключаем панель и запускаем ветер
        hideSequence.OnComplete(() =>
        {
            _windPanel.gameObject.SetActive(false);
            
            // Запускаем ветер с указанными параметрами (1 секунда, сила 1)
            if (WindManager.Instance != null)
            {
                WindManager.Instance.StartWind(1.5f, 3f);
                Debug.Log("Wind started!");
                StartCoroutine(WaitForWindToEnd());
            }
            else
            {
                Debug.LogError("WindManager instance is missing!");
            }
        });
    }
    
    private IEnumerator WaitForWindToEnd()
    {
        // Ждем 1.5 секунды (длительность ветра)
        yield return new WaitForSeconds(1.5f);
    
        Debug.Log("Wind ended! Checking final stability...");
    
        // Проверяем стабильность после ветра
        CheckObjectsStability("Wind Test");
    
        // Выводим финальный результат
        Debug.Log($"🏆 FINAL RESULT: {totalStars}/3 stars earned!");
    }
    
    public void SpawnOnGrid(GameObject prefab, GameObject button)
    {
        if (grid == null || prefab == null)
        {
            Debug.LogWarning("GridBoundaryController или Prefab не назначен!");
            return;
        }

        if (_currentObject != null)
        {
            _currentObject.SetActive(false);
            _currentObject = null;
        }
        
        _currentButton = button;
        
        Vector2 spawnPosition = FindFreeSpawnPosition(prefab);
        
        if (spawnPosition != Vector2.zero)
        {
            Vector3 spawnPos = new Vector3(spawnPosition.x, spawnPosition.y, 0);
            GameObject obj = Instantiate(prefab, spawnPos, prefab.transform.rotation);
            _currentObject = obj;
            
            UIController.Instance.CloseBuildingPanel();
        }
        else
        {
            Debug.Log("Не удалось найти свободное место для спавна!");
            // Можно добавить уведомление для игрока о том, что нет места
        }
    }

    private Vector2 FindFreeSpawnPosition(GameObject prefab)
    {
        float gSize = grid.GridSize;

        // Получаем границы сетки
        Vector3 gridMin = grid.GetBottomLeft();
        Vector3 gridMax = grid.GetTopRight();

        // Определяем размер объекта
        Vector2 objectSize = GetPrefabSize(prefab);

        List<Vector2> allCells = new List<Vector2>();

        // Рассчитываем допустимые границы для центра объекта с учетом его размера
        float minX = gridMin.x + objectSize.x / 2f;
        float maxX = gridMax.x - objectSize.x / 2f;
        float minY = gridMin.y + objectSize.y / 2f;
        float maxY = gridMax.y - objectSize.y / 2f;

        // Проверяем, что объект вообще может поместиться в сетку
        if (minX > maxX || minY > maxY)
        {
            Debug.LogWarning($"Объект слишком большой для сетки! Размер объекта: {objectSize}, размер сетки: {gridMax - gridMin}");
            return Vector2.zero;
        }

        // Собираем все возможные позиции на сетке с учетом размера объекта
        for (float x = minX; x <= maxX; x += gSize)
        {
            for (float y = minY; y <= maxY; y += gSize)
            {
                // Привязываем к сетке
                float alignedX = Mathf.Round(x / gSize) * gSize;
                float alignedY = Mathf.Round(y / gSize) * gSize;
                
                // Проверяем, что выровненная позиция все еще в допустимых границах
                if (alignedX >= minX && alignedX <= maxX && 
                    alignedY >= minY && alignedY <= maxY)
                {
                    Vector2 cellCenter = new Vector2(alignedX, alignedY);
                    allCells.Add(cellCenter);
                }
            }
        }

        // Перемешиваем список для случайного порядка проверки
        for (int i = 0; i < allCells.Count; i++)
        {
            Vector2 temp = allCells[i];
            int randomIndex = Random.Range(i, allCells.Count);
            allCells[i] = allCells[randomIndex];
            allCells[randomIndex] = temp;
        }

        // Проверяем каждую позицию на свободность
        foreach (Vector2 cellCenter in allCells)
        {
            if (IsPositionFree(cellCenter, objectSize))
            {
                return cellCenter;
            }
        }

        // Если не нашли свободных клеток, возвращаем нулевой вектор
        return Vector2.zero;
    }

    private Vector2 GetPrefabSize(GameObject prefab)
    {
        Vector2 size = Vector2.one;

        // Создаем временный объект для точного измерения
        GameObject tempObj = Instantiate(prefab);
    
        // Пробуем получить размер из коллайдера
        Collider2D prefabCollider = tempObj.GetComponent<Collider2D>();
        if (prefabCollider != null)
        {
            size = prefabCollider.bounds.size;
        }
        else
        {
            // Если коллайдера нет, пробуем SpriteRenderer
            SpriteRenderer prefabSprite = tempObj.GetComponent<SpriteRenderer>();
            if (prefabSprite != null && prefabSprite.sprite != null)
            {
                size = prefabSprite.bounds.size;
            }
            else
            {
                size = Vector2.one * grid.GridSize;
            }
        }
    
        // Удаляем временный объект
        DestroyImmediate(tempObj);
    
        return size;
    }

    private bool IsPositionFree(Vector2 centerPosition, Vector2 objectSize)
    {
        // Проверяем, что весь объект помещается в границы сетки
        Vector2 halfSize = objectSize / 2f;
        Vector3 gridMin = grid.GetBottomLeft();
        Vector3 gridMax = grid.GetTopRight();

        float leftEdge = centerPosition.x - halfSize.x;
        float rightEdge = centerPosition.x + halfSize.x;
        float bottomEdge = centerPosition.y - halfSize.y;
        float topEdge = centerPosition.y + halfSize.y;

        // Проверяем границы сетки
        float epsilon = 0.01f;
        if (leftEdge < gridMin.x + epsilon || 
            rightEdge > gridMax.x - epsilon || 
            bottomEdge < gridMin.y + epsilon || 
            topEdge > gridMax.y - epsilon)
        {
            return false;
        }

        // ВАЖНО: используем полный размер объекта, а не уменьшенный
        Vector2 checkSize = objectSize;
    
        Collider2D[] overlapping = Physics2D.OverlapBoxAll(centerPosition, checkSize, 0f);
    
        foreach (Collider2D col in overlapping)
        {
            if (col != null && !col.isTrigger)
            {
                return false;
            }
        }

        return true;
    }

    // Дополнительный метод для принудительного поиска места (если нужно)
    public Vector2 FindNearestFreePosition(Vector2 preferredPosition, GameObject prefab)
    {
        float gSize = grid.GridSize;
        Vector2 objectSize = GetPrefabSize(prefab);

        // Сначала проверяем предпочитаемую позицию
        if (IsPositionFree(preferredPosition, objectSize))
        {
            return preferredPosition;
        }

        // Спиральный поиск вокруг предпочитаемой позиции
        Vector3 gridMin = grid.GetBottomLeft();
        Vector3 gridMax = grid.GetTopRight();
        
        int maxRadius = Mathf.Max(
            Mathf.CeilToInt((gridMax.x - gridMin.x) / gSize),
            Mathf.CeilToInt((gridMax.y - gridMin.y) / gSize)
        );

        for (int radius = 1; radius <= maxRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    // Проверяем только клетки на границе текущего радиуса
                    if (Mathf.Abs(x) == radius || Mathf.Abs(y) == radius)
                    {
                        Vector2 testPosition = preferredPosition + new Vector2(x * gSize, y * gSize);
                        
                        if (IsPositionFree(testPosition, objectSize))
                        {
                            return testPosition;
                        }
                    }
                }
            }
        }

        return Vector2.zero; // Не найдено свободного места
    }
}