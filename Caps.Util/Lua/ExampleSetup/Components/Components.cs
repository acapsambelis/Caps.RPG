namespace Caps.Util.Lua.ExampleSetup.Componenets
{
    public enum Ranks
    {
        None = 0,
        Ensign = 1,
        Lieutenant = 2,
        Commander = 3,
        Captain = 4,
        Admiral = 5,
    }

    public class Position
    {
        [Required]
        public double X;
        [Required]
        public double Y;

        public string Label; // Optional, not marked as required

        public override string ToString() =>
            $"Position(X={X},Y={Y})";
    }

    public class SpaceshipInfo
    {
        public string Name;
        public int Firepower;
        public NPCInfo Captain;
        public NPCInfo FirstMate;
        public List<NPCInfo> Crew;
        public override string ToString() =>
            $"SpaceshipInfo(\n  Name={Name},\n  Firepower={Firepower},\n  Captain={Captain},\n  FirstMate={FirstMate},\n  Crew=[{(Crew == null ? "" : string.Join(",\n    ", Crew))}]\n)";
    }

    public class PlanetInfo
    {
        public string Name;
        public float Radius;
        public Dictionary<string, LandmarkInfo> Landmarks;
        public override string ToString() =>
            $"PlanetInfo(\n  Name={Name},\n  Radius={Radius},\n  Landmarks=[{(Landmarks == null ? "" : string.Join(",\n    ", Landmarks.Values))}]\n)";
    }

    public class NPCInfo
    {
        public string Name;
        public string Dialogue;
        public Ranks Rank;
        public override string ToString() =>
            $"NPCInfo(\n  Name={Name},\n  Dialogue={Dialogue},\n  Rank={Rank}\n)";
    }

    public class LandmarkInfo
    {
        public string Name;
        public Position Position;
        public string Description;
        public List<SpaceshipInfo> DockedShips;
        public override string ToString() =>
            $"LandmarkInfo(\n  Name={Name},\n  Position={Position},\n  Description={Description},\n  DockedShips=[{(DockedShips == null ? "" : string.Join(",\n    ", DockedShips))}]\n)";
    }
}
