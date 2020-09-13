#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// チューブメッシュ生成
/// </summary>
public class TubeMeshEditor : MonoBehaviour
{
    /// <summary>
    /// メッシュデータ
    /// </summary>
    private TubeMeshData meshData = null;
    /// <summary>
    /// メッシュ
    /// </summary>
    private Mesh mesh = null;

    /// <summary>
    /// メッシュ生成
    /// </summary>
    private void CreateMesh()
    {
        this.mesh = this.meshData.CreateMesh();
    }

    /// <summary>
    /// メッシュ保存
    /// </summary>
    private void SaveMesh()
    {
        //メッシュパス決定
        string MESH_DIRECTORY_KEY = typeof(TubeMeshEditor).Name + ".meshDirectory";
        string meshDirectory = EditorPrefs.GetString(MESH_DIRECTORY_KEY);
        string meshPath = EditorUtility.SaveFilePanelInProject("Save Mesh", this.mesh.name, "asset", "", meshDirectory);

        if (string.IsNullOrEmpty(meshPath))
        {
            //キャンセルされたのでreturn
            return;
        }

        //メッシュデータパス決定
        string MESHDATA_DIRECTORY_KEY = typeof(TubeMeshEditor).Name + ".meshDataDirectory";
        string meshDataDirectory = EditorPrefs.GetString(MESHDATA_DIRECTORY_KEY);
        string meshDataPath = EditorUtility.SaveFilePanelInProject("Save MeshData", this.mesh.name + "-meshdata", "asset", "", meshDataDirectory);

        if (string.IsNullOrEmpty(meshDataPath))
        {
            //キャンセルされたのでreturn
            return;
        }

        //確定したパスを記憶
        EditorPrefs.SetString(MESH_DIRECTORY_KEY, Path.GetDirectoryName(meshPath));
        EditorPrefs.SetString(MESHDATA_DIRECTORY_KEY, Path.GetDirectoryName(meshDataPath));

        //メッシュが既存かどうか
        var existMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);

        if (existMesh == null)
        {
            //新規保存
            AssetDatabase.CreateAsset(this.mesh, meshPath);
        }
        else
        {
            //上書き保存
            existMesh.Clear();
            existMesh.name = this.mesh.name;
            existMesh.vertices = this.mesh.vertices;
            existMesh.triangles = this.mesh.triangles;
            existMesh.uv = this.mesh.uv;
            existMesh.RecalculateBounds();
            existMesh.RecalculateNormals();
        }

        //メッシュデータが既存かどうか
        var existMeshData = AssetDatabase.LoadAssetAtPath<TubeMeshData>(meshDataPath);

        if (existMeshData == null)
        {
            //新規保存
            AssetDatabase.CreateAsset(this.meshData, meshDataPath);
        }
        else
        {
            //上書き保存
            existMeshData.bezier = this.meshData.bezier;
            existMeshData.radius = this.meshData.radius;
            existMeshData.edgeCount = this.meshData.edgeCount;
            existMeshData.scale = this.meshData.scale;
        }

        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// OnDrawGizmos
    /// </summary>
    private void OnDrawGizmos()
    {
        if (this.mesh != null)
        {
            var color = Gizmos.color;

            color.a = 0.5f;
            Gizmos.color = color;

            //メッシュ描画
            Gizmos.DrawMesh(this.mesh);

            color.a = 1f;
            Gizmos.color = color;

            //メッシュの各頂点描画
            foreach (var p in this.mesh.vertices)
            {
                Gizmos.DrawSphere(p, this.meshData.radius * 0.1f);
            }
        }
    }

    /// <summary>
    /// カスタムインスペクター
    /// </summary>
    [CustomEditor(typeof(TubeMeshEditor))]
    private class CustomInspector : Editor
    {
        /// <summary>
        /// ターゲット
        /// </summary>
        private new TubeMeshEditor target => base.target as TubeMeshEditor;
        /// <summary>
        /// メッシュデータEditor
        /// </summary>
        private Editor meshDataEditor = null;

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            //メッシュデータ生成
            if (this.target.meshData == null)
            {
                this.target.meshData = ScriptableObject.CreateInstance<TubeMeshData>();
            }

            //メッシュデータEditor生成
            if (this.meshDataEditor == null)
            {
                CreateCachedEditor(this.target.meshData, null, ref this.meshDataEditor);
            }

            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(this.target.meshData.bezier == null);
                {
                    //生成ボタン
                    if (GUILayout.Button("Create Mesh"))
                    {
                        this.target.CreateMesh();
                    }
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(this.target.mesh == null);
                {
                    //保存ボタン
                    if (GUILayout.Button("Save Mesh"))
                    {
                        this.target.SaveMesh();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();

            //メッシュデータEditorの描画
            {
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1f));

                EditorGUI.indentLevel++;

                this.meshDataEditor.OnInspectorGUI();

                EditorGUI.indentLevel--;
            }
        }
    }
}
#endif