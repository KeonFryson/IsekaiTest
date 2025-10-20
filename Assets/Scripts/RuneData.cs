using UnityEngine;

[CreateAssetMenu(fileName = "NewRune", menuName = "Magic/Rune Data")]
public class RuneData : ScriptableObject
{
    public string runeName;
    public Color runeColor;
    public GameObject runePrefab;
    public GameObject effectPrefab;
    public float lifetime = 10f;
    public float postionOffset = 3.01f; // Slightly above the ground
    public float manaCost = 20f;
    public bool isInfinite = false;
}
