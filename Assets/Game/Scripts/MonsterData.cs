using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterData : Musha.MasterData
{
    public int hp;
    public ItemData[] items;
}

public class MonsterDB : Musha.StandardMasterDB<MonsterDB, MonsterData>
{
    public override string path => "AssetBundle/MonsterData";

    public void MonsterTest()
    {

    }
}