using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Upgrades/Better Unit Upgrade")]
public class BetterUnitUpgrade : Upgrade
{
    [System.Serializable]
    public struct BetterUnitUpgradeStage
    {
        public Currency cost;
        public Unit newUnit;
    }

    public BetterUnitUpgradeStage[] stages;

    public override void OnUpgrade(int stage)
    {
        SpawnManager.instance.AddNewSpawnableUnit(stages[stage].newUnit);

        if (stage != stages.Length - 1)
            NextStage(stage + 1);
    }

    public void NextStage(int stage)
    {
        Setup(stage);
    }

    public override Currency GetCost(int stage)
    {
        return stages[stage].cost;
    }

    public override string GetDescription(int stage)
    {
        return "Spawn better people";
    }

    public override string GetImprovementText(int stage)
    {
        if (stage == 0)
            return "Baby -> " + stages[stage+1].newUnit.unitName;
        else if (stage > 0 && stage <= stages.Length)
            return stages[stage].newUnit.unitName + " -> " + stages[stage+1].newUnit.unitName;
        else
            return "";
    }

    public override void Setup(int stage)
    {
        stages[stage].cost.ToFloat();
    }
}
