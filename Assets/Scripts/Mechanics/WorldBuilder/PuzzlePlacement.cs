/*using Unity.Collections;
using Unity.Netcode;

public struct PuzzlePlacement : INetworkSerializable
{
    public int PuzzleIndex;   // índice en puzzles[]
    public int RouteIndex;    // 0..3
    public int LevelIndex;    // 0..3
    public FixedString32Bytes Color;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref PuzzleIndex);
        serializer.SerializeValue(ref RouteIndex);
        serializer.SerializeValue(ref LevelIndex);
        serializer.SerializeValue(ref Color);
    }
}*/