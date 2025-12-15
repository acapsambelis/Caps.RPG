using Caps.RPG.Rules.Inventory;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System.Linq;

namespace Caps.RPG.DungeonCrawler.UI
{
    public class InventoryButton
    {
        private readonly Button button;
        private readonly Label iconLabel;
        private readonly int slotIndex;
        private readonly ItemType? equipmentType;
        private readonly bool isEquipmentSlot;

        public Button Button => button;
        public int SlotIndex => slotIndex;
        public ItemType? EquipmentType => equipmentType;
        public bool IsEquipmentSlot => isEquipmentSlot;
        public bool IsSelected => button.Checked;

        public InventoryButton(int slotIndex, Item item, int buttonSize) : this(slotIndex, item, buttonSize, null, false) { }

        public InventoryButton(ItemType equipmentType, Item item, int buttonSize) : this(-1, item, buttonSize, equipmentType, true) { }

        private InventoryButton(int slotIndex, Item item, int buttonSize, ItemType? equipmentType, bool isEquipmentSlot)
        {
            this.slotIndex = slotIndex;
            this.equipmentType = equipmentType;
            this.isEquipmentSlot = isEquipmentSlot;
            string slotText = item != null ? item.Name : " ";

            if (isEquipmentSlot)
            {
                // Equipment button initialization
                button = new Button("", ButtonSkin.Default, Anchor.CenterRight, size: new Vector2(buttonSize))
                {
                    Tag = equipmentType.ToString(),
                    Padding = Vector2.Zero,
                    Enabled = item != null,
                    ToggleMode = true
                };
                iconLabel = null;
                if (item != null)
                {
                    button.ToolTipText = item.Name;
                }
            }
            else
            {
                // Inventory button initialization
                button = new Button("", ButtonSkin.Default, Anchor.AutoInline, size: new Vector2(buttonSize))
                {
                    ToolTipText = $"{slotText}",
                    Tag = slotIndex.ToString(),
                    ToggleMode = true
                };
            }

            iconLabel = new Label(slotText[0].ToString(), Anchor.Center, size: new Vector2(buttonSize - 10))
            {
                Scale = 1.0f,
                Padding = Vector2.Zero
            };
            button.AddChild(iconLabel, true);
        }

        public void UpdateItem(Item item)
        {
            if (item != null)
            {
                iconLabel.Text = item.Name[0].ToString();
                button.ToolTipText = item.Name;
                if (isEquipmentSlot)
                {
                    button.Enabled = true;
                    button.ButtonParagraph.FillColor = Color.White;
                }
            }
            else
            {
                ClearItem();
            }
        }

        public void ClearItem()
        {
            if (iconLabel != null)
            {
                iconLabel.Text = "";
            }
            button.ToolTipText = isEquipmentSlot ? $"{equipmentType} Slot" : "";
            if (isEquipmentSlot)
            {
                button.Enabled = false;
            }
        }

        public void Select()
        {
            button.Checked = true;
        }

        public void Deselect()
        {
            button.Checked = false;
        }

        public void SetEnabled(bool enabled)
        {
            button.Enabled = enabled;
            if (isEquipmentSlot)
            {
                button.ButtonParagraph.FillColor = enabled ? Color.White : Color.Gray;
            }
        }
    }
}