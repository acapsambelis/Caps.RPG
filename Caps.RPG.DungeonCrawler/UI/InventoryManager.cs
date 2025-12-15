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
        private Dictionary<ItemType, InventoryButton> equipmentButtons = [];
        private List<InventoryButton> inventoryButtons = [];
        private InventoryButton selectedInventoryButton;

        public Panel Panel => panel;

        public InventoryManager(ref Combattant combattant, Sprite combattantSprite)
        {
            this.combattant = combattant;

            panel = new Panel(new Vector2(800, 800))
            {
                Draggable = true,
                Visible = false,
                Padding = new Vector2(10)
            };

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

            var equipmentTypes = new[] {
                ItemType.Crown, ItemType.Face,
                ItemType.Neck,  ItemType.Shoulders,
                ItemType.Chest, ItemType.Back,
                ItemType.Arms,  ItemType.Gloves,
                ItemType.Belt,  ItemType.Jewelry,
                ItemType.Pants, ItemType.Boots
            };
            for (int i = 0; i < equipmentTypes.Length; i++)
            {
                var type = equipmentTypes[i];
                var column = i % 2;  // Alternates 0, 1, 0, 1...

                Panel slotPanel = new(new Vector2(0, buttonSize), PanelSkin.None, Anchor.Auto)
                {
                    Padding = new Vector2(10, 0),
                    SpaceAfter = Vector2.Zero,
                    SpaceBefore = Vector2.Zero
                };

                Label slotLabel = new(type.ToString(), Anchor.CenterLeft, size: new Vector2(0, buttonSize))
                {
                    Scale = 0.8f,
                    Padding = Vector2.Zero
                };
                slotPanel.AddChild(slotLabel);

                var equipmentButton = new InventoryButton(
                    type,
                    combattant.Creature.Inventory.EquippedItems.GetSlotForItemType(type).Item,
                    buttonSize
                );
                equipmentButton.Button.OnClick += EquipmentButtonClicked;
                slotPanel.AddChild(equipmentButton.Button);

                gearColumns[column].AddChild(slotPanel);
                equipmentButtons[type] = equipmentButton;
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

            var handsButton = new InventoryButton(ItemType.Hands, combattant.Creature.Inventory.EquippedItems.Hands.Item, buttonSize);
            handsButton.Button.OnClick += EquipmentButtonClicked;
            handPanel.AddChild(handsButton.Button);

            columns[0].AddChild(handPanel);
            equipmentButtons[ItemType.Hands] = handsButton;

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
                var inventoryButton = new InventoryButton(i, slot.Item, buttonSize);
                inventoryButton.Button.OnClick += InventoryButtonClicked;
                
                inventoryButtons.Add(inventoryButton);
                inventorySlots.AddChild(inventoryButton.Button);
            }
        }

        private void EquipmentButtonClicked(Entity entity)
        {
            var destination = equipmentButtons.FirstOrDefault(eb => eb.Value.Button == entity).Value;
            if (destination == null) return;

            var destinationSlot = combattant.Creature.Inventory.EquippedItems.GetSlotForItemType(destination.EquipmentType.Value);
            var destinationItem = destinationSlot.Item;

            // toggle click off
            if (selectedInventoryButton == destination)
            {
                selectedInventoryButton = null;
                destination.Deselect();
                return;
            }

            // if first click
            if (selectedInventoryButton == null)
            {
                selectedInventoryButton = destination;
                if (destinationItem == null)
                    destination.Deselect();
            }
            // if second click
            else
            {
                Item sourceItem = null;

                // from inventory to equipment
                if (!selectedInventoryButton.IsEquipmentSlot)
                {
                    sourceItem = combattant.Creature.Inventory.Slots[selectedInventoryButton.SlotIndex].Item;
                    
                    // swap: unequip destination item and place in inventory, then equip source item
                    if (destinationItem != null)
                    {
                        combattant.Creature.Inventory.Unequip(destinationSlot);
                        combattant.Creature.Inventory.Slots[selectedInventoryButton.SlotIndex].Item = destinationItem;
                        selectedInventoryButton.UpdateItem(destinationItem);
                    }
                    // move to empty equipment slot
                    else
                    {
                        combattant.Creature.Inventory.Slots[selectedInventoryButton.SlotIndex].Item = null;
                        selectedInventoryButton.ClearItem();
                    }
                    
                    combattant.Creature.Inventory.Equip(sourceItem);
                    destination.UpdateItem(sourceItem);
                }
                // from equipment to equipment (swap equipped items)
                else
                {
                    var sourceSlot = combattant.Creature.Inventory.EquippedItems.GetSlotForItemType(selectedInventoryButton.EquipmentType.Value);
                    sourceItem = sourceSlot.Item;

                    // swap equipped items
                    if (destinationItem != null && sourceItem != null)
                    {
                        combattant.Creature.Inventory.Unequip(sourceSlot);
                        combattant.Creature.Inventory.Unequip(destinationSlot);
                        combattant.Creature.Inventory.Equip(destinationItem);
                        combattant.Creature.Inventory.Equip(sourceItem);
                        
                        selectedInventoryButton.UpdateItem(destinationItem);
                        destination.UpdateItem(sourceItem);
                    }
                    // move to empty equipment slot
                    else if (sourceItem != null)
                    {
                        combattant.Creature.Inventory.Unequip(sourceSlot);
                        combattant.Creature.Inventory.Equip(sourceItem);
                        selectedInventoryButton.ClearItem();
                        destination.UpdateItem(sourceItem);
                    }
                }

                // cleanup
                destination.Deselect();
                selectedInventoryButton.Deselect();
                selectedInventoryButton = null;
            }
        }

        private void InventoryButtonClicked(Entity entity)
        {
            var destination = inventoryButtons.FirstOrDefault(ib => ib.Button == entity);
            if (destination == null) return;

            var destinationItem = combattant.Creature.Inventory.Slots[destination.SlotIndex].Item;

            // toggle click off
            if (selectedInventoryButton == destination)
            {
                selectedInventoryButton = null;
                destination.Deselect();
                if (destinationItem != null)
                    equipmentButtons[destinationItem.Type].SetEnabled(false);
                return;
            }

            // if first click
            if (selectedInventoryButton == null)
            {
                selectedInventoryButton = destination;
                if (destinationItem != null)
                    equipmentButtons[destinationItem.Type].SetEnabled(true);
                else
                    destination.Deselect();
            }
            // if second click
            else
            {
                // from equipment to inventory
                if (selectedInventoryButton.IsEquipmentSlot)
                {
                    var itemSlot = combattant.Creature.Inventory.EquippedItems.GetSlotForItemType(selectedInventoryButton.EquipmentType.Value);
                    var sourceItem = itemSlot.Item;
                    
                    // swap: unequip source item, equip destination item (if exists), then place source in inventory
                    if (destinationItem != null)
                    {
                        combattant.Creature.Inventory.Unequip(itemSlot);
                        combattant.Creature.Inventory.Equip(destinationItem);
                        combattant.Creature.Inventory.Slots[destination.SlotIndex].Item = sourceItem;
                        
                        selectedInventoryButton.UpdateItem(destinationItem);
                        destination.UpdateItem(sourceItem);
                    }
                    // move to empty inventory slot
                    else
                    {
                        combattant.Creature.Inventory.Unequip(itemSlot);
                        combattant.Creature.Inventory.Slots[destination.SlotIndex].Item = sourceItem;
                        selectedInventoryButton.ClearItem();
                        selectedInventoryButton.SetEnabled(false);
                        
                        destination.UpdateItem(sourceItem);
                        equipmentButtons[sourceItem.Type].SetEnabled(false);
                    }
                }
                // from inventory to inventory (move or swap)
                else
                {
                    var sourceItem = combattant.Creature.Inventory.Slots[selectedInventoryButton.SlotIndex].Item;
                    
                    // swap items
                    if (destinationItem != null)
                    {
                        combattant.Creature.Inventory.Slots[selectedInventoryButton.SlotIndex].Item = destinationItem;
                        combattant.Creature.Inventory.Slots[destination.SlotIndex].Item = sourceItem;
                        
                        selectedInventoryButton.UpdateItem(destinationItem);
                        destination.UpdateItem(sourceItem);
                        
                        // Disable equipment buttons for both items now in inventory
                        equipmentButtons[sourceItem.Type].SetEnabled(false);
                        equipmentButtons[destinationItem.Type].SetEnabled(false);
                    }
                    // move to empty slot
                    else
                    {
                        sourceItem = combattant.Creature.Inventory.MoveItem(selectedInventoryButton.SlotIndex, destination.SlotIndex);
                        selectedInventoryButton.ClearItem();
                        destination.UpdateItem(sourceItem);
                        equipmentButtons[sourceItem.Type].SetEnabled(false);
                    }
                }

                // cleanup
                destination.Deselect();
                selectedInventoryButton.Deselect();
                selectedInventoryButton = null;
            }
        }
    }
}
