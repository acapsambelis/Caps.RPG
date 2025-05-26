using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Dungeon
{
    [DataClass("Dungeons")]
    public class Dungeon : IGenericDataObject<Dungeon>
    {
        private string _name;
        private string _description;

        private List<Floor> _floors;
        private List<Encounter> _encounters;

        [DataProperty("Name")]
        public string Name { get { return _name; } set { _name = value; } }
        [DataProperty("Description")]
        public string Description { get { return _description; } set { _description = value; } }

        [DataProperty("Floors")]
        public List<Floor> FloorList { get { return _floors; } set { _floors = value; } }
        [DataProperty("Encounters")]
        public List<Encounter> EncounterList { get { return _encounters; } set { _encounters = value; } }

        private bool _wasLoaded = false;
        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public Dungeon() { }
        public Dungeon(string name, string description)
        {
            _name = name;
            _description = description;
            _floors = new List<Floor>();
            _encounters = new List<Encounter>();
        }

        public override bool Equals(object? obj)
        {
            return obj is Dungeon dungeon &&
                   Name == dungeon.Name &&
                   Description == dungeon.Description; //&&
                   //FloorList.Equals(dungeon.FloorList) &&
                   //EncounterList.Equals(dungeon.EncounterList);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_name, _description, _floors, _encounters);
        }


        public static bool operator ==(Dungeon left, Dungeon right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Dungeon left, Dungeon right)
        {
            return !(left == right);
        }

        public void AddFloors(Floor[] floors)
        {
            foreach (var floor in floors)
            {
                AddFloor(floor);
            }
        }

        public void AddFloor(Floor floor)
        {
            _floors.Add(floor);
        }

        public void AddEncounters(Encounter[] encounters)
        {
            foreach (var encounter in encounters)
            {
                AddEncounter(encounter);
            }
        }

        public void AddEncounter(Encounter encounter)
        {
            _encounters.Add(encounter);
        }
    }
}
