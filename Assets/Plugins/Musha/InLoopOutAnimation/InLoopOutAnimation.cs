using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Musha
{
    /// <summary>
    /// In-Loop-Outアニメーション基礎
    /// </summary>
    public abstract class InLoopOutAnimation : MonoBehaviour
    {
        /// <summary>
        /// アニメーター
        /// </summary>
        [SerializeField]
        public Animator animator = null;

        /// <summary>
        /// Inアニメーション終了時コールバック
        /// </summary>
        [SerializeField]
        public UnityEvent onFinishedIn = null;

        /// <summary>
        /// Outアニメーション終了時コールバック
        /// </summary>
        [SerializeField]
        public UnityEvent onFinishedOut = null;

        /// <summary>
        /// Inアニメーション終了時
        /// </summary>
        protected virtual void OnFinishedIn()
        {
            this.onFinishedIn?.Invoke();
        }

        /// <summary>
        /// Outアニメーション終了時
        /// </summary>
        protected virtual void OnFinishedOut()
        {
            this.onFinishedOut?.Invoke();
        }

        /// <summary>
        /// In再生
        /// </summary>
        public abstract void PlayIn();

        /// <summary>
        /// Out再生
        /// </summary>
        public abstract void PlayOut();
    }
}