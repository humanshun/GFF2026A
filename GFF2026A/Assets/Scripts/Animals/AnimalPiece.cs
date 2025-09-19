using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AnimalPiece : MonoBehaviour
{
    private GameObject _gameObject;
    private IAnimalRegistry _registry;
    private bool _landedOnce;

    public event Action OnFirstLand;

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
        if (_landedOnce) return; // 2回目以降は無視
        _landedOnce = true;
        OnFirstLand?.Invoke();
    }
}
