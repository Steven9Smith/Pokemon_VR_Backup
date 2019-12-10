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
		//if you get A and exspect something else then check your values
		public byte Get(int index)
		{
			switch (index)
			{
				case 0: return A;
				case 1: return B;
				case 2: return C;
				case 3: return D;
				case 4: return E;
				case 5: return F;
				case 6: return G;
				case 7: return H;
				case 8: return I;
				case 9: return J;
				case 10: return K;
				case 11: return L;
				case 12: return M;
				case 13: return N;
				case 14: return O;
				case 15: return P;
				case 16: return Q;
				case 17: return R;
				case 18: return S;
				case 19: return T;
				case 20: return U;
				case 21: return V;
				case 22: return W;
				case 23: return X;
				case 24: return Y;
				case 25: return Z;
				case 26: return AA;
				case 27: return AB;
				case 28: return AC;
				case 29: return AD;
				default: return A;
			}
		}
		public bool Equals(ByteString30 bs)
		{
			if (bs.length != length) return false;
			for (int i = 0; i < length; i++)
				if (Get(i) != bs.Get(i)) return false;
			return true;
		}
		public override string ToString()
		{
			string a = "";
			for (int i = 0; i < length; i++)
				a += (char)Get(i);
			return a;
		}
	}
    public class ByteString30Component : ComponentDataProxy<ByteString30> { }
}