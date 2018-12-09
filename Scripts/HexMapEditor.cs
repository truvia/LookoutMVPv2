using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class HexMapEditor : MonoBehaviour {

	enum OptionalToggle{
		Ignore, Yes, No
	}

	private TerrainType terrainType;
	public HexGrid hexGrid;
	public int activeElevation;
	bool applyTerrain;
	bool applyElevation = true;

	#region riverVars
	OptionalToggle riverMode;
	bool isDrag;
	HexDirection dragDirection;
	HexCell previousCell;

	#endregion
	int brushSize;

	void Awake () {
		hexGrid = FindObjectOfType<HexGrid> ();
	}

	void Update(){
		if (Input.GetMouseButton (0) &&
		    !EventSystem.current.IsPointerOverGameObject ()) {
			HandleInput ();
		} else {
			previousCell = null;
		}



	}


	void HandleInput () {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (inputRay, out hit)) {
			HexCell currentCell = hexGrid.GetCellByPosition (hit.point);

			//Check to see if we are draggging the mouse
			if (previousCell && previousCell != currentCell) {
				ValidateDrag (currentCell);
			} else {
				isDrag = false;
			}



			EditCells (currentCell);
			previousCell = currentCell;
		} else {
			previousCell = null;
		}
	}

	void EditCells(HexCell centerCell){

		BoardManager boardManager = FindObjectOfType<BoardManager> ();
		List<int> allCellsToEdit = boardManager.FindAllHexCellsWithinNSteps (brushSize, centerCell);

		EditCell (centerCell);
		foreach (int i in allCellsToEdit) {
			HexCell cellToEdit = hexGrid.cells [i];

			EditCell (cellToEdit);
		}



	}

	void EditCell (HexCell cell) {
		if (applyTerrain) {
			cell.TerrainType = terrainType;
		}

		if (applyElevation) {
			cell.Elevation = activeElevation;
		}

		//Drawing rivers or removing rivers
		if (riverMode == OptionalToggle.No) {
			cell.RemoveRiver ();
		}
		if (isDrag && riverMode == OptionalToggle.Yes) {
			HexCell otherCell = cell.GetHexCellNeighbour (dragDirection.Opposite ());
			if (otherCell) {
				otherCell.SetOutgoingRiver (dragDirection);
			}

		}


	}
	public void SetElevation(float sliderValue){
		activeElevation = (int)sliderValue;
	}

	public void SelectTerrain(int i){
		applyTerrain = i >= 0;
		if (applyTerrain) {
			terrainType = (TerrainType)i;
		}
	}

	public void SetElevationToggle(bool b){
		applyElevation = b;
	}

	public void SetBrushSize(float size){
		brushSize = (int)size;
	}


	#region rivers

	public void SetRiverMode(int mode){
		riverMode = (OptionalToggle)mode;
	}

	public void ValidateDrag(HexCell currentCell){
		for (dragDirection = HexDirection.NE;
			dragDirection <= HexDirection.NW;
			dragDirection++) {

			if (previousCell.GetHexCellNeighbour (dragDirection) == currentCell) {
				isDrag = true;
				return;
			}
		}
		isDrag = false;
	}

	#endregion

}
