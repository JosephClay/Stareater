﻿using Stareater.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stareater.GLData.OrbitShader
{
	class ArcBorderBuilder
	{
		private readonly List<Circle> wholeCircles = new List<Circle>();
		private readonly Dictionary<Circle, Queue<ArcPoint>> arcPoints = new Dictionary<Circle, Queue<ArcPoint>>();

		public void AddCircles(IEnumerable<Circle> circles)
		{
			foreach (var circle in circles)
			{
				var arcs = new List<Arc>();
				bool enclosed = false;

				foreach (var other in circles.Where(x => x != circle))
				{
					var dist = (float)(circle.Center - other.Center).Length;

					if (dist > circle.Radius + other.Radius)
						continue;
					if (dist + circle.Radius < other.Radius)
					{
						enclosed = true;
						break;
					}

					var facing = (circle.Center - other.Center).Unit;

					arcs.Add(new Arc(
						circle,
						facing,
						-(dist * dist - other.Radius * other.Radius + circle.Radius * circle.Radius) / 2 / dist
					));
				}

				if (!enclosed && !arcs.Any())
					this.wholeCircles.Add(new Circle(circle.Center, circle.Radius));

				if (!enclosed && arcs.Any())
					arcPoints[circle] = arcsToPoints(arcs);
			}
		}

		private static Queue<ArcPoint> arcsToPoints(List<Arc> arcs)
		{
			var points = new LinkedList<ArcPoint>(arcs.
				SelectMany(x => x.Points).
				OrderBy(x => x.Angle).
				ThenBy(x => x.RightEnd)
			);

			//Find the start of an arc
			var originalFirst = points.First.Value;
			while (!points.First.Value.RightEnd || points.Last.Value.RightEnd)
			{
				//Rotate the list
				var first = points.First.Value;
				points.RemoveFirst();
				points.AddLast(first);

				//Check if rotation went full circle, if so then
				//all arcs are overlapping in the exclusive manner
				if (points.First.Value == originalFirst)
					return new Queue<ArcPoint>();
			}

			//Remove excluded points from overlapping arcs
			for (var current = points.First; current != points.Last; /* no step */)
				if (current.Value.RightEnd == current.Next.Value.RightEnd)
				{
					if (current.Value.RightEnd)
					{
						current = current.Next;
						points.Remove(current.Previous);
					}
					else
						points.Remove(current.Next);
				}
				else
					current = current.Next;

			return new Queue<ArcPoint>(points);
		}

		public int Count
		{
			get { return this.arcPoints.Count + this.wholeCircles.Count; }
		}

		public IEnumerable<ArcVertices> Vertices()
		{
			foreach(var circle in this.wholeCircles)
				yield return new ArcVertices(
					OrbitHelpers.Quad((float)circle.Radius),
					circle.Center, (float)circle.Radius
				);

			foreach (var circle in this.arcPoints)
			{
				var data = new List<float>();

				while (circle.Value.Any())
				{
					var rightPoint = circle.Value.Dequeue();
					var leftPoint = circle.Value.Dequeue();

					if (rightPoint.Parent == leftPoint.Parent)
						singleArcVertices(rightPoint.Parent, data);
					else
					{
						var facing = (rightPoint.Point + leftPoint.Point).Unit;

						if (rightPoint.Point == -leftPoint.Point)
							facing = rightPoint.Point.PerpendicularLeft.Unit;
						else if (facing.Cross(rightPoint.Point) > 0)
							facing = -facing;

						multipleArcVertices(rightPoint, facing, data);
						multipleArcVertices(leftPoint, facing, data);
					}
				}

				yield return new ArcVertices(data, circle.Key.Center, (float)circle.Key.Radius);
			}
		}

		private void singleArcVertices(Arc arc, List<float> outputData)
		{
			var left = arc.Facing * arc.Parent.Radius;
			var right = arc.Facing * arc.Openness;
			var top = arc.Facing.PerpendicularRight * arc.Parent.Radius;

			outputData.AddRange(orbitVertex(left + top));
			outputData.AddRange(orbitVertex(right + top));
			outputData.AddRange(orbitVertex(right - top));

			outputData.AddRange(orbitVertex(right - top));
			outputData.AddRange(orbitVertex(left - top));
			outputData.AddRange(orbitVertex(left + top));
		}

		private void multipleArcVertices(ArcPoint point, Vector2D facing, List<float> outputData)
		{
			var circle = point.Parent.Parent;
			var pointFacing = point.Parent.Facing;
			var left = pointFacing * circle.Radius;
			var right = pointFacing * point.Parent.Openness;
			var top = point.Parent.Parent.Radius * (point.RightEnd ? 
				pointFacing.PerpendicularRight : 
				pointFacing.PerpendicularLeft);

			var endShape = new LinkedList<Vector2D>(new[]{
				left,
				left + top,
				right + top,
				right
			});

			var facingNormal = point.RightEnd ? facing.PerpendicularRight : facing.PerpendicularLeft;
			for (var current = endShape.First; current != null; /* no step */)
			{
				var next = current.Next ?? endShape.First;
				if ((current.Value.Dot(facingNormal) < 0) != (next.Value.Dot(facingNormal) < 0))
				{
					var line = next.Value - current.Value;
					var height = current.Value.Dot(facingNormal);
					var speed = -height / line.Dot(facingNormal);
					endShape.AddAfter(current, current.Value + line * speed);
				}

				if (current.Value.Dot(facingNormal) < 0)
				{
					var realNext = current.Next;
					endShape.Remove(current);
					current = realNext;
				}
				else
					current = current.Next;
			}

			var triangles = new List<Vector2D[]>();
			for (var current = endShape.First.Next.Next; current != null; current = current.Next)
				triangles.Add(makeTriangle(endShape.First.Value, current.Previous.Value, current.Value));

			var facingSide = facing.Cross(pointFacing);
			if (point.RightEnd && facingSide < 0 || !point.RightEnd && facingSide > 0)
				triangles.Add(makeTriangle(new Vector2D(0, 0), facing * 1.5f, pointFacing * 1.5f));

			outputData.AddRange(triangles.SelectMany(x => x).SelectMany(x => orbitVertex(x)));
		}

		private static IEnumerable<float> orbitVertex(Vector2D p)
		{
			yield return (float)p.X;
			yield return (float)p.Y;
			yield return (float)p.X;
			yield return (float)p.Y;
		}

		private Vector2D[] makeTriangle(Vector2D point1, Vector2D point2, Vector2D point3)
		{
			var triangle = new[] { point1, point2, point3 };

			var edge1 = triangle[1] - triangle[0];
			var edge2 = triangle[2] - triangle[0];
			if (edge1.Cross(edge2) > 0)
			{
				var temp = triangle[1];
				triangle[1] = triangle[2];
				triangle[2] = temp;
			}

			return triangle;
		}

		class Arc
		{
			public Circle Parent { get; private set; }
			public Vector2D Facing { get; private set; }
			public double Openness { get; private set; }

			public Arc(Circle parent, Vector2D facing, double openness)
			{
				this.Parent = parent;
				this.Facing = facing;
				this.Openness = openness;
			}

			public float OpenningHeight
			{
				get
				{
					return (float)Math.Sqrt(this.Parent.Radius * this.Parent.Radius - this.Openness * this.Openness);
				}
			}

			public IEnumerable<ArcPoint> Points
			{
				get
				{
					yield return new ArcPoint(this, this.Facing * this.Openness + this.Facing.PerpendicularRight * this.OpenningHeight, true);
					yield return new ArcPoint(this, this.Facing * this.Openness + this.Facing.PerpendicularLeft * this.OpenningHeight, false);
				}
			}
		}

		class ArcPoint
		{
			public Arc Parent { get; private set; }
			public Vector2D Point { get; private set; }
			public bool RightEnd { get; private set; }

			public double Angle { get; private set; }

			public ArcPoint(Arc parent, Vector2D point, bool rightEnd)
			{
				this.Parent = parent;
				this.Point = point;
				this.RightEnd = rightEnd;

				var direction = new Vector2D(this.Point.X, this.Point.Y).Unit;
				this.Angle = (direction.Y > 0) ? 1 - direction.X : 3 + direction.X;
			}
		}
	}
}