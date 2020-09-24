using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Musha
{
    /// <summary>
    /// マスターデータ基底
    /// </summary>
    public abstract class MasterData
    {
        /// <summary>
        /// ID
        /// </summary>
        public long id;
    }

    /// <summary>
    /// マスターデータベース基底
    /// </summary>
    public abstract class MasterDB<InstanceType, DataType, KeyType>
        where InstanceType : new()
        where DataType : MasterData
    {
        /// <summary>
        /// インスタンス
        /// </summary>
        public readonly static InstanceType Instance = new InstanceType();

        /// <summary>
        /// Jsonパス
        /// </summary>
        public abstract string path { get; }

        /// <summary>
        /// Dictionary化する際のKey
        /// </summary>
        protected abstract Func<DataType, KeyType> keySelector { get; }

        /// <summary>
        /// データリスト
        /// </summary>
        private Dictionary<KeyType, DataType> dataList = null;

        /// <summary>
        /// データリスト
        /// </summary>
        public Dictionary<KeyType, DataType> DataList
        {
            get
            {
#if UNITY_EDITOR
                if (this.dataList == null)
                {
                    var jsonAssetPath = AssetDatabase
                        .FindAssets(string.Format("{0} t:TextAsset", Path.GetFileNameWithoutExtension(this.path), new string[]{ "Assets" }))
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .FirstOrDefault(x => Path.ChangeExtension(x, null).EndsWith(this.path, StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrEmpty(jsonAssetPath))
                    {
                        var json = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonAssetPath);
                        this.SetDataList(json.text);
                    }
                }
#endif
                if (this.dataList == null)
                {
                    this.dataList = new Dictionary<KeyType, DataType>();
                }

                return this.dataList;
            }
        }

        /// <summary>
        /// データリストのセット
        /// </summary>
        public void SetDataList(Dictionary<KeyType, DataType> dataList)
        {
            this.dataList = dataList;
        }

        /// <summary>
        /// データリストのセット
        /// </summary>
        public void SetDataList(string json)
        {
            this.SetDataList(Utf8Json.JsonSerializer.Deserialize<DataType[]>(json).ToDictionary(this.keySelector));
        }
    }

    /// <summary>
    /// IDがKeyのスタンダードなマスターDB
    /// </summary>
    public abstract class StandardMasterDB<InstanceType, DataType> : MasterDB<InstanceType, DataType, long>
        where InstanceType : new()
        where DataType : MasterData
    {
        /// <summary>
        /// Dictionary化する際のKey
        /// </summary>
        protected override Func<DataType, long> keySelector => (x) => x.id;
    }
}
