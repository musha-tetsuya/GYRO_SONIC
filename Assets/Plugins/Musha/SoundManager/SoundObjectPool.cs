using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Musha
{
    public class SoundObjectPool : MonoBehaviour
    {
        [SerializeField]
        private SoundObject soundObjectPrefab = null;

        [SerializeField]
        private int maxSize = 2;

        private List<SoundObject> soundObjects = new List<SoundObject>();

        public SoundObject Play(SoundData soundData, int priority = 0, int polyphonySize = 1)
        {
            SoundObject freeSoundObject = null;

            var sameTracks = this.soundObjects.FindAll(x => x.soundData == soundData && x.isPlaying);
            if (sameTracks.Count >= polyphonySize)
            {
                //既に同時発音数以上発声していたら、発声中トラックのうち一番古いトラックを利用する
                freeSoundObject = sameTracks[0];
            }
            else
            {
                //空いてるトラックを探す
                freeSoundObject = this.soundObjects.Find(x => !x.isPlaying);

                //空いてるトラックがなかった
                if (freeSoundObject == null)
                {
                    if (this.soundObjects.Count < this.maxSize)
                    {
                        //まだ余裕があるのでトラックを追加
                        freeSoundObject = Instantiate(this.soundObjectPrefab, this.transform, false);
                        freeSoundObject.onDestroy += (obj) => this.soundObjects?.Remove(obj);
                    }
                    else
                    {
                        //プライオリティが低いトラックを利用する
                        freeSoundObject = this.soundObjects[0];
                    }
                }
            }

            freeSoundObject.Stop();
            freeSoundObject.Init(soundData, priority);

            this.soundObjects.Remove(freeSoundObject);
            this.soundObjects.Add(freeSoundObject);
            this.soundObjects.Sort((a, b) => a.priority - b.priority);

            freeSoundObject.gameObject.SetActive(true);
            freeSoundObject.Play();
            freeSoundObject.onStop += (obj) =>
            {
                obj.gameObject.SetActive(false);
                obj.transform.SetParent(this.transform);
            };

            return freeSoundObject;
        }

        public void Stop(SoundObject soundObject)
        {
            if (this.soundObjects.Contains(soundObject))
            {
                soundObject.Stop();
            }
        }
    }
}