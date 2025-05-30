using Caps.RPG.Rules.Modifiers;
using Caps.RPG.Rules.Creatures;
using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Inventory
{
    [DataClass("CreatureInventories")]
    public class CreatureInventory : Inventory, IGenericDataObject<CreatureInventory>
    {
        private Creature? _creature;
        private bool _wasLoaded = false;

        [SubDataObject("EquippedItems")]
        public Equipment EquipedItems { get; set; }

        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public CreatureInventory() { }
        public CreatureInventory(Creature creature, int size = 15) : base(size)
        {
            this._creature = creature;
            EquipedItems = new Equipment();
        }
        public CreatureInventory(int size) : base(size)
        {
            _creature = null;
            EquipedItems = new Equipment();
        }

        public void Equip(Item i)
        {
            var possibleNull = EquipedItems.GetSlotForItem(i);
            InventorySlot slot = possibleNull != null ? possibleNull : new InventorySlot(ItemType.None);
            Modifier[] modifiers = slot.SetItem(i);
            foreach (Modifier m in modifiers)
            {
                _creature?.AddModifier(m, i);
            }
        }

        public void AssignCreature(Creature creature)
        {
            _creature = creature;
        }
    }
}
