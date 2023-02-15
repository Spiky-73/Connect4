namespace Connect4;

public class Program {

    public static void Main(string[] args) {
        // NeuralNetwork.Mutator.Evolve(42, new int[] { 16, 16, 16, 7 },
        //     NeuralNetwork.NetworkAI.TestNetwork, 20, 500,
        //     "Networks/", 10
        // );

        Core.Connect4 game = new(7, 6, new Core.Player(), new NeuralNetwork.NetworkAI(new("Networks/Generation#10/network#1.txt")));
        game.Run(display: true);
    }
}