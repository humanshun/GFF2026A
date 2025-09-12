using UnityEngine;

public abstract class SpawnDecider : ScriptableObject
{
    public abstract AnimalData RollNext(AnimalDatabase db);
}
