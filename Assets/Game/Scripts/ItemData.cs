using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData : Musha.MasterData
{
    public string name;
    public int power;
}

public class ItemDB : Musha.StandardMasterDB<ItemDB, ItemData>
{
    public override string path => "AssetBundle/ItemData";
}