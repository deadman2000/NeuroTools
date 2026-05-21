using BitsKit.IO;

namespace NeuroTools;

public static class HuffmanDecoder
{
    public static byte[] Decode(byte[] src, int offset = 0)
    {
        var bits = new BitReader(src);
        bits.Seek(offset * 8, SeekOrigin.Begin);

        var decodedLength = bits.ReadInt32LSB(32);
        if (decodedLength > 100000)
        {
            throw new Exception($"Wrong length {decodedLength}");
        }

        var result = new byte[decodedLength];

        var root = BuildTree(bits);
        var node = root;

        var i = 0;
        while (i < decodedLength)
        {
            node = (bits.ReadBitMSB() ? node.Left : node.Right) ?? throw new Exception($"Tree build error");
            if (node.Left == null)
            {
                result[i++] = node.Value;
                node = root;
            }
        }

        return result;
    }

    private static Node BuildTree(BitReader bits)
    {
        if (bits.ReadBitMSB())
        {
            return new(bits.ReadUInt8MSB(8), null, null);
        }

        var right = BuildTree(bits);
        var left = BuildTree(bits);
        return new Node(0, left, right);
    }

    record Node(byte Value, Node? Left, Node? Right);
}
