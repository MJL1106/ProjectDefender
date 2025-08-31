using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(TileSlot)), CanEditMultipleObjects]
public class TileSlotEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        if (GUILayout.Button("My First Button"))
        {
            foreach (var obj in targets)
            {
                ((TileSlot)obj).ButtonCheck();
            }
        }
    }
}
