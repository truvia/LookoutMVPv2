using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexMetrics {

	public static Texture2D noiseSource;


	public const float outerToInner = 0.866025404f;
	public const float innerToOuter = 1f / outerToInner;

	public const float outerRadius = 10f;
	public const float innerRadius = outerRadius * outerToInner;	
	public enum HexEdgeType
	{
		Flat,
		Slope,
		Cliff
	}

	public const float solidFactor = 0.8f;
	public const float blendfactor = 1f - solidFactor; 
	public const float elevationStep = 3f;
	public const int terracesPerSlope = 3;
	public const int terraceSteps = terracesPerSlope * 2 +1;
	public const float horizontalTerraceStepSize = 1f / terraceSteps; 
	public const float verticalTerraceStepSize = 1f/ (terracesPerSlope + 1);

	public const float streamBedElevationOffset = -1f;


	public const float cellPerturbStrength = 0f; //4f
	public const float elevationPerturbStrength = 1.5f;
	public const float noiseScale = 0.003f;

	#region largerMaps
	public const int chunkSizex = 5, chunkSizeZ = 5;
	#endregion


	/// <summary>
	/// Defines the basic external structure of a hexagon, with seven points ( points 1 and 7 being the same spot. 
	/// </summary>
	public static Vector3[] corners = {
		new Vector3(0f, 0f, outerRadius), //NE(1)
		new Vector3(innerRadius, 0f, 0.5f * outerRadius), //E (2)
		new Vector3(innerRadius, 0f, -0.5f * outerRadius), //SE (3)
		new Vector3(0f, 0f, -outerRadius), //SW (4)
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius), //W (5)
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius), //NW (6)
		new Vector3(0f, 0f, outerRadius) //NE(7)
	};


	/// <summary>
	/// Gets the first external point of a hexagon, relative to the diretion you input. This is the furthest boundary of the cell
	/// </summary>
	/// <returns>The first corner.</returns>
	/// <param name="direction">Direction.</param>
	public static Vector3  GetFirstCorner(HexDirection direction){
		return corners[(int)direction];
	}

	/// <summary>
	/// Gets the second external point of a hexagon, relative to the diretion you input. This is the furthest boundary of the cell
	/// </summary>
	/// <returns>The second corner.</returns>
	/// <param name="direction">Direction.</param>
	public static Vector3 GetSecondCorner(HexDirection direction){
		return corners[(int)direction + 1];
	}


	/// <summary>
	///Gets the first internal point of a cell (i.e. the part of the cell which is "solid" in color and shape and which doesn't blend with other cells) relative to the direction you input. 
	/// </summary>
	/// <returns>The first solid corner.</returns>
	/// <param name="direction">Direction.</param>
	public static Vector3 GetFirstSolidCorner(HexDirection direction){
		return corners[(int) direction] * solidFactor;
	}


	/// <summary>
	///Gets the second internal point of a cell (i.e. the part of the cell which is "solid" in color and shape and which doesn't blend with other cells) relative to the direction you input. 
	/// </summary>
	/// <returns>The first solid corner.</returns>
	/// <param name="direction">Direction.</param>
	public static Vector3 GetSecondSolidCorner(HexDirection direction){
		return corners [(int)direction + 1] * solidFactor; 
	}



	public static Vector3 GetBridge(HexDirection direction){
		return (corners[(int)direction] + corners[(int)direction + 1]) * blendfactor;
	}

	public static	Vector3 TerraceLerp(Vector3 a, Vector3 b, int step){

		float horizontal = step * HexMetrics.horizontalTerraceStepSize;
		a.x += (b.x - a.x) * horizontal;
		a.z += (b.z - a.z) * horizontal;

		float vertical = ((step + 1) / 2) * HexMetrics.verticalTerraceStepSize;
		a.y += (b.y - a.y) * vertical; 
		return a;
	}

	public static Color TerraceLerp(Color a, Color b, int step){
		float horizontal = step * HexMetrics.horizontalTerraceStepSize;
		return Color.Lerp (a, b, horizontal);
	}

	public static HexEdgeType GetHexEdgeType(int elevation1, int elevation2){

		if(elevation1 == elevation2){
			return HexEdgeType.Flat;
		}

		int delta = elevation2 - elevation1;

		if (delta == -1 || delta == 1) {
			return HexEdgeType.Slope;

		}
		return HexEdgeType.Cliff;

	}

	#region Making Hexagons Irregular
	public static Vector4 SampleNoise(Vector3 position){
		return noiseSource.GetPixelBilinear(position.x * noiseScale, position.z * noiseScale);
	}



	public static Vector3 Perturb(Vector3 position){
		Vector4 sample = SampleNoise(position);
		position.x += (sample.x * 2f - 1f) * cellPerturbStrength;
		//position.y += (sample.y * 2f - 1f) * HexMetrics.verticalPerturbStrength; //To keep cell centre flat don't adjust the y coord.
		position.z += (sample.z * 2f - 1f) * cellPerturbStrength;
		return position;
	}



	#endregion

	#region rivers
	public static Vector3 GetSolidEdgeMiddle(HexDirection direction){
		//averages two adjecent corner vectors  and applys the solid factor.
		return (corners[(int)direction] + corners[(int)direction + 1]) * (0.5f * solidFactor);
	}
	#endregion



}
