using Unity.Entities;
using System;

[GenerateAuthoringComponent]
public struct MoveForwardData : IComponentData
{
    public float speed;
}
