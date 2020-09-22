using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Title : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas = null;

    // Start is called before the first frame update
    void Start()
    {
        var data = Musha.AssetManager.Instance.Load<TitleTestData>("AssetBundle/TitleTestData");
        data.id = 999;

        var obj = Musha.AssetManager.Instance.Load<TitleTestDataBehaviour>("AssetBundle/TitleTestDataBehaviour");
        obj = Instantiate(obj, canvas.transform, false);
        obj.id = 254;
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
