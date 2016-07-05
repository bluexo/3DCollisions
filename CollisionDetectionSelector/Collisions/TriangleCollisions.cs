﻿using System;
using Math_Implementation;
using CollisionDetectionSelector.Primitive;

namespace CollisionDetectionSelector.Collisions {
    class TriangleCollisions {
        //Same Side Technique
        public static bool PointInTriangle(Point _p, Triangle t) {
            //locals for easier math
            Vector3 p = new Vector3(_p.X, _p.Y, _p.Z);
            Vector3 a = new Vector3(t.p0.X, t.p0.Y, t.p0.Z);
            Vector3 b = new Vector3(t.p1.X, t.p1.Y, t.p1.Z);
            Vector3 c = new Vector3(t.p2.X, t.p2.Y, t.p2.Z);

            //transform unknown point into triangles origin
            a -= p;
            b -= p;
            c -= p;

            // The point should be moved too, so they are both
            // relative, but because we don't use p in the
            // equation anymore, we don't need it!
            // p -= p;

            //compute normal vectors for triangles
            //u = normal of PBC
            //v = normal of PCA
            //w = normal of PAB
            Vector3 u = Vector3.Cross(b, c);
            Vector3 v = Vector3.Cross(c, a);
            Vector3 w = Vector3.Cross(a, b);

            //test to see if normals are facing same direction
            if (Vector3.Dot(u, v) < 0f) {
                return false;
            }
            if (Vector3.Dot(u, w) < 0.0f) {
                return false;
            }

            //normals face same way
            return true;
        }
        public static bool PointInTriangle(Triangle t,Point p) {
            //utility
            return PointInTriangle(p, t);
        }
        
