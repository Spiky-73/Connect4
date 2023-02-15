namespace Connect4;

public class Program {

    public static void Main(string[] args) {
        NeuralNetwork[] roots = new NeuralNetwork[20];
        for (int i = 0; i < roots.Length; i++) roots[i] = new(6 * 7, 16, 16, 16, 7);

        int gen = 0;
        while (gen < 50) {
            Evolution.Mutations.NextGeneration(roots, 500, (NeuralNetwork child, NeuralNetwork other) => {
                int fitness = 0;
                Connect4 game = new(7, 6, new Players.NetworkAI(child), new Players.NetworkAI(other));
                fitness += Players.NetworkAI.Fitness(game.Run(0)) + Players.NetworkAI.Fitness(game.Run(1));
                return fitness;
            }, $"Networks/Generation#{gen}");
            gen++;
        }

        Connect4 game = new(7, 6, new Players.Human(), new Players.NetworkAI(new("Networks/Generation#26/network#1.NN")));
        game.Run(display: true);
    }
}