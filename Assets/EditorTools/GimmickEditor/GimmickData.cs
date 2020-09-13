using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "ScriptableObject/GimmickData")]
public class GimmickData : ScriptableObject
{
    [SerializeField]
    public float startPosition = 0f;

    [SerializeField]
    public float goalPosition = 0f;

    [SerializeField, HideInInspector]
    public List<GimmickParameter> gimmickList = null;

#if UNITY_EDITOR
    [CustomEditor(typeof(GimmickData))]
    public class CustomInspector : Editor
    {
        public SimpleReorderableList gimmickList = null;

        private void OnEnable()
        {
            this.gimmickList = new SimpleReorderableList(this.serializedObject.FindProperty("gimmickList"), typeof(GimmickParameter));
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            base.OnInspectorGUI();

            this.gimmickList.DoLayoutList(270);

            this.serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
