using UnityEngine;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    public GameObject currentBuilding;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // пофіксити з'єднювання обєктів
    }
}
