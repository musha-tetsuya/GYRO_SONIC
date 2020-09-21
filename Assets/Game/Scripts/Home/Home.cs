using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Home : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void OnClickButton()
    {
        Musha.GameSceneManager.Instance.ChangeSceneAsync("Game/Scenes/Test");
    }
}
