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
        /*
        var monsters = new MonsterData[2];
        monsters[0] = new MonsterData
        {
            id = 1,
            hp = 10,
            items = new ItemData[]
            {
                new ItemData{ id = 1, name = "a", power = 100 },
                new ItemData{ id = 2, name = "b", power = 200 },
            }
        };
        monsters[1] = new MonsterData
        {
            id = 2,
            hp = 20,
            items = new ItemData[]
            {
                new ItemData{ id = 1, name = "a", power = 100 },
                new ItemData{ id = 2, name = "b", power = 200 },
            }
        };

        var json = JsonUtilityEx.ToJson(monsters);
        Debug.Log(json);
        */

        var jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/AssetBundle/MonsterData.txt");
        var monsterData = JsonUtilityEx.FromJson<List<MonsterData>>(jsonAsset.text);
        Debug.Log(monsterData.Count);

        /*var items = new ItemData[]
        {
            new ItemData{ id = 1, name = "a", power = 100 },
            new ItemData{ id = 2, name = "b", power = 200 }
        };

        var json = JsonUtilityEx.ToJson(items);*/
        //var json = JsonUtility.ToJson(items);

        //Debug.Log(JsonUtility.ToJson(new ItemData{ id = 1, name = "a", power = 200 }));
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnClickButton()
    {
        GameSceneManager.Instance.ChangeSceneAsync("Title");
    }
}

public static class JsonUtilityEx
{
    public static string ToJson(object obj)
    {
        string json = null;

        if (obj is IList)
        {
            var a = obj as IList;

            json += "[";

            for (int i = 0, imax = a.Count; i < imax; i++)
            {
                if (i > 0)
                {
                    json += ",";
                }

                json += ToJson(a[i]);
            }

            json += "]";
        }
        else
        {
            json = JsonUtility.ToJson(obj);
        }

        return json;
    }

    public static T FromJson<T>(string json)
    {
        if (json.StartsWith("["))
        {
            var lines = new List<string>();

            int startIndex = 0;
            int endIndex = 0;
            int count = 0;

            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] == '{')
                {
                    if (count == 0)
                    {
                        startIndex = i;
                    }
                    count++;
                }
                else if (json[i] == '}')
                {
                    count--;
                    if (count == 0)
                    {
                        endIndex = i;
                        lines.Add(json.Substring(startIndex, endIndex - startIndex + 1));
                    }
                }
            }

            var typeName = typeof(T).ToString();
            startIndex = typeName.LastIndexOf('[');
            endIndex = typeName.LastIndexOf(']');
            typeName = typeName.Substring(startIndex + 1, endIndex - startIndex - 1);
            Debug.Log(typeName);

            var itemType = Type.GetType(typeName);
            var result = Activator.CreateInstance(typeof(T));
            Debug.Log(result is IList);

            foreach (var line in lines)
            {
                Debug.Log(line);
                Debug.Log(itemType);
                var item = JsonUtility.FromJson(line, itemType);
                Debug.Log(item.GetType());
                (result as IList).Add(item);
            }

            return (T)result;
        }

        if (typeof(T).IsAssignableFrom(typeof(List<>)))
        {
            Debug.Log("List<>");
        }
        else if (typeof(T).IsArray)
        {
            Debug.Log("IsArray");
        }

        return default(T);
    }
}
