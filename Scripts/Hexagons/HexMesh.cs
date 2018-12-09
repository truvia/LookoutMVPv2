using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

		Mesh hexMesh;
		static List<Vector3> vertices = new List<Vector3> ();
		static List<int> triangles = new List<int> ();
		static List<Color> colors = new List<Color> ();
		MeshCollider meshCollider;

	// Use this for initialization
	void Awake () {
		GetComponent<MeshFilter> ().mesh = hexMesh = new Mesh ();
		meshCollider = gameObject.AddComponent<MeshCollider> ();
		hexMesh.name = "Hex Mesh";


	}
	public void TriangulateAllCells(HexCell[] allCells){
		hexMesh.Clear ();
		vertices.Clear ();
		triangles.Clear ();
		colors.Clear ();
		foreach (HexCell cell in allCells) {
			PublicTriangulate (cell);
		}
		hexMesh.vertices = vertices.ToArray ();
		hexMesh.triangles = triangles.ToArray ();
		hexMesh.colors = colors.ToArray ();
		hexMesh.RecalculateNormals ();
		meshCollider.sharedMesh = hexMesh;
	}


	public void PublicTriangulate (HexCell cell){


		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
			Triangulate (d, cell);
		}	



	}

	void Triangulate(HexDirection direction, HexCell cell){
		Vector3 center = cell.Position;
		EdgeVertices e = new EdgeVertices (
			center + HexMetrics.GetFirstSolidCorner(direction),
			center + HexMetrics.GetSecondSolidCorner(direction),
			0.25f

		);

		if (cell.HasRiver) {
			if (cell.HasRiverThroughEdge(direction)) {
				e.v3.y = cell.StreamBedY;
				if (cell.HasRiverBeginOrEnd) {
					TriangulateWithRiverBeginningOrEnd(direction, cell, center, e);
				}
				else {
					TriangulateWithRiver(direction, cell, center, e);
				}
			}
			else {
				TriangulateAdjacentToRiver(direction, cell, center, e);
			}
		}
		else {
			TriangulateEdgeFan(center, e, cell.Color);
		}
			if (direction <= HexDirection.SE) {
				TriangulateConnection (direction, cell, e);
			}

	

	}

	void TriangulateWithRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e){

		Debug.Log (direction);

		Vector3 centerL, centerR;


		//if the cell has a river going in the direct opposite direction of the way it entered, then it is a straight river. 
		if (cell.HasRiverThroughEdge (direction.Opposite ())) {
			//For rivers that flow stright through a cell
			//firstly stretch the center into a  line;
			//assuming direction is flowing from the east to the west (along the flat line), we need the NE corner to start with. So get the north east corner of the "solid / inner" hex (direction.Previous()), times that by a quarter to get 
			centerL = center + HexMetrics.GetFirstSolidCorner (direction.Previous ()) * 0.25f;
			//assuming river is flowing east to west, the next corner y ou need dto get is the South East corner
			centerR = center + HexMetrics.GetSecondSolidCorner (direction.Next ()) * 0.25f;
		} else if (cell.HasRiverThroughEdge (direction.Next ())) {
			//then it is a very sharp turn - i.. the river flows in the east and goes out south east. 
			centerL = center;
			centerR = Vector3.Lerp (center, e.v5, 2f / 3f); //

		} else if (cell.HasRiverThroughEdge (direction.Previous ())) {
			//then it is also a very sharp turn - i.e. the river flows in form the east and goes out north east
			centerL = Vector3.Lerp (center, e.v1, 2f / 3f); //
			centerR = center;
		
		}else if (cell.HasRiverThroughEdge(direction.Next2())) {
		//it is a meander
			centerL = center;
			centerR = center +
			HexMetrics.GetSolidEdgeMiddle (direction.Next ()) * (0.5f * HexMetrics.innerToOuter);
			}
			else {
			//normal curve
				centerL = center +
				HexMetrics.GetSolidEdgeMiddle(direction.Previous()) * (0.5f * HexMetrics.innerToOuter);
				centerR = center;
			}
		center = Vector3.Lerp (centerL, centerR, 0.5f);
	
		//the midpoints between  these (see page 111 of my blue book) are as follows:
		EdgeVertices m = new EdgeVertices (
			                 Vector3.Lerp (centerL, e.v1, 0.5f), //this is corner 1 ( i.e. the halfway point between the new Center line to the left and the first corner of the inner hex
							Vector3.Lerp (centerR, e.v5, 0.5f), // thhis is corner 2 (i.e. the halfway point between the new Center line to the right and the second corner of the inner hex
								1f/6f
		                 );


		//this results in us now have defined line between points m1, m2, m3, m4, m5 (or more accuarately, m.v1, m.v2, m.v3

		//we now need to set the height of the middle vertices so that it is lower thn the ones around in order to create a V channel. Since we've already done this for the outer hex limit, just copy this for the center and m middle vertices.  
		m.v3.y = center.y = e.v3.y;

		//since TriangulateEdgeStrip already creates a strip of quads, lets use that to fill in quads 1, 3, 5, 6.
		TriangulateEdgeStrip(m, cell.Color, e, cell.Color);

		AddTriangle (centerL, m.v1, m.v2);
		AddTriangleColor (cell.Color, cell.Color, cell.Color);

		AddQuad (centerL, center, m.v2, m.v3);
		AddQuadColor (cell.Color, cell.Color);

		AddQuad (center, centerR, m.v3, m.v4);
		AddQuadColor (cell.Color, cell.Color);
		AddTriangle (centerR, m.v4, m.v5);
		AddTriangleColor (cell.Color, cell.Color, cell.Color);

	}

	void TriangulateWithRiverBeginningOrEnd(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e){


		EdgeVertices m = new EdgeVertices (Vector3.Lerp (center, e.v1, 0.4f), Vector3.Lerp (center, e.v5, 0.5f), 1 / 4);

		m.v3.y = e.v3.y;

		//This will create a cell in which the river is pinched, which is fine in this context.
		//Lets first creat the four quads using EdgeStrip
		TriangulateEdgeStrip(m, cell.Color, e, cell.Color);
		TriangulateEdgeFan (center, m, cell.Color);
	}

	void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1){
		HexCell neighbour =	 cell.GetHexCellNeighbour (direction);
		if (neighbour == null) {
			return;
		}
		Vector3 bridge = HexMetrics.GetBridge (direction);
		bridge.y = neighbour.Position.y - cell.Position.y;
		EdgeVertices e2 = new EdgeVertices(e1.v1 + bridge, e1.v5 +bridge, 1f/4f);



		if (cell.HasRiverThroughEdge (direction)) {
			e2.v3.y = neighbour.StreamBedY;
		}

		if (cell.GetEdgeTypeByDirection (direction) == HexMetrics.HexEdgeType.Slope) {
			TriangulateEdgeTerraces(e1, cell, e2, neighbour);
		}else {
			TriangulateEdgeStrip(e1, cell.Color, e2, neighbour.Color);

		}

		HexCell nextNeighbour = cell.GetHexCellNeighbour (direction.Next ());

		if (direction <= HexDirection.E && nextNeighbour != null) {
			Vector3 v5 = e1.v5 + HexMetrics.GetBridge (direction.Next ());
			v5.y = nextNeighbour.Position.y;


			if (cell.Elevation <= neighbour.Elevation) {
				if (cell.Elevation <= nextNeighbour.Elevation) {
					TriangulateCorner(e1.v5, cell, e2.v5, neighbour, v5, nextNeighbour);
				}
				else {
					TriangulateCorner(v5, nextNeighbour, e1.v5, cell, e2.v5, neighbour);
				}
			}
			else if (neighbour.Elevation <= nextNeighbour.Elevation) {
				TriangulateCorner(e2.v5, neighbour, v5, nextNeighbour, e1.v5, cell);
			}
			else {
				TriangulateCorner(v5, nextNeighbour, e1.v5, cell, e2.v5, neighbour);
			}

		}
	}

	void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3){
		int vertexIndex = vertices.Count;
		vertices.Add (HexMetrics.Perturb(v1));
		vertices.Add (HexMetrics.Perturb(v2));
		vertices.Add (HexMetrics.Perturb(v3));
		triangles.Add (vertexIndex);
		triangles.Add (vertexIndex + 1);
		triangles.Add (vertexIndex + 2);
	}

	void AddUnperturbedTriangle(Vector3 v1, Vector3 v2, Vector3 v3){
		int vertexIndex = vertices.Count;
		vertices.Add (v1);
		vertices.Add (v2);
		vertices.Add (v3);
		triangles.Add (vertexIndex);
		triangles.Add (vertexIndex + 1);
		triangles.Add (vertexIndex + 2);
	}

	void AddTriangleColor(Color c1, Color c2, Color c3){
				colors.Add (c1);
				colors.Add (c2);
		colors.Add (c3);
	
	}

	void TriangulateEdgeTerraces (EdgeVertices begin, HexCell beginCell, EdgeVertices end, HexCell endCell) {
		EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
		Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

		TriangulateEdgeStrip(begin, beginCell.Color, e2, c2);

		for (int i = 2; i < HexMetrics.terraceSteps; i++) {
			EdgeVertices e1 = e2;
			Color c1 = c2;
			e2 = EdgeVertices.TerraceLerp(begin, end, i);
			c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);
			TriangulateEdgeStrip(e1, c1, e2, c2);
		}

		TriangulateEdgeStrip(e2, c2, end, endCell.Color);
	}

	void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4){
		int vertexIndex = vertices.Count;
		vertices.Add (HexMetrics.Perturb(v1));
		vertices.Add (HexMetrics.Perturb(v2));
		vertices.Add (HexMetrics.Perturb(v3));
		vertices.Add (HexMetrics.Perturb(v4));
		triangles.Add (vertexIndex);
		triangles.Add (vertexIndex +2);
		triangles.Add (vertexIndex + 1);
		triangles.Add (vertexIndex+ 1);
		triangles.Add (vertexIndex +2);
		triangles.Add (vertexIndex + 3);

	}

	void AddQuadColor(Color c1, Color c2){
		colors.Add (c1);
		colors.Add (c1);
		colors.Add (c2);
		colors.Add (c2);
	}

	void AddQuadColor(Color c1, Color c2, Color c3, Color c4){
		colors.Add (c1);
		colors.Add (c2);
		colors.Add (c3);
		colors.Add (c4);
	}

	void AddQuadColor(Color c1){
		colors.Add (c1);
		colors.Add (c1);
		colors.Add (c1);
		colors.Add (c1);
	}



	/// <summary>
	/// Triangulates the slope of the corner between three cells.
	/// </summary>
	/// <param name="bottom">Vector3 location of the lowest indeces of the three cells. 
	/// </param>
	/// <param name="bottomCell">lowest cell.</param>
	/// <param name="left">Left most cell.</param>
	/// <param name="leftCell">Left cell.</param>
	/// <param name="right">Right.</param>
	/// <param name="rightCell">Right cell.</param>
	void TriangulateCorner(
		Vector3 bottom, HexCell bottomCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	){
		HexMetrics.HexEdgeType leftEdgeType = bottomCell.GetEdgeTypeByCell (leftCell); //the type of slope between the bottommost cell and the cell to its left 
		HexMetrics.HexEdgeType rightEdgeType = bottomCell.GetEdgeTypeByCell (rightCell); // the type of slope between the bottomost cel land the cell to its right


		if (leftEdgeType == HexMetrics.HexEdgeType.Slope) {
			if (rightEdgeType == HexMetrics.HexEdgeType.Slope) {
				TriangulateCornerTerraces (
					bottom, bottomCell, left, leftCell, right, rightCell
				);
			} else if (rightEdgeType == HexMetrics.HexEdgeType.Flat) {
				TriangulateCornerTerraces (
					left, leftCell, right, rightCell, bottom, bottomCell
				);

			} else {
				TriangulateCornerTerracesCliff (
					bottom, bottomCell, left, leftCell, right, rightCell
				);
			
			}
		} else if (rightEdgeType == HexMetrics.HexEdgeType.Slope) {
			if (leftEdgeType == HexMetrics.HexEdgeType.Flat) {
				TriangulateCornerTerraces (
					right, rightCell, bottom, bottomCell, left, leftCell
				);

			} else {

				TriangulateCornerCliffTerraces (bottom, bottomCell, left, leftCell, right, rightCell);
				return;
			}
		} else if (leftCell.GetEdgeTypeByCell (rightCell) == HexMetrics.HexEdgeType.Slope) {
			if (leftCell.Elevation < rightCell.Elevation) {
				TriangulateCornerCliffTerraces (
					right, rightCell, bottom, bottomCell, left, leftCell
				);
			} else {
				TriangulateCornerTerracesCliff (
					left, leftCell, right, rightCell, bottom, bottomCell
				);
			}

		} else {

			AddTriangle (bottom, left, right);
			AddTriangleColor (bottomCell.Color, leftCell.Color, rightCell.Color);
		}
	
	}


	void TriangulateCornerTerraces(
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell

	){

		Vector3 v3 = HexMetrics.TerraceLerp (begin, left, 1);
		Vector3 v4 = HexMetrics.TerraceLerp (begin, right, 1);
		Color c3 = HexMetrics.TerraceLerp (beginCell.Color, leftCell.Color, 1);
		Color c4 = HexMetrics.TerraceLerp (beginCell.Color, rightCell.Color, 1);

		AddTriangle(begin, v3, v4);
		AddTriangleColor(beginCell.Color, c3, c4);

		for (int i = 2; i < HexMetrics.terraceSteps; i++) {
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c3;
			Color c2 = c4;
			v3 = HexMetrics.TerraceLerp(begin, left, i);
			v4 = HexMetrics.TerraceLerp(begin, right, i);
			c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
			c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i);
			AddQuad(v1, v2, v3, v4);
			AddQuadColor(c1, c2, c3, c4);
		}

		AddQuad(v3, v4, left, right);
		AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
		
		
	}
	/// <summary>
	/// Triangulates the corner terraces when a step terrace is adjacent to a cliff.
	/// </summary>
	void TriangulateCornerTerracesCliff (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		float b = 1f / (rightCell.Elevation - beginCell.Elevation);
		if (b < 0) {
			b = -b;
		}
		Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b);
		Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);


		TriangulateBoundaryTriangle (begin, beginCell, left, leftCell, boundary, boundaryColor);

		if (leftCell.GetEdgeTypeByCell (rightCell) == HexMetrics.HexEdgeType.Slope) {
			TriangulateBoundaryTriangle (left, leftCell, right, rightCell, boundary, boundaryColor);
		} else {
			AddUnperturbedTriangle(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
			AddTriangleColor (leftCell.Color, rightCell.Color, boundaryColor);
		}
	}

	void TriangulateCornerCliffTerraces (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		float b = 1f / (leftCell.Elevation - beginCell.Elevation);
		if (b < 0) {
			b = -b;
		}
		Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
		Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);


		TriangulateBoundaryTriangle (right, rightCell, begin, beginCell, boundary, boundaryColor);

		if (leftCell.GetEdgeTypeByCell (rightCell) == HexMetrics.HexEdgeType.Slope) {
			TriangulateBoundaryTriangle (left, leftCell, right, rightCell, boundary, boundaryColor);
		} else {
			AddUnperturbedTriangle(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
			AddTriangleColor (leftCell.Color, rightCell.Color, boundaryColor);
		}
	}


	void TriangulateBoundaryTriangle (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 boundary, Color boundaryColor
	) {
		Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
		Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

		AddUnperturbedTriangle(HexMetrics.Perturb(begin), v2, boundary);
		AddTriangleColor(beginCell.Color, c2, boundaryColor);

		for (int i = 2; i < HexMetrics.terraceSteps; i++) {
			Vector3 v1 = v2;
			Color c1 = c2;
			v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
			c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
			AddUnperturbedTriangle(v1, v2, boundary);
			AddTriangleColor(c1, c2, boundaryColor);
		}

		AddUnperturbedTriangle(v2, HexMetrics.Perturb(left), boundary);
		AddTriangleColor(c2, leftCell.Color, boundaryColor);
	}




	void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color){
		AddTriangle (center, edge.v1, edge.v2);
		AddTriangleColor (color, color, color);


		AddTriangle (center, edge.v2, edge.v3);
		AddTriangleColor (color, color, color);

	
		AddTriangle (center, edge.v3, edge.v4);
		AddTriangleColor (color, color, color);


		AddTriangle (center, edge.v4, edge.v5);
		AddTriangleColor (color, color, color);


	}

	/// <summary>
	/// Creates a strip of four quads using two sets of vertices (the left line and the right line). 
	/// </summary>
	/// <param name="e1">All the points along the leftmost line.</param>
	/// <param name="c1">The color on the left.</param>
	/// <param name="e2">All the points along the rightmost line.</param>
	/// <param name="c2">The color on the right.</param>
	void TriangulateEdgeStrip(EdgeVertices e1, Color c1, EdgeVertices e2, Color c2){
		AddQuad (e1.v1, e1.v2, e2.v1, e2.v2);
		AddQuadColor (c1, c2);

		AddQuad (e1.v2, e1.v3, e2.v2, e2.v3);
		AddQuadColor (c1, c2);

		AddQuad (e1.v3, e1.v4, e2.v3, e2.v4);
		AddQuadColor (c1, c2);

		AddQuad (e1.v4, e1.v5, e2.v4, e2.v5);
		AddQuadColor (c1, c2);
	}


	/// <summary>
	/// Fills in the rest of the cell that is left empty by a river
	/// </summary>
	/// <param name="direction">Direction.</param>
	/// <param name="cell">Cell.</param>
	/// <param name="center">Center.</param>
	/// <param name="e">E.</param>
	void TriangulateAdjacentToRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e){

		if (cell.HasRiverThroughEdge (direction.Next ())) {
			if (cell.HasRiverThroughEdge (direction.Previous ())) {
				//Then the bit that we're filling in of the cell is on the inside of the curve
				center += HexMetrics.GetSolidEdgeMiddle (direction) * (HexMetrics.innerToOuter * 0.5f);
			}else if(cell.HasRiverThroughEdge(direction.Previous2())){
				//then we must be on one side of a a straight river.
				center += HexMetrics.GetFirstSolidCorner(direction) * 0.25f;
			}
		} else if(cell.HasRiverThroughEdge(direction.Previous()) && cell.HasRiverThroughEdge(direction.Next2())){
				//then we must be on the other side of a straight river - so move the center to the next solid corner 
			center += HexMetrics.GetSecondSolidCorner(direction) * 0.25f;
		}





		EdgeVertices m = new EdgeVertices(
			Vector3.Lerp(center, e.v1, 0.5f),
			Vector3.Lerp(center, e.v5, 0.5f),
			1f/4f
		);

		TriangulateEdgeStrip (m, cell.Color, e, cell.Color);
		TriangulateEdgeFan (center, m, cell.Color);
	
	}


}
