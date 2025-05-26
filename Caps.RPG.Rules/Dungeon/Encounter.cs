using Caps.RPG.Rules.Creatures;
using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Dungeon
{
    [DataClass("Encounters")]
    public class Encounter : IIDDataObject<Encounter>
    {
        private static int _idCounter;
        private int id;
        private List<Combattant> _participants;

        [DataProperty("ID")]
        public int ID { get { return id; } set { id = value; } }
        [DataProperty("Participants")]
        public List<Combattant> Participants { get { return _participants; } set { _participants = value; } }

        private bool _wasLoaded = false;
        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }


        public Encounter() { }
        public Encounter(List<Combattant> participants)
        {
            id = _idCounter++;
            _participants = participants;
        }

        public override bool Equals(object? obj)
        {
            return obj is Encounter encounter &&
                   ID == encounter.ID &&
                   Participants.Equals(encounter.Participants);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, _participants);
        }
    }
}
