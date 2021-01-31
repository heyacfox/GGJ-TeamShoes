using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshHideOrganization))]
public class MeshHideOrganizationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MeshHideOrganization myScript = (MeshHideOrganization)target;
        if(GUILayout.Button("Apply MeshHide assets"))
        {
            myScript.ReorganizeMeshHide();
        }
        if(GUILayout.Button("Resort list by names"))
        {
            myScript.SortByName();
        }
        if(GUILayout.Button("Documentation"))
        {
            myScript.CreateDocumentation();
        }
        if(GUILayout.Button("Verify if files _Recipe exist that are not part of this list"))
        {
            myScript.FindMissingFiles();
        }
    }
}
