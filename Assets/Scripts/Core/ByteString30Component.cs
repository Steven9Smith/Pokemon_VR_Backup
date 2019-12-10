using Unity.Entities;
using Unity.Mathematics;
using System;
using UnityEngine;
using System.Text;

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
		public ByteString30(string a)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(a);
			length = a.Length;
			A = 0;
			B = 0;
			C = 0;
			D = 0;
			E = 0;
			F = 0;
			G = 0;
			H = 0;
			I = 0;
			J = 0;
			K = 0;
			L = 0;
			M = 0;
			N = 0;
			O = 0;
			P = 0;
			Q = 0;
			R = 0;
			S = 0;
			T = 0;
			U = 0;
			V = 0;
			W = 0;
			X = 0;
			Y = 0;
			Z = 0;
			AA = 0;
			AB = 0;
			AC = 0;
			AD = 0;
			for (int i = 0; i < length; i++)
			{
				switch (i) {
					case 0:
						A = bytes[i];
						break;
					case 1:
						B = bytes[i];
						break;
					case 2:
						C = bytes[i];
						break;
					case 3:
						D = bytes[i];
						break;
					case 4:
						E = bytes[i];
						break;
					case 5:
						F = bytes[i];
						break;
					case 6:
						G = bytes[i];
						break;
					case 7:
						H = bytes[i];
						break;
					case 8:
						I = bytes[i];
						break;
					case 9:
						J = bytes[i];
						break;
					case 10:
						K = bytes[i];
						break;
					case 11:
						L = bytes[i];
						break;
					case 12:
						M = bytes[i];
						break;
					case 13:
						N = bytes[i];
						break;
					case 14:
						O = bytes[i];
						break;
					case 15:
						P = bytes[i];
						break;
					case 16:
						Q = bytes[i];
						break;
					case 17:
						R = bytes[i];
						break;
					case 18:
						S = bytes[i];
						break;
					case 19:
						T = bytes[i];
						break;
					case 20:
						U = bytes[i];
						break;
					case 21:
						V = bytes[i];
						break;
					case 22:
						W = bytes[i];
						break;
					case 23:
						X = bytes[i];
						break;
					case 24:
						Y = bytes[i];
						break;
					case 25:
						Z = bytes[i];
						break;
					case 26:
						AA = bytes[i];
						break;
					case 27:
						AB = bytes[i];
						break;
					case 28:
						AC = bytes[i];
						break;
					case 29:
						AD = bytes[i];
						break;
				}
			}
		}
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