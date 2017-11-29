public class NPVoxRayCastHit
{
    public NPVoxRayCastHit(bool isHit, NPVoxCoord coord)
    {
        this.isHit = isHit;
        this.coord = coord;
    }

    private bool isHit;
    public bool IsHit
    {
        get
        {
            return isHit;
        }
    }

    private NPVoxCoord coord;
    public NPVoxCoord Coord
    {
        get
        {
            return coord;
        }
    }
}
