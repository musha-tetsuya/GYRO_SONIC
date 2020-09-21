using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Musha
{
    /// <summary>
    /// アセットバンドルアセットハンドラ
    /// </summary>
    public class AssetBundleAssetHandler : AssetHandler
    {
        /// <summary>
        /// アセットタイプがMonoBehaviourを継承しているかどうか
        /// </summary>
        private bool isMonoBehaviour = false;

        /// <summary>
        /// アセットバンドルハンドラ
        /// </summary>
        private AssetBundleHandler handler = null;

        /// <summary>
        /// construct
        /// </summary>
        public AssetBundleAssetHandler(string path, Type type, AssetBundleInfo info)
            : base(path, type)
        {
            this.isMonoBehaviour = this.type != null && this.type.IsSubclassOf(typeof(MonoBehaviour));
            this.handler = AssetBundleHandler.GetOrCreate(info);
            this.handler.AddReferenceUser(this);
        }

        /// <summary>
        /// 非同期ロード
        /// </summary>
        public override void LoadAsync(Action onLoaded)
        {
            //ステータスをロード中に
            this.status = Status.Loading;

            //アセットバンドルのロード開始
            this.handler.LoadAsync(() =>
            {
                if (this.handler.assetBundle.isStreamedSceneAssetBundle)
                {
                    //シーンアセットならこの時点で完了
                    this.status = Status.Completed;
                    onLoaded?.Invoke();
                }
                else
                {
                    //アセット名
                    var assetName = Path.GetFileName(this.path);

                    //サブアセットかどうか
                    var isSubAsset = !this.path.Equals(this.handler.assetBundle.name, StringComparison.OrdinalIgnoreCase);

                    //MonoBehaviourを継承しているアセットはGameObject型でロードする
                    var assetType = this.isMonoBehaviour ? typeof(GameObject) : this.type;

                    //アセットのロード開始
                    var request = isSubAsset
                                ? this.handler.assetBundle.LoadAssetWithSubAssetsAsync(assetName, assetType)
                                : this.handler.assetBundle.LoadAssetAsync(assetName, assetType);

                    request.completed += (_) =>
                    {
                        //アセットを確保
                        this.asset = request.asset;

                        if (this.isMonoBehaviour)
                        {
                            this.asset = (this.asset as GameObject).GetComponent(this.type);
                        }

                        //ステータスを完了に
                        this.status = Status.Completed;

                        //ロード完了を通知
                        onLoaded?.Invoke();
                    };
                }
            });
        }

        /// <summary>
        /// アンロード
        /// </summary>
        public override void Unload()
        {
            if (this.isUnloadable)
            {
                this.handler?.Unload(this);
                this.handler = null;
            }
        }
    }
}