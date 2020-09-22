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
