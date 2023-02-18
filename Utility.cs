namespace Connect4;

public static class Utility {
    public static readonly Random Random = new();
    public static readonly object RNGLock = new();

    public static float[] RandomArray(int length) {
        float[] array = new float[length];
        lock (RNGLock) {
            for (int i = 0; i < array.Length; i++) array[i] = (Random.NextSingle() - 0.5f) * 4;
        }
        return array;
    }

    public static T WeigthedChoice<T>(ICollection<T> choices, Func<T, float> weight) {
        float totalWeight = 0;
        foreach(T c in choices) totalWeight += weight(c);

        float choice = Random.NextSingle() * totalWeight;
        float sum = 0;
        foreach(T c in choices) {
            sum += weight(c);
            if(choice < sum) return c;
        }
        throw new NotSupportedException();
    }

    public static float ReLU(float f) => f < 0 ? 0 : f;

    public enum InclusionFlag {
        Min = 0x01,
        Max = 0x10,
        Both = Min | Max
    }

    public static bool InRange<T>(this T self, T min, T max, InclusionFlag flags = InclusionFlag.Min) where T : IComparable<T> {
        int l = self.CompareTo(min);
        int r = self.CompareTo(max);
        return (l > 0 || (flags.HasFlag(InclusionFlag.Min) && l == 0)) && (r < 0 || (flags.HasFlag(InclusionFlag.Max) && r == 0));
    }
}