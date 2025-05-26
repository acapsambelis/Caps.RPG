using System;
using Caps.RPG.MonoGame.Graphics;
using Gum.DataTypes;
using Gum.DataTypes.Variables;
using Gum.Graphics.Animation;
using Gum.Managers;
using Microsoft.Xna.Framework.Audio;
using MonoGameGum.Forms.Controls;
using MonoGameGum.GueDeriving;

namespace Caps.RPG.DungeonCrawler.UI
{
    public class BlueButton : Button
    {
        public static SoundEffect _uiHoverChange;

        /// <summary>
        /// Creates a new AnimatedButton instance using graphics from the specified texture atlas.
        /// </summary>
        /// <param name="atlas">The texture atlas containing button graphics and animations</param>
        public BlueButton(TextureAtlas atlas, string text, string fontFntFile)
        {
            // Create the top-level container that will hold all visual elements
            // Width is relative to children with extra padding, height is fixed
            ContainerRuntime topLevelContainer = new ContainerRuntime();
            topLevelContainer.Height = 14f;
            topLevelContainer.HeightUnits = DimensionUnitType.Absolute;
            topLevelContainer.Width = 21f;
            topLevelContainer.WidthUnits = DimensionUnitType.RelativeToChildren;

            // Create the nine-slice background that will display the button graphics
            // A nine-slice allows the button to stretch while preserving corner appearance
            NineSliceRuntime nineSliceInstance = new NineSliceRuntime();
            nineSliceInstance.Height = 0f;
            nineSliceInstance.Texture = atlas.Texture;
            nineSliceInstance.TextureAddress = TextureAddress.Custom;
            nineSliceInstance.Dock(Gum.Wireframe.Dock.Fill);
            topLevelContainer.Children.Add(nineSliceInstance);

            // Create the text element that will display the button's label
            TextRuntime textInstance = new TextRuntime();
            // Name is required so it hooks in to the base Button.Text property
            textInstance.Name = "TextInstance";
            textInstance.Text = text;
            textInstance.Blue = 255;
            textInstance.Green = 255;
            textInstance.Red = 255;
            textInstance.UseCustomFont = true;
            textInstance.CustomFontFile = fontFntFile;
            textInstance.FontScale = 0.25f;
            textInstance.Anchor(Gum.Wireframe.Anchor.Center);
            textInstance.Width = 0;
            textInstance.WidthUnits = DimensionUnitType.RelativeToChildren;
            topLevelContainer.Children.Add(textInstance);

            // Get the texture region for the unfocused button state from the atlas
            TextureRegion unfocusedTextureRegion = atlas.GetRegion("unfocused-button-blue");

            // Create an animation chain for the unfocused state with a single frame
            AnimationChain unfocusedAnimation = new AnimationChain();
            unfocusedAnimation.Name = nameof(unfocusedAnimation);
            AnimationFrame unfocusedFrame = new AnimationFrame
            {
                TopCoordinate = unfocusedTextureRegion.TopTextureCoordinate,
                BottomCoordinate = unfocusedTextureRegion.BottomTextureCoordinate,
                LeftCoordinate = unfocusedTextureRegion.LeftTextureCoordinate,
                RightCoordinate = unfocusedTextureRegion.RightTextureCoordinate,
                FrameLength = 0.3f,
                Texture = unfocusedTextureRegion.Texture
            };
            unfocusedAnimation.Add(unfocusedFrame);

            // Get the texture region for the focused button state from the atlas
            TextureRegion focusedTextureRegion = atlas.GetRegion("focused-button-blue");
            // Create an animation chain for the focused state with a single frame
            AnimationChain focusedAnimation = new AnimationChain();
            unfocusedAnimation.Name = nameof(unfocusedAnimation);
            AnimationFrame focusedFrame = new AnimationFrame
            {
                TopCoordinate = focusedTextureRegion.TopTextureCoordinate,
                BottomCoordinate = focusedTextureRegion.BottomTextureCoordinate,
                LeftCoordinate = focusedTextureRegion.LeftTextureCoordinate,
                RightCoordinate = focusedTextureRegion.RightTextureCoordinate,
                FrameLength = 0.3f,
                Texture = focusedTextureRegion.Texture
            };
            focusedAnimation.Add(focusedFrame);


            // Assign both animation chains to the nine-slice background
            nineSliceInstance.AnimationChains = new AnimationChainList
            {
                unfocusedAnimation,
                focusedAnimation
            };

            // Create a state category for button states
            StateSaveCategory category = new StateSaveCategory();
            category.Name = Button.ButtonCategoryName;
            topLevelContainer.AddCategory(category);

            // Create the enabled (default/unfocused) state
            StateSave enabledState = new StateSave();
            enabledState.Name = FrameworkElement.EnabledStateName;
            enabledState.Apply = () =>
            {
                // When enabled but not focused, use the unfocused animation
                nineSliceInstance.CurrentChainName = unfocusedAnimation.Name;
            };
            category.States.Add(enabledState);

            // Create the focused state
            StateSave focusedState = new StateSave();
            focusedState.Name = FrameworkElement.FocusedStateName;
            focusedState.Apply = () =>
            {
                // When focused, use the focused animation and enable animation playback
                nineSliceInstance.CurrentChainName = focusedAnimation.Name;
                nineSliceInstance.Animate = true;
            };
            category.States.Add(focusedState);

            // Create the highlighted+focused state (for mouse hover while focused)
            // by cloning the focused state since they appear the same
            StateSave highlightedFocused = focusedState.Clone();
            highlightedFocused.Name = FrameworkElement.HighlightedFocusedStateName;
            category.States.Add(highlightedFocused);

            // Create the highlighted state (for mouse hover)
            // by cloning the enabled state since they appear the same
            StateSave highlighted = enabledState.Clone();
            highlighted.Name = FrameworkElement.HighlightedStateName;
            category.States.Add(highlighted);

            // Add event handler for mouse hover focus.
            topLevelContainer.RollOn += HandleRollOn;
            topLevelContainer.RollOff += HandleRollOff;

            // Assign the configured container as this button's visual
            Visual = topLevelContainer;
        }

        /// <summary>
        /// Automatically focuses the button when the mouse hovers over it.
        /// </summary>
        private void HandleRollOn(object sender, EventArgs e)
        {
            IsFocused = true;
            _uiHoverChange?.Play();
        }

        /// <summary>
        /// Automatically unfocuses the button when the mouse hovers off of it.
        /// </summary>
        private void HandleRollOff(object sender, EventArgs e)
        {
            IsFocused = false;
        }
    }
}
