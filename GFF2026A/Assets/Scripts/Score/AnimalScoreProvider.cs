using UnityEngine;

public class AnimalScoreProvider : MonoBehaviour
{
    [SerializeField] private AnimalData data;
    public int GetScore() => data ? data.ScoreValue : 0;
}
