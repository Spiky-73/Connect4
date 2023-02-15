namespace Connect4;

public class Program {

    public static void Main(string[] args) {
        Connect4 game = new(7, 6, new Players.NetworkAI(), new Players.RandomAI());
        game.Run(true);
    }
}