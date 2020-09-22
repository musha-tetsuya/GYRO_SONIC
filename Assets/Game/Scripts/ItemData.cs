using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData : Musha.MasterData
{
    public string name;
    public int power;
}

public class ItemDB : Musha.MasterDB<ItemDB, ItemData>
{
    public override string path => "ItemData";
}