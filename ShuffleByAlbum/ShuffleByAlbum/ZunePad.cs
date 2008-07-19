using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ShuffleByAlbum
{
  public struct ZunePadState
  {
    GamePadState state;
    Vector2 flick;
    bool tapped;

    public bool IsTouched {
      get { return state.Buttons.LeftStick == ButtonState.Pressed; }
    }

    public bool IsTapped {
      get { return tapped; }
    }

    public Vector2 Flick {
      get { return flick; }
    }

    public Vector2 TouchPosition {
      get { return state.ThumbSticks.Left; }
    }

    public ButtonState BackButton {
      get { return state.Buttons.Back; }
    }

    public ButtonState PlayButton {
      get { return state.Buttons.B; }
    }

    //used when the zune's pad is clicked in the center
    public ButtonState SelectButton {
      get { return state.Buttons.A; }
    }

    //tells if the zune's pad has been clicked at any point
    public ButtonState PadPressed {
      get { return state.Buttons.LeftShoulder; }
    }

    public GamePadDPad DPad {
      get { return state.DPad; }
    }

    public ZunePadState(GamePadState state, Vector2 flick, bool tapped) {
      this.state = state;
      this.flick = flick;
      this.tapped = tapped;
    }
  }

  public static class ZunePad
  {
    static ZunePadState zps;

    static TimeSpan flickStartTime;
    static Vector2 flickStart;

    /// <summary>
    /// Gets the state of the Zune input allowing for flicks and taps.
    /// </summary>
    /// <param name="gameTime">The current time snapshot</param>
    /// <returns>The new ZunePadState object.</returns>
    public static ZunePadState GetState(GameTime gameTime) {
      GamePadState gps = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.None);
      Vector2 flick = Vector2.Zero;
      bool tapped = false;

      if (gps.Buttons.LeftStick == ButtonState.Pressed && !zps.IsTouched) {
        flickStart = gps.ThumbSticks.Left;
        flickStartTime = gameTime.TotalGameTime;
      }
      else if (gps.Buttons.LeftStick == ButtonState.Released && zps.IsTouched) {
        flick = zps.TouchPosition - flickStart;
        TimeSpan elapsed = gameTime.TotalGameTime - flickStartTime;

        //scale the flick based on how long it took
        flick /= (float)elapsed.TotalSeconds;

        //adjust the .5 and .3 to fit your sensitivity needs. .5 and .3 seem
        //to be pretty decent, but they might need tweaking for some situations
        tapped = (flick.Length() < .5f && elapsed.TotalSeconds < .3f);

        flickStart = Vector2.Zero;
      }

      zps = new ZunePadState(gps, flick, tapped);

      return zps;
    }

    /// <summary>
    /// Gets the state of the Zune input without flicks or taps.
    /// </summary>
    /// <returns>The new ZunePadState object.</returns>
    public static ZunePadState GetState() {
      GamePadState gps = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.None);

      zps = new ZunePadState(gps, Vector2.Zero, false);

      return zps;
    }
  }
}