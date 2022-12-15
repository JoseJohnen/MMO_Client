using Stride.Engine;

namespace MMO_Client
{
    class MMO_ClientApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
