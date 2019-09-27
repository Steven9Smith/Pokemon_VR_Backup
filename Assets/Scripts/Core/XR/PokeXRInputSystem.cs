using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine.XR;

namespace Pokemon
{
	namespace XR
	{
		/*		public class PokeXRInputEntitySystem : ComponentSystem
				{
					private PokeXR pokeXR;
					private bool debug = true;

					//Player1
					private PokeXRBody player1Body;

					Animator anim;
					//  Bone stuff
					Quaternion rotation;
					Vector3 position;
					Transform transform;


					protected override void OnStartRunning()
					{
						pokeXR = GameObject.FindWithTag("PokeXR").GetComponent<PokeXR>();
						if (pokeXR == null) Debug.LogError("Failed to get pokeXR");
						else if (debug) DO("Successfully got pokeXR");
						//set it to what your not looking for
						if (pokeXR.isEnabled)
						{
							anim = GameObject.FindWithTag("Player").GetComponent<Animator>();
							if (anim == null) Debug.LogError("Failed to get animator on player");
							else
							{
								getVRStuff();
							}
						}
					}
					protected override void OnUpdate()
					{
						if (pokeXR != null)
						{
							if (player1Body.hasHead)
							{
								player1Body.Head.TryGetRotation(out rotation);
								transform = anim.GetBoneTransform(HumanBodyBones.Head);
								anim.SetBoneLocalRotation(HumanBodyBones.Head, rotation);
								Debug.Log("Head: "+rotation+","+transform.position);
							}
							if (player1Body.hasLeftHand)
							{
								player1Body.LeftHand.TryGetRotation(out rotation);
								player1Body.LeftHand.TryGetPosition(out position);
								transform = anim.GetBoneTransform(HumanBodyBones.Head);
								anim.SetBoneLocalRotation(HumanBodyBones.Head, rotation);
								anim.SetIKPosition(AvatarIKGoal.LeftHand, position + transform.localPosition);
								Debug.Log("Left Hand: " + rotation + "," + transform.localPosition+","+position);
							}
							if (player1Body.hasRightHand)
							{
								player1Body.RightHand.TryGetRotation(out rotation);
								player1Body.RightHand.TryGetPosition(out position);
								transform = anim.GetBoneTransform(HumanBodyBones.RightHand);
								anim.SetBoneLocalRotation(HumanBodyBones.RightHand, rotation);
								anim.SetIKPosition(AvatarIKGoal.RightHand, position + transform.localPosition);
								Debug.Log("Right Hand: " + rotation + "," + transform.localPosition+","+position);
							}
						}
						else DO("unable to set position");
					}
					private void DO(string message) { Debug.Log("PokeXRInputEntitySystem: "+message); }
					private void getVRStuff()
					{
						player1Body = pokeXR.getPlayer1Body();
						//test it
						if (!player1Body.hasHead && !player1Body.hasLeftHand && !player1Body.hasRightHand) Debug.LogError("Failed to get player 1 head");
						else DO("Successfully got player 1 body");
					}*/
	}

}