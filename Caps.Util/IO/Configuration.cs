using SNS.Data.DataSerializer;
using SNS.Data.DataSerializer.XmlExtensions;

namespace Caps.Util.IO
{
    [DataClass("Configuration")]
    public class Configuration : IGenericDataObject<Configuration>
    {
        public bool WasLoaded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Save(string path)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                var xmlString = this.ToXml();
                var bytes = System.Text.Encoding.UTF8.GetBytes(xmlString);
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
