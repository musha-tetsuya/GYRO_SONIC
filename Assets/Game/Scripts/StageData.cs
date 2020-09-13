using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class StageData : MonoBehaviour
{
    [SerializeField]
    public PathCreation.PathCreator bezier = null;

    [SerializeField]
    public float scale = 1f;
}
