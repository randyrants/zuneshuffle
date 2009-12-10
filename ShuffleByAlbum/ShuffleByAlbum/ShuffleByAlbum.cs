using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;

namespace ShuffleByAlbum
{
  /// <summary>
  /// This is the main type for your game
  /// </summary>
  public class ShuffleByAlbum : Microsoft.Xna.Framework.Game
  {
    // Length of a flick on the touchpad
    const float FlickLength = 2.0f;

    // Drawing members
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    SpriteFont regularFont, boldFont;
    Texture2D currentAlbumArt,
              previousAlbumArt,
              nextAlbumArt,
              defaultAlbumArt,
              highlightAlbumArt,
              cursorTexture;

    // Input members
    ZunePadState oldZunePadState;

    // Uses one Random class to minimize overlap
    Random random;

    // Media/player members
    AlbumCollection albumCollection;
    SongCollection songCollection;
    bool wasPausePressed;

    // High level members, set by Update, displayed by Draw
    string currentAlbum, currentArtist, currentSong;
    string currentVolume, currentElapsedTime, currentRemianingTime;
    string currentState, currentTrack;
    int currentIndex, nextIndex;

    public ShuffleByAlbum() {
      graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";
    }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize() {
      // set to be 30 fps 
      TargetElapsedTime = TimeSpan.FromSeconds(1 / 30.0);
      
      // load the media library and initialize a media queue
      MediaLibrary mediaLibrary = new MediaLibrary();
      albumCollection = mediaLibrary.Albums;
      wasPausePressed = false;

      // initialize the member variables
      currentAlbum = "Shuffle by Album";
      currentArtist = "version 1.1 - XNA 3.1.10527.0";
      currentSong = "press play to begin";
      currentVolume = "";
      currentElapsedTime = "00:00";
      currentRemianingTime = "-00:00";
      currentTrack = "";
      currentState = "";

      // set to null and 0 since there's no song playing yet
      currentIndex = 0;
      nextIndex = -1;
      currentAlbumArt = null;
      previousAlbumArt = null;
      nextAlbumArt = null;
      songCollection = null;

      // seed with a random number
      random = new Random((int)DateTime.Now.Ticks);

      // make sure the player is stopped
      MediaPlayer.Stop();

      base.Initialize();
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent() {
      // Create a new SpriteBatch, which can be used to draw textures.
      spriteBatch = new SpriteBatch(GraphicsDevice);

      // load your two fonts
      boldFont = Content.Load<SpriteFont>("boldFont");
      regularFont = Content.Load<SpriteFont>("regularFont");

      // load/set default images
      defaultAlbumArt = Content.Load<Texture2D>("stockImage");
      currentAlbumArt = defaultAlbumArt;
      highlightAlbumArt = Content.Load<Texture2D>("highlight");
      previousAlbumArt = null;
      nextAlbumArt = null;
      cursorTexture = Content.Load<Texture2D>("cursor");
    }

    /// <summary>
    /// UnloadContent will be called once per game and is the place to unload
    /// all content.
    /// </summary>
    protected override void UnloadContent() {
      // nothing that requires an undelete
    }

    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime) {
      // get the current gamepad state
      ZunePadState zunePadState = ZunePad.GetState(gameTime);

      // see what was pressed before and what the new state is, toggling states accordingly
      bool backKey = (oldZunePadState.BackButton == ButtonState.Pressed) && (zunePadState.BackButton == ButtonState.Released);
      bool playKey = (oldZunePadState.PlayButton == ButtonState.Pressed) && (zunePadState.PlayButton == ButtonState.Released);
      bool selectKey = (oldZunePadState.SelectButton == ButtonState.Pressed) && (zunePadState.SelectButton == ButtonState.Released);
      bool upKey = (oldZunePadState.DPad.Up == ButtonState.Pressed) && (zunePadState.DPad.Up == ButtonState.Released);
      bool downKey = (oldZunePadState.DPad.Down == ButtonState.Pressed) && (zunePadState.DPad.Down == ButtonState.Released);
      bool rightKey = (oldZunePadState.DPad.Right == ButtonState.Pressed) && (zunePadState.DPad.Right == ButtonState.Released);
      bool leftKey = (oldZunePadState.DPad.Left == ButtonState.Pressed) && (zunePadState.DPad.Left == ButtonState.Released);

      // capture the current state and store it for the next loop
      oldZunePadState = zunePadState;

      // stop the player and exit the app
      if (backKey) {
        MediaPlayer.Stop();
        this.Exit();
      }

      // pressing play will toggle the music (and set a flag to show it was a manual selected state)
      // if a track is currently playing, pause it 
      // if it was paused, allow it to resume
      // if the player was stopped, play the collection
      else if (playKey) {
        if (MediaPlayer.State == MediaState.Playing) {
          MediaPlayer.Pause();
          wasPausePressed = true;
        }
        else if (MediaPlayer.State == MediaState.Paused) {
          MediaPlayer.Resume();
          wasPausePressed = false;
        }
        else {
          if (songCollection == null) {
            SetNextAlbum();
          }
          MediaPlayer.Play(songCollection);
        }
      }

      // select simply moves to the next disc
      else if (selectKey) {
        StopStartPlay();
      }

      // adjust the current app's volume (not that device)
      else if ((upKey) || (zunePadState.Flick.Y > FlickLength)) {
        MediaPlayer.Volume = MediaPlayer.Volume + 0.02f;
      }

      // adjust the current app's volume (not that device)
      else if ((downKey) || (zunePadState.Flick.Y < -FlickLength)) {
        MediaPlayer.Volume = MediaPlayer.Volume - 0.02f;
      }

      // if there IS another track on the current album go to it
      // if there isn't, jump to the next track
      else if ((rightKey) || (zunePadState.Flick.X > FlickLength)) {
        if (MediaPlayer.State != MediaState.Stopped) {
          if (IsLastSong()) {
            StopStartPlay();
          }
          else {
            MediaPlayer.MoveNext();
          }
        }
      }

      // if less than five seconds on the current track has been played, move back a the track
      // more than five seconds, restart the track
      else if ((leftKey) || (zunePadState.Flick.X < -FlickLength)) {
        if (MediaPlayer.State != MediaState.Stopped) {
          if (MediaPlayer.PlayPosition.TotalSeconds <= 5) {
            MediaPlayer.MovePrevious();
          }
          else {
            // Restart this song
            int index = MediaPlayer.Queue.ActiveSongIndex;
            MediaPlayer.Stop();
            MediaPlayer.Play(songCollection, index);
          }
        }
      }

      // update all of the visual fields
      TimeSpan remainingTime;
      switch (MediaPlayer.State) {
        case MediaState.Playing:
          currentState = "";
          currentElapsedTime = String.Format("{0,2:00}:{1,2:00}", MediaPlayer.PlayPosition.Minutes, MediaPlayer.PlayPosition.Seconds);
          remainingTime = MediaPlayer.Queue.ActiveSong.Duration - MediaPlayer.PlayPosition;
          currentRemianingTime = String.Format("-{0,2:00}:{1,2:00}", remainingTime.Minutes, remainingTime.Seconds);
          currentSong = MediaPlayer.Queue.ActiveSong.Name;
          currentTrack = String.Format("Track: {0,2:00} of {1,2:00}", MediaPlayer.Queue.ActiveSongIndex + 1, MediaPlayer.Queue.Count);
          break;
        case MediaState.Paused:
          if ((!wasPausePressed) && (MediaPlayer.PlayPosition.TotalSeconds == 0)) {
            StopStartPlay();
            break;
          }
          currentState = "-- paused --";
          currentElapsedTime = String.Format("{0,2:00}:{1,2:00}", MediaPlayer.PlayPosition.Minutes, MediaPlayer.PlayPosition.Seconds);
          remainingTime = MediaPlayer.Queue.ActiveSong.Duration - MediaPlayer.PlayPosition;
          currentRemianingTime = String.Format("-{0,2:00}:{1,2:00}", remainingTime.Minutes, remainingTime.Seconds);

          currentSong = MediaPlayer.Queue.ActiveSong.Name;
          currentTrack = String.Format("Track: {0,2:00} of {1,2:00}", MediaPlayer.Queue.ActiveSongIndex + 1, MediaPlayer.Queue.Count);
          break;
        default:
          currentState = "-- stopped --";
          currentElapsedTime = "00:00";
          currentRemianingTime = "-00:00";
          break;
      }
      currentVolume = String.Format("Vol: {0,2:00.0}", MediaPlayer.Volume * 10.0f);
      
      // required call
      base.Update(gameTime);
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime) {
      graphics.GraphicsDevice.Clear(Color.Black);

      // draw all of the visuals in one batch
      spriteBatch.Begin();
      // Draw all of the text for timing
      spriteBatch.DrawString(regularFont, currentElapsedTime, new Vector2(2, 222), Color.White);
      spriteBatch.DrawString(regularFont, currentRemianingTime, new Vector2(200, 222), Color.White);
      // Draw the current state
      spriteBatch.DrawString(regularFont, currentState, new Vector2(78, 222), Color.White);
      // Draw the track information
      spriteBatch.DrawString(boldFont, currentSong, new Vector2(2, 254), Color.White);
      spriteBatch.DrawString(regularFont, currentArtist, new Vector2(2, 270), Color.White);
      spriteBatch.DrawString(regularFont, currentAlbum, new Vector2(2, 286), Color.White);
      spriteBatch.DrawString(regularFont, currentTrack, new Vector2(1, 302), Color.White);
      // Draw the volume
      spriteBatch.DrawString(regularFont, currentVolume, new Vector2(182, 302), Color.White);
      // Draw the three different album arts
      if (previousAlbumArt != null)
        spriteBatch.Draw(previousAlbumArt, new Rectangle(0, 50, 120, 120), Color.Gray);
      if (nextAlbumArt != null)
        spriteBatch.Draw(nextAlbumArt, new Rectangle(180, 50, 120, 120), Color.Gray);
      if (currentAlbumArt != null) {
        spriteBatch.Draw(highlightAlbumArt, new Rectangle(29, 19, 182, 182), Color.White);
        spriteBatch.Draw(currentAlbumArt, new Rectangle(30, 20, 180, 180), Color.White);
      }
      // Draw the current track bar
      spriteBatch.Draw(highlightAlbumArt, new Rectangle(0, 221, 240, 2), Color.White);
      if (MediaPlayer.State == MediaState.Playing) {
        int x = 0;
        x = (((int)MediaPlayer.PlayPosition.TotalSeconds * 100 / (int)MediaPlayer.Queue.ActiveSong.Duration.TotalSeconds) * 240 / 100) - 20;
        spriteBatch.Draw(cursorTexture, new Rectangle(x, 219, 20, 6), Color.White);
      }
      spriteBatch.End();

      base.Draw(gameTime);
    }

