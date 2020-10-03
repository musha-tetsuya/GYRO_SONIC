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
        /// <summary>
        /// イベントリスナー
        /// </summary>
        public interface IEventListener
        {
            /// <summary>
            /// シーン切り替え時
            /// </summary>
            void OnChangeScene(string sceneName);

            /// <summary>
            /// シーンロード完了時
            /// </summary>
            void OnSceneLoaded();
        }

        /// <summary>
        /// イベントリスナー
        /// </summary>
        public IEventListener listener = null;

        /// <summary>
        /// 現在のイベントリスナー
        /// </summary>
        private IEventListener currentListener = null;

        /// <summary>
        /// ロードしたアセットバンドルシーン
        /// </summary>
        private List<AssetHandler> sceneHandlers = new List<AssetHandler>();

        /// <summary>
        /// アセットバンドルのシーンかどうか
        /// </summary>
        private bool IsAssetBundleScene(string sceneName)
        {
            bool isAssetBundle = AssetManager.Instance.FindAssetBundleInfo(sceneName) != null;
#if UNITY_EDITOR
            isAssetBundle |= SceneAssetInfo.Infos.Any(x => x.isAssetBundle && x.name.Equals(sceneName, StringComparison.OrdinalIgnoreCase));
#endif
            return isAssetBundle;
        }

        /// <summary>
        /// シーン切り替え
        /// </summary>
        public void ChangeSceneAsync(string sceneName)
        {
            if (this.listener != null && this.listener != this.currentListener)
            {
                //リスナーがいるなら処理を移譲
                this.currentListener = this.listener;
                this.currentListener.OnChangeScene(sceneName);
                return;
            }
            this.currentListener = null;

            //現在のシーンをアンロードする前にHierarchyが空にならないように空シーンを作成
            SceneManager.CreateScene("Empty");

            //現在のシーンをアンロード
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene()).completed += (a) =>
            {
                //アセットバンドルシーンのアンロード
                for (int i = 0, imax = this.sceneHandlers.Count; i < imax; i++)
                {
                    AssetManager.Instance.Unload(this.sceneHandlers[i]);
                }

                this.sceneHandlers.Clear();
#if DEBUG
                //シーン消えたけどUnloadされてないアセット一覧をログ表示
                foreach (var handler in AssetManager.Instance.handlers)
                {
                    Debug.LogFormat("未アンロード：{0}, referenceCount={1}, isDontDestroy={2}", handler.path, handler.referenceCount, handler.isDontDestroy);
                }
#endif
                //リソースアンロード
                Resources.UnloadUnusedAssets().completed += (b) =>
                {
                    //GC整理
                    GC.Collect();

                    //次のシーンをロードするアクション
                    Action loadNextScene = () =>
                    {
                        this.LoadSceneAsync(sceneName, LoadSceneMode.Single, () =>
                        {
                            //シーンロード完了イベント
                            this.listener?.OnSceneLoaded();
                        });
                    };

                    if (this.IsAssetBundleScene(sceneName))
                    {
                        //アセットバンドルシーンなら、アセットバンドルロード後次のシーンをロード
                        var handler = AssetManager.Instance.LoadSceneAssetAsync(sceneName, loadNextScene);
                        this.sceneHandlers.Add(handler);
                    }
                    else
                    {
                        //次のシーンをロード
                        loadNextScene();
                    }
                };
            };
        }

        /// <summary>
        /// 非同期ロード
        /// </summary>
        /// <br>
        /// sceneNameには、BuildSettingsに含まれている場合はシーン名を。
        /// 含まれていないアセットバンドルのシーンの場合はシーンパスを入れる。（「Assets/」と「.unity」は不要）
        /// </br>
        public void LoadSceneAsync(string sceneName, LoadSceneMode mode, Action onLoaded = null)
        {
#if UNITY_EDITOR
            //シーン情報検索
            var info = SceneAssetInfo.Infos.FirstOrDefault(x => x.name.Equals(sceneName, StringComparison.OrdinalIgnoreCase));
            if (info == null)
            {
                Debug.LogErrorFormat("{0}に一致するシーンは存在しません。", sceneName);
                return;
            }

            //アセットバンドルシーンの場合
            if (info.isAssetBundle)
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
                    EditorSceneManager.LoadSceneAsyncInPlayMode(info.path, new LoadSceneParameters(mode)).completed += (_) =>
                    {
                        onLoaded?.Invoke();
                    };
                    return;
                }
            }
#endif
            //シーンロード
            SceneManager.LoadSceneAsync(sceneName, mode).completed += (_) =>
            {
                onLoaded?.Invoke();
            };
        }

        /// <summary>
        /// 非同期アンロード
        /// 主にAdditiveしたシーンに対して行う
        /// </summary>
        public void UnloadSceneAsync(string sceneName, Action onCompleted = null)
        {
            SceneManager.UnloadSceneAsync(sceneName).completed += (_) =>
            {
                onCompleted?.Invoke();
            };
        }

#if UNITY_EDITOR
        /// <summary>
        /// シーン情報
        /// </summary>
        private class SceneAssetInfo
        {
            /// <summary>
            /// パス
            /// </summary>
            public string path;

            /// <summary>
            /// 名前
            /// </summary>
            public string name;

            /// <summary>
            /// アセットバンドルかどうか
            /// </summary>
            public bool isAssetBundle;

            /// <summary>
            /// Assets内全シーン情報
            /// </summary>
            private static SceneAssetInfo[] infos = null;

            /// <summary>
            /// Assets内全シーン情報
            /// </summary>
            public static SceneAssetInfo[] Infos
            {
                get
                {
                    if (infos == null)
                    {
                        //BuildSettingsに含まれているシーンのパス一覧
                        var buildSettingsScenePaths = EditorBuildSettings.scenes
                            .Select(x => x.path)
                            .ToArray();

                        //Assets内全シーン情報
                        infos = AssetDatabase
                            .FindAssets("t:SceneAsset", new string[]{ "Assets" })
                            .Select(AssetDatabase.GUIDToAssetPath)
                            .Select(path =>
                            {
                                var info = new SceneAssetInfo();
                                info.path = path;
                                info.isAssetBundle = !buildSettingsScenePaths.Contains(path);
                                info.name = info.isAssetBundle
                                    ? path.Remove(0, "Assets/".Length).Replace(".unity", null)
                                    : Path.GetFileNameWithoutExtension(path);
                                return info;
                            })
                            .ToArray();
                    }
                    return infos;
                }
            }
        }
#endif
    }
}