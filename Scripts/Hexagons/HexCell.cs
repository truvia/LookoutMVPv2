using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum HexDirection{
	NE,
	E,
	SE,
	SW,
	W,
	NW	
}

/// <summary>
/// Hex direction extensions.
/// </summary>
public static class HexDirectionExtensions{

	/// <summary>
	/// Return the opposite of a sepcified direction		/// </summary>
	/// <param name="direction">Direction.</param>
	public static HexDirection Opposite (this HexDirection direction){
		return (int)direction < 3 ? (direction + 3) : (direction - 3);
	}


	public static HexDirection Previous (this HexDirection direction){
		return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
	}

	public static HexDirection Next (this HexDirection direction){
		return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
	}

	public static HexDirection Previous2(this HexDirection direction){
		direction -= 2;
		return direction >= HexDirection.NE ? direction : (direction + 6);
	}

	public static HexDirection Next2(this HexDirection direction){
		direction += 2;
		return direction <= HexDirection.NW ? direction : (direction - 6);
	}
}

public enum TerrainType{
	River,
	Mountain,
	Grass,
	Desert,
	Forest,
	Hills
}


public class HexCell : MonoBehaviour {


	public Color riverColor = Color.blue;
	public Color mountainColor = Color.grey;
	public Color grassColor = Color.green;
	public Color desertColor = Color.yellow;
	public Color forestColor = Color.red;
	public Color hillsColor = Color.black;

	public HexGridChunk chunk; //the chunk of  the grid that this cell belongs to

	public int id;
	public Color defaultColour; 
	public HexCoordinates coordinates; 
	Color color;
	TerrainType terrainType;
	public int movementCost;
	public RectTransform uiRect;

	#region rivers
	bool hasIncomingRiver, hasOutgoingRiver;
	HexDirection incomingRiver, outGoingRiver; 

	public bool HasIncomingRiver{
		get{ 
			return hasIncomingRiver;
		}
	}

	public bool HasOutgoingRiver{
		get{ 
			return hasOutgoingRiver;
		}
	}

	public HexDirection IncomingRiver{
		get{ 
			return incomingRiver;
		}
	}

	public HexDirection OutGoingRiver{
		get{ 
			return outGoingRiver;
		}
	}

	public bool HasRiver{
		get{ 
			return hasIncomingRiver || hasOutgoingRiver;
		}
	}

	public bool HasRiverBeginOrEnd{
		get{ 
			return hasIncomingRiver != hasOutgoingRiver;
		}
	}

	public bool HasRiverThroughEdge(HexDirection direction){
		return hasIncomingRiver && incomingRiver == direction ||
			hasOutgoingRiver && outGoingRiver == direction;
	}

