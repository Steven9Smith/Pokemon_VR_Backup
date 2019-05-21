using System;
using Unity.Entities;
using UnityEngine;
namespace Pokemon
{
    [Serializable]
    public struct TransformMatrix : IComponentData
    {
        public Matrix4x4 matrixValue;
    }
    public class TransformMatrixComponent : ComponentDataProxy<TransformMatrix> { }
}