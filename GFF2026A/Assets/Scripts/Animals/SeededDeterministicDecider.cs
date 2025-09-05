using UnityEngine;

[CreateAssetMenu(menuName = "DTBXT/Spawn/Seeded Deterministic Decider")]
public class SeededDeterministicDecider : SpawnDecider
{
    [SerializeField] private int seed = 12345;
    private System.Random rng;

    // ScriptableObjectがロードされたときに初期化
    private void OnEnable()
    {
        rng = new System.Random(seed);
    }

    // 外部からシードを設定（オンライン時にサーバーが決める想定）
    public void SetSeed(int s)
    {
        seed = s;
        rng = new System.Random(seed);
    }

    // 必ず rng を用意してから使う
    private void EnsureRng()
    {
        rng ??= new System.Random(seed);
    }

    public override AnimalData RollNext(AnimalDatabase db)
    {
        EnsureRng(); // ← 忘れず呼ぶ
        return db.GetRandomByWeight(null, rng);
    }
}
