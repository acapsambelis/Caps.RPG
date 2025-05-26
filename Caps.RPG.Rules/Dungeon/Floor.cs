
using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Dungeon
{
    [DataClass("Floors")]
    public class Floor : IIDDataObject<Floor>
    {
        private static int _idCounter;
        private int id;
        private List<Encounter> _encounters;

        [DataProperty("ID")]
        public int ID { get { return id; } set { id = value; } }

        [DataProperty("Encounters")]
        public List<Encounter> Encounters { get { return _encounters; } set { _encounters = value; } }

        private bool _wasLoaded = false;
        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }


        public Floor()
        {
            id = _idCounter++;
            _encounters = new List<Encounter>();
        }
        public Floor(Encounter[] encounters)
        {
            id = _idCounter++;
            _encounters = new List<Encounter>(encounters);
        }

        public override bool Equals(object? obj)
        {
            return obj is Floor floor &&
                   ID == floor.ID &&
                   Encounters.Equals(floor.Encounters);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, _encounters);
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
