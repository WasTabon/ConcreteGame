using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GridMovement : MonoBehaviour
{
    public AudioClip buildSound;
    public AudioClip denySound;
    
    [SerializeField] private float _animSpeed = 0.15f;
    [SerializeField] private bool _isBuilded;

    [Header("UI кнопки")]
    [SerializeField] private RectTransform _yesButton;
    [SerializeField] private RectTransform _noButton;

    [SerializeField] private float gridSize = 1f;
    
    [Header("Настройки проскока")]
    [SerializeField] private bool allowPassthrough = true;

    [Header("Звуки движения")]
    [SerializeField] private AudioClip[] movementSounds;
    [SerializeField] private AudioClip matchSound;

    [Header("Звуки столкновения")]
    [SerializeField] private AudioClip collisionSound;
    [SerializeField] private float minCollisionVelocity = 0.1f;
    [SerializeField] private float collisionSoundInterval = 0.1f;

    private Camera mainCamera;
    private bool isDragging = false;
    private bool hasCollision = false;
    private int fingerId = -1;
    private Tween moveTween;
    private Vector3 lastPosition;
    private float soundTimer = 0f;
    private const float SOUND_INTERVAL = 0.1f;
    private float lastCollisionSoundTime = 0f;

    private bool blockLeft, blockRight, blockUp, blockDown;

    private Bounds debugBounds;
    private bool drawDebug;

    private Rigidbody2D rb2d;

    void Start()
    {
        mainCamera = Camera.main;
        rb2d = GetComponent<Rigidbody2D>();

        _yesButton.localScale = Vector3.zero;
        _noButton.localScale = Vector3.zero;

        _yesButton.gameObject.GetComponent<Button>().onClick.AddListener(AlLowBuild);
        _noButton.gameObject.GetComponent<Button>().onClick.AddListener(DenyBuild);

        lastPosition = transform.position;
    }

    void Update()
    {
        if (_isBuilded) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif

        if (isDragging)
        {
            DragToGrid();
            HandleMovementSounds();
        }
        
        CheckNeighbors();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb2d != null && rb2d.bodyType == RigidbodyType2D.Dynamic)
        {
            float relativeVelocity = collision.relativeVelocity.magnitude;
            
            if (relativeVelocity > minCollisionVelocity)
            {
                if (Time.time - lastCollisionSoundTime >= collisionSoundInterval)
                {
                    PlayCollisionSound();
                    lastCollisionSoundTime = Time.time;
                }
            }
        }
    }

    private void PlayCollisionSound()
    {
        if (collisionSound != null && MusicController.Instance != null)
        {
            MusicController.Instance.PlaySpecificSound(collisionSound);
        }
    }

    private void HandleMovementSounds()
    {
        bool isMoving = Vector3.Distance(transform.position, lastPosition) > 0.01f;
        
        if (isMoving && !hasCollision)
        {
            soundTimer += Time.deltaTime;
            
            if (soundTimer >= SOUND_INTERVAL)
            {
                PlayRandomMovementSound();
                soundTimer = 0f;
            }
        }
        
        lastPosition = transform.position;
    }

    private void PlayRandomMovementSound()
    {
        if (movementSounds != null && movementSounds.Length > 0 && MusicController.Instance != null)
        {
            int randomIndex = Random.Range(0, movementSounds.Length);
            AudioClip randomSound = movementSounds[randomIndex];
            
            if (randomSound != null)
            {
                MusicController.Instance.PlaySpecificSound(randomSound);
            }
        }
    }

    private void PlayMatchSound()
    {
        if (matchSound != null && MusicController.Instance != null)
        {
            MusicController.Instance.PlaySpecificSound(matchSound);
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverThisObject(Input.mousePosition))
            {
                isDragging = true;
                soundTimer = 0f;
                lastPosition = transform.position;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            soundTimer = 0f;
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    if (IsPointerOverThisObject(touch.position))
                    {
                        isDragging = true;
                        fingerId = touch.fingerId;
                        soundTimer = 0f;
                        lastPosition = transform.position;
                    }
                }

                if (touch.fingerId == fingerId &&
                   (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
                {
                    isDragging = false;
                    fingerId = -1;
                    soundTimer = 0f;
                }
            }
        }
    }

    private void DragToGrid()
    {
        Vector3 screenPos = Input.touchCount > 0 && fingerId != -1
            ? (Vector3)Input.GetTouch(fingerId).position
            : Input.mousePosition;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);

        float gSize = GridBoundaryController.Instance != null
            ? GridBoundaryController.Instance.GridSize
            : gridSize;

        Bounds bounds = GetBounds(gSize);
        Vector3 size = bounds.size;

        float snappedX = Mathf.Round((worldPos.x - size.x / 2f) / gSize) * gSize + size.x / 2f;
        float snappedY = Mathf.Round((worldPos.y - size.y / 2f) / gSize) * gSize + size.y / 2f;

        Vector3 targetPos = new Vector3(snappedX, snappedY, transform.position.z);

        if (allowPassthrough && CanMoveToPositionWithPassthrough(targetPos, gSize))
        {
            moveTween?.Kill();
            moveTween = transform.DOMove(targetPos, 0.1f)
                .SetEase(Ease.OutQuad);
        }
        else if (!allowPassthrough && CanMoveToPosition(targetPos, gSize))
        {
            moveTween?.Kill();
            moveTween = transform.DOMove(targetPos, 0.1f)
                .SetEase(Ease.OutQuad);
        }
    }

    private bool CanMoveToPositionWithPassthrough(Vector3 targetPos, float gSize)
    {
        Bounds bounds = GetBounds(gSize);
        bounds.center = targetPos;

        debugBounds = bounds;
        drawDebug = true;

        Collider2D[] hits = Physics2D.OverlapBoxAll(bounds.center, bounds.size * 0.9f, 0f);

        foreach (var hit in hits)
        {
            if (hit != null && hit.gameObject != gameObject)
                return false;
        }

        if (GridBoundaryController.Instance != null && !GridBoundaryController.Instance.IsInsideBounds(bounds.center))
            return false;

        return true;
    }

    private void CheckNeighbors()
    {
        blockLeft = blockRight = blockUp = blockDown = false;

        float gSize = GridBoundaryController.Instance != null
            ? GridBoundaryController.Instance.GridSize
            : gridSize;

        Vector3 currentPos = transform.position;
        bool foundNeighbor = false;

        Vector3[] directions = {
            Vector3.right * gSize,
            Vector3.left * gSize,
            Vector3.up * gSize,
            Vector3.down * gSize
        };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 checkPosition = currentPos + directions[i];
            
            Bounds checkBounds = GetBounds(gSize);
            checkBounds.center = checkPosition;
            
            Collider2D[] hits = Physics2D.OverlapBoxAll(checkBounds.center, checkBounds.size * 0.9f, 0f);
            
            foreach (var hit in hits)
            {
                if (hit != null && hit.gameObject != gameObject)
                {
                    foundNeighbor = true;
                    
                    if (!allowPassthrough)
                    {
                        switch (i)
                        {
                            case 0: blockRight = true; break;
                            case 1: blockLeft = true; break;
                            case 2: blockUp = true; break;
                            case 3: blockDown = true; break;
                        }
                    }
                    break;
                }
            }
        }

        Bounds currentBounds = GetBounds(gSize);
        Collider2D[] currentHits = Physics2D.OverlapBoxAll(currentBounds.center, currentBounds.size * 0.9f, 0f);
        
        foreach (var hit in currentHits)
        {
            if (hit != null && hit.gameObject != gameObject)
            {
                foundNeighbor = true;
                break;
            }
        }

        if (foundNeighbor && !hasCollision)
        {
            hasCollision = true;
            ShowBuildButtons();
            PlayMatchSound();
        }
        else if (!foundNeighbor && hasCollision)
        {
            hasCollision = false;
            HideBuildButtons();
        }
    }

    private bool CanMoveToPosition(Vector3 targetPos, float gSize)
    {
        Vector3 delta = targetPos - transform.position;

        float deltaX = Mathf.Round(delta.x / gSize);
        float deltaY = Mathf.Round(delta.y / gSize);

        if (deltaX > 0 && blockRight) return false;
        if (deltaX < 0 && blockLeft) return false;
        if (deltaY > 0 && blockUp) return false;
        if (deltaY < 0 && blockDown) return false;

        if (deltaX > 0 && deltaY > 0 && (blockRight || blockUp)) return false;
        if (deltaX < 0 && deltaY > 0 && (blockLeft || blockUp)) return false;
        if (deltaX > 0 && deltaY < 0 && (blockRight || blockDown)) return false;
        if (deltaX < 0 && deltaY < 0 && (blockLeft || blockDown)) return false;

        Bounds bounds = GetBounds(gSize);
        bounds.center = targetPos;

        debugBounds = bounds;
        drawDebug = true;

        Collider2D[] hits = Physics2D.OverlapBoxAll(bounds.center, bounds.size * 0.9f, 0f);

        foreach (var hit in hits)
        {
            if (hit != null && hit.gameObject != gameObject)
                return false;
        }

        if (GridBoundaryController.Instance != null && !GridBoundaryController.Instance.IsInsideBounds(bounds.center))
            return false;

        return true;
    }

    private void ShowBuildButtons()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Join(_yesButton.DOScale(Vector3.one, _animSpeed).SetEase(Ease.OutBack));
        sequence.Join(_noButton.DOScale(Vector3.one, _animSpeed).SetEase(Ease.OutBack));
    }

    private void HideBuildButtons()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Join(_yesButton.DOScale(Vector3.zero, _animSpeed).SetEase(Ease.InBack));
        sequence.Join(_noButton.DOScale(Vector3.zero, _animSpeed).SetEase(Ease.InBack));
    }

    private Bounds GetBounds(float gSize)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Collider2D col = GetComponent<Collider2D>();

        if (sr != null) return sr.bounds;
        if (col != null) return col.bounds;

        return new Bounds(transform.position, Vector3.one * gSize);
    }

    private bool IsPointerOverThisObject(Vector3 screenPos)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos2D, Vector2.zero);
    
        RaycastHit2D topHit = default;
        float highestZ = float.MinValue;
    
        foreach (var hit in hits)
        {
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                float zPos = hit.collider.transform.position.z;
                if (zPos > highestZ)
                {
                    highestZ = zPos;
                    topHit = hit;
                }
            }
        }
    
        return topHit.collider != null && topHit.collider.transform == transform;
    }
    
    private bool IsPointerOverThisObjectWithLayer(Vector3 screenPos)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        int layerMask = 1 << gameObject.layer;
        RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero, Mathf.Infinity, layerMask);
    
        return hit.collider != null && hit.collider.transform == transform;
    }

    public void AlLowBuild()
    {
        _isBuilded = true;
        HideBuildButtons();
        MusicController.Instance.PlaySpecificSound(buildSound);
        LevelController.Instance.AllowBuild();
    }

    public void DenyBuild()
    {
        MusicController.Instance.PlaySpecificSound(denySound);
        LevelController.Instance.DenyBuild();
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(debugBounds.center, debugBounds.size);
    }
}