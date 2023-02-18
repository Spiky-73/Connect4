namespace Connect4;

public class Program {

    public static void Main(string[] args) {
        SharpNeat.RealNeatAI.Test();
        Neat.NetworkAI.Test();
        FixedNetwork.NetworkAI.Test();
    }
}