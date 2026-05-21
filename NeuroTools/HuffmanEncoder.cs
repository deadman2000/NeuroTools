using BitsKit.IO;

namespace NeuroTools;

public static class HuffmanEncoder
{
    public static byte[] Encode(byte[] data)
    {
        int length = data.Length;

        // Подсчёт частот
        var freq = new int[256];
        foreach (byte b in data)
            freq[b]++;

        var root = BuildHuffmanTree(freq);

        var buffer = new byte[Math.Max(4096, length * 2)];
        var writer = new BitWriter(buffer);

        // 1. Длина (32 бита LSB)
        writer.WriteInt32LSB(length, 32);

        // 2. Дерево
        WriteTree(writer, root);

        // 3. Коды символов
        var codes = BuildCodes(root);

        // 4. Данные
        foreach (byte b in data)
        {
            if (codes.TryGetValue(b, out var code))
            {
                foreach (bool bit in code)
                    writer.WriteBitMSB(bit);
            }
            else
            {
                throw new Exception($"Symbol {b} not in tree");
            }
        }

        // Обрезаем до реально записанных байт
        var totalBits = writer.Position;                    // текущая позиция = кол-во записанных бит
        var byteCount = (totalBits + 7) / 8;

        var result = new byte[byteCount];
        Array.Copy(buffer, result, byteCount);
        return result;
    }

    private static Node BuildHuffmanTree(int[] freq)
    {
        var pq = new PriorityQueue<Node, int>();

        int unique = 0;
        byte singleSymbol = 0;

        for (int i = 0; i < 256; i++)
        {
            if (freq[i] > 0)
            {
                unique++;
                singleSymbol = (byte)i;
                pq.Enqueue(new Node((byte)i, freq[i], null, null), freq[i]);
            }
        }

        if (unique == 0)
            return new Node(0, 0, null, null);

        if (unique == 1)
        {
            // Специальный случай: добавляем фиктивный лист
            var real = new Node(singleSymbol, freq[singleSymbol], null, null);
            byte dummyVal = singleSymbol == 0 ? (byte)1 : (byte)0;
            var dummy = new Node(dummyVal, 1, null, null); // частота не важна

            return new Node(0, real.Frequency + dummy.Frequency, real, dummy);
        }

        // Обычное построение
        while (pq.Count > 1)
        {
            var left = pq.Dequeue();   // меньшая частота
            var right = pq.Dequeue();

            var parent = new Node(
                Value: 0,
                Frequency: left.Frequency + right.Frequency,
                Left: left,      // Left  ← bit 1
                Right: right     // Right ← bit 0
            );

            pq.Enqueue(parent, parent.Frequency);
        }

        return pq.Dequeue();
    }

    private static void WriteTree(BitWriter writer, Node node)
    {
        if (node.Left == null && node.Right == null) // лист
        {
            writer.WriteBitMSB(true);
            writer.WriteUInt8MSB(node.Value, 8);
        }
        else // внутренний узел
        {
            writer.WriteBitMSB(false);
            WriteTree(writer, node.Right!);   // сначала right (как в BuildTree)
            WriteTree(writer, node.Left!);    // потом left
        }
    }

    private static Dictionary<byte, List<bool>> BuildCodes(Node root)
    {
        var codes = new Dictionary<byte, List<bool>>();

        void Traverse(Node node, List<bool> path)
        {
            if (node.Left == null && node.Right == null)
            {
                codes[node.Value] = [.. path];
                return;
            }

            // 1 → Left
            if (node.Left != null)
            {
                path.Add(true);
                Traverse(node.Left, path);
                path.RemoveAt(path.Count - 1);
            }

            // 0 → Right
            if (node.Right != null)
            {
                path.Add(false);
                Traverse(node.Right, path);
                path.RemoveAt(path.Count - 1);
            }
        }

        Traverse(root, []);
        return codes;
    }

    record Node(byte Value, int Frequency, Node? Left, Node? Right);
}
