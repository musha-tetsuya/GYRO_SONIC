using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class GimmickEditor : MonoBehaviour
{
    [SerializeField]
    private PathCreation.PathCreator bezier = null;

    private void Start()
    {

    }

    private void OnGUI()
    {
        if (GUILayout.Button("Start"))
        {

        }
    }
}
