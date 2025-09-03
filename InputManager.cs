
using Microsoft.Xna.Framework.Input;

public record InputState
{
    public bool Up = false;
    public bool Down = false;
    public bool Left = false;
    public bool Right = false;
    public bool Space = false;
    public bool SpacePulse = false;
    public bool SpaceHold = false;
    public bool Esc = false;
    public bool LeftClick = false;
    public bool RightClick = false;
    public bool MiddleClick = false;
    public bool LeftHold = false;
    public bool RightHold = false;
    public bool MiddleHold = false;

    public bool Shoot = false;

    public Point MousePosition;
}

public class InputManager
{
    private InputState previous = new();
    public InputState current = new();

    public void Update(KeyboardState state, MouseState mouseState)
    {
        current.Up = state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.W);
        current.Down = state.IsKeyDown(Keys.Down) || state.IsKeyDown(Keys.S);
        current.Left = state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.A);
        current.Right = state.IsKeyDown(Keys.Right) || state.IsKeyDown(Keys.D);
        current.Space = state.IsKeyDown(Keys.Space);
        current.SpacePulse = current.Space && !previous.Space;
        current.SpaceHold = current.Space && previous.Space;


        current.LeftClick = mouseState.LeftButton == ButtonState.Pressed;
        current.RightClick = mouseState.RightButton == ButtonState.Pressed;
        current.MiddleClick = mouseState.MiddleButton == ButtonState.Pressed;
        current.LeftHold = current.LeftClick && previous.LeftClick;
        current.RightHold = current.RightHold && previous.RightHold;
        current.MiddleHold = current.MiddleHold && previous.MiddleHold;
        current.MousePosition = mouseState.Position;
        current.Shoot = (current.LeftClick && !current.LeftHold) || current.SpacePulse;
        

        previous = current with { };
    }
}


