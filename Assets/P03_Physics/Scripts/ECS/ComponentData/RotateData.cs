using Unity.Entities;

[GenerateAuthoringComponent]
public struct RotateData : IComponentData
{
    public float angularSpeed;
}