        public static Point ClosestPointTriangle(Point _p,Triangle t) {
            Point point = new Point(_p.X, _p.Y, _p.Z);
            //create plane out fo triangle
            Plane plane = new Plane(t.p0, t.p1, t.p2);

            //get closest point to plane
            point = Collisions.PlaneCollision.ClosestPoint(plane, point);

            if (PointInTriangle(t, point)) {
                //if point is in triangle, return it
                return new Point(point.X, point.Y, point.Z);
            }

            //break triangle down into Line components
            Line AB = new Line(t.p0, t.p1);
            Line BC = new Line(t.p1, t.p2);
            Line CA = new Line(t.p2, t.p0);

            //find closest point to each line
            Point c1 = Collisions.LineCollisions.ClosestPoint(AB, point);
            Point c2 = Collisions.LineCollisions.ClosestPoint(BC, point);
            Point c3 = Collisions.LineCollisions.ClosestPoint(CA, point);

            //mag is magnitudeSquared. Magnitude to unknown point
            float mag1 = (point.ToVector() - c1.ToVector()).LengthSquared();
            float mag2 = (point.ToVector() - c2.ToVector()).LengthSquared();
            float mag3 = (point.ToVector() - c3.ToVector()).LengthSquared();

            //find smallest magnitude(shortest distance)
            float min = System.Math.Min(mag1, mag2);
            min = System.Math.Min(min, mag3);

            if (min== mag1) {
                return c1;
            }
            else if (min == mag2) {
                return c2;
            }
            return c3;
        }
        public static Point ClosestPointTriangle(Triangle t, Point p) {
            return ClosestPointTriangle(p, t);
        }
        public static bool SphereIntersect(Triangle triangle, Sphere sphere) {
            //get closest point on triangle to center of sphere
            Point p = ClosestPointTriangle(sphere.Position, triangle);

            //check distanceSq between center and point on triangle
            float distSq = Vector3.LengthSquared(sphere.vPosition - p.ToVector());

            //if distance is < r2 then there is a collision
            if (distSq < (sphere.Radius * sphere.Radius)) {
                return true;
            }

            return false;
        }
        public static bool SphereIntersect(Sphere sphere, Triangle triangle) {
            return SphereIntersect(triangle, sphere);
        }
        public static bool AABBIntersect(Triangle triangle, AABB aabb) {
            //get triangle corners as vectors
            /*
            Vector3 v0 = triangle.p0.ToVector();
            Vector3 v1 = triangle.p1.ToVector();
            Vector3 v2 = triangle.p2.ToVector();
            */
            Vector3[] v = new Vector3[3] { triangle.p0.ToVector(),
                                          triangle.p1.ToVector(),
                                          triangle.p2.ToVector() };
            //convert aabb to center-extents
            Vector3 center = aabb.Center.ToVector();
            Vector3 extent = aabb.Extents;

            //translate triangle so aabb is center of world
            /*
            v0 -= center;
            v1 -= center;
            v2 -= center;
            */
            for (int i = 0; i < 3; i++) {
                v[i] -= center;
            }
            /*
            //get edge vectors of triangle ABC
            Vector3 f0 = v1 - v0;//b-a
            Vector3 f1 = v2 - v1;//c-b
            Vector3 f2 = v0 - v2;//a-c
            */
            //find face normals of aabb (normals are xyz axis
            
            Vector3 u0 = new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 u1 = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 u2 = new Vector3(0.0f, 0.0f, 1.0f);

            // Compute the 9 axis
            /*
            //recreated as axis staggered array below
            Vector3 axis_u0_f0 = Vector3.Cross(u0, f0);
            Vector3 axis_u0_f1 = Vector3.Cross(u0, f1);
            Vector3 axis_u0_f2 = Vector3.Cross(u0, f2);

            Vector3 axis_u1_f0 = Vector3.Cross(u1, f0);
            Vector3 axis_u1_f1 = Vector3.Cross(u1, f1);
            Vector3 axis_u1_f2 = Vector3.Cross(u2, f2);

            Vector3 axis_u2_f0 = Vector3.Cross(u2, f0);
            Vector3 axis_u2_f1 = Vector3.Cross(u2, f1);
            Vector3 axis_u2_f2 = Vector3.Cross(u2, f2);
            */

            Vector3[] f = new Vector3[3] { v1 - v0/*A-B */, v2 - v1/*B-C */, v0 - v2 /*A-C */};
            Vector3[] u = new Vector3[3] { u0, u1, u2 };

            Vector3[][] axis = new Vector3[3][];
            for (int i = 0; i < 3; i++) {
                axis[i] = new Vector3[3];
            }
            //create all possible axis
            //u=face normals of AABB
            //f = edge vectors of triangle ABC
            for (int _u = 0; _u < 3; _u++) {
                for(int _f = 0; _f < 3; _f++) {
                    axis[_u][_f] = Vector3.Cross(u[_u], f[_f]);
                }
            }
            //Test SAT
        }
        protected bool TriangleSat(Vector3 v0,Vector3 v1, Vector3 v2, Vector3 u0,Vector3 u1,Vector3 u2,Vector3 extents,Vector3 axii0,Vector3 axii1,Vector3 axii2) {
            // Project all 3 vertices of the triangle onto the Seperating axis
            float p0 = Vector3.Dot(v0, axii0);
            float p1 = Vector3.Dot(v1, axii1);
            float p2 = Vector3.Dot(v2, axii2);
            // Project the AABB onto the seperating axis
            // We don't care about the end points of the prjection
            // just the length of the half-size of the AABB
            // That is, we're only casting the extents onto the 
            // seperating axis, not the AABB center. We don't
            // need to cast the center, because we know that the
            // aabb is at origin compared to the triangle!
            float r = extents.X * Math.Abs(Vector3.Dot(u0, axii0)) +
                        extents.Y * Math.Abs(Vector3.Dot(u1, axii0)) +
                        extents.Z * Math.Abs(Vector3.Dot(u2, axii0));
            // Now do the actual test, basically see if either of
            // the most extreme of the triangle points intersects r
            // You might need to write Min & Max functions that take 3 arguments
            if (System.Math.Max(-(System.Math.Max(System.Math.Max(p0, p1), p2)), System.Math.Min(System.Math.Min(p0, p1), p2)) > r) {
                // This means BOTH of the points of the projected triangle
                // are outside the projected half-length of the AABB
                // Therefore the axis is seperating and we can exit
                return false;
            }
            return true;
        }
    }
}
