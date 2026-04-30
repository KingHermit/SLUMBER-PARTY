// Change 'GridManager' to whatever your actual script name is
using Unity.Entities.UniversalDelegates;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkPlayer))]
public class GridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. Reference your actual script
        NetworkPlayer player = (NetworkPlayer)target;

        // 2. Draw the default fields first (Header, Prefabs, Holder)
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("PLAYER INFO", EditorStyles.boldLabel);

        if (player != null)
        {
            EditorGUILayout.BeginHorizontal();

            //EditorGUILayout.LabelField(player.playerName,
            //            EditorStyles.label, GUILayout.Width(30));
            //EditorGUILayout.
        }

        // 3. Draw the 2D Grid
        //if (manager.playerCharacterList != null)
        //{
        //    for (int i = 0; i < manager.playerCharacterList.Count; i++)
        //    {
        //        EditorGUILayout.BeginHorizontal();

        //        for (int j = 0; j < manager.playerCharacterList.Count; j++)
        //        {
        //            EditorGUILayout.LabelField(manager.playerCharacterList[i].playerName,
        //                EditorStyles.label, GUILayout.Width(30));
        //            //manager.playerCharacterList[i].columns[j] = EditorGUILayout.IntField(
        //            //    manager.playerCharacterList[i].columns[j], GUILayout.Width(30));
        //        }
        //        EditorGUILayout.EndHorizontal();
        //    }
        //}

        // 4. Manual "Save" button to ensure Unity records changes
        if (GUI.changed) { EditorUtility.SetDirty(player); }
    }
}