    private void SetNextAlbum() {

      // Set the previous album art to the current running album, unless its the first time running
      try {
        if (nextIndex < 0) {
          previousAlbumArt = null;
        }
        else {
          previousAlbumArt = currentAlbumArt;
        }
      }
      catch {
        previousAlbumArt = defaultAlbumArt;
      }

      // if this is the first time running, grab a random number that will be used as the current album
      if (nextIndex < 0) {
        nextIndex = random.Next(albumCollection.Count);
      }

      // use the forward looking index and select the new album
      currentIndex = nextIndex;
      currentAlbum = albumCollection[currentIndex].Name;
      currentArtist = albumCollection[currentIndex].Artist.Name;

      // there is conflicting information coming from the object model at times - HasArt returns true even if hasArt is false
      try {
        if (albumCollection[currentIndex].HasArt)
          currentAlbumArt = albumCollection[currentIndex].GetAlbumArt(Content.ServiceProvider);
        else
          currentAlbumArt = defaultAlbumArt;
      }
      catch {
        currentAlbumArt = defaultAlbumArt;
      }

      // now get the next random album to show the "next" disc to come
      nextIndex = random.Next(albumCollection.Count);
      try {
        if (albumCollection[nextIndex].HasArt)
          nextAlbumArt = albumCollection[nextIndex].GetAlbumArt(Content.ServiceProvider);
        else
          nextAlbumArt = defaultAlbumArt;
      }
      catch {
        nextAlbumArt = defaultAlbumArt;
      }

      // put the selected album's songs into the member selection
      songCollection = albumCollection[currentIndex].Songs;
    }

    // helper functions

    private void StopStartPlay() {
      MediaPlayer.Stop();
      SetNextAlbum();
      MediaPlayer.Play(songCollection);
    }

    private bool IsLastSong() {
      return (MediaPlayer.Queue.ActiveSongIndex + 1 == MediaPlayer.Queue.Count);
    }
  }
}
