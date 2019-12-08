using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using UnityEngine;
using Pokemon;

namespace Core.Spawning {
	public class GroupIndexSystem : JobComponentSystem
	{
		private EntityCommandBufferSystem ecbs;
		private static NativeArray<GroupIndexGroup> IndexGroups, tempIndexGroups;
		private EntityQuery removeRequests,changeRequests,preventSystemFromStopping;
		private int i,j;
		private string a; //used for debugging

		protected override void OnCreate()
		{
			ecbs = World.GetOrCreateSystem<EntityCommandBufferSystem>();
			removeRequests = GetEntityQuery(typeof(GroupIndexChangeRemoveRequest),typeof(GroupIndexInfo));
			changeRequests = GetEntityQuery(typeof(GroupIndexChangeRequest),typeof(GroupIndexInfo));
			preventSystemFromStopping = GetEntityQuery(typeof(GroupIndexInfo)); // this prevents the system from stopping and causing us to lose the presistant data

			IndexGroups = new NativeArray<GroupIndexGroup>(1, Allocator.Persistent);
		}
		protected override void OnDestroy()
		{
			IndexGroups.Dispose();
		}
		protected override void OnStopRunning()
		{
			IndexGroups.Dispose();
		}
		//not in use
		private struct GroupIndexUpdateJob : IJob
		{
			public NativeArray<Entity> entities;
			public NativeArray<GroupIndexChangeRequest> requests;
			[DeallocateOnJobCompletion] public NativeArray<GroupIndexInfo> groupIndexInfos;
			public NativeArray<GroupIndexGroup> indexGroups; //this is persistant
			private int i,j;
			private GroupIndexGroup gig;
			public void Execute()
			{
				for (i = 0; i < entities.Length; i++)
				{
					Debug.Log("INFOMATION "+groupIndexInfos[i].GroupIndex+","+requests[i].newIndexGroup);
					j = indexGroups[requests[i].newIndexGroup].Length();
					//move the entity to the new group
					if (j < indexGroups[requests[i].newIndexGroup].MaxLength())
					{
						gig = indexGroups[requests[i].newIndexGroup];
						gig.Set(j,new GroupIndexGroupMemeber { memeber = entities[i], isValid = true });
						indexGroups[requests[i].newIndexGroup] = gig;
					}
					else Debug.Log("Cannot add Entity to GroupIndexd because the GroupIndexGroup has reach maximum occupancy");
					//if the new index group and the current are the same then the entity is just trying to be added to the array
					if (groupIndexInfos[i].GroupIndex != requests[i].newIndexGroup)
					{
						//remove the entity from the old group
						//NOte since the GroupIndexChangeRequestJob hasn't been executed then groupIndexInfos[i].GroupIndex is the old index
						gig = indexGroups[groupIndexInfos[i].GroupIndex];
						int length = gig.Length();
						for (j = 0; j < gig.Length(); j++)
							if (gig.Get(j).isValid && gig.Get(j).memeber.Equals(entities[i]))
							{
								gig.Set(j, new GroupIndexGroupMemeber { });
								//shift remaining members to the left
								for (; j < gig.Length(); j++)
								{
									if (gig.Get(j + 1).isValid) gig.Set(j, gig.Get(j + 1));
									else break;
								}
								break;
							}
						if (length == gig.Length()) Debug.LogError("Failed to find matching Entity for removal");
						else indexGroups[groupIndexInfos[i].GroupIndex] = gig;
					}
				}
			}
		}
		/// <summary>
		/// sets the current index back to the old index
		/// </summary>
		[BurstCompile]
		private struct IndexGrouopRemoveUpdateJob : IJobForEach<GroupIndexChangeRemoveRequest, GroupIndexInfo>
		{
			public void Execute(ref GroupIndexChangeRemoveRequest c0, ref GroupIndexInfo c1)
			{
				c1.GroupIndex = c1.oldIndexGroup; //its so simple
			}
		}
		/// <summary>
		/// this job changes the colliders groupindex into a new one
		/// </summary>
		//	[BurstCompile] <- in order for this to work we need to remove string from the getPokemonPhysicsCollider function
		private struct GroupIndexChangeRequestJob : IJobForEachWithEntity<GroupIndexChangeRequest, PhysicsCollider, PokemonEntityData,GroupIndexInfo>
		{
			public void Execute(Entity entity, int index, ref GroupIndexChangeRequest request, ref PhysicsCollider collider, ref PokemonEntityData ped,ref GroupIndexInfo gii)
			{	
				//now change the GroupIndex in the entity
				collider = PokemonDataClass.getPokemonPhysicsCollider(PokemonIO.ByteString30ToString(request.pokemonName),
					ped, new CollisionFilter {
						BelongsTo = collider.Value.Value.Filter.BelongsTo,
						CollidesWith = collider.Value.Value.Filter.CollidesWith,
						GroupIndex = request.newIndexGroup
					}, PokemonDataClass.GetPokemonColliderMaterial(PokemonIO.ByteString30ToString(request.pokemonName)));
				gii = new GroupIndexInfo { GroupIndex = request.newIndexGroup,oldIndexGroup = gii.GroupIndex };
				
			}
		}
		
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			//this keeps the system running preventing IndexGroups from being deleted
			if (preventSystemFromStopping.CalculateEntityCount() == 0)
			{
				Debug.Log("IDK how this happened!");
				IndexGroups.Dispose();
				return inputDeps;
			}
			i = changeRequests.CalculateEntityCount();
			j = removeRequests.CalculateEntityCount();
			//change entitiy IndexGroup
			if (i > 0 || j > 0)
			{
				JobHandle changeRequestJob = inputDeps;
				JobHandle removeUpdateJob = inputDeps;
				/*a = "IndexSystem Start:\n";
				for (i = 0; i < IndexGroups.Length; i++)
				{
					a += "Group " + i + " has " + IndexGroups[i].ToString() + " valid members who are\n\t";
					for (j = 0; j < IndexGroups[i].Length(); j++)
						a += EntityManager.GetName(IndexGroups[i].Get(j).memeber) + " valid = " + IndexGroups[i].Get(j).isValid.Value + "\n\t";
					a += "\n";
				}
				Debug.Log(a);*/
				if (i > 0)
				{
					int k = j;
					NativeArray<GroupIndexChangeRequest> requests = changeRequests.ToComponentDataArray<GroupIndexChangeRequest>(Allocator.TempJob);
					tempIndexGroups = new NativeArray<GroupIndexGroup>(IndexGroups.Length, Allocator.Temp);
					NativeArray<Entity> entities = changeRequests.ToEntityArray(Allocator.TempJob);
					NativeArray<GroupIndexInfo> groupIndexInfos = changeRequests.ToComponentDataArray<GroupIndexInfo>(Allocator.TempJob);
					GroupIndexGroup gig;
					for (i = 0; i < requests.Length; i++)
					{
				//		Debug.Log("Entity \"" + EntityManager.GetName(entities[i]) + " has a request!");
						if (requests[i].newIndexGroup >= IndexGroups.Length)
						{
	//						Debug.Log("GroupIndex: requires new indexes");
							if (requests[i].newIndexGroup > IndexGroups.Length) requests[i] = new GroupIndexChangeRequest { newIndexGroup = IndexGroups.Length, pokemonName = requests[i].pokemonName };
							tempIndexGroups = new NativeArray<GroupIndexGroup>(IndexGroups, Allocator.Temp);
							//	for (j = 0; j < tempIndexGroups.Length; j++) tempIndexGroups[j] = IndexGroups[j];
							IndexGroups.Dispose();
							IndexGroups = new NativeArray<GroupIndexGroup>(tempIndexGroups.Length + 1, Allocator.Persistent);
							for (j = 0; j < tempIndexGroups.Length; j++) IndexGroups[j] = tempIndexGroups[j];
						}
						else if (IndexGroups[requests[i].newIndexGroup].Length() >= IndexGroups[requests[i].newIndexGroup].MaxLength()) Debug.LogWarning("cannot add entity to IndexGroup " + requests[i].newIndexGroup.ToString() + ", at max capacity!");
						j = IndexGroups[requests[i].newIndexGroup].Length();
						//move the entity to the new group
				//		Debug.Log("adding Entity \""+EntityManager.GetName(entities[i])+"\" to group "+requests[i].newIndexGroup);
						if (j < IndexGroups[requests[i].newIndexGroup].MaxLength())
						{
							gig = IndexGroups[requests[i].newIndexGroup];
							gig.Set(j, new GroupIndexGroupMemeber { memeber = entities[i], isValid = true });
							IndexGroups[requests[i].newIndexGroup] = gig;
						}
						else Debug.Log("Cannot add Entity to GroupIndexd because the GroupIndexGroup has reach maximum occupancy");
						//if the new index group and the current are the same then the entity is just trying to be added to the array
						if (groupIndexInfos[i].GroupIndex != requests[i].newIndexGroup)
						{
							//remove the entity from the old group
							//NOte since the GroupIndexChangeRequestJob hasn't been executed then groupIndexInfos[i].GroupIndex is the old index
							gig = IndexGroups[groupIndexInfos[i].GroupIndex];
							int length = gig.Length();
							for (j = 0; j < gig.Length(); j++)
								if (gig.Get(j).isValid && gig.Get(j).memeber.Equals(entities[i]))
								{
									gig.Set(j, new GroupIndexGroupMemeber { });
									//shift remaining members to the left
									for (; j < length; j++)
									{
										gig.Set(j, gig.Get(j + 1));
										if(!gig.Get(j + 1).isValid) break;
									}
									//gotta make sure the l;ast one is set to nothing
									if (length == gig.MaxLength()) gig.Set(gig.MaxLength(), new GroupIndexGroupMemeber { }); 
									break;
								}
							if (length == gig.Length()) Debug.LogError("Failed to find matching Entity for removal");
							else IndexGroups[groupIndexInfos[i].GroupIndex] = gig;
						}

					}
					groupIndexInfos.Dispose();
				/*	JobHandle GroupIndexUpdateJobHandle = new GroupIndexUpdateJob
					{
						entities = entities,
						requests = requests,
						indexGroups = IndexGroups,
						groupIndexInfos = changeRequests.ToComponentDataArray<GroupIndexInfo>(Allocator.TempJob)
					}.Schedule(inputDeps);*/
				//	GroupIndexUpdateJobHandle.Complete();
				//	changeRequestJob = new GroupIndexChangeRequestJob { }.Schedule(this, GroupIndexUpdateJobHandle);
					changeRequestJob = new GroupIndexChangeRequestJob { }.Schedule(this, inputDeps);
					//run the request job
					changeRequestJob.Complete();
					for (int i = 0; i < entities.Length; i++)
						EntityManager.RemoveComponent<GroupIndexChangeRequest>(entities[i]);
					entities.Dispose();
					requests.Dispose();
					tempIndexGroups.Dispose();
					j = k;
				}
				//debugging
			/*	a = "IndexSystem Middle:\n";
				for (i = 0; i < IndexGroups.Length; i++)
				{
					a += "Group " + i + " has " + IndexGroups[i].ToString() + " valid members who are\n\t";
					for (j = 0; j < IndexGroups[i].Length(); j++)
						a += EntityManager.GetName(IndexGroups[i].Get(j).memeber) + " valid = " + IndexGroups[i].Get(j).isValid.Value + "\n\t";
					a += "\n";
				}
				Debug.Log(a);*/
				////////////
				if (j > 0) {
					//update the IndexGroups array
					NativeArray<GroupIndexChangeRemoveRequest> mRemoveRequests = removeRequests.ToComponentDataArray<GroupIndexChangeRemoveRequest>(Allocator.TempJob);
					NativeArray<GroupIndexInfo> mGroupIndexInfos = removeRequests.ToComponentDataArray<GroupIndexInfo>(Allocator.TempJob);
					NativeArray<Entity> mEntities = removeRequests.ToEntityArray(Allocator.TempJob);
					for (i = 0; i < mRemoveRequests.Length; i++)
					{
	//					Debug.Log("removing request on \""+EntityManager.GetName(mEntities[i])+"\","+mGroupIndexInfos[i].GroupIndex+","+mGroupIndexInfos[i].oldIndexGroup+","+IndexGroups.Length);
						if (IndexGroups[mGroupIndexInfos[i].GroupIndex].Length() == 1 && IndexGroups.Length-1 == mGroupIndexInfos[i].GroupIndex)
						{
	//						Debug.Log("changing size of GroupIndex");
							//the current entity is the only one in the group so we must delete the group
							tempIndexGroups = new NativeArray<GroupIndexGroup>(IndexGroups.Slice(0, IndexGroups.Length - 1).ToArray(), Allocator.TempJob);
							IndexGroups.Dispose();
							IndexGroups = new NativeArray<GroupIndexGroup>(tempIndexGroups.ToArray(), Allocator.Persistent);

							tempIndexGroups.Dispose();
						}
						else
						{
							//just remove the entitiy from the group without deleting the group
							GroupIndexGroup gig = IndexGroups[mGroupIndexInfos[i].GroupIndex];
							int length = gig.Length();
							for (j = 0; j < gig.Length(); j++)
								if (gig.Get(j).isValid && gig.Get(j).memeber.Equals(mEntities[i]))
								{
									gig.Set(j, new GroupIndexGroupMemeber { });
									//shift remaining members to the left
									for (; j < gig.Length(); j++)
									{
										if (gig.Get(j + 1).isValid) gig.Set(j, gig.Get(j + 1));
										else break;
									}
									break;
								}
							if (length == gig.Length()) Debug.LogError("Failed to find matching Entity for removal");
							else IndexGroups[mGroupIndexInfos[i].GroupIndex] = gig;
						}
					/*	a = "IndexSystem Enasdasdd:\n";
						for (int k = 0; k < IndexGroups.Length; k++)
						{
							a += "Group " + i + " has " + IndexGroups[k].ToString() + " valid members who are\n\t";
							for (j = 0; j < IndexGroups[k].Length(); j++)
								a += EntityManager.GetName(IndexGroups[k].Get(j).memeber) + " valid = " + IndexGroups[k].Get(j).isValid.Value + "\n\t";
							a += "\n";
						}
						Debug.Log(a);*/
						if (!mRemoveRequests[i].removeFromArray)
						{
							//so we must put the Etntiy back in its old group 
		//					Debug.Log("Current Length of Index Groups = " + IndexGroups.Length + ", " + i + ", " + mGroupIndexInfos[i].oldIndexGroup + "," + mGroupIndexInfos[i].GroupIndex);
							if (IndexGroups[mGroupIndexInfos[i].oldIndexGroup].Length() < IndexGroups[mGroupIndexInfos[i].oldIndexGroup].MaxLength())
							{
								GroupIndexGroup gig = IndexGroups[mGroupIndexInfos[i].oldIndexGroup];
								gig.Set(gig.Length(), new GroupIndexGroupMemeber { isValid = true, memeber = mEntities[i] });
								IndexGroups[mGroupIndexInfos[i].oldIndexGroup] = gig;
							}
							else Debug.LogError("Cannot put entity back in old group, the group is full!");
						}
					//	else Debug.Log("Removing "+EntityManager.GetName(mEntities[i])+" from array");
					}
					removeUpdateJob = new IndexGrouopRemoveUpdateJob { }.Schedule(this, changeRequestJob);
					removeUpdateJob.Complete();
					//remove the request component
					for (i = 0; i < mRemoveRequests.Length; i++)
						EntityManager.RemoveComponent<GroupIndexChangeRemoveRequest>(mEntities[i]);
					
					mRemoveRequests.Dispose();
					mGroupIndexInfos.Dispose();
					mEntities.Dispose();
				}
				///debugging /////////////
			/*	a = "IndexSystem End:\n";
				for (i = 0; i < IndexGroups.Length; i++)
				{
					a += "Group " + i + " has " + IndexGroups[i].ToString() + " valid members who are\n\t";
					for (j = 0; j < IndexGroups[i].Length(); j++)
						a += EntityManager.GetName(IndexGroups[i].Get(j).memeber)+" valid = "+IndexGroups[i].Get(j).isValid.Value+"\n\t";
					a+="\n";
				}
				Debug.Log(a);*/
				////////////////////////
				return removeUpdateJob;
			}
			return inputDeps;
		}
		//returns stuff...im tired
		public int GetAllGroupsBut(int exclude)
		{
			int data = 0;
			for (int i = 0; i < IndexGroups.Length; i++)
				if (i != exclude)
					data = data | i;
			return data;
		}
		//get length of IndexGroups, idk why this is here
		public int getLength()
		{
			return IndexGroups.Length;
		}
	}
	[Serializable]
	public struct GroupIndexChangeRequest : IComponentData
	{
		public int newIndexGroup;
		public ByteString30 pokemonName;
	}
	[Serializable]
	public struct GroupIndexChangeRemoveRequest : IComponentData {
		public BlittableBool removeFromArray;
	}
	//for now we used a fixed set of group memebers
	[Serializable]
	public struct GroupIndexGroup : IComponentData
	{
		//mimic indexing
		public GroupIndexGroupMemeber member0;
		public GroupIndexGroupMemeber member1;
		public GroupIndexGroupMemeber member2;
		public GroupIndexGroupMemeber member3;
		public GroupIndexGroupMemeber member4;
		public GroupIndexGroupMemeber member5;
		public GroupIndexGroupMemeber member6;
		public GroupIndexGroupMemeber member7;
		public GroupIndexGroupMemeber member8;
		public GroupIndexGroupMemeber member9;
		public GroupIndexGroupMemeber member10;
		public GroupIndexGroupMemeber member11;
		public GroupIndexGroupMemeber member12;
		public GroupIndexGroupMemeber member13;
		public GroupIndexGroupMemeber member14;
		public GroupIndexGroupMemeber member15;
		public GroupIndexGroupMemeber member16;
		public GroupIndexGroupMemeber member17;
		public GroupIndexGroupMemeber member18;
		public GroupIndexGroupMemeber member19;
		public GroupIndexGroupMemeber member20;
		public GroupIndexGroupMemeber member21;
		public GroupIndexGroupMemeber member22;
		public GroupIndexGroupMemeber member23;
		public GroupIndexGroupMemeber member24;
		public GroupIndexGroupMemeber member25;
		public GroupIndexGroupMemeber member26;
		public GroupIndexGroupMemeber member27;
		public GroupIndexGroupMemeber member28;
		public GroupIndexGroupMemeber member29;
		public GroupIndexGroupMemeber Get(int index)
		{
			switch (index)
			{
				case 0: return member0;
				case 1: return member1;
				case 2: return member2;
				case 3: return member3;
				case 4: return member4;
				case 5: return member5;
				case 6: return member6;
				case 7: return member7;
				case 8: return member8;
				case 9: return member9;
				case 10: return member10;
				case 11: return member11;
				case 12: return member12;
				case 13: return member13;
				case 14: return member14;
				case 15: return member15;
				case 16: return member16;
				case 17: return member17;
				case 18: return member18;
				case 19: return member19;
				case 20: return member20;
				case 21: return member21;
				case 22: return member22;
				case 23: return member23;
				case 24: return member24;
				case 25: return member25;
				case 26: return member26;
				case 27: return member27;
				case 28: return member28;
				case 29: return member29;
				default: return this.member0;
			}
		}
		public void Set(int index, GroupIndexGroupMemeber gim)
		{
			switch (index)
			{
				case 0: member0 = gim; break;
				case 1: member1 = gim; break;
				case 2: member2 = gim; break;
				case 3: member3 = gim; break;
				case 4: member4 = gim; break;
				case 5: member5 = gim; break;
				case 6: member6 = gim; break;
				case 7: member7 = gim; break;
				case 8: member8 = gim; break;
				case 9: member9 = gim; break;
				case 10: member10 = gim; break;
				case 11: member11 = gim; break;
				case 12: member12 = gim; break;
				case 13: member13 = gim; break;
				case 14: member14 = gim; break;
				case 15: member15 = gim; break;
				case 16: member16 = gim; break;
				case 17: member17 = gim; break;
				case 18: member18 = gim; break;
				case 19: member19 = gim; break;
				case 20: member20 = gim; break;
				case 21: member21 = gim; break;
				case 22: member22 = gim; break;
				case 23: member23 = gim; break;
				case 24: member24 = gim; break;
				case 25: member25 = gim; break;
				case 26: member26 = gim; break;
				case 27: member27 = gim; break;
				case 28: member28 = gim; break;
				case 29: member29 = gim; break;
			}
		}

		public int Length()
		{
			int i;
			for (i = 0; i < 30; i++) if (!this.Get(i).isValid) break;
			return i;
		}
		public int MaxLength() { return 30; }
		public override string ToString()
		{
			return "IndexGroup Valid Members: " + this.Length().ToString();
		}
	}
	//each group memeber needs a verification of being valid
	[Serializable]
	public struct GroupIndexGroupMemeber : IComponentData
	{
		public Entity memeber;
		public BlittableBool isValid;
	}

	[Serializable]
	public struct GroupIndexInfo : IComponentData{
		public int GroupIndex;
		public int oldIndexGroup;
	}
}
