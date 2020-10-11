using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Musha
{
    /// <summary>
    /// サウンドオブジェクト
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundObject : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource = null;

        public SoundData soundData { get; private set; }

        public int priority { get; private set;}

        private float loopStartTime = 0f;
        private float loopEndTime = 0f;

        public bool isPlaying { get; private set; }
        public bool isPause { get; private set; }
        private float time = 0f;

        public event Action<SoundObject> onStop = null;
        public event Action<SoundObject> onDestroy = null;

        private void OnDestroy()
        {
            this.onDestroy?.Invoke(this);
            this.onDestroy = null;
        }

        private void Update()
        {
            if (this.isPlaying && !this.isPause)
            {
                if (this.audioSource.isPlaying)
                {
                    if (this.audioSource.loop && this.audioSource.time >= this.loopStartTime)
                    {
                        this.audioSource.time = this.loopStartTime + (this.audioSource.time - this.loopEndTime);
                    }

                    this.time = this.audioSource.time;
                }
                else
                {
                    this.Stop();
                }
            }
        }

        public void Init(SoundData soundData, int priority)
        {
            this.soundData = soundData;
            this.priority = priority;
            this.audioSource.clip = this.soundData.audioClip;

            this.loopStartTime = this.audioSource.clip.length * this.soundData.loopStartNormalizedTime;
            this.loopEndTime = this.audioSource.clip.length * this.soundData.loopEndNormalizedTime;
        }

        public void Play()
        {
            if (!this.isPlaying || this.isPause)
            {
                this.isPlaying = true;
                this.isPause = false;

                this.audioSource.Play();
                this.audioSource.time = this.time;
            }
        }

        public void Pause()
        {
            if (this.isPlaying && !this.isPause)
            {
                this.isPause = true;
                this.time = this.audioSource.time;
                this.audioSource.Stop();
            }
        }

        public void Stop()
        {
            if (this.isPlaying)
            {
                this.isPlaying = false;
                this.isPause = false;
                this.time = 0f;
                this.audioSource.Stop();
                this.onStop?.Invoke(this);
                this.onStop = null;
            }
        }
    }
}