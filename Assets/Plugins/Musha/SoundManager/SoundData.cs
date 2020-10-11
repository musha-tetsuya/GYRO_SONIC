using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Musha
{
    /// <summary>
    /// サウンドデータ
    /// </summary>
    [CreateAssetMenu(menuName = "Musha/ScriptableObject/SoundData")]
    public class SoundData : ScriptableObject
    {
        /// <summary>
        /// AudioClip
        /// </summary>
        [SerializeField]
        public AudioClip audioClip = null;

        /// <summary>
        /// 音量
        /// </summary>
        [SerializeField, Range(0f, 1f)]
        public float volume = 1f;

        /// <summary>
        /// ループ開始位置
        /// </summary>
        [SerializeField, HideInInspector]
        public float loopStartNormalizedTime = 0f;

        /// <summary>
        /// ループ終了位置
        /// </summary>
        [SerializeField, HideInInspector]
        public float loopEndNormalizedTime = 1f;

        /// <summary>
        /// カスタムインスペクター
        /// </summary>
        [CustomEditor(typeof(SoundData))]
        private class MyInspector : Editor
        {
            /// <summary>
            /// target
            /// </summary>
            private new SoundData target => base.target as SoundData;

            /// <summary>
            /// OnInspectorGUI
            /// </summary>
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                this.serializedObject.Update();

                if (this.target.audioClip != null)
                {
                    float length = this.target.audioClip.length;
                    this.target.loopStartNormalizedTime = EditorGUILayout.Slider("ループ開始位置", length * this.target.loopStartNormalizedTime, 0f, length) / length;
                    this.target.loopEndNormalizedTime = EditorGUILayout.Slider("ループ終了位置", length * this.target.loopEndNormalizedTime, 0f, length) / length;
                }

                this.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}