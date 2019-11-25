using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Unity.Mathematics;

public class VR_Controller_Tests : MonoBehaviour
{
	
	public SteamVR_Action_Skeleton Left_Hand_Skeleton;
	public SteamVR_ActionSet actionSet;

	FingerData fingerData;
	SteamVR_Action_Boolean triggered;

	private void Awake()
	{
		if (actionSet == null)
			actionSet = SteamVR_Actions.Controller_Tests;
		actionSet.Activate(SteamVR_Input_Sources.Any, 0,true);
		Left_Hand_Skeleton = SteamVR_Actions.Controller_Tests.Left_Hand_Skeleton;
		triggered = SteamVR_Actions.Controller_Tests.Left_Trigger_Click;

		triggered[SteamVR_Input_Sources.Any].onStateDown += PreformAction;

	}

	private void OnEnable()
	{
		if(triggered == null)
		{
			Debug.LogError("failed to assign an action for triggered");
			return;
		}
		Debug.Log("The ActionSet is "+triggered.active+", active = "+triggered.setActive+", klkl");
	}
	private void Start()
	{
		Debug.Log("Action set enabled = " + actionSet.IsActive(SteamVR_Input_Sources.Any));
	}
	private void PreformAction(SteamVR_Action_Boolean actionIn,SteamVR_Input_Sources source)
	{
		Debug.Log("Something");
	}
	void Update()
	{
		if (Left_Hand_Skeleton != null)
		{
			fingerData = new FingerData(Left_Hand_Skeleton);
			Debug.Log(fingerData.ToString());
		}
		else Debug.Log("No skeleton");
	}
	
}
/// <summary>
/// This struct makes accessing HandSkeletonFinger data a little easier to debug
/// </summary>
public struct FingerData
{
	//these are pretty self explanitory
	public float indexValue;
	public float pinkyValue;
	public float middleValue;
	public float ringValue;
	public float thumbValue;
	/// <summary>
	/// initializer
	/// </summary>
	/// <param name="skel"></param>
	public FingerData(SteamVR_Action_Skeleton skel)
	{
		indexValue = skel.indexCurl;
		pinkyValue = skel.pinkyCurl;
		ringValue = skel.ringCurl;
		middleValue = skel.middleCurl;
		thumbValue = skel.thumbCurl;
	}
	/// <summary>
	/// print the to string version of the data in this order [index,middle,ring,pinky,thumb]
	/// </summary>
	/// <returns></returns>
	public override string ToString()
	{
		return "FingerData: ["+indexValue.ToString("F2")+","+middleValue.ToString("F2") +","+ringValue.ToString("F2") +","+pinkyValue.ToString("F2") +","+thumbValue.ToString("F2") +"]";
	}
}
