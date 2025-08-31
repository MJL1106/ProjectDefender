using UnityEngine;

public class TileSlot : MonoBehaviour
{
    public bool canBuild;

    public void ButtonCheck()
    {
        canBuild = !canBuild;
        Debug.Log("haha");
    }
}
