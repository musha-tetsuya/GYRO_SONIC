using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GimmickParameter
{
    [SerializeField]
    public float position = 0f;

    [SerializeField]
    public float angle = 0f;

    [SerializeField]
    public GimmickType type = GimmickType.None;
}
