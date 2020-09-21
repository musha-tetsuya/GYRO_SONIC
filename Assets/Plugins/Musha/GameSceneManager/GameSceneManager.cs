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
        /// シーン切り替えアニメーションプレハブ
        /// </summary>
        [SerializeField]
        private InLoopOutAnimation sceneChangeAnimationPrefab = null;

        /// <summary>
        /// シーン切り替えアニメーションを自動で消すかどうか
        /// </summary>
        [NonSerialized]
        public bool isAutoHideSceneChangeAnimation = true;

        /// <summary>
        /// シーン切り替えアニメーション
        /// </summary>
        [NonSerialized]
        public InLoopOutAnimation sceneChangeAnimation = null;

        /// <summary>
        /// ロードしたアセットバンドルシーン
        /// </summary>
        private List<AssetHandler> sceneHandlers = new List<AssetHandler>();

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
                .FindAssets("t:SceneAsset", new string[]{ "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path =>
                {
                    var sceneInfo = new EditorSceneInfo();
                    sceneInfo.path = path;
                    sceneInfo.isAssetBundle = !buildSettingsScenePaths.Contains(path);
                    sceneInfo.name = sceneInfo.isAssetBundle
                        ? path.Remove(0, "Assets/".Length).Replace(".unity", null)
                        : Path.GetFileNameWithoutExtension(path);
                    return sceneInfo;
                })
                .ToArray();
#endif
        }

        /// <summary>
        /// アセットバンドルのシーンかどうか
        /// </summary>
        private bool IsAssetBundleScene(string sceneName)
        {
            bool isAssetBundle = AssetManager.Instance.FindAssetBundleInfo(sceneName) != null;
#if UNITY_EDITOR
            isAssetBundle |= this.sceneInfos.Any(x => x.isAssetBundle && x.name.Equals(sceneName, StringComparison.OrdinalIgnoreCase));
#endif
            return isAssetBundle;
        }

        /// <summary>
        /// シーン切り替え
        /// </summary>
        public void ChangeSceneAsync(string sceneName)
        {
            if (this.sceneChangeAnimation != null && this.sceneChangeAnimation.gameObject != null)
            {
                //シーン切り替え中なので不可
                return;
            }

            //ローディング自動非表示フラグON
            this.isAutoHideSceneChangeAnimation = true;

            //ローディング開始
            this.sceneChangeAnimation = Instantiate(this.sceneChangeAnimationPrefab, GameCommon.Instance.fadeRoot, false);
            this.sceneChangeAnimation.onFinishedIn.AddListener(() => this.OnFinishedIn(sceneName));
            this.sceneChangeAnimation.onFinishedOut.AddListener(this.OnFinishedOut);
            this.sceneChangeAnimation.PlayIn();
        }

        /// <summary>
        /// ローディング表示完了後、シーン遷移処理
        /// </summary>
        private void OnFinishedIn(string sceneName)
        {
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
                            if (this.isAutoHideSceneChangeAnimation)
                            {
                                //ローディング自動非表示
                                this.sceneChangeAnimation.PlayOut();
                            }
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
        /// ローディング非表示完了後、破棄
        /// </summary>
        private void OnFinishedOut()
        {
            if (this.sceneChangeAnimation != null && this.sceneChangeAnimation.gameObject != null)
            {
                Destroy(this.sceneChangeAnimation.gameObject);
            }

            this.sceneChangeAnimation = null;
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
                    EditorSceneManager.LoadSceneAsyncInPlayMode(sceneInfo.path, new LoadSceneParameters(mode)).completed += (_) =>
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
    }
}