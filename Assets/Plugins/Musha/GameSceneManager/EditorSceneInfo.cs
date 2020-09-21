#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Musha
{
    /// <summary>
    /// Editor用シーン情報
    /// </summary>
    public class EditorSceneInfo
    {
        /// <summary>
        /// パス
        /// </summary>
        public string path;

        /// <summary>
        /// 名前
        /// </summary>
        public string name;

        /// <summary>
        /// アセットバンドルかどうか
        /// </summary>
        public bool isAssetBundle;
    }
}
#endif