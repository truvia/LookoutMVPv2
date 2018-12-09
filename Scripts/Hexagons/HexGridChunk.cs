using UnityEngine.UI;
using UnityEngine;

public class HexGridChunk : MonoBehaviour {

	HexCell[] cells;
	HexMesh hexMesh;
	public Canvas gridCanvas;

	void Awake(){
		gridCanvas = GetComponentInChildren<Canvas> ();
		hexMesh = GetComponentInChildren<HexMesh> ();
		cells = new HexCell[HexMetrics.chunkSizex * HexMetrics.chunkSizeZ]; 
		ShowThisChunkCellIndexes (false);
	}




	public void AddCell(int index, HexCell cell){
		cells [index] = cell;
		cell.transform.SetParent (transform, false);
		cell.uiRect.SetParent (gridCanvas.transform, false);
		cell.chunk = this;
	}

	public void RefreshGrid(){
	//	hexMesh.TriangulateAllCells(cells);
		enabled = true;
	}

	void LateUpdate(){
		hexMesh.TriangulateAllCells (cells);
		enabled = false;
	}

	public void ShowThisChunkCellIndexes(bool visible){
		
		gridCanvas.gameObject.SetActive (visible);
	}


}
