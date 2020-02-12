using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Entities
{
    public sealed unsafe partial class EntityManager
    {
        // ----------------------------------------------------------------------------------------------------------
        // PUBLIC
        // ----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Protects a chunk, and the entities within it, from structural changes.
        /// </summary>
        /// <remarks>
        /// When locked, entities cannot be added to or removed from the chunk; components cannot be added to or
        /// removed from the entities in the chunk; the values of shared components cannot be changed; and entities
        /// in the chunk cannot be destroyed. You can change the values of components, other than shared components.
        ///
        /// Call <see cref="UnlockChunk(ArchetypeChunk)"/> to unlock the chunk.
        ///
        /// You can lock a chunk temporarily and then unlock it, or you can lock it for the lifespan of your application.
        /// For example, if you have a gameboard with a fixed number of tiles, you may want the entities representing
        /// those tiles in a specific order. Locking the chunk prevents the ECS framework from rearranging them once you
        /// have set the desired order.
        ///
        /// Use <see cref="SwapComponents"/> to re-order entities in a chunk.
        /// </remarks>
        /// <param name="chunk">The chunk to lock.</param>
        public void LockChunk(ArchetypeChunk chunk)
        {
            EntityComponentStore->LockChunks(&chunk, 1,  ChunkFlags.Locked);
        }

        /// <summary>
        /// Locks a set of chunks.
        /// </summary>
        /// <param name="chunks">An array of chunks to lock.</param>
        /// <seealso cref="EntityManager.LockChunk(ArchetypeChunk"/>
        public void LockChunk(NativeArray<ArchetypeChunk> chunks)
        {
            EntityComponentStore->LockChunks((ArchetypeChunk*)chunks.GetUnsafePtr(), chunks.Length, ChunkFlags.Locked);
        }

        /// <summary>
        /// Unlocks a chunk
        /// </summary>
        /// <param name="chunk">The chunk to unlock.</param>
        public void UnlockChunk(ArchetypeChunk chunk)
        {
            EntityComponentStore->UnlockChunks(&chunk, 1,  ChunkFlags.Locked);
        }

        /// <summary>
        /// Unlocks a set of chunks.
        /// </summary>
        /// <param name="chunks">An array of chunks to unlock.</param>
        public void UnlockChunk(NativeArray<ArchetypeChunk> chunks)
        {
            EntityComponentStore->UnlockChunks((ArchetypeChunk*)chunks.GetUnsafePtr(), chunks.Length, ChunkFlags.Locked);
        }

        [Obsolete("LockChunkOrder has been deprecated, and its usage is a no-op. (RemovedAfter 2020-04-09)")]
        public void LockChunkOrder(EntityQuery query)
        {
        }

        [Obsolete("LockChunkOrder has been deprecated, and its usage is a no-op. (RemovedAfter 2020-04-09)")]
        public void LockChunkOrder(ArchetypeChunk chunk)
        {
        }

        [Obsolete("UnlockChunkOrder has been deprecated, and its usage is a no-op. (RemovedAfter 2020-04-09)")]
        public void UnlockChunkOrder(EntityQuery query)
        {
        }

        [Obsolete("UnlockChunkOrder has been deprecated, and its usage is a no-op. (RemovedAfter 2020-04-09)")]
        public void UnlockChunkOrder(ArchetypeChunk chunk)
        {
        }

        // ----------------------------------------------------------------------------------------------------------
        // INTERNAL
        // ----------------------------------------------------------------------------------------------------------

    }
}
