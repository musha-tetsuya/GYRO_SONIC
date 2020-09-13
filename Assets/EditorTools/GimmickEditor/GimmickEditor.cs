using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class GimmickEditor : MonoBehaviour
{
    [SerializeField]
    private Transform cameraOrigin = null;

    [SerializeField]
    private TubeMeshData tubeMeshData = null;

    [SerializeField]
    private GimmickData gimmickData = null;

    private float position = 0f;

    private List<(GameObject gimmick, GimmickParameter parameter)> gimmickList = new List<(GameObject gimmick, GimmickParameter parameter)>();

    private void Start()
    {
        this.SetCameraPosition(0f);

        for (int i = 0; i < this.gimmickData.gimmickList.Count; i++)
        {
            var param = this.gimmickData.gimmickList[i];
            var prefab = Resources.Load<GameObject>("Gimmick/" + param.type);
            var gobj = Instantiate(prefab);
            gobj.transform.localPosition = this.tubeMeshData.bezier.path.GetPointAtDistance(param.position, PathCreation.EndOfPathInstruction.Stop) * this.tubeMeshData.scale;
            gobj.transform.forward = this.tubeMeshData.bezier.path.GetDirectionAtDistance(param.position, PathCreation.EndOfPathInstruction.Stop);

            this.gimmickList.Add((gobj, param));
        }
    }

    private void SetCameraPosition(float position)
    {
        this.cameraOrigin.localPosition = this.tubeMeshData.bezier.path.GetPointAtDistance(position, PathCreation.EndOfPathInstruction.Stop) * this.tubeMeshData.scale;

        this.cameraOrigin.forward = this.tubeMeshData.bezier.path.GetDirectionAtDistance(position, PathCreation.EndOfPathInstruction.Stop);
    }

    [CustomEditor(typeof(GimmickEditor))]
    private class CustomInspector : Editor
    {
        private new GimmickEditor target => base.target as GimmickEditor;

        private GimmickType gimmickType = GimmickType.None;

        private Editor gimmickDataEditor = null;

        public override void OnInspectorGUI()
        {
            if (this.gimmickDataEditor == null && this.target.gimmickData != null)
            {
                CreateCachedEditor(this.target.gimmickData, null, ref this.gimmickDataEditor);
            }

            if (this.gimmickDataEditor != null && this.target.gimmickData == null)
            {
                this.gimmickDataEditor = null;
            }

            base.OnInspectorGUI();

            if (Application.isPlaying)
            {
                this.target.position = EditorGUILayout.Slider("Position", this.target.position, 0f, this.target.tubeMeshData.bezier.path.length);

                this.target.SetCameraPosition(this.target.position);

                GUILayout.BeginHorizontal();
                {
                    this.gimmickType = (GimmickType)EditorGUILayout.EnumPopup(this.gimmickType);

                    EditorGUI.BeginDisabledGroup(this.target.gimmickData == null);
                    {
                        if (GUILayout.Button("Add Gimmick"))
                        {
                            var gimmickParameter = new GimmickParameter();
                            gimmickParameter.position = this.target.position;
                            gimmickParameter.angle = 0f;
                            gimmickParameter.type = this.gimmickType;

                            this.target.gimmickData.gimmickList.Add(gimmickParameter);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                GUILayout.EndHorizontal();
            }

            if (this.gimmickDataEditor != null)
            {
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1f));

                EditorGUI.indentLevel++;

                this.gimmickDataEditor.OnInspectorGUI();

                EditorGUI.indentLevel--;
            }

            if (Application.isPlaying)
            {
                if (this.target.gimmickData.gimmickList.Count > this.target.gimmickList.Count)
                {

                }
                else
                {
                    var removedElement = this.target.gimmickList
                        .Select(x => x.parameter)
                        .Except(this.target.gimmickData.gimmickList);

                    Debug.LogFormat("gimmickList.Count = {0}", this.target.gimmickList.Count);
                    Debug.LogFormat("gimmickData.gimmickList.Count = {0}", this.target.gimmickData.gimmickList.Count);
                    Debug.LogFormat("removedElement.Count = {0}", removedElement.Count());
                }
            }
        }
    }
}
