using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Musha
{
    /// <summary>
    /// ダミーアセットハンドラ
    /// </summary>
    public class DummyAssetHandler : AssetHandler
    {
        /// <summary>
        /// construct
        /// </summary>
        public DummyAssetHandler(string path)
            : base(path, null)
        {
        }

        /// <summary>
        /// 非同期ロード
        /// </summary>
        public override void LoadAsync(Action onLoaded)
        {
            //ステータスをロード中に
            this.status = Status.Loading;

            //1フレ後にロード完了
            AssetManager.Instance.StartDelayActionCoroutine(null, () =>
            {
                //ステータスを完了に
                this.status = Status.Completed;

                //ロード完了を通知
                onLoaded?.Invoke();
            });
        }
    }
}