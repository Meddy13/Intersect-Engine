﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Gwen.Input;
using Intersect.Client.Framework.Input;
using Intersect.Client.General;
using Intersect.Client.MonoGame.Graphics;
using Intersect.Client.UI;
using Intersect.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Keys = Intersect.Client.Framework.GenericClasses.Keys;

namespace Intersect.Client.MonoGame.Input
{
    public class MonoInput : GameInput
    {
        private Dictionary<Keys, Microsoft.Xna.Framework.Input.Keys> mKeyDictionary;
        private int mMouseX;
        private int mMouseY;
        private Game mMyGame;
        private KeyboardState mLastKeyboardState;
        private MouseState mLastMouseState;

        public MonoInput(Game myGame)
        {
            myGame.Window.TextInput += Window_TextInput;
            mMyGame = myGame;
            mKeyDictionary = new Dictionary<Keys, Microsoft.Xna.Framework.Input.Keys>();
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (!mKeyDictionary.ContainsKey(key))
                {
                    foreach (
                        Microsoft.Xna.Framework.Input.Keys monoKey in
                        Enum.GetValues(typeof(Microsoft.Xna.Framework.Input.Keys)))
                    {
                        if (key == Keys.Shift)
                        {
                            mKeyDictionary.Add(key, Microsoft.Xna.Framework.Input.Keys.LeftShift);
                            break;
                        }
                        if (key == Keys.Control || key == Keys.LControlKey)
                        {
                            mKeyDictionary.Add(key, Microsoft.Xna.Framework.Input.Keys.LeftControl);
                            break;
                        }
                        if (key == Keys.RControlKey)
                        {
                            mKeyDictionary.Add(key, Microsoft.Xna.Framework.Input.Keys.RightControl);
                            break;
                        }
                        if (key == Keys.Return)
                        {
                            mKeyDictionary.Add(key, Microsoft.Xna.Framework.Input.Keys.Enter);
                            break;
                        }
                        else
                        {
                            if (key.ToString() == monoKey.ToString())
                            {
                                mKeyDictionary.Add(key, monoKey);
                                break;
                            }
                        }
                    }
                }
                if (!mKeyDictionary.ContainsKey(key))
                {
                    Debug.WriteLine("Mono does not have a key to match: " + key);
                }
            }
        }

        private void Window_TextInput(object sender, TextInputEventArgs e)
        {
            Gui.GwenInput.ProcessMessage(new GwenInputMessage(IntersectInput.InputEvent.TextEntered,
                GetMousePosition(), (int) MouseButtons.None, Keys.Alt, false, false, false, e.Character.ToString()));
        }

        public override bool MouseButtonDown(MouseButtons mb)
        {
            switch (mb)
            {
                case MouseButtons.Left:
                    return (mLastMouseState.LeftButton == ButtonState.Pressed);
                case MouseButtons.Right:
                    return (mLastMouseState.RightButton == ButtonState.Pressed);
                case MouseButtons.Middle:
                    return (mLastMouseState.MiddleButton == ButtonState.Pressed);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mb), mb, null);
            }
        }

        public override bool KeyDown(Keys key)
        {
            if (mKeyDictionary.ContainsKey(key))
            {
                if (mLastKeyboardState.IsKeyDown(mKeyDictionary[key]))
                {
                    return true;
                }
            }
            return false;
        }

        public override Pointf GetMousePosition()
        {
            return new Pointf(mMouseX, mMouseY);
        }

        private void CheckMouseButton(ButtonState bs, MouseButtons mb)
        {
            if (Globals.GameState == GameStates.Intro) return; //No mouse input allowed while showing intro slides
            if (bs == ButtonState.Pressed && !MouseButtonDown(mb))
            {
                Gui.GwenInput.ProcessMessage(new GwenInputMessage(IntersectInput.InputEvent.MouseDown,
                    GetMousePosition(), (int) mb, Keys.Alt));
                GameInputHandler.OnMouseDown(mb);
            }
            else if (bs == ButtonState.Released && MouseButtonDown(mb))
            {
                Gui.GwenInput.ProcessMessage(new GwenInputMessage(IntersectInput.InputEvent.MouseUp,
                    GetMousePosition(), (int) mb, Keys.Alt));
                GameInputHandler.OnMouseUp(mb);
            }
        }

        public override void Update()
        {
            if (mMyGame.IsActive)
            {
                KeyboardState kbState = Keyboard.GetState();
                MouseState state = Mouse.GetState();

                if (state.X != mMouseX || state.Y != mMouseY)
                {
                    mMouseX = (int) (state.X * ((MonoRenderer) GameGraphics.Renderer).GetMouseOffset().X);
                    mMouseY = (int) (state.Y * ((MonoRenderer) GameGraphics.Renderer).GetMouseOffset().Y);
                    Gui.GwenInput.ProcessMessage(new GwenInputMessage(IntersectInput.InputEvent.MouseMove,
                        GetMousePosition(), (int) MouseButtons.None, Keys.Alt));
                }

                //Check for state changes in the left mouse button
                CheckMouseButton(state.LeftButton, MouseButtons.Left);
                CheckMouseButton(state.RightButton, MouseButtons.Right);
                CheckMouseButton(state.MiddleButton, MouseButtons.Middle);

                foreach (KeyValuePair<Keys, Microsoft.Xna.Framework.Input.Keys> key in mKeyDictionary)
                {
                    if (kbState.IsKeyDown(key.Value) && !mLastKeyboardState.IsKeyDown(key.Value))
                    {
                        Log.Diagnostic($"{key.Key.ToString()} -> {key.Value.ToString()}");
                        Gui.GwenInput.ProcessMessage(new GwenInputMessage(IntersectInput.InputEvent.KeyDown,
                            GetMousePosition(), (int) MouseButtons.None, key.Key));
                        GameInputHandler.OnKeyPressed(key.Key);
                    }
                    else if (!kbState.IsKeyDown(key.Value) && mLastKeyboardState.IsKeyDown(key.Value))
                    {
                        Gui.GwenInput.ProcessMessage(new GwenInputMessage(IntersectInput.InputEvent.KeyUp,
                            GetMousePosition(), (int) MouseButtons.None, key.Key));
                        GameInputHandler.OnKeyReleased(key.Key);
                    }
                }

                mLastKeyboardState = kbState;
                mLastMouseState = state;
            }
            else
            {
                foreach (KeyValuePair<Keys, Microsoft.Xna.Framework.Input.Keys> key in mKeyDictionary)
                {
                    if (mLastKeyboardState.IsKeyDown(key.Value))
                    {
                        Gui.GwenInput.ProcessMessage(new GwenInputMessage(IntersectInput.InputEvent.KeyUp,
                            GetMousePosition(), (int) MouseButtons.None, key.Key));
                        GameInputHandler.OnKeyReleased(key.Key);
                    }
                }
                CheckMouseButton(ButtonState.Released, MouseButtons.Left);
                CheckMouseButton(ButtonState.Released, MouseButtons.Right);
                CheckMouseButton(ButtonState.Released, MouseButtons.Middle);
                mLastKeyboardState = new KeyboardState();
                mLastMouseState = new MouseState();
            }
        }

        public override void OpenKeyboard(KeyboardType type, string text, bool autoCorrection, bool multiLine,
            bool secure)
        {
            return; //no on screen keyboard for pc clients
        }
    }
}