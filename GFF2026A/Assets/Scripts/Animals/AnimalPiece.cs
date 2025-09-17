using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AnimalPiece : MonoBehaviour
{
    private GameObject _gameObject;
    private IAnimalRegistry _registry;

    private void Awake()
    {
        _gameObject = gameObject;
    }

    public void Initialize(IAnimalRegistry registry)
    {
        if (_registry != null) _registry.Unregister(_gameObject);
        _registry = registry;
        _registry?.Register(_gameObject);
    }

    private void OnDestroy()
    {
        _registry?.Unregister(_gameObject);
    }
}
