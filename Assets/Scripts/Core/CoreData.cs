using Pokemon;
using Unity.Entities;
namespace Core
{
	public struct CoreData : IComponentData
	{
		public ByteString30 Name;
		public ByteString30 BaseName;
	}
}
