using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Random = UnityEngine.Random;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    public GameObject allPanel;
    public GameObject surePanel;
    public GameObject buyPanel;
    
    public GameObject removeBuildingPanel;
    public bool isRemove = false;
    
    public AudioLowPassFilter lowPassFilter;
    public AudioClip spawnSound;
    public AudioClip sound;
    public AudioClip music;
    
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
        
        _meteorIconInitialPos = _meteorIcon.anchoredPosition;
        _meteorsBackgroundInitialPos = _meteorsBackground.anchoredPosition;
        _earthquakeIconInitialPos = _earthquakeIcon.anchoredPosition;
        _earthquakeBackgroundInitialPos = _earthquakeBackground.anchoredPosition;
        _windIconInitialPos = _windIcon.anchoredPosition;
        _windBackgroundInitialPos = _windBackground.anchoredPosition;
        
        InitializeMeteorElements();
        InitializeEarthquakeElements();
        InitializeWindElements();
        
        int remove =  PlayerPrefs.GetInt("isRemove", 0);
        if (remove == 1)
            isRemove = true;
        else
            isRemove = false;
        
        PlayerPrefs.SetInt("isRemove", 0);
    }

    public void OpenBuyPanel()
    {
        if (isRemove)
        {
            surePanel.SetActive(false);
            allPanel.SetActive(false);
        }
        else
        {
            buyPanel.SetActive(true);
        }
    }
    
    private void InitializeMeteorElements()
    {
        _meteorsPanel.gameObject.SetActive(false);
        _meteorsPanel.localScale = Vector3.zero;
        
        _meteorIcon.anchoredPosition = _meteorIconInitialPos + Vector3.right * 2000f;
        _meteorsBackground.anchoredPosition = _meteorsBackgroundInitialPos + Vector3.left * 2000f;
    }

    private void InitializeEarthquakeElements()
    {
        _earthquakePanel.gameObject.SetActive(false);
        _earthquakePanel.localScale = Vector3.zero;
        
        _earthquakeIcon.anchoredPosition = _earthquakeIconInitialPos + Vector3.right * 2000f;
        _earthquakeBackground.anchoredPosition = _earthquakeBackgroundInitialPos + Vector3.left * 2000f;
    }

    private void InitializeWindElements()
    {
        _windPanel.gameObject.SetActive(false);
        _windPanel.localScale = Vector3.zero;
        
        _windIcon.anchoredPosition = _windIconInitialPos + Vector3.right * 2000f;
        _windBackground.anchoredPosition = _windBackgroundInitialPos + Vector3.left * 2000f;
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
                
                SwitchMusicSmooth(music);
                
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
        
        float posYDeviation = Mathf.Abs(currentPos.y - initialPos.y) / Mathf.Abs(initialPos.y);
        if (posYDeviation > 0.2f)
        {
            allObjectsStable = false;
            Debug.Log($"Object {i} failed Y position check: deviation {posYDeviation:P1}");
            break;
        }
        
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
        _meteorsPanel.gameObject.SetActive(true);
        
        _meteorsPanel.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                Sequence meteorSequence = DOTween.Sequence();
                
                meteorSequence.Append(_meteorIcon.DOAnchorPos(_meteorIconInitialPos, 0.6f)
                    .SetEase(Ease.OutQuart)
                    .OnStart(() => {
                        if (sound != null && MusicController.Instance != null)
                        {
                            MusicController.Instance.PlaySpecificSound(sound);
                        }
                    }));
                
                meteorSequence.Insert(0.2f, _meteorsBackground.DOAnchorPos(_meteorsBackgroundInitialPos, 0.6f)
                    .SetEase(Ease.OutQuart));
                
                meteorSequence.AppendInterval(0.5f);
                
                meteorSequence.AppendCallback(() => HideMeteorElements());
            });
    }

    private void HideMeteorElements()
    {
        Sequence hideSequence = DOTween.Sequence();
        
        hideSequence.Append(_meteorIcon.DOAnchorPos(_meteorIconInitialPos + Vector3.right * 2000f, 0.4f)
            .SetEase(Ease.InQuart));
        
        hideSequence.Join(_meteorsBackground.DOAnchorPos(_meteorsBackgroundInitialPos + Vector3.left * 2000f, 0.4f)
            .SetEase(Ease.InQuart));
        
        hideSequence.OnComplete(() =>
        {
            _meteorsPanel.gameObject.SetActive(false);
            
            SpawnMeteors();
        });
    }

    private void SpawnMeteors()
    {
        MeteorsSpawner.Instance.SpawnObjects();
        
        Debug.Log("Meteors spawned!");
        
        StartCoroutine(WaitForMeteorsToFall());
    }

    private IEnumerator WaitForMeteorsToFall()
    {
        while (MeteorsSpawner.Instance.IsSpawning() || MeteorsSpawner.Instance.GetSpawnedObjects().Count < 5)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("All 5 meteors spawned! Starting 3-second timer...");
        
        yield return new WaitForSeconds(3f);
        
        DestroyAllMeteors();
        
        CheckObjectsStability("Meteors Test");
        
        StartEarthquakeAnimation();
    }

    private void DestroyAllMeteors()
    {
        MeteorsSpawner.Instance.ClearPreviousObjects();
        Debug.Log("All meteors destroyed!");
    }

    private void StartEarthquakeAnimation()
    {
        _earthquakePanel.gameObject.SetActive(true);
        
        _earthquakePanel.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                Sequence earthquakeSequence = DOTween.Sequence();
                
                earthquakeSequence.Append(_earthquakeIcon.DOAnchorPos(_earthquakeIconInitialPos, 0.6f)
                    .SetEase(Ease.OutQuart)
                    .OnStart(() => {
                        if (sound != null && MusicController.Instance != null)
                        {
                            MusicController.Instance.PlaySpecificSound(sound);
                        }
                    }));
                
                earthquakeSequence.Insert(0.2f, _earthquakeBackground.DOAnchorPos(_earthquakeBackgroundInitialPos, 0.6f)
                    .SetEase(Ease.OutQuart));
                
                earthquakeSequence.AppendInterval(0.5f);
                
                earthquakeSequence.AppendCallback(() => HideEarthquakeElementsAndStartQuake());
            });
    }

    private void HideEarthquakeElementsAndStartQuake()
    {
        Sequence hideSequence = DOTween.Sequence();
        
        hideSequence.Append(_earthquakeIcon.DOAnchorPos(_earthquakeIconInitialPos + Vector3.right * 2000f, 0.4f)
            .SetEase(Ease.InQuart));
        
        hideSequence.Join(_earthquakeBackground.DOAnchorPos(_earthquakeBackgroundInitialPos + Vector3.left * 2000f, 0.4f)
            .SetEase(Ease.InQuart));
        
        hideSequence.OnComplete(() =>
        {
            _earthquakePanel.gameObject.SetActive(false);
            
            if (_earthquakeManager != null)
            {
                _earthquakeManager.StartEarthquake(3f, 0.05f, 3f);
                Debug.Log("Earthquake started!");
                
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
        yield return new WaitForSeconds(3f);
        
        Debug.Log("Earthquake ended! Starting wind animation...");
        
        CheckObjectsStability("Earthquake Test");
        
        StartWindAnimation();
    }

    private void StartWindAnimation()
    {
        _windPanel.gameObject.SetActive(true);
        
        _windPanel.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                Sequence windSequence = DOTween.Sequence();
                
                windSequence.Append(_windIcon.DOAnchorPos(_windIconInitialPos, 0.6f)
                    .SetEase(Ease.OutQuart)
                    .OnStart(() => {
                        if (sound != null && MusicController.Instance != null)
                        {
                            MusicController.Instance.PlaySpecificSound(sound);
                        }
                    }));
                
                windSequence.Insert(0.2f, _windBackground.DOAnchorPos(_windBackgroundInitialPos, 0.6f)
                    .SetEase(Ease.OutQuart));
                
                windSequence.AppendInterval(0.5f);
                
                windSequence.AppendCallback(() => HideWindElementsAndStartWind());
            });
    }

    private void HideWindElementsAndStartWind()
    {
        Sequence hideSequence = DOTween.Sequence();
        
        hideSequence.Append(_windIcon.DOAnchorPos(_windIconInitialPos + Vector3.right * 2000f, 0.4f)
            .SetEase(Ease.InQuart));
        
        hideSequence.Join(_windBackground.DOAnchorPos(_windBackgroundInitialPos + Vector3.left * 2000f, 0.4f)
            .SetEase(Ease.InQuart));
        
        hideSequence.OnComplete(() =>
        {
            _windPanel.gameObject.SetActive(false);
            
            if (WindManager.Instance != null)
            {
                WindManager.Instance.StartWind(2f, 10f);
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
        yield return new WaitForSeconds(2f);
    
        Debug.Log("Wind ended! Checking final stability...");
        
        Invoke("SetWin", 0.5f);
    }

    public void SwitchMusicSmooth(AudioClip music, float fadeDuration = 1f)
    {
        if (MusicController.Instance._audioSourceMusic.clip == music)
            return;

        float originalVolume = MusicController.Instance._audioSourceMusic.volume;
    
        MusicController.Instance._audioSourceMusic.DOFade(0f, fadeDuration / 2f)
            .OnComplete(() =>
            {
                MusicController.Instance._audioSourceMusic.clip = music;
                MusicController.Instance._audioSourceMusic.Play();
            
                MusicController.Instance._audioSourceMusic.DOFade(originalVolume, fadeDuration / 2f);
            });
    }
    
    public void SetWin()
    {
        CheckObjectsStability("Wind Test");
        Debug.Log($"🏆 FINAL RESULT: {totalStars}/3 stars earned!");
        lowPassFilter.cutoffFrequency = 500f;
        WinController.Instance.ShowWinAnimation(totalStars);
    }
    
    public void SpawnOnGrid(GameObject prefab, GameObject button)
    {
        if (isRemove)
        {
            isRemove = false;
            button.SetActive(false);
            removeBuildingPanel.SetActive(true);
            _neededBuldings--;
            _buildingsCountText.text = $"{_buildingsCount}/{_neededBuldings}";
            if (_buildingsCount >= _neededBuldings)
            {
                ShowStartGameButton();
            }
            return;
        }
        
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
            
            MusicController.Instance.PlaySpecificSound(spawnSound);
            
            UIController.Instance.CloseBuildingPanel();
        }
        else
        {
            Debug.Log("Не удалось найти свободное место для спавна!");
        }
    }

    private Vector2 FindFreeSpawnPosition(GameObject prefab)
    {
        float gSize = grid.GridSize;

        Vector3 gridMin = grid.GetBottomLeft();
        Vector3 gridMax = grid.GetTopRight();

        Vector2 objectSize = GetPrefabSize(prefab);

        List<Vector2> allCells = new List<Vector2>();

        float minX = gridMin.x + objectSize.x / 2f;
        float maxX = gridMax.x - objectSize.x / 2f;
        float minY = gridMin.y + objectSize.y / 2f;
        float maxY = gridMax.y - objectSize.y / 2f;

        if (minX > maxX || minY > maxY)
        {
            Debug.LogWarning($"Объект слишком большой для сетки! Размер объекта: {objectSize}, размер сетки: {gridMax - gridMin}");
            return Vector2.zero;
        }

        for (float x = minX; x <= maxX; x += gSize)
        {
            for (float y = minY; y <= maxY; y += gSize)
            {
                float alignedX = Mathf.Round(x / gSize) * gSize;
                float alignedY = Mathf.Round(y / gSize) * gSize;
                
                if (alignedX >= minX && alignedX <= maxX && 
                    alignedY >= minY && alignedY <= maxY)
                {
                    Vector2 cellCenter = new Vector2(alignedX, alignedY);
                    allCells.Add(cellCenter);
                }
            }
        }

        for (int i = 0; i < allCells.Count; i++)
        {
            Vector2 temp = allCells[i];
            int randomIndex = Random.Range(i, allCells.Count);
            allCells[i] = allCells[randomIndex];
            allCells[randomIndex] = temp;
        }

        foreach (Vector2 cellCenter in allCells)
        {
            if (IsPositionFree(cellCenter, objectSize))
            {
                return cellCenter;
            }
        }

        return Vector2.zero;
    }

    private Vector2 GetPrefabSize(GameObject prefab)
    {
        Vector2 size = Vector2.one;

        GameObject tempObj = Instantiate(prefab);
    
        Collider2D prefabCollider = tempObj.GetComponent<Collider2D>();
        if (prefabCollider != null)
        {
            size = prefabCollider.bounds.size;
        }
        else
        {
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
    
        DestroyImmediate(tempObj);
    
        return size;
    }

    private bool IsPositionFree(Vector2 centerPosition, Vector2 objectSize)
    {
        Vector2 halfSize = objectSize / 2f;
        Vector3 gridMin = grid.GetBottomLeft();
        Vector3 gridMax = grid.GetTopRight();

        float leftEdge = centerPosition.x - halfSize.x;
        float rightEdge = centerPosition.x + halfSize.x;
        float bottomEdge = centerPosition.y - halfSize.y;
        float topEdge = centerPosition.y + halfSize.y;

        float epsilon = 0.01f;
        if (leftEdge < gridMin.x + epsilon || 
            rightEdge > gridMax.x - epsilon || 
            bottomEdge < gridMin.y + epsilon || 
            topEdge > gridMax.y - epsilon)
        {
            return false;
        }

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

    public Vector2 FindNearestFreePosition(Vector2 preferredPosition, GameObject prefab)
    {
        float gSize = grid.GridSize;
        Vector2 objectSize = GetPrefabSize(prefab);

        if (IsPositionFree(preferredPosition, objectSize))
        {
            return preferredPosition;
        }

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

        return Vector2.zero;
    }
}