using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Home : MonoBehaviour
{
    [SerializeField]
    private TextAsset json = null;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    public void OnClickButton()
    {
        Musha.GameSceneManager.Instance.ChangeSceneAsync("Title");
    }
}
