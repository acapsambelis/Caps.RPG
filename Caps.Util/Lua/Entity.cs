using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.Util.Lua
{
    public class Entity
    {
        public int Id { get; }
        private static int _nextId = 0;
        private Dictionary<Type, object> _components = new();

        public Entity()
        {
            Id = _nextId++;
        }

        public void AddComponent<T>(T component)
        {
            _components[typeof(T)] = component;
        }

        public void AddComponent(object component)
        {
            _components[component.GetType()] = component;
        }

        public T GetComponent<T>() => (T)_components[typeof(T)];
        public bool HasComponent<T>() => _components.ContainsKey(typeof(T));
        public IEnumerable<object> GetAllComponents() => _components.Values;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class Required : Attribute { }
}
