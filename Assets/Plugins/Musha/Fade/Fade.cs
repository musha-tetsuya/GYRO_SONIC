using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Musha
{
    /// <summary>
    /// フェード
    /// </summary>
    public class Fade : InLoopOutAnimation
    {
        /// <summary>
        /// In再生
        /// </summary>
        public override void PlayIn()
        {
            this.animator.Play("In", 0, 0f);
            this.animator.ResetTrigger("LoopBreak");
        }

        /// <summary>
        /// Out再生
        /// </summary>
        public override void PlayOut()
        {
            var state = this.animator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName("Idle"))
            {
                this.animator.Play("FadeIn", 0, 0f);
            }
            else
            {
                this.animator.SetTrigger("LoopBreak");
            }
        }
    }
}