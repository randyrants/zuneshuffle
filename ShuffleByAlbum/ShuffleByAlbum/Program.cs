using System;

namespace ShuffleByAlbum
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main(string[] args) {
      using (ShuffleByAlbum game = new ShuffleByAlbum()) {
        game.Run();
      }
    }
  }
}