	public float StreamBedY{
		get{ 
			return (elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep;
		}
	}

	#endregion


	public int Elevation{
		get{
			return elevation;	
		}
		set{ 
			if (elevation == value) {
				return;
				}
			elevation = value;
			Vector3 position = transform.localPosition;
			position.y = value * HexMetrics.elevationStep;
			position.y += (HexMetrics.SampleNoise(position).y *2f - 1f) * HexMetrics.elevationPerturbStrength;
			transform.localPosition = position;

			Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = -position.y;
			uiRect.localPosition = uiPosition;

			if(hasOutgoingRiver && elevation < GetHexCellNeighbour(outGoingRiver).elevation){
				RemoveOutgoingRiver();
			}

			
			if (hasIncomingRiver && elevation < GetHexCellNeighbour (incomingRiver).elevation) {
				RemoveIncomingRiver ();
			}


			Refresh();
		}
	}
	int elevation = int.MinValue;


	public TerrainType TerrainType{
		get{ 
			return terrainType;
		}
		set{ 
			if (terrainType == value) {
				return;
			}
			terrainType = value;
			SetTerrainColor ();
			Refresh ();
		}
	}

	public Color Color{
		get{ 
			return color;
		}

		set{ 
			if (color == value) {
				return;
			}
			color = value;

			Refresh();

		}
	}


	public 	Vector3 Position {
		get{ 
			return transform.localPosition;
		}
	}

	#region newVars
	[SerializeField]
	HexCell[] neighbours = new HexCell[6];
	#endregion

	// Use this for initialization
	void Start () {
		if (terrainType == TerrainType.Grass || terrainType == TerrainType.Desert) {
			movementCost = 1;
		} else if(terrainType == TerrainType.Forest || terrainType == TerrainType.Hills){
			movementCost = 2;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	#region newCode

	/// <summary>
	/// Gets the neighbour of the cell in any given direction
	/// </summary>
	/// <returns>The hexcell neighbour.</returns>
	/// <param name="cell">Cell to start from</param>
	/// <param name="direction">Direction to look in (integer array) </param>
	public HexCell GetHexCellNeighbour(HexDirection direction){
		//	Debug.Log ("direction x: " + direction [0] + ", " + direction [1] + ", " + direction [2]);
		//	Debug.Log ("cell new x coord" + cell.coordinates.X);

		return neighbours [(int)direction];
	}


	public void SetNeighbour(HexDirection direction, HexCell cell){
		neighbours [(int)direction] = cell;
		cell.neighbours [(int)direction.Opposite ()] = this;
	}




	public void SetTerrainColor(){

		riverColor = Color.blue;
		mountainColor = Color.grey;
		grassColor = Color.green;
		desertColor = Color.yellow;
		forestColor = Color.red;
		hillsColor = Color.black;

			if (terrainType == TerrainType.River) {
				defaultColour = riverColor;
			} else if (terrainType == TerrainType.Mountain) {
				defaultColour = mountainColor;
			} else if (terrainType == TerrainType.Grass) {
				defaultColour = grassColor;
			} else if (terrainType == TerrainType.Desert) {
				defaultColour = desertColor;
			} else if (terrainType == TerrainType.Forest) {
				defaultColour = forestColor;
			} else {
				defaultColour = hillsColor;
			}
		color = defaultColour;
}

	public HexMetrics.HexEdgeType GetEdgeTypeByDirection(HexDirection direction){
		return HexMetrics.GetHexEdgeType (elevation, neighbours [(int)direction].elevation);
	}


	/// <summary>
	/// Gets the type of slope between two cells, using HexCell as the "other" cell.
	/// </summary>
	/// <returns>The edge type by cell.</returns>
	/// <param name="otherCell">Other cell.</param>
	public HexMetrics.HexEdgeType GetEdgeTypeByCell(HexCell otherCell){
		return HexMetrics.GetHexEdgeType (elevation, otherCell.elevation);
	}

	void Refresh(){
		if (chunk) {
			chunk.RefreshGrid ();
		}

		for (int i = 0; i < neighbours.Length; i++) {
			HexCell neighbourCell = neighbours [i];
			if (neighbourCell != null && neighbourCell.chunk != chunk) {
				neighbourCell.chunk.RefreshGrid ();
			}
		}
	}

	public void RefreshSelfOnly(){
		if (chunk) {
			chunk.RefreshGrid ();
		}
	}

	#endregion


	#region Rivers

	public void RemoveOutgoingRiver(){
		if (!hasOutgoingRiver) {
			return;
		}

		hasOutgoingRiver = false;
		RefreshSelfOnly ();

		HexCell neighbourcell = GetHexCellNeighbour(outGoingRiver);
		neighbourcell.hasIncomingRiver = false;
		neighbourcell.RefreshSelfOnly();
	}

	public void RemoveIncomingRiver(){
		if (!hasIncomingRiver) {
			return;
		}

		hasIncomingRiver = false;
		RefreshSelfOnly ();
		HexCell neighbourCell = GetHexCellNeighbour (incomingRiver);
		neighbourCell.hasOutgoingRiver = false;
		neighbourCell.RefreshSelfOnly ();
	}

	public void RemoveRiver(){
		RemoveOutgoingRiver ();
		RemoveIncomingRiver ();
	}

	public void SetOutgoingRiver(HexDirection direction){
		if (hasOutgoingRiver && outGoingRiver == direction) {
			return;
		}
		HexCell neighbour = GetHexCellNeighbour (direction);
		if(!neighbour || elevation < neighbour.Elevation){
			return;
		} 

		if (hasOutgoingRiver && incomingRiver == direction) {
			RemoveIncomingRiver ();
		}

		hasOutgoingRiver = true;
		outGoingRiver = direction;
		RefreshSelfOnly ();

		neighbour.hasIncomingRiver = true;
		neighbour.incomingRiver = direction.Opposite ();
		neighbour.RefreshSelfOnly ();
	}

	#endregion

		











}
