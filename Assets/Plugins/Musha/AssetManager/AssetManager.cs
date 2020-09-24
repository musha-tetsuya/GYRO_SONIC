using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Musha
{
    /// <summary>
    /// アセットマネージャー
    /// </summary>
    public class AssetManager : SingletonMonoBehaviour<AssetManager>
    {
        /// <summary>
        /// アセットバンドルディレクトリパス
        /// </summary>
        private static string assetBundleDirectoryPath = null;

        /// <summary>
        /// 最大スレッド数
        /// </summary>
        [SerializeField]
        private int maxThreadCount = 5;

        /// <summary>
        /// アセットバンドル情報リスト
        /// </summary>
        public List<AssetBundleInfo> infoList = new List<AssetBundleInfo>();

        /// <summary>
        /// ロード中 or ロード済みのアセットリスト
        /// </summary>
        public List<AssetHandler> handlers = new List<AssetHandler>();

        /// <summary>
        /// 積まれているコールバック一覧
        /// </summary>
        private List<(AssetHandler handler, Action callback)> callbacks = new List<(AssetHandler, Action)>();

        /// <summary>
        /// アセットバンドルディレクトリパスを取得
        /// </summary>
        public static string GetAssetBundleDirectoryPath()
        {
            if (string.IsNullOrEmpty(assetBundleDirectoryPath))
            {
#if UNITY_EDITOR
                assetBundleDirectoryPath = Application.dataPath.Replace("Assets", "AssetBundle");
#elif UNITY_PS4
                assetBundleDirectoryPath = Application.streamingAssetsPath + "/AssetBundle";
#else
                assetBundleDirectoryPath = Application.persistentDataPath + "/AssetBundle";
#endif
            }

            return assetBundleDirectoryPath;
        }

        /// <summary>
        /// アセットバンドル情報の検索
        /// </summary>
        public AssetBundleInfo FindAssetBundleInfo(string path)
        {
            int imax = this.infoList.Count;

            //パスとアセットバンドル名の完全一致検索
            for (int i = 0; i < imax; i++)
            {
                if (path.Equals(this.infoList[i].assetBundleName, StringComparison.OrdinalIgnoreCase))
                {
                    return this.infoList[i];
                }
            }

            string lowerPath = path.ToLower();

            //パスがアセットバンドル名を含んでいるか検索
            for (int i = 0; i < imax; i++)
            {
                if (lowerPath.Contains(this.infoList[i].assetBundleName))
                {
                    return this.infoList[i];
                }
            }

            return null;
        }

        /// <summary>
        /// ロード済み or ロード中のアセットハンドラを検索
        /// </summary>
        public AssetHandler FindAssetHandler(string path, Type type = null)
        {
            return this.handlers.Find(handler =>
            {
                if (path.Equals(handler.path, StringComparison.OrdinalIgnoreCase))
                {
                    return (type == null)
                        || (type == handler.type)
                        || (handler.type != null && handler.type.IsSubclassOf(type));
                }
                return false;
            });
        }

        /// <summary>
        /// 同期ロード
        /// </summary>
        public T Load<T>(string path) where T : UnityEngine.Object
        {
            //既に読み込みがかかっているか検索
            var handler = this.FindAssetHandler(path, typeof(T));

            //ハンドルが既存
            if (handler != null)
            {
                //参照カウンタ増加
                handler.referenceCount++;
            }
            else
            {
                //アセットバンドルかどうかの情報を検索
                var info = this.FindAssetBundleInfo(path);
                if (info == null)
                {
                    //Resources用アセットハンドラ生成
                    handler = new ResourcesAssetHandler(path, typeof(T));
                }
                else
                {
                    //AssetBundle用アセットハンドラ生成
                    handler = new AssetBundleAssetHandler(path, typeof(T), info);
                }

                //リストにアセットハンドラを保持
                this.handlers.Add(handler);
            }

            //ロード
            handler.Load();

            return handler.asset as T;
        }

        /// <summary>
        /// 非同期ロード
        /// </summary>
        public AssetHandler LoadAsync<T>(string path, Action<T> onLoaded = null) where T : UnityEngine.Object
        {
            //既に読み込みがかかっているか検索
            var handler = this.FindAssetHandler(path, typeof(T));

            //ハンドルが既存
            if (handler != null)
            {
                //参照カウンタ増加
                handler.referenceCount++;

                //コールバックを積む
                this.callbacks.Add((handler, () => onLoaded?.Invoke(handler.asset as T)));

                //ハンドルがロード済みなら積んであるコールバックの消化を試みる（1フレ後に）
                if (handler.status == AssetHandler.Status.Completed)
                {
                    this.StartDelayActionCoroutine(null, this.OnLoadedAssetHandler);
                }
            }
            //ハンドルが存在しなかったら
            else
            {
                //アセットバンドルかどうかの情報を検索
                var info = this.FindAssetBundleInfo(path);
                if (info == null)
                {
                    //Resources用アセットハンドラ生成
                    handler = new ResourcesAssetHandler(path, typeof(T));
                }
                else
                {
                    //AssetBundle用アセットハンドラ生成
                    handler = new AssetBundleAssetHandler(path, typeof(T), info);
                }

                //リストにアセットハンドラを保持
                this.handlers.Add(handler);

                //コールバックを積む
                this.callbacks.Add((handler, () => onLoaded?.Invoke(handler.asset as T)));

                //ロード開始
                this.LoadStartIfCan();
            }

            return handler;
        }

        /// <summary>
        /// シーンアセットの同期ロード
        /// </summary>
        public void LoadSceneAsset(string path)
        {
            //既に読み込みがかかっているか検索
            var handler = this.FindAssetHandler(path);

            //ハンドルが既存
            if (handler != null)
            {
                //参照カウンタ増加
                handler.referenceCount++;
            }
            //ハンドルが存在しなかったら
            else
            {
                //アセットバンドル情報を検索
                var info = this.FindAssetBundleInfo(path);
                if (info == null)
                {
#if UNITY_EDITOR
                    //ダミー用アセットハンドラ生成
                    handler = new DummyAssetHandler(path);
#else
                    Debug.LogErrorFormat("{0}のアセットバンドル情報がありません。", path);
                    return;
#endif
                }
                else
                {
                    //AssetBundle用アセットハンドラ生成
                    handler = new AssetBundleAssetHandler(path, null, info);
                }

                //リストにアセットハンドラを保持
                this.handlers.Add(handler);
            }

            //ロード
            handler.Load();
        }

        /// <summary>
        /// シーンアセットの非同期ロード
        /// </summary>
        public AssetHandler LoadSceneAssetAsync(string path, Action onLoaded = null)
        {
            //既に読み込みがかかっているか検索
            var handler = this.FindAssetHandler(path);

            //ハンドルが既存
            if (handler != null)
            {
                //参照カウンタ増加
                handler.referenceCount++;

                //コールバックを積む
                this.callbacks.Add((handler, onLoaded));

                //ハンドルがロード済みなら積んであるコールバックの消化を試みる（1フレ後に）
                if (handler.status == AssetHandler.Status.Completed)
                {
                    this.StartDelayActionCoroutine(null, this.OnLoadedAssetHandler);
                }
            }
            //ハンドルが存在しなかったら
            else
            {
                //アセットバンドル情報を検索
                var info = this.FindAssetBundleInfo(path);
                if (info == null)
                {
#if UNITY_EDITOR
                    //ダミー用アセットハンドラ生成
                    handler = new DummyAssetHandler(path);
#else
                    Debug.LogErrorFormat("{0}のアセットバンドル情報がありません。", path);
                    return null;
#endif
                }
                else
                {
                    //AssetBundle用アセットハンドラ生成
                    handler = new AssetBundleAssetHandler(path, null, info);
                }

                //リストにアセットハンドラを保持
                this.handlers.Add(handler);

                //コールバックを積む
                this.callbacks.Add((handler, onLoaded));

                //ロード開始
                this.LoadStartIfCan();
            }

            return handler;
        }

        /// <summary>
        /// スレッドに余裕があるならロード開始
        /// </summary>
        private void LoadStartIfCan()
        {
            for (int i = 0, imax = this.handlers.Count, loadingCount = 0; i < imax; i++)
            {
                if (this.handlers[i].status == AssetHandler.Status.Loading)
                {
                    //ロード中のハンドラ数をカウント
                    loadingCount++;

                    if (loadingCount >= this.maxThreadCount)
                    {
                        //スレッドが埋まっているのでロード不可
                        return;
                    }
                }
            }

            //スレッドに余裕があるので未処理ハンドラのロードを開始
            var handler = this.handlers.Find(x => x.status == AssetHandler.Status.None);
            if (handler != null)
            {
                handler.LoadAsync(this.OnLoadedAssetHandler);
            }
        }

        /// <summary>
        /// アセットハンドラのロード完了時
        /// </summary>
        private void OnLoadedAssetHandler()
        {
            //先頭のコールバックに対応するハンドラがロード済みなら、コールバック消化
            while (this.callbacks.Count > 0 && this.callbacks[0].handler.status == AssetHandler.Status.Completed)
            {
                this.callbacks[0].callback?.Invoke();
                this.callbacks.RemoveAt(0);
            }

            //スレッドに余裕があるならロード開始
            this.LoadStartIfCan();
        }

        /// <summary>
        /// アンロード
        /// </summary>
        public void Unload(AssetHandler handler)
        {
            if (this.handlers.Contains(handler))
            {
                if (handler.referenceCount > 0)
                {
                    //参照カウンタ減少
                    handler.referenceCount--;
                }

                if (handler.isUnloadable)
                {
                    //可能なら破棄
                    handler.Unload();
                    this.handlers.Remove(handler);
                    this.callbacks.RemoveAll(x => x.handler == handler);
                }
            }
        }

        /// <summary>
        /// 遅延実行
        /// </summary>
        private IEnumerator DelayAction(object obj, Action callback)
        {
            yield return obj;
            callback?.Invoke();
        }

        /// <summary>
        /// 遅延実行コルーチンの開始
        /// </summary>
        public Coroutine StartDelayActionCoroutine(object obj, Action callback)
        {
            return StartCoroutine(this.DelayAction(obj, callback));
        }

#if UNITY_EDITOR
        /// <summary>
        /// アセットバンドル名を付与する
        /// </summary>
        [MenuItem("Assets/AssetManager/SetAssetBundleName")]
        private static void SetAssetBundleName()
        {
            foreach (int instanceId in Selection.instanceIDs)
            {
                //選択しているアセットのパス
                string assetPath = AssetDatabase.GetAssetPath(instanceId);

                //パスからアセットバンドル名を設定
                string assetBundleName = assetPath;

                //先頭の「Assets/」を除去
                assetBundleName = assetBundleName.Remove(0, "Assets/".Length);

                //拡張子を除去
                if (Path.HasExtension(assetBundleName))
                {
                    string extension = Path.GetExtension(assetBundleName);
                    assetBundleName = assetBundleName.Replace(extension, null);
                }

                var importer = AssetImporter.GetAtPath(assetPath);
                importer.assetBundleName = assetBundleName;
            }

            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        /// <summary>
        /// アセットバンドルビルド
        /// </summary>
        [MenuItem("Assets/AssetManager/BuildAssetBundle")]
        private static void BuildAssetBundle()
        {
            if (!Directory.Exists(GetAssetBundleDirectoryPath()))
            {
                Directory.CreateDirectory(GetAssetBundleDirectoryPath());
            }

            BuildPipeline.BuildAssetBundles(
                GetAssetBundleDirectoryPath(),
                BuildAssetBundleOptions.None,
                EditorUserBuildSettings.activeBuildTarget
            );
        }
#endif
    }

}
