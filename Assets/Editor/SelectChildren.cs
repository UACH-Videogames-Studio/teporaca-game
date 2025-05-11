using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SelectChildren
{
    [MenuItem("GameObject/Select Children of Selected", false, 0)]
    static void SelectChildrenFromAllSelected()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No GameObjects selected.");
            return;
        }

        List<GameObject> allChildren = new List<GameObject>();

        foreach (GameObject obj in selectedObjects)
        {
            foreach (Transform child in obj.transform)
            {
                allChildren.Add(child.gameObject);
            }
        }

        if (allChildren.Count == 0)
        {
            Debug.LogWarning("No children found.");
        }
        else
        {
            Selection.objects = allChildren.ToArray();
        }
    }
}
