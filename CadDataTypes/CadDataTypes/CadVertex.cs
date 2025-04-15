using OpenTK;
using OpenTK.Mathematics;
using System;

namespace CadDataTypes;

public struct CadVertex : IEquatable<CadVertex>
{
    public static byte INVALID = 0x80;
    public static byte SELECTED = 0x01;
    public static byte HANDLE = 0x02;

    public byte Flag;


    public vcompo_t X
    {
        set
        {
            vector.X = value;
        }

        get
        {
            return vector.X;
        }
    }

    public vcompo_t Y
    {
        set
        {
            vector.Y = value;
        }

        get
        {
            return vector.Y;
        }
    }

    public vcompo_t Z
    {
        set
        {
            vector.Z = value;
        }

        get
        {
            return vector.Z;
        }
    }

    public vector3_t vector;

    public bool Selected
    {
        get
        {
            return (Flag & SELECTED) != 0;
        }

        set
        {
            Flag = value ? (byte)(Flag | SELECTED) : (byte)(Flag & ~SELECTED);
        }
    }

    public bool IsHandle
    {
        get
        {
            return (Flag & HANDLE) != 0;
        }

        set
        {
            Flag = value ? (byte)(Flag | HANDLE) : (byte)(Flag & ~HANDLE);
        }
    }


    public bool Valid
    {
        set
        {
            Invalid = !value;
        }

        get
        {
            return !Invalid;
        }
    }

    public bool Invalid
    {
        set
        {
            Flag = value ? (byte)(Flag | INVALID) : (byte)(Flag & ~INVALID);
        }

        get
        {
            return (Flag & INVALID) != 0;
        }
    }

    public static CadVertex Zero = default;

    public static CadVertex UnitX = CadVertex.Create(1, 0, 0);
    public static CadVertex UnitY = CadVertex.Create(0, 1, 0);
    public static CadVertex UnitZ = CadVertex.Create(0, 0, 1);

    public static CadVertex InvalidValue = CadVertex.CreateInvalid();

    public static CadVertex MaxValue = CadVertex.Create(vcompo_t.MaxValue);
    public static CadVertex MinValue = CadVertex.Create(vcompo_t.MinValue);

    public CadVertex(vcompo_t x, vcompo_t y, vcompo_t z)
    {
        vector.X = x;
        vector.Y = y;
        vector.Z = z;

        Flag = 0;
    }

    public CadVertex(vector3_t pos)
    {
        vector = pos;

        Flag = 0;
    }

    public CadVertex(vcompo_t x, vcompo_t y, vcompo_t z, byte flag)
    {
        vector = new vector3_t(x, y, z);

        Flag = flag;
    }

    private static CadVertex Create(vcompo_t v)
    {
        return Create(v, v, v);
    }

    public static CadVertex Create(vcompo_t x, vcompo_t y, vcompo_t z)
    {
        CadVertex v = default;
        v.Set(x, y, z);

        v.Flag = 0;

        return v;
    }

    // Cpp CLI で使用する際にOverloadが上手く解釈されないので、別名を付ける
    public static CadVertex Create3(vcompo_t x, vcompo_t y, vcompo_t z)
    {
        return Create(x, y, z);
    }

    public static CadVertex Create()
    {
        CadVertex v = default;
        v.Set(0, 0, 0);

        v.Flag = 0;

        return v;
    }

    public static CadVertex Create(vector3_t v)
    {
        CadVertex p = default;
        p.Set(v.X, v.Y, v.Z);

        p.Flag = 0;

        return p;
    }

    public static CadVertex Create(vector4_t v)
    {
        CadVertex p = default;
        p.Set(v.X, v.Y, v.Z);

        p.Flag = 0;

        return p;
    }

    public static CadVertex CreateInvalid()
    {
        CadVertex p = default;
        p.Valid = false;
        return p;
    }

    public bool IsZero()
    {
        return X == 0 && Y == 0 && Z == 0;
    }

