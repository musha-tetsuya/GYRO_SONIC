using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/StageData")]
public class StageData : ScriptableObject
{
    [SerializeField]
    public TubeMeshData tubeMeshData = null;

    [SerializeField]
    public float startPosition = 0f;

    [SerializeField]
    public float goalPosition = 1f;
}
