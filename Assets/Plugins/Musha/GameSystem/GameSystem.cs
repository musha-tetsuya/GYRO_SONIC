using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Musha
{
    /// <summary>
    /// ゲーム中共通オブジェクト
    /// </summary>
    public class GameSystem : SingletonMonoBehaviour<GameSystem>
    {
        /// <summary>
        /// オーバーレイキャンバス
        /// </summary>
        [SerializeField]
        public Canvas overlayCanvas = null;

        /// <summary>
        /// レイヤー名リスト
        /// </summary>
        [SerializeField, HideInInspector]
        private List<string> layerNames = null;

        /// <summary>
        /// タッチブロック
        /// </summary>
        public Image touchBlock { get; private set; }

        /// <summary>
        /// Awake
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(this.gameObject);

            //レイヤー作成
            for (int i = 0, imax = this.layerNames.Count; i < imax; i++)
            {
                var rectTransform = this.overlayCanvas.transform.Find(this.layerNames[i]) as RectTransform;
                if (rectTransform == null)
                {
                    var layer = new GameObject(this.layerNames[i], typeof(RectTransform));
                    rectTransform = layer.transform as RectTransform;
                    rectTransform.SetParent(this.overlayCanvas.transform);
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.pivot = Vector2.one * 0.5f;
                }
                else
                {
                    rectTransform.SetAsLastSibling();
                }
            }

            //タッチブロック作成
            this.touchBlock = this.overlayCanvas.transform.Find("TouchBlock").gameObject.AddComponent<Image>();
            this.touchBlock.color = Color.clear;
            this.touchBlock.enabled = false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// カスタムインスペクター
        /// </summary>
        [CustomEditor(typeof(GameSystem))]
        private class CustomInspector : Editor
        {
            /// <summary>
            /// レイヤー名リスト
            /// </summary>
            private SimpleReorderableList layerNames = null;

            /// <summary>
            /// OnEnable
            /// </summary>
            private void OnEnable()
            {
                this.layerNames = new SimpleReorderableList(this.serializedObject.FindProperty("layerNames"));
            }

            /// <summary>
            /// OnInspectorGUI
            /// </summary>
            public override void OnInspectorGUI()
            {
                this.serializedObject.Update();

                base.OnInspectorGUI();

                GUILayout.Space(10);

                this.layerNames.DoLayoutList();

                this.serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}