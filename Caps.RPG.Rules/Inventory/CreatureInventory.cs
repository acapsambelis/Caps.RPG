using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Modifiers;
using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Inventory
{
    [DataClass("CreatureInventories")]
    public class CreatureInventory : Inventory, IGenericDataObject<CreatureInventory>
    {
        private bool equipmentChanged = false;
        public bool EquipmentChanged
        {
            get { return equipmentChanged; }
            set { equipmentChanged = value; }
        }

        [SubDataObject("EquippedItems")]
        public Equipment EquippedItems { get; set; }
        
        public CreatureInventory() : base (50)
        {
            EquippedItems = new Equipment();
        }
        public CreatureInventory(int size = 50) : base(size)
        {
            EquippedItems = new Equipment();
        }

        public void Equip(Item i)
        {
            equipmentChanged = EquippedItems.GetSlotForItem(i).SetItem(i);
        }

        public void Unequip(InventorySlot slot)
        {
            equipmentChanged = slot.SetItem(null);
        }

        public Item? MoveItem(int oldSlot, int newSlot)
        {
            var item = Slots[oldSlot].Item;
            Slots[oldSlot].Item = null;
            Slots[newSlot].Item = item;
            return Slots[newSlot].Item;
        }

        public Dictionary<ModifiedValue, List<Modifier>> GetModifiers()
        {
            return EquippedItems.GetAllModifiers();
        }

        public List<CombatAction> GetCombatActions()
        {
            return EquippedItems.GetCombatActions();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not CreatureInventory other)
                return false;

            bool isEqual = true;
            isEqual &= base.Equals(obj);
            isEqual &= ID == other.ID;
            isEqual &= EquippedItems == other.EquippedItems;
            return isEqual;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), ID, Slots, EquippedItems);
        }

        public static bool operator ==(CreatureInventory? left, CreatureInventory? right)
        {
            return EqualityComparer<CreatureInventory>.Default.Equals(left, right);
        }

        public static bool operator !=(CreatureInventory? left, CreatureInventory? right)
        {
            return !(left == right);
        }
    }
}
