using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(TileSlot)), CanEditMultipleObjects]
public class TileSlotEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        float buttonWidth = (EditorGUIUtility.currentViewWidth - 25) / 2;
        
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Field", GUILayout.Width(buttonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileField;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }
        
        if (GUILayout.Button("Road", GUILayout.Width(buttonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileRoad;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }
        
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Sideway", GUILayout.Width(buttonWidth * 2)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileSideway;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }
        
        GUILayout.EndHorizontal();
    }
}
