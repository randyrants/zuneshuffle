## Zune Shuffle by Album

## Project Description
This "game" is actually an application for Zune devices, built by using the XNA Game Studio 3.0 SDK  Use it for when you want the player to randomly select the first track of an album, plays the entire album, and then selects a new album.

**Overview**: The majority of the heavy lifting for this Zune app is done by the Framework.  The Framework exposes all of the information about music library including the track information and album art.  That said, within the project there are three major components, all of which were designed to mirror the built-in playback controls of the Zune:

Content - this includes all of the stock bitmaps and fonts that the application uses as part of its UI.  

ZunePad.cs - this is a generic interface to all of the Zune controls, including the Z-Pad that is part of the new 4/8/80/120 devices.  Picked that up off [http://xnawiki.com/index.php?title=ZunePadState](http://xnawiki.com/index.php?title=ZunePadState) and it's a very handy class to have.

ShuffleByAlbum.cs - the main functionality of the application lives here.

## Features/Instructions:
* Play/Pause works as you'd expect it to
* Left/Right is track control - Right is always next; Left is "restart current track" unless its in the first five seconds of a track
* Up/Down is for relative volume control
* Center button is next album
* Back button exits the application
* Album art is used to show the current, previous, and next album

## Known limitations:
* Unplugging a set of headphones does <i>not</i> pause playback - no access to see this via the Framework
* Screen stays on at all times - no access to dim/turn off the screen via the Framework
* Volume is relative to the app - no access to device-level volume control
* Application reboots on exit - all games do this
* Playback-bar outlook is cheesy - no skills for art work
