using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilderRPG.Content
{
    public class Input_Manager
    {
        public KeyboardState currentKeyboardState;
        public KeyboardState previousKeyboardState;
        public MouseState currentMouseState;
        public MouseState previousMouseState;
        public Vector2 mousePosition;

        private static Input_Manager instance;

        public static Input_Manager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Input_Manager();
                }
                return instance;
            }
        }

        private Input_Manager() { }

        public void PreUpdate()
        {
            previousKeyboardState = currentKeyboardState;
            previousMouseState = currentMouseState;
        }

        public void PostUpdate()
        {
            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();

            mousePosition = new Vector2(currentMouseState.X, currentMouseState.Y);
        }

        public bool IsKeyDown(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key);
        }

        public bool IsKeySinglePress(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
        }

        public bool IsKeyReleased(Keys key)
        {
            return !currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key);
        }

        public bool IsButtonReleased(bool leftClick)
        {
            return leftClick
                ? currentMouseState.LeftButton == ButtonState.Released
                : currentMouseState.RightButton == ButtonState.Released;
        }

        public bool IsButtonPressed(bool leftClick)
        {
            return leftClick
                ? currentMouseState.LeftButton == ButtonState.Pressed
                : currentMouseState.RightButton == ButtonState.Pressed;
        }

        public bool IsButtonSingleClick(bool leftClick)
        {
            return leftClick
                ? currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released
                : currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released;
        }

        public bool IsMouseOnInventory()
        {
            Rectangle inventoryRectangle = new Rectangle((int)Main.inventoryPos.X, (int)Main.inventoryPos.Y - 24, 190, Main.texInventory.Height + Main.texInventoryExtras.Height);
            return inventoryRectangle.Contains(mousePosition) ? true : false;
        }
    }

}
