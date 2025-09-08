using SNS.Data.DataSerializer;
using SNS.Data.DataSerializer.XmlExtensions;

namespace Caps.RPG.Rules.Saves
{
    [DataClass("SaveState")]
    public class SaveState : IGenericDataObject<SaveState>
    {
        private bool wasLoaded;
        public bool WasLoaded { get => wasLoaded; set => wasLoaded = value; }
        public SaveState()
        {

        }

        public void Save(string path)
        {
            File.WriteAllText(path, this.ToXml());
        }
    }
}
