using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Title : MonoBehaviour
{
    static bool isSet = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!isSet)
        {
            isSet = true;

            Musha.AssetManager.Instance.infoList.Add(new Musha.AssetBundleInfo
            {
                assetBundleName = "game/scenes/test",
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnClickButton()
    {
        Musha.GameSceneManager.Instance.ChangeSceneAsync("Home");
    }
}
