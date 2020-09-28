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
//        Debug.Log(ItemDB.Instance.DataList[1].name);
//        Debug.Log(MonsterDB.Instance.DataList[1].hp);
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
