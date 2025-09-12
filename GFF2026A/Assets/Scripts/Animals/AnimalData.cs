using UnityEngine;

[CreateAssetMenu(fileName = "Animal", menuName = "Scriptable Objects/Animal Data")]
public class AnimalData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("動物を一意に識別するID（保存や検索用）。英数字で重複しないようにする。")]
    [SerializeField] private string id;

    [Tooltip("ゲーム内に表示する名前（UIやログ用）。")]
    [SerializeField] private string displayName;

    [Tooltip("UIやリスト表示などに使うアイコン画像。")]
    [SerializeField] private Sprite icon;

    [Header("Prefabs")]
    [Tooltip("ゲーム内に生成する動物のプレハブ。見た目やColliderを含む。")]
    [SerializeField] private GameObject prefab;

    [Header("Physics Settings")]
    [Tooltip("質量（Rigidbody2D.mass に反映）。重いほど落下が速く押し返しに強い。")]
    [SerializeField] private float mass = 1f;

    [Tooltip("Collider2D に設定する物理マテリアル。摩擦やバウンドを制御。")]
    [SerializeField] private PhysicsMaterial2D physMat;

    [Header("Spawn Weight")]
    [Tooltip("ランダム出現時の重み。数値が大きいほど出やすくなる。")]
    [SerializeField] private int spawnWeight = 10;

    [Header("Score")]
    [Tooltip("タイムアップ時に残っていた場合の得点。1体あたり固定で加算される。")]
    [SerializeField] private int scoreValue = 100;

    /// <summary>
    /// 外部公開用プロパティ
    /// </summary>
    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject Prefab => prefab;
    public float Mass => mass;
    public PhysicsMaterial2D PhysMat => physMat;
    public int SpawnWeight => spawnWeight;
    public int ScoreValue => scoreValue;
}
