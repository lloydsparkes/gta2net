﻿//Created: 23.02.2010
//Source: http://www.codeproject.com/KB/GDI-plus/PolygonCollision.aspx --> ported to XNA

using System;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Helper
{
    public class SeparatingAxisTheorem
    {
        // Structure that stores the results of the PolygonCollision function
        public struct PolygonCollisionResult
        {
            public bool WillIntersect; // Are the polygons going to intersect forward in time?
            public bool Intersect; // Are the polygons currently intersecting
            public Vector2 MinimumTranslationVector; // The translation to apply to polygon A to push the polygons appart.
            //public int? CollidingEdgeIndexA; //Shows witch edge of polygon A is colliding.
            //public int? CollidingEdgeIndexB; //Shows witch edge of polygon A is colliding.
        }

        // Check if polygon A is going to collide with polygon B for the given velocity
        public static PolygonCollisionResult PolygonCollision(ref Polygon polygonA, ref Polygon polygonB, ref Vector2 velocity)
        {
            polygonA.BuildEdges();
            polygonB.BuildEdges();
            PolygonCollisionResult result = new PolygonCollisionResult();
            result.Intersect = true;
            result.WillIntersect = true;

            int edgeCountA = polygonA.Edges.Count;
            int edgeCountB = polygonB.Edges.Count;
            float minIntervalDistance = float.PositiveInfinity;
            Vector2 translationAxis = new Vector2();
            Vector2 edge;


            // Loop through all the edges of both polygons
            for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                if (edgeIndex < edgeCountA)
                {
                    edge = polygonA.Edges[edgeIndex];
                }
                else
                {
                    edge = polygonB.Edges[edgeIndex - edgeCountA];
                }

                // ===== 1. Find if the polygons are currently intersecting =====

                // Find the axis perpendicular to the current edge
                Vector2 axis = new Vector2(-edge.Y, edge.X);
                axis.Normalize();

                // Find the projection of the polygon on the current axis
                float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
                ProjectPolygon(ref axis, ref polygonA, ref minA, ref maxA);
                ProjectPolygon(ref axis, ref polygonB, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting
                if (IntervalDistance(ref minA, ref maxA, ref minB, ref maxB) > 0)
                    result.Intersect = false;

                // ===== 2. Now find if the polygons *will* intersect =====

                // Project the velocity on the current axis
                //float velocityProjection = axis.DotProduct(velocity); //Backup
                float velocityProjection = Vector2.Dot(axis,velocity);

                // Get the projection of polygon A during the movement
                if (velocityProjection < 0)
                {
                    minA += velocityProjection;
                }
                else
                {
                    maxA += velocityProjection;
                }

                // Do the same test as above for the new projection
                float intervalDistance = IntervalDistance(ref minA, ref maxA, ref minB, ref maxB);
                if (intervalDistance > 0) result.WillIntersect = false;

                // If the polygons are not intersecting and won't intersect, exit the loop
                if (!result.Intersect && !result.WillIntersect) break;


                // Check if the current interval distance is the minimum one. If so store
                // the interval distance and the current distance.
                // This will be used to calculate the minimum translation vector
                intervalDistance = Math.Abs(intervalDistance);
                if (intervalDistance < minIntervalDistance)
                {
                    minIntervalDistance = intervalDistance;
                    translationAxis = axis;

                    Vector2 d = polygonA.Center - polygonB.Center;
                    if (Vector2.Dot(d, translationAxis) < 0)
                        translationAxis = -translationAxis;
                    //if (d.DotProduct(translationAxis) < 0) translationAxis = -translationAxis; //Backup
                }
            }

            // The minimum translation vector can be used to push the polygons appart.
            // First moves the polygons by their velocity
            // then move polygonA by MinimumTranslationVector.
            if (result.WillIntersect) result.MinimumTranslationVector = translationAxis * minIntervalDistance;

            return result;
        }

        // Calculate the distance between [minA, maxA] and [minB, maxB]
        // The distance will be negative if the intervals overlap
        private static float IntervalDistance(ref float minA, ref float maxA, ref float minB, ref float maxB)
        {
            if (minA < minB)
            {
                return minB - maxA;
            }
            else
            {
                return minA - maxB;
            }
        }

        // Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
        private static void ProjectPolygon(ref Vector2 axis, ref Polygon polygon, ref float min, ref float max)
        {
            // To project a point on an axis use the dot product
            //float d = axis.DotProduct(polygon.Points[0]); //Backup
            float d = Vector2.Dot(axis, polygon.Points[0]);
            min = d;
            max = d;
            for (int i = 0; i < polygon.Points.Count; i++)
            {
                //d = polygon.Points[i].DotProduct(axis); //Backup
                d = Vector2.Dot(polygon.Points[i], (axis));
                if (d < min)
                {
                    min = d;
                }
                else
                {
                    if (d > max)
                    {
                        max = d;
                    }
                }
            }
        }
    }
}