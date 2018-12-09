	using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {
	#region largerMaps
	public int chunkCountX = 4, chunkCountZ = 3;
	int cellCountX, cellCountZ;
	public HexGridChunk chunkPrefab;

	public HexGridChunk[] chunks;
	#endregion

	public HexCell cellPrefab;
	public Text cellLabelPrefab;

	public Texture2D noiseSource;

	public bool showCoordinates;

//	public Canvas gridCanvas;
//	public HexMesh hexMesh;
	MeshCollider meshCollider;


	public 	HexCell[] cells;


	#region colours
	public Color highlightRouteColor;
	public Color routeOutOfRangeColor;


	#endregion

	#region testvars
	HexCell startCell;
	#endregion






	void Awake () {
	//	gridCanvas = GetComponentInChildren<Canvas> (); 
	//	hexMesh = GetComponentInChildren<HexMesh> ();
		meshCollider = gameObject.AddComponent<MeshCollider> ();
		HexMetrics.noiseSource = noiseSource;

		cellCountX = chunkCountX * HexMetrics.chunkSizex;
		cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;
		CreateChunks ();
		CreateCells ();
	}

	void OnEnable(){
		HexMetrics.noiseSource = noiseSource;
	}


	void CreateCells () {
		cells = new HexCell[cellCountZ * cellCountX];

		for (int z = 0, i = 0; z < cellCountZ; z++) {
			for (int x = 0; x < cellCountX; x++) {
				CreateCell(x, z, i++);
			}
		}
	}

	void CreateCell (int x, int z, int i) {
		Vector3 position;

		position.x = (x + z * 0.5f - z/2) * (HexMetrics.innerRadius * 2f);
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);


		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.localPosition = position;
		cell.id = i;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates (x, z);


		if(i == 0 || i == 10 || i == 11 || i == 22 || i == 99 || i == 89 || i == 88 || i == 77 || i == 67 || i == 55 || i == 56 || i ==33 || i ==32){
			cell.TerrainType = TerrainType.River;
		} else	if(i == 95 || i == 94 ||i == 84 || i == 73 || i == 43|| i == 42 || i == 65 || i == 66 || i == 76){
			cell.TerrainType = TerrainType.Hills;
		} else	if (i >= 22 && i <= 24 || i >= 32 && i <= 34) {
			cell.TerrainType = TerrainType.Hills;
		} else if (i == 5 || i == 15 || i == 26 || i == 4) {
			cell.TerrainType = TerrainType.Hills;
		} else if (i == 47 || i == 46) {
			cell.TerrainType = TerrainType.Hills;
		} else {
			cell.TerrainType = TerrainType.Grass;
		}
		cell.name = "Cell " + i;


		if (x > 0) {
			cell.SetNeighbour (HexDirection.W, cells [i - 1]);
		}

		if (z > 0) {
			if ((z & 1) == 0) {
				cell.SetNeighbour (HexDirection.SE, cells[i - cellCountX]);
					if(x > 0){
					cell.SetNeighbour(HexDirection.SW, cells[i-cellCountX - 1]);
					} 

			
			}else{
				cell.SetNeighbour (HexDirection.SW, cells [i - cellCountX]);
				if (x < cellCountX - 1) {
					cell.SetNeighbour (HexDirection.SE, cells [i - cellCountX + 1]);
				}
			}
		}
			if (showCoordinates) {
			Text label = Instantiate<Text> (cellLabelPrefab);
			label.rectTransform.anchoredPosition = new Vector2 (position.x, position.z);
			label.tag = "coordinateLabel";
			label.text = cell.id.ToString();
			cell.uiRect = label.rectTransform;
			cell.Elevation = 0;

			AddCellToChunk (x, z, cell);
		}
	}

	void AddCellToChunk(int x, int z, HexCell cell){
		int chunkX = x / HexMetrics.chunkSizex;
		int chunkz = z / HexMetrics.chunkSizeZ;
		HexGridChunk chunk = chunks [chunkX + chunkz * chunkCountX];

		int localX = x - chunkX * HexMetrics.chunkSizex;
		int localZ = z - chunkz * HexMetrics.chunkSizeZ;
		chunk.AddCell (localX + localZ * HexMetrics.chunkSizex, cell);
	}

//	public HexGridChunk GetRelevantChunk(int x, int z){
//		int chunkX = x / HexMetrics.chunkSizex;
//		int chunkz = z / HexMetrics.chunkSizeZ;
//		HexGridChunk chunk = chunks [chunkX + chunkz * chunkCountX];
//		return chunk;
//	}


	void CreateChunks(){
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];
		for(int z = 0, i = 0; z < chunkCountZ; z++){
			for (int x = 0; x < chunkCountX; x++) {
				HexGridChunk chunk = chunks [i++] = Instantiate (chunkPrefab);
				chunk.transform.SetParent (transform);
			}
		}
	}
	#region TestingGround

	public void ColorCell(HexCell cell, Color color){
		cell.Color = color;

	}

	public void ReturnAllCellsToOriginalColor(){
		foreach (HexCell cell in cells) {
			ColorCell (cell, cell.defaultColour);
		}
		
	}

	public void ReturnListOfCellsToOriginalColor(List<HexCell> theseCells){
		foreach (HexCell cell in theseCells) {
			ColorCell (cell, cell.defaultColour);
		}
	}


	public void HideCellCoordinateLabels(){
		GameObject[] coordinateLabels = GameObject.FindGameObjectsWithTag ("coordinateLabel");

		foreach (GameObject thisGameObject in coordinateLabels) {
			
				thisGameObject.SetActive (false);

		}
	}

	public void ReturnCellToDefaultColor(HexCell cell){
		cell.Color = cell.defaultColour;
	}

	public void ShowCellIndexNumbers(bool visible){
		foreach (HexGridChunk chunk in chunks) {
			chunk.ShowThisChunkCellIndexes (visible);
		}
	}



	#endregion

	#region FindCells
	public int GetCellIndexFromCoordinates(HexCoordinates coordinates){
		return coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
	}




	public HexCell FindHexCellByIntCoordinates(int x, int z){

		foreach (HexCell cell in cells) {		
			if (cell.coordinates.X == x && cell.coordinates.Z == z) {
				return cell;
			}
		}

		return null;
	}




	public HexCell FindHexCellByHexCoordinate(HexCoordinates coordinate){

		int index = GetCellIndexFromCoordinates (coordinate);
		if (index < cells.Length) {
			return cells [index];
		}
		return null;
		}

	public void setMaterial(){
		
	}

	public HexCell GetCellByPosition(Vector3 cellPosition){
		cellPosition = transform.InverseTransformPoint (cellPosition);
		HexCoordinates coordinates = HexCoordinates.FromPosition (cellPosition);
		int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
		return cells [index];
	}


	public void RefreshAllChunkGrids(){

		foreach (HexGridChunk hexChunk in chunks) {
			hexChunk.RefreshGrid ();
		}
	}

	#endregion


	}
