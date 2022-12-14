 using UnityEngine;
using System.Collections;
using System.Linq;
using MIConvexHull;

/// <summary>
/// A vertex is a simple class that stores the postion of a point, node or vertex.
/// </summary>
public class Cell2 : TriangulationCell<Vertex2, Cell2>
{
	Vector2? circumCenter, centroid;
	
	public Vector2 Circumcenter {
		get {
			circumCenter = circumCenter ?? GetCircumcenter();
			return circumCenter.Value;
		}
	}
	
	public Vector2 Centroid {
		get {
            centroid = centroid ?? GetCentroid();
			return centroid.Value;
		}
	}

    public float Area {
        get {
            var a = Vertices[0].Position;
            var b = Vertices[1].Position;
            var c = Vertices[2].Position;
            return 0.5f * Mathf.Abs((float)(a[0] * b[1] + b[0] * c[1] + c[0] * a[1] - a[0] * c[1] - c[0] * b[1] - b[0] * a[1]));
        }
    }

	public Cell2 ()
	{

	}

	double LengthSquared (double[] v)
	{
		double norm = 0;
		for (int i = 0; i < v.Length; i++) {
			var t = v [i];
			norm += t * t;
		}
		return norm;
	}

	double Determinant(double[,] m)
	{
		double fCofactor00 = m[1,1] * m[2,2] - m[1,2] * m[2,1];
		double fCofactor10 = m[1,2] * m[2,0] - m[1,0] * m[2,2];
		double fCofactor20 = m[1,0] * m[2,1] - m[1,1] * m[2,0];
		
		double fDet = m[0,0] * fCofactor00 + m[0,1] * fCofactor10 + m[0,2] * fCofactor20;
		
		return fDet;
	}
	
	Vector2 GetCircumcenter ()
	{
		// From MathWorld: http://mathworld.wolfram.com/Circumcircle.html
	
		var points = Vertices;
	
		double[,] m = new double[3, 3];
	
		// x, y, 1
		for (int i = 0; i < 3; i++) {
			m[i, 0] = points[i].x;
			m[i, 1] = points[i].y;
			m[i, 2] = 1;
		}
		var a = Determinant(m);
	
		// size, y, 1
		for (int i = 0; i < 3; i++) {
			m[i, 0] = LengthSquared (points[i].Position);
		}
		var dx = -Determinant(m);
	
		// size, x, 1
		for (int i = 0; i < 3; i++) {
			m[i, 1] = points[i].x;
		}
		var dy = Determinant(m);
	
		// size, x, y
		//for (int i = 0; i < 3; i++) {
		//	m[i, 2] = points[i].y;
		//}
		//var c = -Det(m);
	
		var s = -1.0 / (2.0 * a);
		//var r = System.Math.Abs(s) * System.Math.Sqrt(dx * dx + dy * dy - 4 * a * c);

		return new Vector2((float)(s * dx), (float)(s * dy));
	}
	
	Vector2 GetCentroid ()
	{
        return new Vector2((float)Vertices.Average(v => v.Position[0]), (float)Vertices.Average(v => v.Position[1]));
	}    

}















