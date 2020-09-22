using Pokemon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Core.GameConfig
{
	public static class GameConfigClass
	{
		public static GameConfig mGameConfig;
		/// <summary>
		/// Saves the gameconfig data
		/// </summary>
		/// <param name="gc"></param>
		public static void SaveGameConfig(GameConfig gc)
		{
			FileStream file = null;
			BinaryFormatter bf = new BinaryFormatter();
			try
			{
				file = File.Create(Application.dataPath + "/Config/Config.cfg");
				bf.Serialize(file, gc);
				file.Close();
			}
			catch (Exception E)
			{
				Debug.LogError("Failed to save file with error:\n" + E.Message);
			}
		}
		/// <summary>
		/// Loads the GameConfigData
		/// </summary>
		/// <param name="reset">reset the gameconfig data</param>
		/// <param name="reload">try reloading it</param>
		/// <returns></returns>
		public static void LoadGameConfig(bool reset = false, bool reload = false)
		{
			GameConfig gameConfig = new GameConfig { };
			FileStream file = null;
			BinaryFormatter bf = new BinaryFormatter();
			try
			{
				if (!reset)
				{
					file = File.Open(Application.dataPath + "/Config/Config.cfg", FileMode.Open);
					gameConfig = (GameConfig)bf.Deserialize(file);
					file.Close();
				}
			}
			catch (Exception e)
			{
				//file doesn't exist
				reset = true;
			}
			finally
			{
				if (reset)
				{
					gameConfig = new GameConfig
					{
						PlayerInputConfigs = new PlayerInputConfigs
						{
							Player1 = new PlayerInputConfig
							{
								AttackAKey = KeyCode.Alpha1,
								AttackBKey = KeyCode.Alpha2,
								AttackCKey = KeyCode.Alpha3,
								AttackDKey = KeyCode.Alpha4,
								CrouchKey = KeyCode.LeftControl,
								ItemAKey = KeyCode.Alpha5,
								ItemBKey = KeyCode.Alpha6,
								ItemCKey = KeyCode.Alpha7,
								ItemDKey = KeyCode.Alpha8,
								FocusKey = KeyCode.Mouse2,
								InteractKey = KeyCode.F,
								SwitchAttackLeftKey = KeyCode.Q,
								SwitchAttackRightKey = KeyCode.E,
								SwitchItemLeftKey = KeyCode.C,
								SwitchItemRightKey = KeyCode.V,
								ProneKey = KeyCode.LeftAlt,
								RunKey = KeyCode.LeftShift,
								AttackKey = KeyCode.Mouse1,
								JumpKey = KeyCode.Space,
								LookXAxis = "Mouse X",
								LookYAxis = "Mouse Y",
								MoveXAxis = "Horizontal",
								MoveYAxis = "Vertical"

							}
						}
					};
					Debug.LogWarning("Creating new Config file at \""+Application.dataPath+"/Config/Config.cfg");
					file = File.Create(Application.dataPath + "/Config/Config.cfg");
					bf.Serialize(file, gameConfig);
					file.Close();
					reload = true;
				}
			}
			LoadGameConfig(false, false);
			mGameConfig = gameConfig;
		}
		/// <summary>
		/// returns the InputConfigs for the desired player
		/// </summary>
		/// <param name="pic">PlayerInfoConfigs</param>
		/// <param name="player">desired player.0 = 1, 1 =2, 2 = 3, 3 = 4</param>
		/// <returns>PlayerInputConfig</returns>
		public static PlayerInputConfig GetPlayerInputConfig(PlayerInputConfigs pic,int player = 0)
		{
			switch (player)
			{
				case 0: return pic.Player1;
				case 1: return pic.Player2;
				case 3: return pic.Player3;
				default: return pic.Player4;
			}
		}
		//checks if the gameconfig class is valid
		public static bool isValid() { return !mGameConfig.Equals(new GameConfig { }); }
	}
	[Serializable]
	public struct GameConfig
	{
		public PlayerInputConfigs PlayerInputConfigs;
	}
}