using Pokemon.XR;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.XR;
public class PokeXR : MonoBehaviour
{
	public bool isEnabled = true;
	public GameViewRenderMode gameViewRenderMode = GameViewRenderMode.BothEyes;
	public float eyeTectureResolutionScale = 1f;
	public float occlusionMaskScale = 1f;
	public float renderViewportScale = 1f;
	public bool useOcclusionMesh = true;

	private PokeXRInputDevices pokeXRInputDevices;
	private PokeXRUIDebugger pokeXRUIDebugger;


	public bool debuggerMode;
	void Awake()
	{
		XRSettings.enabled = isEnabled;
		if (isEnabled)
		{
			XRSettings.gameViewRenderMode = gameViewRenderMode;
			XRSettings.eyeTextureResolutionScale = eyeTectureResolutionScale;
			XRSettings.occlusionMaskScale = occlusionMaskScale;
			XRSettings.renderViewportScale = renderViewportScale;
			XRSettings.useOcclusionMesh = useOcclusionMesh;

			PokeXRUIDebuggerData pokeXRUIDebuggerData = GameObject.FindWithTag("UIXRDebugConsole").GetComponent<PokeXRUIDebuggerData>();
			if (pokeXRUIDebuggerData == null) Debug.LogError("Failed to get pokeXRUIDebuggerData");
			else DO("Successfully got debugger data!");
			pokeXRInputDevices = new PokeXRInputDevices(debuggerMode);
			pokeXRUIDebugger = new PokeXRUIDebugger(pokeXRInputDevices, pokeXRUIDebuggerData, debuggerMode);
			DO("Setup Complete");
		}

	}
	private void DO(string message) { Debug.Log("PokeXR: "+message); }
	public PokeXRBody getPlayer1Body()
	{
		return pokeXRInputDevices.PokeXRInputTracking.getPokeBody();
	}
	public PokeXRDeviceBody GetPokeXRDeviceBody() { return pokeXRInputDevices.pokeXRDeviceBody; }
}

namespace Pokemon
{
	namespace XR
	{
		public class PokeXRUIDebugger : MonoBehaviour
		{
			private PokeXRUIDebuggerData PokeXRUIDebuggerData;
			private PokeXRInputDevices PokeXRInputDevices;
			private bool isEnabled;
			public PokeXRUIDebugger(PokeXRInputDevices pokeXRInputDevices, PokeXRUIDebuggerData pokeXRUIDebuggerData, bool enable)
			{
				PokeXRUIDebuggerData = pokeXRUIDebuggerData;
				PokeXRInputDevices = pokeXRInputDevices;
				if (enable) this.enable(); 
			}
			public void enable(){isEnabled = true;if (PokeXRUIDebuggerData != null) PokeXRUIDebuggerData.enabled = true;}
			public void disable() { isEnabled = false; if (PokeXRUIDebuggerData != null) PokeXRUIDebuggerData.isEnabled = false; }
			
			void Update()
			{
				if (isEnabled)
				{
					PokeXRUIDebuggerData.TextMeshProUGUI.text = XRSettingsToString();
					
				}
				else if (PokeXRUIDebuggerData.TextMeshProUGUI.text != "") PokeXRUIDebuggerData.TextMeshProUGUI.text = "";
			}
			private string XRSettingsToString()
			{
				return "Device Eye Texture Dimension: " + XRSettings.deviceEyeTextureDimension +
						  "\nXR Enabled: " + XRSettings.enabled +
						  "\nEye Texture Desc: " + XRSettings.eyeTextureDesc +
						  "\nEye Texture Height: " + XRSettings.eyeTextureHeight +
						  "\nEye Texture Resolution Scale: " + XRSettings.eyeTextureResolutionScale +
						  "\nEye Texture Width: " + XRSettings.eyeTextureWidth +
						  "\nGame View Render Mode: " + XRSettings.gameViewRenderMode +
						  "\nIs Device Active: " + XRSettings.isDeviceActive +
						  "\nLoaded Device Name: " + XRSettings.loadedDeviceName +
						  "\nOcclusion Mask Scale: " + XRSettings.occlusionMaskScale +
						  "\nRender ViewPort Scale: " + XRSettings.renderViewportScale;
			}
		
		}
		public class PokeXRInputDevices : MonoBehaviour
		{
			public List<InputDevice> XRInputDevices = new List<InputDevice>();
			public PokeXRInputTracking PokeXRInputTracking;
			public InputDevice Player1InputDevice;
			public PokeXRBody Player1Body;
			public PokeXRDeviceBody pokeXRDeviceBody;
			public bool debugMode;
			private int i;
			public PokeXRInputDevices(bool _debugMode){
				debugMode = _debugMode;
				InputDevices.GetDevices(XRInputDevices);
				PokeXRInputTracking = new PokeXRInputTracking(debugMode);
				PokeXRInputTracking.refreshInputDevices();
				if (debugMode)
				{
					for (i = 0; i < XRInputDevices.Count; i++) {
						List<InputFeatureUsage> ifu = new List<InputFeatureUsage>();
						string a = "XRInputDevice " + i + ": " + XRInputDevices[i].name + ", isValid: " + XRInputDevices[i].isValid;
						XRInputDevices[i].TryGetFeatureUsages(ifu);
						for (int j = 0; j < ifu.Count; j++)
							a+="\nfu = "+ifu[j].name;
						Debug.Log(a);
					}
					Debug.Log(PokeXRInputTracking.toString());
				}
				if (XRInputDevices.Count > 0)
				{
					Player1InputDevice = XRInputDevices[0];
					Player1Body = PokeXRInputTracking.getPokeBody();
					pokeXRDeviceBody = PokeXRInputTracking.getPokeDeviceBody();
				}
				else Debug.LogWarning("Failed to detect and");
			}
			
		}
		public  struct PokeXRBody
		{
			public XRNodeState Head;
			public XRNodeState LeftHand;
			public XRNodeState RightHand;
			public bool hasHead;
			public bool hasLeftHand;
			public bool hasRightHand;
			public bool isValid;
		}
		public struct PokeXRDeviceBody
		{
			public InputDevice Head;
			public InputDevice LeftHand;
			public InputDevice RightHand;

