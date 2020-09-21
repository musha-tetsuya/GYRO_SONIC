using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Musha
{
    /// <summary>
    /// アセットバンドルハンドラ
    /// </summary>
    public class AssetBundleHandler
    {
        /// <summary>
        /// ステータス
        /// </summary>
        public enum Status
        {
            None,
            Loading,
            Completed,
        }

        /// <summary>
        /// ロード中 or ロード済みのアセットバンドルリスト
        /// </summary>
        private static List<AssetBundleHandler> handlers = new List<AssetBundleHandler>();

        /// <summary>
        /// アセットバンドル情報
        /// </summary>
        public AssetBundleInfo info { get; private set; }

        /// <summary>
        /// ステータス
        /// </summary>
        public Status status { get; private set; }

        /// <summary>
        /// アセットバンドル
        /// </summary>
        public AssetBundle assetBundle { get; private set; }

        /// <summary>
        /// 依存関係のアセットバンドルハンドラ一覧
        /// </summary>
        private AssetBundleHandler[] dependencies = new AssetBundleHandler[0];

        /// <summary>
        /// 参照しているユーザー
        /// </summary>
        private List<AssetBundleAssetHandler> referenceUsers = new List<AssetBundleAssetHandler>();

        /// <summary>
        /// ロード完了時コールバック
        /// </summary>
        private Action onLoaded = null;

        /// <summary>
        /// construct
        /// </summary>
        private AssetBundleHandler(AssetBundleInfo info)
        {
            handlers.Add(this);
            
            this.info = info;

            if (this.info.dependencies != null)
            {
                this.dependencies = this.info.dependencies
                    .Select(AssetManager.Instance.FindAssetBundleInfo)
                    .Select(GetOrCreate)
                    .ToArray();
            }
        }

        /// <summary>
        /// アセットバンドルハンドラの取得 or 生成
        /// </summary>
        public static AssetBundleHandler GetOrCreate(AssetBundleInfo info)
        {
            var handler = handlers.Find(x => x.info == info);
            if (handler == null)
            {
                handler = new AssetBundleHandler(info);
            }
            return handler;
        }

        /// <summary>
        /// 参照ユーザー登録
        /// </summary>
        public void AddReferenceUser(AssetBundleAssetHandler user)
        {
            if (!this.referenceUsers.Contains(user))
            {
                this.referenceUsers.Add(user);

                //依存関係のアセットバンドルにも参照ユーザー登録
                foreach (var dependency in this.dependencies)
                {
                    dependency.AddReferenceUser(user);
                }
            }
        }

        /// <summary>
        /// 同期ロード
        /// </summary>
        public void Load()
        {
            switch (this.status)
            {
                case Status.None:
                {
                    //依存関係のロード
                    foreach (var dependency in this.dependencies)
                    {
                        dependency.Load();
                    }

                    string path = Path.Combine(AssetManager.GetAssetBundleDirectoryPath(), this.info.assetBundleName);

                    //アセットバンドル確保
                    this.assetBundle = AssetBundle.LoadFromFile(path);

                    //ロード完了
                    this.status = Status.Completed;
                }
                break;

                case Status.Loading:
                {
                    Debug.LogErrorFormat("アセットバンドル:{0}は非同期ロード中のため、同期ロード出来ません。", this.info.assetBundleName);
                }
                break;
            }
        }

        /// <summary>
        /// 非同期ロード
        /// </summary>
        public void LoadAsync(Action onLoaded)
        {
            switch (this.status)
            {
                case Status.None:
                {
                    //ステータスをロード中に
                    this.status = Status.Loading;

                    //依存関係を先にロード
                    foreach (var dependency in this.dependencies)
                    {
                        if (dependency.status != Status.Completed)
                        {
                            dependency.LoadAsync(() =>
                            {
                                this.status = Status.None;
                                this.LoadAsync(onLoaded);
                            });

                            return;
                        }
                    }

                    //自身のロードを開始
                    string path = Path.Combine(AssetManager.GetAssetBundleDirectoryPath(), this.info.assetBundleName);
                    var request = AssetBundle.LoadFromFileAsync(path);
                    request.completed += (_) =>
                    {
                        //アセットバンドル確保
                        this.assetBundle = request.assetBundle;

                        //ロード完了を通知
                        this.status = Status.Completed;
                        onLoaded?.Invoke();
                        this.onLoaded?.Invoke();
                        this.onLoaded = null;
                    };
                }
                break;

                case Status.Loading:
                {
                    //ロード中なのでコールバック追加
                    this.onLoaded += onLoaded;
                }
                break;

                case Status.Completed:
                {
                    //ロード済みなのでコールバック実行
                    onLoaded?.Invoke();
                }
                break;
            }
        }

        /// <summary>
        /// アンロード
        /// </summary>
        public void Unload(AssetBundleAssetHandler user)
        {
            //参照ユーザーの解除
            this.referenceUsers.Remove(user);

            //参照ユーザーがいなくなった
            if (this.referenceUsers.Count == 0)
            {
                //アセットバンドル破棄
                this.assetBundle?.Unload(true);
                this.assetBundle = null;

                //自信をリストから除去
                handlers.Remove(this);
            }

            //依存関係のアンロード
            foreach (var dependency in this.dependencies)
            {
                dependency.Unload(user);
            }
        }
    }
}