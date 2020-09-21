#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

/// <summary>
/// ステージエディタ
/// </summary>
public class StageEditor : MonoBehaviour
{
    /// <summary>
    /// メインカメラ
    /// </summary>
    [SerializeField]
    private Camera mainCamera = null;
    /// <summary>
    /// プレイヤー位置
    /// </summary>
    [SerializeField]
    private Transform playerPosition = null;
    /// <summary>
    /// プレイヤー角度
    /// </summary>
    [SerializeField]
    private Transform playerAngle = null;
    /// <summary>
    /// ステージデータ
    /// </summary>
    [SerializeField]
    private StageData stageData = null;
    /// <summary>
    /// ステージプレハブ
    /// </summary>
    [SerializeField]
    private GameObject stagePrefab = null;

    [SerializeField]
    private Animator animator = null;

    [SerializeField]
    private float runSpeed = 1f;

    [SerializeField]
    private float angleSpeed = 1f;

    /// <summary>
    /// 生成したステージ
    /// </summary>
    private GameObject stageObj = null;
    /// <summary>
    /// ギミック名一覧
    /// </summary>
    private string[] gimmickNames = null;
    /// <summary>
    /// ステージ内位置
    /// </summary>
    private float position = 0f;
    /// <summary>
    /// ステージ内位置の文字列
    /// </summary>
    private string positionString = "0";
    /// <summary>
    /// ギミックリストのスクロール位置
    /// </summary>
    private Vector2 gimmickListScrollPosition = Vector2.zero;
    /// <summary>
    /// 自動移動コルーチン
    /// </summary>
    private Coroutine autoMoveCoroutine = null;

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        //ギミック名一覧取得
        this.gimmickNames = AssetDatabase
            .FindAssets("", new string[]{ "Assets/AssetBundle/Resources/Gimmick" })
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => Path.GetFileNameWithoutExtension(path))
            .ToArray();

        this.position = this.stageData.startPosition;
        this.positionString = this.position.ToString();
    }

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        //ステージ生成
        this.stageObj = PrefabUtility.InstantiatePrefab(this.stagePrefab) as GameObject;

        //プレイヤー位置を初期位置にセット
        this.SetPlayerPosition(this.stageData.startPosition);
    }

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        //クリックしたオブジェクトをフォーカスする
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = this.mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Selection.activeGameObject = hit.collider.gameObject;
            }
        }

        //回転
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            var angle = this.playerAngle.localEulerAngles;
            angle.z -= this.angleSpeed;
            this.playerAngle.localEulerAngles = angle;
        }

        //回転
        if (Input.GetKey(KeyCode.RightArrow))
        {
            var angle = this.playerAngle.localEulerAngles;
            angle.z += this.angleSpeed;
            this.playerAngle.localEulerAngles = angle;
        }
    }

    /// <summary>
    /// 自動移動
    /// </summary>
    private IEnumerator AutoMove()
    {
        //while (this.position < this.stageData.tubeMeshData.bezier.path.length)
        while (this.position < this.stageData.goalPosition)
        {
            this.position += this.runSpeed * 0.001f;
            this.positionString = this.position.ToString();
            this.animator.speed = 2.5f * this.runSpeed;
            yield return null;
        }

        //this.position = this.stageData.tubeMeshData.bezier.path.length;
        this.position = this.stageData.goalPosition;
        this.autoMoveCoroutine = null;
        this.animator.Play("Standing(loop)");
        this.animator.speed = 1f;
    }

    /// <summary>
    /// OnGUI
    /// </summary>
    private void OnGUI()
    {
        if (this.autoMoveCoroutine == null)
        {
            if (GUILayout.Button("Play", GUILayout.Width(100f)))
            {
                this.autoMoveCoroutine = StartCoroutine(this.AutoMove());
                this.animator.Play("Running(loop)");
                this.animator.speed = 2.5f * this.runSpeed;
            }
        }
        else
        {
            if (GUILayout.Button("Stop", GUILayout.Width(100f)))
            {
                StopCoroutine(this.autoMoveCoroutine);
                this.autoMoveCoroutine = null;
                this.animator.Play("Standing(loop)");
                this.animator.speed = 1f;
            }
        }

        GUILayout.BeginHorizontal();
        {
            //ステージ内位置選択のスライダー
            float newPosition = GUILayout.HorizontalSlider(this.position, 0f, this.stageData.tubeMeshData.bezier.path.length, GUILayout.Width(Screen.width * 0.75f));

            //スライダーによってステージ内位置が変更されたら現在値を更新
            if (newPosition != this.position)
            {
                this.position = newPosition;
                this.positionString = newPosition.ToString();
            }

            //ステージ内位置のテキスト表示
            this.positionString = GUILayout.TextField(this.positionString, GUILayout.Width(Screen.width * 0.1f));

            //入力されたテキストをパースしてステージ内位置を更新
            if (float.TryParse(this.positionString, out newPosition))
            {
                this.position = Mathf.Clamp(newPosition, 0f, this.stageData.tubeMeshData.bezier.path.length);
            }

            //プレイヤー位置をセット
            this.SetPlayerPosition(this.position);
        }
        GUILayout.EndHorizontal();

        //ギミック一覧をスクロールビュー表示
        this.gimmickListScrollPosition = GUILayout.BeginScrollView(this.gimmickListScrollPosition, GUILayout.Width(230f), GUILayout.Height(150f));
        {
            for (int i = 0; i < this.gimmickNames.Length; i++)
            {
                if (GUILayout.Button(this.gimmickNames[i], GUILayout.Width(200f)))
                {
                    //選択したギミックを現在位置に配置
                    this.AddGimmick(this.position, this.gimmickNames[i]);
                }
            }
        }
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// プレイヤー位置のセット
    /// </summary>
    private void SetPlayerPosition(float position)
    {
        this.playerPosition.localPosition = this.stageData.tubeMeshData.bezier.path.GetPointAtDistance(position, PathCreation.EndOfPathInstruction.Stop) * this.stageData.tubeMeshData.scale;
        this.playerPosition.forward = this.stageData.tubeMeshData.bezier.path.GetDirectionAtDistance(position, PathCreation.EndOfPathInstruction.Stop);
    }

    /// <summary>
    /// ギミックを配置する
    /// </summary>
    private void AddGimmick(float position, string gimmickName)
    {
        var gimmickPrefab = Resources.Load<GameObject>("Gimmick/" + gimmickName);
        var gimmickObj = PrefabUtility.InstantiatePrefab(gimmickPrefab, this.stageObj.transform) as GameObject;
        gimmickObj.transform.localPosition = this.stageData.tubeMeshData.bezier.path.GetPointAtDistance(position, PathCreation.EndOfPathInstruction.Stop) * this.stageData.tubeMeshData.scale;
        gimmickObj.transform.forward = this.stageData.tubeMeshData.bezier.path.GetDirectionAtDistance(position, PathCreation.EndOfPathInstruction.Stop);
    }

    /// <summary>
    /// カスタムインスペクター
    /// </summary>
    [CustomEditor(typeof(StageEditor))]
    private class CustomInspector : Editor
    {
        private new StageEditor target => base.target as StageEditor;

        private Editor stageDataEditor = null;

        public override void OnInspectorGUI()
        {
            if (this.stageDataEditor == null && this.target.stageData != null)
            {
                CreateCachedEditor(this.target.stageData, null, ref this.stageDataEditor);
            }

            if (this.stageDataEditor != null && this.target.stageData == null)
            {
                this.stageDataEditor = null;
            }

            base.OnInspectorGUI();

            if (this.stageDataEditor != null)
            {
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1f));

                EditorGUI.indentLevel++;

                this.stageDataEditor.OnInspectorGUI();

                EditorGUI.indentLevel--;
            }
        }
    }
}
#endif
