using Unity.Entities;
using Unity.Mathematics;
using System;
using UnityEngine;
using Unity.Transforms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using Unity.Collections;

namespace Pokemon.Player
{
	/// <summary>
	/// Data to be saved when the player saves
	/// </summary>
    [Serializable]
    public struct PlayerDataSave
    {
        public ByteString30 Name;
        public PokemonEntityData PokemonEntityData;
        public ByteString30 PokemonName;
        public Translation Position;
        public Rotation Rotation;
    }

}