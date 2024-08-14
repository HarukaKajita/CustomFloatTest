// See https://aka.ms/new-console-template for more information

using System.Text;

namespace CustomFloatTest;

internal static class Program
{
    struct DecomposedFloat
    {
        public float f;
        public uint asUInt;
        public uint sign;
        public uint exponent;
        public uint mantissa;
        
        public string ToString()
        {
            var intPart = (int)f;
            var fracPart = f - intPart;
            var intStr = intPart.ToString("D9");
            var fracStr = fracPart.ToString("F10").Substring(1);
            return $"f: {intStr}{fracStr} asUInt: {asUInt:D10}, sign: {sign}, exponent: {SignedIntString((int)exponent,3)}:{SignedIntString((int)exponent-127,3)}, mantissa: {mantissa}, binary: {FloatToBinary(f)}";
        }

        private string SignedIntString(int value, int bitDepth)
        {
            var sign = value < 0 ? "" : "+";
            var absValue = Math.Abs(value);
            return $"{sign}{value.ToString("D"+bitDepth)}";
        }
    }
    private static void Main()
    {
        // 乱数生成
        List<float> values = new List<float>();
        List<float> shiftedValues = new List<float>();
        List<DecomposedFloat> decomposedValues = new List<DecomposedFloat>();
        List<DecomposedFloat> shiftedDecomposedValues = new List<DecomposedFloat>();

        // values.AddRange(GenerateRandomFloats(10));
        Random r = new Random();
        for (var i = 0; i < 10; i++)
            values.Add((float)(r.NextDouble()*Math.Pow(2, r.Next(-20, 20))));
        foreach (var v in values)
            shiftedValues.Add((v+1));
        foreach (var v in values)
            decomposedValues.Add(DecomposerFloat(v));
        foreach (var v in shiftedValues)
            shiftedDecomposedValues.Add(DecomposerFloat(v));
        
        // 乱数確認
        Console.WriteLine("Values:");
        foreach (var d in decomposedValues)
            Console.WriteLine(d.ToString());
        Console.WriteLine("ShiftedValues:");
        foreach (var d in shiftedDecomposedValues)
            Console.WriteLine(d.ToString());
        
        // 最大値、最小値
        var max = values.Max();
        var min = values.Min();
        var shiftedMax = shiftedValues.Max();
        var shiftedMin = shiftedValues.Min();
        Console.WriteLine("Max: " + max);
        Console.WriteLine("Min: " + min);
        Console.WriteLine("Max: " + shiftedMax);
        Console.WriteLine("Min: " + shiftedMin);

        var maxInfo = DecomposerFloat(max);
        var minInfo = DecomposerFloat(min);
        var shiftedMaxInfo = DecomposerFloat(shiftedMax);
        var shiftedMinInfo = DecomposerFloat(shiftedMin);
        
        Console.WriteLine(maxInfo.ToString());
        Console.WriteLine(minInfo.ToString());
        Console.WriteLine(shiftedMaxInfo.ToString());
        Console.WriteLine(shiftedMinInfo.ToString());
        
        var exponentRange = maxInfo.exponent - minInfo.exponent;
        var exponentBias = (int)minInfo.exponent-127;
        Console.WriteLine($"exponentRange: {exponentRange} ({maxInfo.exponent} - {minInfo.exponent}), Bias: {exponentBias} ({minInfo.exponent} - 127)");
        var exponentBitDepth = (int)Math.Ceiling(Math.Log2(exponentRange));
        Console.WriteLine($"exponentBitDepth: {exponentBitDepth} ({Math.Log2(exponentRange)})");
        
        var shiftedExponentRange = shiftedMaxInfo.exponent - shiftedMinInfo.exponent;
        var shiftedExponentBias = (int)shiftedMinInfo.exponent-127;
        Console.WriteLine($"exponentRange: {shiftedExponentRange} ({shiftedMaxInfo.exponent} - {shiftedMinInfo.exponent}), Bias: {shiftedExponentBias} ({shiftedMinInfo.exponent} - 127)");
        var shiftedExponentBitDepth = (int)Math.Ceiling(Math.Log2(shiftedExponentRange));
        Console.WriteLine($"exponentBitDepth: {shiftedExponentBitDepth} ({Math.Log2(shiftedExponentRange)})");


        for (var i = 0; i < values.Count; i++)
        {
            var unshifted = shiftedValues[i]-1;
            var diff = values[i] - unshifted;
            Console.WriteLine($"diff: {diff.ToString("F20")} --- {values[i]} - {unshifted}");
        }
        
        
        // var f0 = 1.23456f;
        var f1 = 1;
        PrintFloat(f1);
    }

    private static DecomposedFloat DecomposerFloat(float value)
    {
        //as uint
        var maxAsUInt = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
        // 符号bit
        var sign = (maxAsUInt & 0x80000000) >> 31;
        // 指数部
        var exponent = (maxAsUInt & 0x7F800000) >> 23;
        // 仮数部
        var mantissa = (maxAsUInt & 0x007FFFFF);
        DecomposedFloat df = new DecomposedFloat
        {
            f = value,
            asUInt = maxAsUInt,
            sign = sign,
            exponent = exponent,
            mantissa = mantissa
        };
        return df;
    }

    static float[] GenerateRandomFloats(int count)
    {
        float[] randomFloats = new float[count];
        Random rand = new Random();
        for (int i = 0; i < count; i++) {
            var array = new byte[4];
            rand.NextBytes(array);
            float randomFloat = BitConverter.ToSingle(array, 0);
            randomFloats[i] = randomFloat;
        }
        return randomFloats;
    }

    static void PrintFloat(float f)
    {
        Console.WriteLine("--------------------");
        Console.WriteLine(f);
        Console.WriteLine(ToHexString(f));
        Console.WriteLine(FloatToBinary(f));
        Console.WriteLine("--------------------");
    }
    static string FloatToBinary(float f)
    {
        StringBuilder sb = new StringBuilder();
        Byte[] ba = BitConverter.GetBytes(f);
        foreach (Byte b in ba)
            for (int i = 0; i < 8; i++)
            {
                sb.Insert(0,((b>>i) & 1) == 1 ? "1" : "0");
            }
        string s = sb.ToString();
        string r = s.Substring(0, 1) + " " + s.Substring(1, 8) + " " + s.Substring(9); //sign exponent mantissa
        return r;
    }

    static string ToHexString(float f) {
        var bytes = BitConverter.GetBytes(f);
        var i = BitConverter.ToInt32(bytes, 0);
        return "0x" + i.ToString("X8");
    }
}

/*
 * メモ
 * 定義域が[N, N+4)    (2^+2)の範囲なら[4, 8)     の範囲のfloatに変換することで、いい感じにできるかも
 * 定義域が[N, N+2)    (2^+1)の範囲なら[2, 4)     の範囲のfloatに変換することで、いい感じにできるかも
 * 定義域が[N, N+1)    (2^0 )の範囲なら[1, 2)     の範囲のfloatに変換することで、いい感じにできるかも
 * 定義域が[N, N+0.5)  (2^-1)の範囲なら[0.5, 1)   の範囲のfloatに変換することで、いい感じにできるかも
 * 定義域が[N, 2+0.25) (2^-2)の範囲なら[0.25, 0.5)の範囲のfloatに変換することで、いい感じにできるかも
 */