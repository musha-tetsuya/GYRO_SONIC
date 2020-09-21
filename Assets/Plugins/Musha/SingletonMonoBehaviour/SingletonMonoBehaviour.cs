using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Musha
{
    /// <summary>
    /// シングルトンMonoBehaviour
    /// </summary>
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// インスタンス
        /// </summary>
        private static T instance = null;

        /// <summary>
        /// インスタンス
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                }

                if (instance == null)
                {
                    var gobj = new GameObject(typeof(T).Name, typeof(T));
                    DontDestroyOnLoad(gobj);
                }

                return instance;
            }
        }

        /// <summary>
        /// Awake
        /// </summary>
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
            else
            {
                Debug.LogErrorFormat("{0}のインスタンスが既存です。", typeof(T).Name);
            }
        }
    }
}