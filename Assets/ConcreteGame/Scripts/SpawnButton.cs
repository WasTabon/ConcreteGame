using UnityEngine;
using UnityEngine.UI;

public class SpawnButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject prefab;

    private void Start()
    {
        button.onClick.AddListener(() =>
        {
            LevelController level = FindObjectOfType<LevelController>();
            if (level != null)
                level.SpawnOnGrid(prefab, gameObject);
        });
    }
}