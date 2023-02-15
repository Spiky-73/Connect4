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

    public static float ReLU(float f) => f < 0 ? 0 : f;
}