#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public class PrefabLinePlacer : EditorWindow
{
    private float spacing = 1.0f;
    private Vector3 startPosition = Vector3.zero;
    private Vector3 direction = Vector3.right;

    [MenuItem("Tools/Prefab Line Placer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabLinePlacer>("Prefab Line Placer");
    }

    void OnGUI()
    {
        GUILayout.Label("Prefab Line Placement Settings", EditorStyles.boldLabel);

        spacing = EditorGUILayout.FloatField("Spacing:", spacing);
        startPosition = EditorGUILayout.Vector3Field("Start Position:", startPosition);
        direction = EditorGUILayout.Vector3Field("Direction:", direction).normalized;

        if (GUILayout.Button("Place Prefabs in Line"))
        {
            PlacePrefabsInLine();
        }
    }

    void PlacePrefabsInLine()
    {
        // Load all prefabs from Resources folder
        //GameObject[] prefabs = Resources.LoadAll<GameObject>(resourcesPath);

        //if (prefabs == null || prefabs.Length == 0)
        //{
        //    Debug.LogError($"No prefabs found in Resources/{resourcesPath}");
        //    return;
        //}

        GameObject[] prefabs = new PrefabFetcher().FetchAllPrefabs().ToArray();

        // Create parent object to keep hierarchy clean
        GameObject parent = new GameObject("PrefabLine");
        parent.transform.position = startPosition;

        Undo.RegisterCreatedObjectUndo(parent, "Create Prefab Line");

        // Filter to only include prefabs (not sure if necessary but just in case)
        var validPrefabs = prefabs.Where(p => PrefabUtility.GetPrefabAssetType(p) != PrefabAssetType.NotAPrefab).ToArray();

        for (int i = 0; i < validPrefabs.Length; i++)
        {
            // Calculate position
            Vector3 position = startPosition + direction * spacing * i;

            // Instantiate the prefab while maintaining connection
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(validPrefabs[i]);
            instance.transform.position = position;
            instance.transform.parent = parent.transform;

            Undo.RegisterCreatedObjectUndo(instance, "Instantiate Prefab");
        }

        Debug.Log($"Placed {validPrefabs.Length} prefabs in a line");
    }
}
#endif
