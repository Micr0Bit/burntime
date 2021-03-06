﻿/*
 *  Burntime Platform
 *  Copyright (C) 2009
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *  authors: 
 *    Juernjakob Harder (yn.harada@gmail.com)
 * 
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Burntime.Platform
{
    [Serializable]
    public struct Vector2
    {
        public static readonly Vector2 Zero = new Vector2(0, 0);
        public static readonly Vector2 One = new Vector2(1, 1);

        public int x, y;

        public override string ToString()
        {
            return x.ToString() + "x" + y.ToString();
        }

        //public Vector2()
        //{
        //    x = 0;
        //    y = 0;
        //}

        public Vector2(Vector2 v)
        {
            x = v.x;
            y = v.y;
        }

        public Vector2(int X, int Y)
        {
            x = X;
            y = Y;
        }

        public int Length
        {
            get { return (int)System.Math.Sqrt(x * x + y * y); }
        }

        public static bool operator > (Vector2 left, Vector2 right)
        {
            return left.x > right.x || left.y > right.y;
        }

        public static bool operator <(Vector2 left, Vector2 right)
        {
            return left.x < right.x || left.y < right.y;
        }

        public static Vector2 operator -(Vector2 left, Vector2 right)
        {
            return new Vector2(left.x - right.x, left.y - right.y);
        }

        public static Vector2 operator -(Vector2 left)
        {
            return new Vector2(-left.x, -left.y);
        }

        public static Vector2 operator -(Vector2 left, int right)
        {
            return new Vector2(left.x - right, left.y - right);
        }

        public static Vector2 operator +(Vector2 left, Vector2 right)
        {
            return new Vector2(left.x + right.x, left.y + right.y);
        }

        public static Vector2 operator +(Vector2 left, int right)
        {
            return new Vector2(left.x + right, left.y + right);
        }

        public static Vector2 operator /(Vector2 left, Vector2 right)
        {
            return new Vector2(left.x / right.x, left.y / right.y);
        }

        public static Vector2 operator /(Vector2 left, float right)
        {
            return new Vector2((int)(left.x / right), (int)(left.y / right));
        }

        public static Vector2 operator *(Vector2 left, float right)
        {
            return new Vector2((int)(left.x * right), (int)(left.y * right));
        }

        public static Vector2 operator *(Vector2 left, Vector2 right)
        {
            return new Vector2(left.x * right.x, left.y * right.y);
        }

        public static Vector2 operator %(Vector2 left, Vector2 right)
        {
            return new Vector2(left.x % right.x, left.y % right.y);
        }

        public static bool operator ==(Vector2 left, Vector2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2 left, Vector2 right)
        {
            return !left.Equals(right);
        }

        public static implicit operator Vector2f(Vector2 left)
        {
            return new Vector2f(left.x, left.y);
        }

        public static implicit operator Rect(Vector2 left)
        {
            return new Rect(Vector2.Zero, left);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2)
            {
                Vector2 v = (Vector2)obj;
                return x == v.x && y == v.y;
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode();
        }

        public void ThresholdLT(int threshold)
        {
            if (x < threshold) x = threshold;
            if (y < threshold) y = threshold;
        }

        public void ThresholdLT(Vector2 threshold)
        {
            if (x < threshold.x) x = threshold.x;
            if (y < threshold.y) y = threshold.y;
        }

        public void ThresholdGT(int threshold)
        {
            if (x > threshold) x = threshold;
            if (y > threshold) y = threshold;
        }

        public void ThresholdGT(Vector2 threshold)
        {
            if (x > threshold.x) x = threshold.x;
            if (y > threshold.y) y = threshold.y;
        }

        public int GetIndex(Vector2 borders)
        {
            return x + y * borders.x;
        }

        public int Count
        {
            get { return x * y; }
        }
    }

    [Serializable]
    public struct Vector2f
    {
        public static readonly Vector2f Zero = new Vector2f(0, 0);

        public float x, y;

        //public Vector2f()
        //{
        //    x = 0;
        //    y = 0;
        //}

        public Vector2f(Vector2f v)
        {
            x = v.x;
            y = v.y;
        }

        public Vector2f(float X, float Y)
        {
            x = X;
            y = Y;
        }

        public float Length
        {
            get { return (float)System.Math.Sqrt(x * x + y * y); }
        }

        public static Vector2f operator -(Vector2f left, Vector2f right)
        {
            return new Vector2f(left.x - right.x, left.y - right.y);
        }

        public static Vector2f operator -(Vector2f left)
        {
            return new Vector2f(-left.x, -left.y);
        }

        public static Vector2f operator -(Vector2f left, float right)
        {
            return new Vector2f(left.x - right, left.y - right);
        }

        public static Vector2f operator +(Vector2f left, Vector2f right)
        {
            return new Vector2f(left.x + right.x, left.y + right.y);
        }

        public static Vector2f operator +(Vector2f left, float right)
        {
            return new Vector2f(left.x + right, left.y + right);
        }

        public static Vector2f operator /(Vector2f left, Vector2f right)
        {
            return new Vector2f(left.x / right.x, left.y / right.y);
        }

        public static Vector2f operator /(Vector2f left, float right)
        {
            return new Vector2f((left.x / right), (left.y / right));
        }

        public static Vector2f operator *(Vector2f left, float right)
        {
            return new Vector2f((left.x * right), (left.y * right));
        }

        public static Vector2f operator *(Vector2f left, Vector2f right)
        {
            return new Vector2f(left.x * right.x, left.y * right.y);
        }

        public static Vector2f operator %(Vector2f left, Vector2f right)
        {
            return new Vector2f(left.x % right.x, left.y % right.y);
        }

        public static bool operator ==(Vector2f left, Vector2f right)
        {
            return left.x == right.x && left.y == right.y;
        }

        public static bool operator !=(Vector2f left, Vector2f right)
        {
            //if (!(left is Vector2f && right is Vector2f))
            //    return false;

            return left.x != right.x || left.y != right.y;
        }

        public static implicit operator Vector2 (Vector2f left)
        {
            return new Vector2((int)(left.x + 0.5f), (int)(left.y + 0.5f));
        }

        public override bool Equals(object obj)
        {
            Vector2f v = (Vector2f)obj;
            return x == v.x && y == v.y;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode();
        }

        public void ThresholdLT(float threshold)
        {
            if (x < threshold) x = threshold;
            if (y < threshold) y = threshold;
        }

        public void ThresholdLT(Vector2f threshold)
        {
            if (x < threshold.x) x = threshold.x;
            if (y < threshold.y) y = threshold.y;
        }

        public void ThresholdGT(float threshold)
        {
            if (x > threshold) x = threshold;
            if (y > threshold) y = threshold;
        }

        public void ThresholdGT(Vector2f threshold)
        {
            if (x > threshold.x) x = threshold.x;
            if (y > threshold.y) y = threshold.y;
        }

        public void Normalize()
        {
            float l = Length;
            if (l == 0)
            {
                x = 0;
                y = 0;
            }
            else
            {
                x /= l;
                y /= l;
            }
        }
    }

}
