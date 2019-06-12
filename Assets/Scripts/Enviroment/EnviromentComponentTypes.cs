using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Pokemon
{
    [Serializable]
    public struct EnviromentEntityDataSave
    {
        public ByteString30 EntityName;
		public ByteString30 EntityId;
		public ByteString30 EntityParentId;
		public ByteString30 PathString;
        public byte BoundType;
        public Translation Position;
        public Rotation Rotation;
        public Scale Scale;
    }
}
