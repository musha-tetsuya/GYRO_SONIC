using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Musha
{
    public class SoundManager : SingletonMonoBehaviour<SoundManager>
    {
        [SerializeField]
        private SoundObjectPool bgmPool = null;

        [SerializeField]
        private SoundObjectPool sePool = null;

        public SoundObject PlayBgm(SoundData soundData)
        {
            return this.bgmPool.Play(soundData);
        }

        public void StopBgm(SoundObject soundObject)
        {
            this.bgmPool.Stop(soundObject);
        }

        public SoundObject PlaySe(SoundData soundData, int priority = 0, int polyphonySize = 2)
        {
            return this.sePool.Play(soundData, priority, polyphonySize);
        }
    }
}