using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Musha
{
    /// <summary>
    /// シーン管理
    /// </summary>
    public class GameSceneManager : SingletonMonoBehaviour<GameSceneManager>
    {
#if UNITY_EDITOR
        /// <summary>
        /// シーン情報
        /// </summary>
        private EditorSceneInfo[] sceneInfos = null;
#endif

        /// <summary>
        /// Awake
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

#if UNITY_EDITOR
            //BuildSettingsに含まれているシーンのパス一覧
            var buildSettingsScenePaths = EditorBuildSettings.scenes
                .Select(x => x.path)
                .ToArray();

            //Assets内全シーン情報
            this.sceneInfos = AssetDatabase
                .FindAssets("t:SceneAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path =>
                {
                    var sceneInfo = new EditorSceneInfo();
                    sceneInfo.path = path;
                    sceneInfo.isAssetBundle = !buildSettingsScenePaths.Contains(path);
                    sceneInfo.name = sceneInfo.isAssetBundle
                        ? path.Replace("Assets/", null).Replace(".unity", null)
                        : Path.GetFileNameWithoutExtension(path);
                    return sceneInfo;
                })
                .ToArray();
#endif
        }

        /// <summary>
        /// 非同期ロード
        /// </summary>
        /// <br>
        /// sceneNameには、BuildSettingsに含まれている場合はシーン名を。
        /// 含まれていないアセットバンドルのシーンの場合はシーンパスを入れる。（「Assets/」と「.unity」は不要）
        /// </br>
        public void LoadAsync(string sceneName, LoadSceneMode mode)
        {
#if UNITY_EDITOR
            //シーン情報検索
            var sceneInfo = this.sceneInfos.FirstOrDefault(x => x.name.Equals(sceneName, StringComparison.OrdinalIgnoreCase));
            if (sceneInfo == null)
            {
                Debug.LogErrorFormat("{0}に一致するシーンは存在しません。", sceneName);
                return;
            }

            //アセットバンドルシーンの場合
            if (sceneInfo.isAssetBundle)
            {
                //アセットバンドルがロード済みかチェック
                var handler = AssetManager.Instance.FindAssetHandler(sceneName);
                if (handler == null || handler.status != AssetHandler.Status.Completed)
                {
                    Debug.LogErrorFormat("シーン:{0}はロードされていません。", sceneName);
                    return;
                }

                if (handler is DummyAssetHandler)
                {
                    //ダミーなので生データを読み込む
                    EditorSceneManager.LoadSceneAsyncInPlayMode(sceneInfo.path, new LoadSceneParameters(mode));
                    return;
                }
            }
#endif
            //シーンロード
            SceneManager.LoadSceneAsync(sceneName, mode);
        }
    }
}