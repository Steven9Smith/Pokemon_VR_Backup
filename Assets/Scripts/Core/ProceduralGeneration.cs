using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using System;
using Unity.Collections;

namespace Core.Procedural
{
	
	public class ProceduralGeneration
	{
		/// <summary>
		/// Bounds in which the Procedual Generation will take place
		/// </summary>
		protected virtual Bounds ProcedualBounds { get; set; }
		/// <summary>
		/// the difference of distance between each point on the grid
		/// </summary>
		protected virtual int GridSensitivity {
			get
			{
				return GridSensitivity;
			}
			set
			{
				if (value > 0)
					GridSensitivity = value;
				else
				{
					Debug.LogError("Procedual Generation: cannot have a Grid Sensitivity less than or equal to 0. Setting Sensitivity to 1f by default.");
					GridSensitivity = 1;
				}
			}
		}

		private bool SuppressWarnings = false;

		protected float3[][] ProcedualGrid;

		protected virtual void Generate()
		{
			GenerateGrid();
		}

		/// <summary>
		/// This will Generate a grid based on the Procedual Bounds and GridSensitivity
		/// </summary>
		protected virtual void GenerateGrid()
		{
			int xMax = (int)math.ceil(ProcedualBounds.size.x / GridSensitivity);
			int zMax = (int)math.ceil(ProcedualBounds.size.z / GridSensitivity);

			ProcedualGrid = new float3[xMax][];
			
			for(int i = 0; i < xMax; i+=GridSensitivity)
			{
				ProcedualGrid[i/GridSensitivity] = new float3[zMax];
				for(int j = 0; j < zMax; j+=GridSensitivity)
				{
					ProcedualGrid[i/GridSensitivity][j/GridSensitivity] = new float3(i,0,j);
				}
			}
		}
		/// <summary>
		/// Suppress Warnings
		/// </summary>
		protected void EnableSuppressWarnings()
		{
			SuppressWarnings = true;
		}
		/// <summary>
		/// Don't Suppress Warnings
		/// </summary>
		protected void DisableSuppressWarnings()
		{
			SuppressWarnings = false;
		}

		/// <summary>
		/// If you don't need the data to be persistant than you can clean up the NativeArrays after 
		/// the Generation is complete.
		/// </summary>
		protected virtual void CleanUp()
		{

		}
		/// <summary>
		/// When the class is destroyed it will initiate cleanup
		/// </summary>
		~ProceduralGeneration()
		{
			CleanUp();
		}


	}

	public class ProceduralTerrain : ProceduralGeneration
	{
		public void a()
		{

		}
	}

	
}
