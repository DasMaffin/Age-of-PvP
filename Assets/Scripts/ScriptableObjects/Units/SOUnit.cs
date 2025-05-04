using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Scriptable Objects/Unit")]
public class SOUnit : ScriptableObject
{
    public Unit unit;
    public Sprite unitImage;
    public GameObject unitPrefab;
}