    public void Set(vcompo_t x, vcompo_t y, vcompo_t z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    public CadVertex SetVector(vector3_t v)
    {
        vector = v;
        return this;
    }

    public CadVertex SetVector(CadVertex p)
    {
        vector = p.vector;
        return this;
    }

    public void Set(ref CadVertex p)
    {
        Flag = p.Flag;
        X = p.X;
        Y = p.Y;
        Z = p.Z;
    }

    #region 同値判定

    public bool EqualsThreshold(CadVertex p, vcompo_t m = (vcompo_t)0.000001)
    {
        return (
            X > p.X - m && X < p.X + m &&
            Y > p.Y - m && Y < p.Y + m &&
            Z > p.Z - m && Z < p.Z + m
            );
    }


    public bool Equals(CadVertex v)
    {
        return X == v.X & Y == v.Y & Z == v.Z;
    }

    private const vcompo_t HASH_COEFFICIENT = (vcompo_t)10000.0;

    public override int GetHashCode()
    {
        return
            ((int)(X * HASH_COEFFICIENT)) ^
            ((int)(Y * HASH_COEFFICIENT) << 2) ^
            ((int)(Z * HASH_COEFFICIENT) >> 2);
    }

    public override bool Equals(object obj)
    {
        CadVertex t = (CadVertex)obj;

        return Equals(t);
    }


    public static bool operator ==(CadVertex p1, CadVertex p2)
    {
        return p1.X == p2.X & p1.Y == p2.Y & p1.Z == p2.Z;
    }

    public static bool operator !=(CadVertex p1, CadVertex p2)
    {
        return p1.X != p2.X | p1.Y != p2.Y | p1.Z != p2.Z;
    }
    #endregion


    #region 二項演算子
    public static CadVertex operator +(CadVertex p1, CadVertex p2)
    {
        p1.X += p2.X;
        p1.Y += p2.Y;
        p1.Z += p2.Z;

        return p1;
    }

    public static CadVertex operator -(CadVertex p1, CadVertex p2)
    {
        p1.X -= p2.X;
        p1.Y -= p2.Y;
        p1.Z -= p2.Z;

        return p1;
    }

    public static CadVertex operator *(CadVertex p1, CadVertex p2)
    {
        p1.X *= p2.X;
        p1.Y *= p2.Y;
        p1.Z *= p2.Z;

        return p1;
    }

    public static CadVertex operator /(CadVertex p1, CadVertex p2)
    {
        p1.X /= p2.X;
        p1.Y /= p2.Y;
        p1.Z /= p2.Z;

        return p1;
    }

    public static CadVertex operator +(CadVertex p1, vector3_t p2)
    {
        p1.X += p2.X;
        p1.Y += p2.Y;
        p1.Z += p2.Z;

        return p1;
    }

    public static CadVertex operator -(CadVertex p1, vector3_t p2)
    {
        p1.X -= p2.X;
        p1.Y -= p2.Y;
        p1.Z -= p2.Z;

        return p1;
    }

    public static CadVertex operator *(CadVertex p1, vector3_t p2)
    {
        p1.X *= p2.X;
        p1.Y *= p2.Y;
        p1.Z *= p2.Z;

        return p1;
    }

    public static CadVertex operator /(CadVertex p1, vector3_t p2)
    {
        p1.X /= p2.X;
        p1.Y /= p2.Y;
        p1.Z /= p2.Z;

        return p1;
    }

    public static CadVertex operator +(vector3_t p1, CadVertex p2)
    {
        p1.X += p2.X;
        p1.Y += p2.Y;
        p1.Z += p2.Z;

        return CadVertex.Create(p1);
    }

    public static CadVertex operator -(vector3_t p1, CadVertex p2)
    {
        p1.X -= p2.X;
        p1.Y -= p2.Y;
        p1.Z -= p2.Z;

        return CadVertex.Create(p1);
    }

    public static CadVertex operator *(vector3_t p1, CadVertex p2)
    {
        p1.X *= p2.X;
        p1.Y *= p2.Y;
        p1.Z *= p2.Z;

        return CadVertex.Create(p1);
    }

    public static CadVertex operator /(vector3_t p1, CadVertex p2)
    {
        p1.X /= p2.X;
        p1.Y /= p2.Y;
        p1.Z /= p2.Z;

        return CadVertex.Create(p1);
    }


    public static CadVertex operator *(CadVertex p1, vcompo_t f)
    {
        p1.X *= f;
        p1.Y *= f;
        p1.Z *= f;

        return p1;
    }

    public static CadVertex operator *(vcompo_t f, CadVertex p1)
    {
        p1.X *= f;
        p1.Y *= f;
        p1.Z *= f;

        return p1;
    }

    public static CadVertex operator /(CadVertex p1, vcompo_t f)
    {
        p1.X /= f;
        p1.Y /= f;
        p1.Z /= f;

        return p1;
    }

    public static CadVertex operator -(CadVertex p1, vcompo_t d)
    {
        p1.X -= d;
        p1.Y -= d;
        p1.Z -= d;

        return p1;
    }
    #endregion

    #region 単項演算子
    public static CadVertex operator -(CadVertex p1)
    {
        p1.X *= -1;
        p1.Y *= -1;
        p1.Z *= -1;

        return p1;
    }

    public static CadVertex operator +(CadVertex p1, vcompo_t d)
    {
        p1.X += d;
        p1.Y += d;
        p1.Z += d;

        return p1;
    }
    #endregion

    #region Cast operator
    public static explicit operator vector3_t(CadVertex p)
    {
        return p.vector;
    }

    public static explicit operator CadVertex(vector3_t v)
    {
        return Create(
            v.X,
            v.Y,
            v.Z
            );
    }

    public static explicit operator CadVertex(vector4_t v)
    {
        return Create(
            v.X,
            v.Y,
            v.Z
            );
    }

    public static explicit operator vector4_t(CadVertex p)
    {
        return new vector4_t(
            p.vector.X,
            p.vector.Y,
            p.vector.Z,
            1.0f
            );
    }
    #endregion

    /// <summary>
    /// 二点の成分から最小の成分でVectorを作成
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static CadVertex Min(CadVertex v1, CadVertex v2)
    {
        CadVertex v = default(CadVertex);

        v.X = Math.Min(v1.X, v2.X);
        v.Y = Math.Min(v1.Y, v2.Y);
        v.Z = Math.Min(v1.Z, v2.Z);

        return v;
    }

    /// <summary>
    /// 二点の成分から最大の成分でVectorを作成
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static CadVertex Max(CadVertex v1, CadVertex v2)
    {
        CadVertex v = default(CadVertex);

        v.X = Math.Max(v1.X, v2.X);
        v.Y = Math.Max(v1.Y, v2.Y);
        v.Z = Math.Max(v1.Z, v2.Z);

        return v;
    }



    // ベクトルのノルム(長さ)を求める
    public vcompo_t Norm()
    {
        return (vcompo_t)Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
    }

    public vcompo_t Norm2D()
    {
        return (vcompo_t)Math.Sqrt((X * X) + (Y * Y));
    }

    // 単位ベクトルを求める
    public CadVertex UnitVector()
    {
        CadVertex ret = default;

        vcompo_t norm = this.Norm();

        vcompo_t f = (vcompo_t)1.0 / norm;

        ret.X = X * f;
        ret.Y = Y * f;
        ret.Z = Z * f;

        return ret;
    }

    private string CoordString()
    {
        return X.ToString() + ", " + Y.ToString() + ", " + Z.ToString();
    }

    public override string ToString()
    {
        return CoordString();
    }
}
