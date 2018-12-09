﻿
using UnityEngine;

public struct EdgeVertices {
		public Vector3 v1, v2, v3, v4, v5;


	/// <summary>
	/// Automatically sets all the right vertices inbetween two points, using a definable Lerp "step"/percentage/fraction 	/// </summary>
	/// <param name="corner1">Corner1.</param>
	/// <param name="corner2">Corner2.</param>
	/// <param name="outerStep">Outer step (how often you want your points (e.g. 0.25 to split the line into four lines.</param>
	public EdgeVertices(Vector3 corner1, Vector3 corner2, float outerStep){
		v1 = corner1;
		v2 = Vector3.Lerp(corner1, corner2, outerStep);
		v3 = Vector3.Lerp (corner1, corner2, 0.5f);
		v4 = Vector3.Lerp(corner1, corner2, 1 - outerStep);
		v5 = corner2;
	}

	public static EdgeVertices TerraceLerp (EdgeVertices a, EdgeVertices b, int step){
		EdgeVertices result;
		result.v1 = HexMetrics.TerraceLerp(a.v1, b.v1, step);
		result.v2 = HexMetrics.TerraceLerp(a.v2, b.v2, step);
		result.v3 = HexMetrics.TerraceLerp (a.v3, b.v3, step);
		result.v4 = HexMetrics.TerraceLerp(a.v4, b.v4, step);
		result.v5 = HexMetrics.TerraceLerp(a.v5, b.v5, step);
		return result;
	}
}


