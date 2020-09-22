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
        Debug.Log(ItemDB.Instance.DataList[0].name);
        Debug.Log(ItemDB.Instance.DataList[1].name);
    }

    // Update is called once per frame
    public void OnClickButton()
    {
        Musha.GameSceneManager.Instance.ChangeSceneAsync("Game/Scenes/Test");
    }
}

public class TestData : ISerializationCallbackReceiver
{
    public List<int> id;

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        Debug.LogFormat("OnBefore:id={0}", this.id.Count);
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        Debug.LogFormat("OnAfter:id={0}", this.id.Count);
    }
}
