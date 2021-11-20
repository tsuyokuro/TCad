using OpenTK;
using System;
using CadDataTypes;

namespace Plotter
{
    public struct CadQuaternion
    {
        public double t;
        public double x;
        public double y;
        public double z;

        public CadQuaternion(double t, double x, double y, double z)
        {
            this.t = t;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /**
         * ノルム(長さ)
         * 
         */
        public double norm()
        {
            return Math.Sqrt((t * t) + (x * x) + (y * y) + (z * z));
        }

        /**
         * 共役四元数を返す
         * 
         * 
         */
        public CadQuaternion Conjugate()
        {
            CadQuaternion q = this;

            q.t = t;
            q.x = -x;
            q.y = -y;
            q.z = -z;

            return q;
        }

        /**
         * 掛け算
         * 
         * 
         */
        public static CadQuaternion operator *(CadQuaternion q, CadQuaternion r)
        {
            return Product(q, r);
        }

        /**
         * 和を求める
         * 
         */
        public static CadQuaternion operator +(CadQuaternion q, CadQuaternion r)
        {
            CadQuaternion res;

            res.t = q.t + r.t;
            res.x = q.x + r.x;
            res.y = q.y + r.y;
            res.z = q.z + r.z;

            return res;
        }

        /**
         * 四元数の積を求める
         * q * r
         * 
         */
        public static CadQuaternion Product(CadQuaternion q, CadQuaternion r)
        {
            // A = (a; U)
            // B = (b; V)
            // AB = (ab - U・V; aV + bU + U×V)
            CadQuaternion ans;
            double d1, d2, d3, d4;

            d1 = q.t * r.t;
            d2 = q.x * r.x;
            d3 = q.y * r.y;
            d4 = q.z * r.z;
            ans.t = d1 - d2 - d3 - d4;

            d1 = q.t * r.x;
            d2 = r.t * q.x;
            d3 = q.y * r.z;
            d4 = -q.z * r.y;
            ans.x = d1 + d2 + d3 + d4;

            d1 = q.t * r.y;
            d2 = r.t * q.y;
            d3 = q.z * r.x;
            d4 = -q.x * r.z;
            ans.y = d1 + d2 + d3 + d4;

            d1 = q.t * r.z;
            d2 = r.t * q.z;
            d3 = q.x * r.y;
            d4 = -q.y * r.x;
            ans.z = d1 + d2 + d3 + d4;

            return ans;
        }

        /**
         * 行列に変換する
         * 
         */
        public Matrix4d ToMatrix4d()
        {
            return ToMatrix4d(this);
        }

        public static Matrix4d ToMatrix4d(CadQuaternion q)
        {
            Matrix4d m = default(Matrix4d);

            double xx = q.x * q.x * 2.0;
            double yy = q.y * q.y * 2.0;
            double zz = q.z * q.z * 2.0;
            double xy = q.x * q.y * 2.0;
            double yz = q.y * q.z * 2.0;
            double zx = q.z * q.x * 2.0;
            double xw = q.x * q.t * 2.0;
            double yw = q.y * q.t * 2.0;
            double zw = q.z * q.t * 2.0;


            // 1.0 - yy - zz, xy + zw,       zx - yw,       0.0
            // xy - zw,       1.0 - zz - xx, yz + xw,       0.0
            // zx + yw,       yz - xw,       1.0 - xx - yy, 0.0
            // 0.0,           0.0,           0.0,           1.0

            m.Row0[0] = 1.0 - yy - zz;
            m.Row0[1] = xy + zw;
            m.Row0[2] = zx - yw;
            m.Row0[3] = 0.0;

            m.Row1[0] = xy - zw;
            m.Row1[1] = 1.0 - zz - xx;
            m.Row1[2] = yz + xw;
            m.Row1[3] = 0.0;

            m.Row2[0] = zx + yw;
            m.Row2[1] = yz - xw;
            m.Row2[2] = 1.0 - xx - yy;
            m.Row2[3] = 0.0;

            m.Row3[0] = 0.0;
            m.Row3[1] = 0.0;
            m.Row3[2] = 0.0;
            m.Row3[3] = 1.0;

            return m;
        }

        /**
         * 単位元を作成
         * 
         */
        public static CadQuaternion Unit()
        {
            CadQuaternion res;

            res.t = 1.0;
            res.x = 0;
            res.y = 0;
            res.z = 0;

            return res;
        }

        /**
         * Vector (vx, vy, vz)を回転軸としてradianだけ回転する四元数を作成
         * 
         */
        public static CadQuaternion RotateQuaternion(double vx, double vy, double vz, double radian)
        {
            CadQuaternion ans = default(CadQuaternion);
            double norm;
            double c, s;

            norm = vx * vx + vy * vy + vz * vz;
            if (norm <= 0.0) return ans;

            norm = 1.0 / Math.Sqrt(norm);
            vx *= norm;
            vy *= norm;
            vz *= norm;

            c = Math.Cos(0.5 * radian);
            s = Math.Sin(0.5 * radian);

            ans.t = c;
            ans.x = s * vx;
            ans.y = s * vy;
            ans.z = s * vz;

            return ans;
        }

        /**
         * Vector (v.x, v.y, v.z)を回転軸としてradianだけ回転する四元数を作成
         * 
         */
        public static CadQuaternion RotateQuaternion(Vector3d axis, double radian)
        {
            axis = axis.Normalized();

            CadQuaternion ans = default;
            double c, s;

            c = Math.Cos(0.5 * radian);
            s = Math.Sin(0.5 * radian);

            ans.t = c;
            ans.x = s * axis.X;
            ans.y = s * axis.Y;
            ans.z = s * axis.Z;

            return ans;
        }

        /**
         * CadPoint - 四元数 変換
         * 
         */
        public static CadQuaternion FromPoint(Vector3d point)
        {
            CadQuaternion q;
            q.t = 0.0;
            q.x = point.X;
            q.y = point.Y;
            q.z = point.Z;

            return q;
        }

        public static Vector3d ToPoint(CadQuaternion q)
        {
            return new Vector3d(q.x, q.y, q.z);
        }

        public Vector3d ToPoint()
        {
            Vector3d p = default;

            p.X = x;
            p.Y = y;
            p.Z = z;

            return p;
        }

        /**
         * Vector3d - 四元数 変換
         * 
         */
        public static CadQuaternion FromVector(Vector3d v)
        {
            CadQuaternion q;
            q.t = 0.0;
            q.x = v.X;
            q.y = v.Y;
            q.z = v.Z;

            return q;
        }

        public static Vector3d ToVector3d(CadQuaternion q)
        {
            return new Vector3d(q.x, q.y, q.z);
        }

        public Vector3d ToVector3d()
        {
            return new Vector3d(x, y, z);
        }
    }
}
