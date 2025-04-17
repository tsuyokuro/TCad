namespace Plotter;

public struct LocalCoordinate
{
    public vector3_t BasePoint;

    public LocalCoordinate(vector3_t v = default)
    {
        BasePoint = v;
    }

    vector3_t Trans(vector3_t vector)
    {
        return vector + BasePoint;
    }
}
