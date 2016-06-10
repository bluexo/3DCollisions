﻿using System;
using Math_Implementation;
using CollisionDetectionSelector.Primitive;

namespace CollisionDetectionSelector.Collisions {
    class LineCollisions {
        public static float Clamp (float value, float max, float min) {
            float result = value;
            if (result > max) {
                result = max;
            }
            else if (result < min) {
                result = min;
            }
            return result;
        }
        public static bool PointOnLine(Point p,Line line) {
            float m = (line.End.Y - line.Start.Y) / (line.End.X - line.Start.X);//rise over run
            float b = line.Start.Y - m * line.Start.X;

            //at this point, evaluation equation is:
            //return point.y==m*p.x+b
            //wont work because we use floats
            //floating error can be accumulated, episilon testing
            if (Math.Abs(p.Y-(m*p.X+b)) < 0.0001f) {
                return true;
            }
            return false;
        }
        public static Point ClosestPoint(Line ab,Point c,out float t) {
            Vector3 a = new Vector3(ab.Start.X, ab.Start.Y, ab.Start.Z);
            Vector3 b = new Vector3(ab.End.X, ab.End.Y, ab.End.Z);

            //project c onto b, then find the
            //parametrized position d(t) = a+t*(b-a)
            //t = Dot(c - a, ab) / Dot(ab, ab);
            t = Vector3.Dot((c.ToVector() - a), ab.ToVector()) / Vector3.Dot(ab.ToVector(),ab.ToVector());
           
            //clamp t to 0-1 range
            //if t is outside range, it is outside line
            t = Clamp(t, 1f, 0f);

            //compute project position from clamped t
            Point distance = new Point(a + (ab.ToVector()*t));

            //return result
            return distance;
        }
        public static Point ClosestPoint(Line ab,Point c) {
            float t = 0f;
            return ClosestPoint(ab, c, out t);
        }
    }
}
