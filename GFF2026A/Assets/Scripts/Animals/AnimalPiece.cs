using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AnimalPiece : MonoBehaviour
{
    private GameObject _gameObject;
    private IAnimalRegistry _registry;
    private bool _landedOnce;

    public event Action OnFirstLand;
    public bool HasLanded { get; private set; }

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (HasLanded) return; // 2回目以降は無視
        HasLanded = true;
        if (_landedOnce) return; // 2回目以降は無視
        _landedOnce = true;
        OnFirstLand?.Invoke();
    }
}
