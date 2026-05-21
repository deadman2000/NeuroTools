using NeuroTools;

namespace NeuroTests;

public class HuffmanTests
{
    [Theory]
    [MemberData(nameof(GetRandomData))]
    public void RandomPackUnpack(byte[] data)
    {
        var encoded = HuffmanEncoder.Encode(data);
        var decoded = HuffmanDecoder.Decode(encoded);
        Assert.Equal(data, decoded);
    }

    public static IEnumerable<object[]> GetRandomData()
    {
        var random = new Random(0);

        for (int i = 0; i < 50; i++) // 50 случайных наборов
        {
            var length = random.Next(1, 4000);
            var bytes = new byte[length];
            random.NextBytes(bytes);

            yield return new object[]
            {
                bytes
            };
        }
    }
}
