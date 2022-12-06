using OpenTK.Mathematics;
using System;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace Plotter;

public struct CadQuaternion
{
    public vcompo_t t;
    public vcompo_t x;
    public vcompo_t y;
    public vcompo_t z;

    public CadQuaternion(vcompo_t t, vcompo_t x, vcompo_t y, vcompo_t z)
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
    public vcompo_t norm()
    {
        return (vcompo_t)Math.Sqrt((t * t) + (x * x) + (y * y) + (z * z));
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
        vcompo_t d1, d2, d3, d4;

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
    public matrix4_t Tomatrix4_t()
    {
        return Tomatrix4_t(this);
    }

    public static matrix4_t Tomatrix4_t(CadQuaternion q)
    {
        matrix4_t m = default(matrix4_t);

        vcompo_t xx = q.x * q.x * (vcompo_t)(2.0);
        vcompo_t yy = q.y * q.y * (vcompo_t)(2.0);
        vcompo_t zz = q.z * q.z * (vcompo_t)(2.0);
        vcompo_t xy = q.x * q.y * (vcompo_t)(2.0);
        vcompo_t yz = q.y * q.z * (vcompo_t)(2.0);
        vcompo_t zx = q.z * q.x * (vcompo_t)(2.0);
        vcompo_t xw = q.x * q.t * (vcompo_t)(2.0);
        vcompo_t yw = q.y * q.t * (vcompo_t)(2.0);
        vcompo_t zw = q.z * q.t * (vcompo_t)(2.0);


        // (vcompo_t)(1.0) - yy - zz, xy + zw,       zx - yw,       (vcompo_t)(0.0)
        // xy - zw,       (vcompo_t)(1.0) - zz - xx, yz + xw,       (vcompo_t)(0.0)
        // zx + yw,       yz - xw,       (vcompo_t)(1.0) - xx - yy, (vcompo_t)(0.0)
        // (vcompo_t)(0.0),           (vcompo_t)(0.0),           (vcompo_t)(0.0),           (vcompo_t)(1.0)

        m.Row0[0] = (vcompo_t)(1.0) - yy - zz;
        m.Row0[1] = xy + zw;
        m.Row0[2] = zx - yw;
        m.Row0[3] = (vcompo_t)(0.0);

        m.Row1[0] = xy - zw;
        m.Row1[1] = (vcompo_t)(1.0) - zz - xx;
        m.Row1[2] = yz + xw;
        m.Row1[3] = (vcompo_t)(0.0);

        m.Row2[0] = zx + yw;
        m.Row2[1] = yz - xw;
        m.Row2[2] = (vcompo_t)(1.0) - xx - yy;
        m.Row2[3] = (vcompo_t)(0.0);

        m.Row3[0] = (vcompo_t)(0.0);
        m.Row3[1] = (vcompo_t)(0.0);
        m.Row3[2] = (vcompo_t)(0.0);
        m.Row3[3] = (vcompo_t)(1.0);

        return m;
    }

    /**
     * 単位元を作成
     * 
     */
    public static CadQuaternion Unit()
    {
        CadQuaternion res;

        res.t = (vcompo_t)(1.0);
        res.x = 0;
        res.y = 0;
        res.z = 0;

        return res;
    }

    /**
     * Vector (vx, vy, vz)を回転軸としてradianだけ回転する四元数を作成
     * 
     */
    public static CadQuaternion RotateQuaternion(vcompo_t vx, vcompo_t vy, vcompo_t vz, vcompo_t radian)
    {
        CadQuaternion ans = default(CadQuaternion);
        vcompo_t norm;
        vcompo_t c, s;

        norm = vx * vx + vy * vy + vz * vz;
        if (norm <= (vcompo_t)(0.0)) return ans;

        norm = (vcompo_t)(1.0) / (vcompo_t)Math.Sqrt(norm);
        vx *= norm;
        vy *= norm;
        vz *= norm;

        c = (vcompo_t)Math.Cos((vcompo_t)(0.5) * radian);
        s = (vcompo_t)Math.Sin((vcompo_t)(0.5) * radian);

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
    public static CadQuaternion RotateQuaternion(vector3_t axis, vcompo_t radian)
    {
        axis = axis.Normalized();

        CadQuaternion ans = default;
        vcompo_t c, s;

        c = (vcompo_t)Math.Cos((vcompo_t)(0.5) * radian);
        s = (vcompo_t)Math.Sin((vcompo_t)(0.5) * radian);

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
    public static CadQuaternion FromPoint(vector3_t point)
    {
        CadQuaternion q;
        q.t = (vcompo_t)(0.0);
        q.x = point.X;
        q.y = point.Y;
        q.z = point.Z;

        return q;
    }

    public static vector3_t ToPoint(CadQuaternion q)
    {
        return new vector3_t(q.x, q.y, q.z);
    }

    public vector3_t ToPoint()
    {
        vector3_t p = default;

        p.X = x;
        p.Y = y;
        p.Z = z;

        return p;
    }

    /**
     * vector3_t - 四元数 変換
     * 
     */
    public static CadQuaternion FromVector(vector3_t v)
    {
        CadQuaternion q;
        q.t = (vcompo_t)(0.0);
        q.x = v.X;
        q.y = v.Y;
        q.z = v.Z;

        return q;
    }

    public static vector3_t ToVector3(CadQuaternion q)
    {
        return new vector3_t(q.x, q.y, q.z);
    }

    public vector3_t ToVector3()
    {
        return new vector3_t(x, y, z);
    }
}
