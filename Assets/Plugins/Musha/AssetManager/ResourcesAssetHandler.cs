using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Musha
{
    /// <summary>
    /// リソースアセットハンドラ
    /// </summary>
    public class ResourcesAssetHandler : AssetHandler
    {
#if UNITY_EDITOR
        /// <summary>
        /// アセットタイプがMonoBehaviourを継承しているかどうか
        /// </summary>
        private bool isMonoBehaviour = false;

        /// <summary>
        /// アセットバンドルシミュレーションロード用のパス
        /// </summary>
        private string assetPath = null;
#endif
        /// <summary>
        /// construct
        /// </summary>
        public ResourcesAssetHandler(string path, Type type)
            : base(path, type)
        {
#if UNITY_EDITOR
            this.isMonoBehaviour = this.type.IsSubclassOf(typeof(MonoBehaviour));
            this.assetPath = AssetDatabase
                .FindAssets(string.Format("{0} t:{1}", Path.GetFileNameWithoutExtension(this.path), this.isMonoBehaviour ? "GameObject" : this.type.Name), new string[]{ "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(x => !x.EndsWith(".cs"))
                .FirstOrDefault(x => !x.Contains("/Resources/") && Path.ChangeExtension(x, null).EndsWith(this.path, StringComparison.OrdinalIgnoreCase));
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// アセットの確保
        /// </summary>
        private void SetAsset()
        {
            if (this.isMonoBehaviour)
            {
                //MonoBehaviourを継承しているアセットはGameObject型でロードする
                this.asset = AssetDatabase.LoadAssetAtPath<GameObject>(this.assetPath);
                this.asset = (this.asset as GameObject).GetComponent(this.type);
            }
            else
            {
                this.asset = AssetDatabase.LoadAssetAtPath(this.assetPath, this.type);

                if (!(this.asset is GameObject))
                {
                    //GameObject以外のアセットは生データを書き換えないようにInstantiateしたアセットを確保する
                    this.asset = UnityEngine.Object.Instantiate(this.asset);
                }
            }
        }
#endif

        /// <summary>
        /// 同期ロード
        /// </summary>
        protected override void LoadInternal()
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(this.assetPath))
            {
                //アセットの確保
                this.SetAsset();
                return;
            }
#endif
            //アセットを確保
            this.asset = Resources.Load(this.path, this.type);
        }

        /// <summary>
        /// 非同期ロード
        /// </summary>
        public override void LoadAsync(Action onLoaded)
        {
            //ステータスをロード中に
            this.status = Status.Loading;

#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(this.assetPath))
            {
                AssetManager.Instance.StartDelayActionCoroutine(null, () =>
                {
                    //アセットの確保
                    this.SetAsset();

                    //ステータスを完了に
                    this.status = Status.Completed;

                    //ロード完了を通知
                    onLoaded?.Invoke();
                });
                return;
            }
#endif

            //ロード開始
            var request = Resources.LoadAsync(this.path, this.type);

            request.completed += (_) =>
            {
                //アセットを確保
                this.asset = request.asset;

                //ステータスを完了に
                this.status = Status.Completed;

                //ロード完了を通知
                onLoaded?.Invoke();
            };
        }
    }
}