
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

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

    public bool Dash = false;

    public bool Next = false;

    public Vector2 MousePosition;

    public bool NextRelease = false;
}

public class InputManager
{
    private InputState previous = new();
    public InputState current = new();

    public void Update(KeyboardState state, MouseState mouseState, Camera<Vector2> camera = null)
    {

        current.Up = state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.W);
        current.Down = state.IsKeyDown(Keys.Down) || state.IsKeyDown(Keys.S);
        current.Left = state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.A);
        current.Right = state.IsKeyDown(Keys.Right) || state.IsKeyDown(Keys.D);
        current.Space = state.IsKeyDown(Keys.Space);

        current.SpacePulse = current.Space && !previous.Space;
        var plus = state.IsKeyDown(Keys.OemPlus);
        var plusUp = state.IsKeyUp(Keys.OemPlus);
        current.NextRelease = plusUp;
        current.Next = plus && !previous.Next && previous.NextRelease; // it's down, it wasn't down, and it wasn't released.



        current.SpaceHold = current.Space && previous.Space;


        current.LeftClick = mouseState.LeftButton == ButtonState.Pressed;
        current.RightClick = mouseState.RightButton == ButtonState.Pressed;
        current.MiddleClick = mouseState.MiddleButton == ButtonState.Pressed;
        current.LeftHold = current.LeftClick && previous.LeftClick;
        current.RightHold = current.RightHold && previous.RightHold;
        current.MiddleHold = current.MiddleHold && previous.MiddleHold;
        if (camera != null)
        {
            current.MousePosition = camera.ScreenToWorld(mouseState.Position.ToVector2());
        }
        else
        {

            current.MousePosition = mouseState.Position.ToVector2();
        }

        current.Shoot = current.LeftClick; // let allow rapid fire./
        
        current.Dash = current.SpacePulse;

        previous = current with { };
    }
}


