using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Title : MonoBehaviour
{
    static bool isSet = false;

    private bool isAdd = false;
    private Musha.AssetHandler handler = null;

    // Start is called before the first frame update
    void Start()
    {
        if (!isSet)
        {
            isSet = true;

            Musha.AssetManager.Instance.infoList.Add(new Musha.AssetBundleInfo
            {
                assetBundleName = "game/scenes/test",
                dependencies = new string[]
                {
                    "game/scenes/text"
                }
            });
            Musha.AssetManager.Instance.infoList.Add(new Musha.AssetBundleInfo
            {
                assetBundleName = "game/scenes/text",
            });
        }

        this.handler = Musha.AssetManager.Instance.LoadSceneAssetAsync("Game/Scenes/Test");
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnClickButton()
    {
        if (!isAdd)
        {
            isAdd = true;
            Musha.GameSceneManager.Instance.LoadSceneAsync("Game/Scenes/Test", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
        else
        {
            Musha.AssetManager.Instance.Unload(this.handler);

            isAdd = false;
            //Musha.GameSceneManager.Instance.UnloadSceneAsync("Game/Scenes/Test");
        }
    }
}
