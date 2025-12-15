using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Inventory;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.DungeonCrawler.UI
{
    public class InventoryManager
    {
        private Combattant combattant;
        private Panel panel;
        private Dictionary<ItemType, Button> equipmentButtons = [];
        private Button? selectedButton;

        public Panel Panel => panel;

        public InventoryManager(Combattant combattant, Sprite combattantSprite)
        {
            this.combattant = combattant;

            panel = new Panel(new Vector2(800, 800))
            {
                Draggable = true,
                Visible = false,
                Padding = new Vector2(10)
            };

            //var columns = GeonBit.UI.Utils.PanelsGrid.GenerateColums([new(0.33f), new(0.67f)], panel);
            var columns = GeonBit.UI.Utils.PanelsGrid.GenerateColums(2, panel);
            foreach (var column in columns)
            {
                column.Padding = column.SpaceAfter = column.SpaceBefore = Vector2.Zero;
                column.Anchor = Anchor.AutoInline;
                column.MinSize = column.MaxSize = new Vector2(column.Size.X, (panel.Size.Y - (panel.Padding.Y * 3)) * 2.0f / 3.0f);
                column.Skin = PanelSkin.None;
            }
            Button closeButton = new("X", ButtonSkin.Default, Anchor.TopRight, new Vector2(40, 40))
            {
                Padding = Vector2.Zero,
                SpaceAfter = Vector2.Zero,
                SpaceBefore = Vector2.Zero
            };
            closeButton.ButtonParagraph.AlignToCenter = true;
            closeButton.ButtonParagraph.Padding = Vector2.Zero;
            closeButton.OnClick += (entity) => { panel.Visible = false; };
            columns[1].AddChild(closeButton);

            Label name = new(combattant.Name, Anchor.TopCenter)
            {
                Scale = 1.5f,
                Padding = new Vector2(30)
            };
            columns[0].AddChild(name);

            var texture = combattantSprite.GetTextureWithColor();
            Image character = new(
                texture,
                size: new Vector2((int)(panel.Size.X / 3) - 40),
                anchor: Anchor.Center
            )
            {
                Padding = new Vector2(20)
            };
            columns[0].AddChild(character);

            // inventory button size
            int itemsPerRow = 10;  // inventory, not equiment; match equipment buttons to inventory button size
            int buttonSize = (int)((panel.Size.X - new Vector2(20).X) / itemsPerRow) - (int)(new Vector2(10).X);

            // equipment slots
            var gearColumns = GeonBit.UI.Utils.PanelsGrid.GenerateColums(2, columns[1]);
            foreach (var column in gearColumns)
            {
                column.Padding = column.SpaceAfter = column.SpaceBefore = Vector2.Zero;
                column.Anchor = Anchor.AutoInline;
                column.Skin = PanelSkin.None;
            }

            var equipmentSlots = new[]
            {
                new EquipmentSlot { Type = ItemType.Crown, Tooltip = "Crown Slot", Column = 0 },
                new EquipmentSlot { Type = ItemType.Face, Tooltip = "Face Slot", Column = 1 },
                new EquipmentSlot { Type = ItemType.Neck, Tooltip = "Neck Slot", Column = 0 },
                new EquipmentSlot { Type = ItemType.Shoulders, Tooltip = "Shoulders Slot", Column = 1 },
                new EquipmentSlot { Type = ItemType.Chest, Tooltip = "Chest Slot", Column = 0 },
                new EquipmentSlot { Type = ItemType.Back, Tooltip = "Back Slot", Column = 1 },
                new EquipmentSlot { Type = ItemType.Arms, Tooltip = "Arms Slot", Column = 0 },
                new EquipmentSlot { Type = ItemType.Gloves, Tooltip = "Gloves Slot", Column = 1 },
                new EquipmentSlot { Type = ItemType.Belt, Tooltip = "Belt Slot", Column = 0 },
                new EquipmentSlot { Type = ItemType.Jewelry, Tooltip = "Jewelry Slot", Column = 1 },
                new EquipmentSlot { Type = ItemType.Pants, Tooltip = "Pants Slot", Column = 0 },
                new EquipmentSlot { Type = ItemType.Boots, Tooltip = "Boots Slot", Column = 1 }
            };

            foreach (var slot in equipmentSlots)
            {
                Panel slotPanel = new(new Vector2(0, buttonSize), PanelSkin.None, Anchor.Auto)
                {
                    Padding = new Vector2(10, 0),
                    SpaceAfter = Vector2.Zero,
                    SpaceBefore = Vector2.Zero
                };

                Label slotLabel = new(slot.Type.ToString(), Anchor.CenterLeft, size: new Vector2(0, buttonSize))
                {
                    Scale = 0.8f,
                    Padding = Vector2.Zero
                };
                slotPanel.AddChild(slotLabel);

                Button button = new(slot.Type.ToString()[..2].ToUpper(), ButtonSkin.Default, Anchor.CenterRight, size: new Vector2(buttonSize))
                {
                    Tag = slot.Type.ToString(),
                    Padding = Vector2.Zero,
                    Enabled = false
                };
                button.ButtonParagraph.FillColor = Color.Gray;
                button.OnClick += EquipmentButtonClicked;
                slotPanel.AddChild(button);

                gearColumns[slot.Column].AddChild(slotPanel);
                equipmentButtons[slot.Type] = button;
            }

            // hands
            Panel handPanel = new(new Vector2(0, buttonSize), PanelSkin.None, Anchor.BottomCenter)
            {
                Padding = new Vector2(10, 0),
                SpaceAfter = Vector2.Zero,
                SpaceBefore = Vector2.Zero
            };

            Label handLabel = new("Hands", Anchor.Auto, size: new Vector2(0, buttonSize))
            {
                Scale = 0.8f,
                Padding = Vector2.Zero
            };
            handPanel.AddChild(handLabel);

            Button hand1 = new("HA", ButtonSkin.Default, Anchor.Center, size: new Vector2(buttonSize))
            {
                ToolTipText = "Hands Slot 1",
                Tag = ItemType.Hands.ToString(),
                Padding = Vector2.Zero,
                Enabled = false
            };
            hand1.ButtonParagraph.FillColor = Color.Gray;
            hand1.OnClick += EquipmentButtonClicked;
            handPanel.AddChild(hand1);

            columns[0].AddChild(handPanel);
            equipmentButtons[ItemType.Hands] = hand1;

            // inventory slots
            Panel inventorySlots = new(new Vector2(0, (panel.Size.Y - (columns[0].Padding.Y)) / 3.0f), PanelSkin.Simple, Anchor.AutoInline)
            {
                Padding = Vector2.Zero,
                PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll
            };
            panel.AddChild(inventorySlots);

            for (int i = 0; i < combattant.Creature.Inventory.Slots.Length; i++)
            {
                var slot = combattant.Creature.Inventory.Slots[i];
                string slotText = slot.Item != null ? slot.Item.Name : " ";
                Button inventoryButton = new("", ButtonSkin.Default, Anchor.AutoInline, size: new Vector2(buttonSize))
                {
                    ToolTipText = $"{slotText}",
                    Tag = i.ToString(),
                    ToggleMode = true
                };
                inventoryButton.OnClick += InventoryButtonClicked;


                Label iconLabel = new(slotText[0].ToString(), Anchor.Center, size: new Vector2(buttonSize - 10))
                {
                    Scale = 1.0f,
                    Padding = Vector2.Zero
                };
                inventoryButton.AddChild(iconLabel, true);

                inventorySlots.AddChild(inventoryButton);
            }
        }

        private void EquipmentButtonClicked(Entity entity)
        {
            if (selectedButton != null)
            {
                // move item to this slot
                var item = combattant.Creature.Inventory.Slots[int.Parse(selectedButton.Tag)].Item;
                combattant.Creature.Inventory.Equip(item);
                combattant.Creature.Inventory.Slots[int.Parse(selectedButton.Tag)].Item = null;
                selectedButton.Checked = false;
                selectedButton = null;

                // clear inventory slot
                selectedButton.Checked = false;
                (selectedButton.Children.First(c => c is Label) as Label).Text = "";
                selectedButton.ToolTipText = "";
                selectedButton = null;
            }
        }

        private void InventoryButtonClicked(Entity entity)
        {
            var item = combattant.Creature.Inventory.Slots[int.Parse(entity.Tag)].Item;
            if (selectedButton == entity as Button)
            {
                selectedButton = null;
                equipmentButtons[item.Type].Enabled = false;
                return;
            }

            if (selectedButton == null)
            {
                selectedButton = entity as Button;
                equipmentButtons[item.Type].Enabled = true;
            }
            else
            {
                // move item to this slot
                item = combattant.Creature.Inventory.Slots[int.Parse(selectedButton.Tag)].Item;
                combattant.Creature.Inventory.Slots[int.Parse(selectedButton.Tag)].Item = null;
                combattant.Creature.Inventory.Slots[int.Parse(entity.Tag)].Item = item;

                var newBtn = entity as Button;
                newBtn.Checked = false;
                (newBtn.Children.First(c => c is Label) as Label).Text = item.Name[0].ToString();
                newBtn.ToolTipText = item.Name;

                selectedButton.Checked = false;
                (selectedButton.Children.First(c => c is Label) as Label).Text = "";
                selectedButton.ToolTipText = "";
                selectedButton = null;
            }
        }

        private struct EquipmentSlot
        {
            public ItemType Type;
            public string Label;
            public string Tooltip;
            public int Column;
        }
    }
}
