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
        /// <summary>
        /// construct
        /// </summary>
        public ResourcesAssetHandler(string path, Type type)
            : base(path, type)
        {
        }

        /// <summary>
        /// 非同期ロード
        /// </summary>
        public override void LoadAsync(Action onLoaded)
        {
            //ステータスをロード中に
            this.status = Status.Loading;

#if UNITY_EDITOR
            //未アセットバンドル化のアセットかどうか
            var assetPath = AssetDatabase
                .FindAssets(string.Format("{0} t:{1}", Path.GetFileNameWithoutExtension(this.path), this.type.Name), new string[]{ "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(x => !x.Contains("/Resources/") && Path.ChangeExtension(x, null).EndsWith(this.path, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetManager.Instance.StartDelayActionCoroutine(null, () =>
                {
                    //アセットを確保
                    this.asset = AssetDatabase.LoadAssetAtPath(assetPath, this.type);

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