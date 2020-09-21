using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Musha;
using UnityEditor;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AssetManager.Instance.LoadAsync<Mesh>("AssetBundle/References/Gimmick/Arch180", asset =>
        {
            Debug.Log(asset.name);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
