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
    public abstract class MasterDB<InstanceType, DataType>
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
        /// データリスト
        /// </summary>
        private List<DataType> dataList = null;

        /// <summary>
        /// データリスト
        /// </summary>
        public List<DataType> DataList
        {
            get
            {
#if UNITY_EDITOR
                if (this.dataList == null)
                {
                    var jsonAssetPath = AssetDatabase
                        .FindAssets(string.Format("{0} t:TextAsset", Path.GetFileNameWithoutExtension(path), new string[]{ "Assets" }))
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
                    this.dataList = new List<DataType>();
                }

                return this.dataList;
            }
        }

        /// <summary>
        /// データリストのセット
        /// </summary>
        public void SetDataList(List<DataType> dataList)
        {
            this.dataList = dataList;
        }

        /// <summary>
        /// データリストのセット
        /// </summary>
        public void SetDataList(string json)
        {
            this.SetDataList(Utf8Json.JsonSerializer.Deserialize<List<DataType>>(json));
        }

        /// <summary>
        /// IDからデータを取得
        /// </summary>
        public DataType GetById(long id)
        {
            return this.DataList.Find(x => x.id == id);
        }
    }
}
