using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterData : Musha.MasterData
{
    public int hp;

    public ItemData[] items;
}

public class MonsterDB : Musha.MasterDB<MonsterDB, MonsterData>
{
    public override string path => "MonsterData";

    public void MonsterTest()
    {

    }
}