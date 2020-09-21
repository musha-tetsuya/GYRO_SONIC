using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Musha
{
    /// <summary>
    /// アセットバンドル情報
    /// </summary>
    public class AssetBundleInfo
    {
        /// <summary>
        /// アセットバンドル名
        /// </summary>
        public string assetBundleName;

        /// <summary>
        /// CRC
        /// </summary>
        public uint crc;

        /// <summary>
        /// 依存関係
        /// </summary>
        public string[] dependencies;

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        public long fileSize;
    }
}
