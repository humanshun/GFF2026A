using UnityEngine;

[CreateAssetMenu(menuName = "DTBXT/Spawn/Local Weighted Decider")]
public class LocalWeightedDecider : SpawnDecider
{
    public override AnimalData RollNext(AnimalDatabase db)
    {
        return db.GetRandomByWeight();
    }
}