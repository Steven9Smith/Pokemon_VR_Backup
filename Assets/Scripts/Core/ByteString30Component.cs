using Unity.Entities;
using Unity.Mathematics;
using System;
using UnityEngine;

namespace Pokemon
{
    //Stored in another file name ByteString30Component.cs
    [Serializable]
    public struct ByteString30 : IComponentData
    {
        public byte A;
        public byte B;
        public byte C;
        public byte D;
        public byte E;
        public byte F;
        public byte G;
        public byte H;
        public byte I;
        public byte J;
        public byte K;
        public byte L;
        public byte M;
        public byte N;
        public byte O;
        public byte P;
        public byte Q;
        public byte R;
        public byte S;
        public byte T;
        public byte U;
        public byte V;
        public byte W;
        public byte X;
        public byte Y;
        public byte Z;
        public byte AA;
        public byte AB;
        public byte AC;
        public byte AD;
        public int length;
    }
    public class ByteString30Component : ComponentDataProxy<ByteString30> { }
}