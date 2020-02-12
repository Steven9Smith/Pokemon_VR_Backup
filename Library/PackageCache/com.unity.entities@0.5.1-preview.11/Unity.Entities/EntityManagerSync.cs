using System;
using Unity.Assertions;

namespace Unity.Entities
{
    public sealed unsafe partial class EntityManager
    {
        // ----------------------------------------------------------------------------------------------------------
        // PUBLIC
        // ----------------------------------------------------------------------------------------------------------
        
        //@TODO: Not clear to me what this method is really for...
        /// <summary>
        /// Waits for all Jobs to complete.
        /// </summary>
        /// <remarks>Calling CompleteAllJobs() blocks the main thread until all currently running Jobs finish.</remarks>
        public void CompleteAllJobs()
        {
            DependencyManager->CompleteAllJobsAndInvalidateArrays();
        }

        // ----------------------------------------------------------------------------------------------------------
        // INTERNAL
        // ----------------------------------------------------------------------------------------------------------

        internal void BeforeStructuralChange()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (DependencyManager->IsInTransaction)
            {
                throw new InvalidOperationException(
                    "Access to EntityManager is not allowed after EntityManager.BeginExclusiveEntityTransaction(); has been called.");
            }

            if (DependencyManager->IsInForEachDisallowStructuralChange != 0)
            {
                throw new InvalidOperationException(
                    "Structural changes are not allowed during Entities.ForEach. Please use EntityCommandBuffer instead.");
            }

            // This is not an end user error. If there are any managed changes at this point, it indicates there is some
            // (previous) EntityManager change that is not properly playing back the managed changes that were buffered 
            // afterward. That needs to be found and fixed. 
            Assert.IsTrue(EntityComponentStore->ManagedChangesTracker.Empty);
#endif

            CompleteAllJobs();
        }
    }
}
