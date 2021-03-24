using Unity.Entities;
using Unity.Mathematics;

namespace DOTS_2019
{
    [GenerateAuthoringComponent]
    public struct TransMoveData : IComponentData
    {
        public float speed;
    }
}