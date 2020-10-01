using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Musha
{
    /// <summary>
    /// アセットハンドラ
    /// </summary>
    public abstract class AssetHandler
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
        /// パス
        /// </summary>
        public string path { get; private set; }

        /// <summary>
        /// アセットタイプ
        /// </summary>
        public Type type { get; private set; }

        /// <summary>
        /// ステータス
        /// </summary>
        public Status status { get; protected set; }

        /// <summary>
        /// アセット
        /// </summary>
        public UnityEngine.Object asset { get; protected set; }

        /// <summary>
        /// 参照カウンタ
        /// </summary>
        public int referenceCount = 1;

        /// <summary>
        /// 破棄不可フラグ
        /// </summary>
        public bool isDontDestroy = false;

        /// <summary>
        /// 破棄可能かどうか
        /// </summary>
        public bool isUnloadable => !this.isDontDestroy && this.referenceCount <= 0 && this.status != Status.Loading;

        /// <summary>
        /// construct
        /// </summary>
        protected AssetHandler(string path, Type type)
        {
            this.path = path.Replace('\\', '/');
            this.type = type;
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
                    this.status = Status.Completed;
                    this.LoadInternal();
                }
                break;

                case Status.Loading:
                {
                    Debug.LogErrorFormat("アセット:{0}は非同期ロード中のため、同期ロード出来ません。", this.path);
                }
                break;
            }
        }

        /// <summary>
        /// 同期ロード
        /// </summary>
        protected abstract void LoadInternal();

        /// <summary>
        /// 非同期ロード
        /// </summary>
        public abstract void LoadAsync(Action onLoaded);

        /// <summary>
        /// アンロード
        /// </summary>
        public virtual void Unload(){}
    }
}