			public bool hasHead;
			public bool hasLeftHand;
			public bool hasRightHand;
		}
		public class PokeXRInputTracking : MonoBehaviour
		{
			public List<XRNodeState> xRNodeStates = new List<XRNodeState>();
			bool debugMode;
			public PokeXRInputTracking(bool debugMode = true)
			{
				InputTracking.GetNodeStates(xRNodeStates);
				if (xRNodeStates.Count < 1) DO("Failed to get input tracking nodes....please refresh");
			}
			private void DO(string message) { Debug.Log("PokeXRInputTracking: "+message); }
			public void refreshInputDevices()
			{
				InputTracking.GetNodeStates(xRNodeStates);
			}
			public XRNodeState getHead(InputDevice inputDevice)
			{
				for(int i = 0; i < xRNodeStates.Count; i++)
				{
					Debug.Log("UNIQUE IFDDD"+xRNodeStates[i].uniqueID);
					if (xRNodeStates[i].nodeType == XRNode.RightEye) return xRNodeStates[i];
				}
				Debug.LogError("Failed to find Head");
				return xRNodeStates[0];
			}
			public PokeXRBody getPokeBody()
			{
				PokeXRBody pokeXRBody = new PokeXRBody { };
				bool hasHead = false, hasLeftHand = false,hasRightHand = false;
				for (int i = 0; i < xRNodeStates.Count; i++)
				{
					if (xRNodeStates[i].nodeType == XRNode.Head) { pokeXRBody.Head = xRNodeStates[i]; hasHead = true; }
					else if (xRNodeStates[i].nodeType == XRNode.LeftHand) { pokeXRBody.LeftHand = xRNodeStates[i]; hasLeftHand = true; }
					else if (xRNodeStates[i].nodeType == XRNode.RightHand){pokeXRBody.RightHand = xRNodeStates[i]; hasRightHand = true;}
					
				}
				pokeXRBody.hasHead = hasHead;
				pokeXRBody.hasLeftHand = hasLeftHand;
				pokeXRBody.hasRightHand = hasRightHand;
				pokeXRBody.isValid = hasHead || hasLeftHand || hasRightHand;
				return pokeXRBody;
			}
			public PokeXRDeviceBody getPokeDeviceBody()
			{
				PokeXRDeviceBody pokeXRDeviceBody = new PokeXRDeviceBody { };
				var devices = new List<InputDevice>();
				InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);
				if (devices.Count == 1)
				{
					pokeXRDeviceBody.LeftHand = devices[0];
					pokeXRDeviceBody.hasLeftHand = true;
					Debug.Log(string.Format("Device name '{0}' with role '{1}'", devices[0].name, devices[0].role.ToString()));
				}
				else if (devices.Count > 1){Debug.LogError("Found more than one left hand!");}
				InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
				if (devices.Count == 1)
				{
					pokeXRDeviceBody.RightHand = devices[0];
					pokeXRDeviceBody.hasRightHand = true;
					Debug.Log(string.Format("Device name '{0}' with role '{1}'", devices[0].name, devices[0].role.ToString()));
				}
				else if (devices.Count > 1) { Debug.LogError("Found more than one left hand!"); }
				InputDevices.GetDevicesAtXRNode(XRNode.Head, devices);
				if (devices.Count == 1)
				{
					pokeXRDeviceBody.Head = devices[0];
					pokeXRDeviceBody.hasHead = true;
					Debug.Log(string.Format("Device name '{0}' with role '{1}'", devices[0].name, devices[0].role.ToString()));
				}
				else if (devices.Count > 1) { Debug.LogError("Found more than one left hand!"); }

				return pokeXRDeviceBody;
			}
			public string toString()
			{
				string a = "";
				if (xRNodeStates.Count > 0)
				{
					for (int i = 0; i < xRNodeStates.Count; i++)
					{
						Vector3 position;
						xRNodeStates[i].TryGetPosition(out position);
						a += "\nName =" + InputTracking.GetNodeName(xRNodeStates[i].uniqueID) + "Type = " + xRNodeStates[i].nodeType + " position = " + position.ToString();
					}
				}
				else a = "Failed to get Input Tracking\n";
				return a;
			}

		}
	}
}
