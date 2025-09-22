using UnityEngine;
using System;
using Unity.VisualScripting;

public interface IAnimalRegistry
{
    void Register(GameObject go);
    void Unregister(GameObject go);
}
