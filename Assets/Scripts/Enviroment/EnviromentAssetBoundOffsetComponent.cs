using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core
{
	public class EnviromentAssetBoundOffsetComponent : MonoBehaviour
	{
		public EnviromentEntityBoundOffsets enviromentEntityBoundOffsets;
	}
	[Serializable]
	public struct EnviromentEntityBoundOffsets
	{
		public StringFloat3[] enviromentEntityOffsets;
	}
}
