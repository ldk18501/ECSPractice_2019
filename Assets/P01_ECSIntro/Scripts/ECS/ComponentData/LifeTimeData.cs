using Unity.Entities;

namespace DOTS_2019
{
    [GenerateAuthoringComponent]
    public struct LifeTimeData : IComponentData
    {
        public float existTime;
    }
}