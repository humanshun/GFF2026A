using UnityEngine;

public class KillZone2D : MonoBehaviour
{
    [SerializeField] private GameOverController gameOver;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // AnimalPiece だけ対象にしたいならフィルタ
        if (!other.GetComponentInParent<AnimalPiece>()) return;

        gameOver?.TriggerGameOver(); // publicにしておく
    }
}
