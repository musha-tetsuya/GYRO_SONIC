using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Musha
{
    /// <summary>
    /// ゲーム中共通オブジェクト
    /// </summary>
    public class GameCommon : SingletonMonoBehaviour<GameCommon>
    {
        /// <summary>
        /// ダイアログルート
        /// </summary>
        [SerializeField]
        public RectTransform dialogRoot = null;

        /// <summary>
        /// フェードルート
        /// </summary>
        [SerializeField]
        public RectTransform fadeRoot = null;

        /// <summary>
        /// タッチブロック
        /// </summary>
        [SerializeField]
        public Image touchBlock = null;

        /// <summary>
        /// システムダイアログルート
        /// </summary>
        [SerializeField]
        public RectTransform systemDialogRoot = null;

        /// <summary>
        /// Awake
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);
        }
    }
}