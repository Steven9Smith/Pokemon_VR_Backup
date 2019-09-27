using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pokemon.XR;
using UnityEngine.XR;

public class PokeHandTracking : MonoBehaviour
{
	private PokeXR pokeXR;
	private bool debug = true;

	//Player1
	private PokeXRDeviceBody player1Body;

	Animator anim;
	//  Bone stuff
	Quaternion rotation;
	Vector3 position;
	Transform transform;

	public List<XRNodeState> xRNodeStates = new List<XRNodeState>();
	void Start()
	{
		pokeXR = GameObject.FindWithTag("PokeXR").GetComponent<PokeXR>();
		if (pokeXR == null) Debug.LogError("Failed to get pokeXR");
		else if (debug) DO("Successfully got pokeXR");
		//set it to what your not looking for
		if (pokeXR.isEnabled)
		{
			anim = GetComponent<Animator>();
			if (anim == null) Debug.LogError("Failed to get animator on player");
			else
			{
				getVRStuff();
			}
		}
	}
	void OnAnimatorIK(int layerIndex)
	{
		if (pokeXR != null)
		{
			InputTracking.GetNodeStates(xRNodeStates);
			foreach(var a in xRNodeStates)
			{

				a.TryGetRotation(out rotation);
				a.TryGetPosition(out position);
				if (a.nodeType == XRNode.Head)
				{
					transform = anim.GetBoneTransform(HumanBodyBones.Head);
					rotation.eulerAngles = new Vector3(rotation.z,rotation.x,rotation.y);
					anim.SetBoneLocalRotation(HumanBodyBones.Neck, rotation);
					Debug.Log("Head: " + rotation.eulerAngles);
				}
				else if (a.nodeType == XRNode.LeftHand)
				{
					transform = anim.GetBoneTransform(HumanBodyBones.LeftHand);
					anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.2f);
					anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0.1f);

					anim.SetBoneLocalRotation(HumanBodyBones.LeftHand, rotation);
					anim.SetIKPosition(AvatarIKGoal.LeftHand, transform.TransformPoint(position));
					Debug.Log("Left Hand: " + rotation.eulerAngles + "," + transform.localPosition + "," + position+",,,"+anim.GetIKPosition(AvatarIKGoal.LeftHand)+",,"+anim.GetIKPositionWeight(AvatarIKGoal.LeftHand));
				}
				else if(a.nodeType == XRNode.RightHand)
				{
					transform = anim.GetBoneTransform(HumanBodyBones.RightHand);

					anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
					anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0.1f);

					anim.SetBoneLocalRotation(HumanBodyBones.RightHand, rotation);
					anim.SetIKPosition(AvatarIKGoal.RightHand, transform.TransformPoint(position));
					
					Debug.Log("Right Hand: " + rotation.eulerAngles + "," + transform.localPosition + "," + position+",,," + anim.GetIKPosition(AvatarIKGoal.RightHand) + ",," + anim.GetIKPositionWeight(AvatarIKGoal.RightHand));
				}
			}
		}
		else DO("unable to set position");
	}
	private void DO(string message) { Debug.Log("PokeXRInputEntitySystem: " + message); }
	private void getVRStuff()
	{
		player1Body = pokeXR.GetPokeXRDeviceBody();
		//test it
		if (!player1Body.hasHead && !player1Body.hasLeftHand && !player1Body.hasRightHand) Debug.LogError("Failed to get player 1 head");
		else DO("Successfully got player 1 body");
	}
}
