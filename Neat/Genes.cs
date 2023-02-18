namespace Connect4.Neat.Genes;

public readonly struct Node : IComparable<Node> {
    public readonly int Id;
    public readonly float X;

    internal Node(int id, float x) {
        Id = id;
        X = x;
    }

    public int CompareTo(Node other) => Id.CompareTo(other.Id);

}

public struct Connection : IComparable<Connection> {

    public readonly int Innovation;
    public readonly Node From, To;

    public float weight = 1;
    public bool disabled = false;

    internal Connection(int innov, Node from, Node to) {
        Innovation = innov;
        From = from;
        To = to;
    }

    public int CompareTo(Connection other) => Innovation.CompareTo(other.Innovation);
}