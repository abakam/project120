using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;


namespace CashvaultCore.Utilities
{
    public class CryptoRandom : Random
    {
        private RNGCryptoServiceProvider _rng =
            new RNGCryptoServiceProvider();
        private byte[] _uint32Buffer = new byte[4];

        public CryptoRandom() { }
        public CryptoRandom(Int32 ignoredSeed) { }

        public override Int32 Next()
        {
            _rng.GetBytes(_uint32Buffer);
            return BitConverter.ToInt32(_uint32Buffer, 0) & 0x7FFFFFFF;
        }

        public override Int32 Next(Int32 maxValue)
        {
            if (maxValue < 0)
                throw new ArgumentOutOfRangeException("maxValue");
            return Next(0, maxValue);
        }

        public override Int32 Next(Int32 minValue, Int32 maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException("minValue");
            if (minValue == maxValue) return minValue;
            Int64 diff = maxValue - minValue;
            while (true)
            {
                _rng.GetBytes(_uint32Buffer);
                UInt32 rand = BitConverter.ToUInt32(_uint32Buffer, 0);

                Int64 max = (1 + (Int64)UInt32.MaxValue);
                Int64 remainder = max % diff;
                if (rand < max - remainder)
                {
                    return (Int32)(minValue + (rand % diff));
                }
            }
        }

        public override double NextDouble()
        {
            _rng.GetBytes(_uint32Buffer);
            UInt32 rand = BitConverter.ToUInt32(_uint32Buffer, 0);
            return rand / (1.0 + UInt32.MaxValue);
        }

        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            _rng.GetBytes(buffer);
        }

    }



    public sealed class SecureRandom : Random
    {

        private readonly RandomNumberGenerator _rng;

        public SecureRandom()
        {
            _rng = new RNGCryptoServiceProvider();
        }

        public SecureRandom(int seed)
        {
            var rgb = BitConverter.GetBytes(seed);
            _rng = new RNGCryptoServiceProvider(rgb);
        }

        public override int Next()
        {
            var data = new byte[sizeof(int)];
            _rng.GetBytes(data);
            return BitConverter.ToInt32(data, 0) & (int.MaxValue - 1);
        }

        public override int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue", minValue, "minValue must be less than or equals to maxValue");
            }
            return (int)Math.Floor(minValue + (maxValue - minValue) * NextDouble());
        }

        public override double NextDouble()
        {
            var data = new byte[sizeof(uint)];
            _rng.GetBytes(data);
            var randUint = BitConverter.ToUInt32(data, 0);
            return randUint / (uint.MaxValue + 1.0);
        }

        public override void NextBytes(byte[] data)
        {
            _rng.GetBytes(data);
        }

        public override string ToString()
        {
            return _rng.ToString();
        }

        public override bool Equals(object obj)
        {
            return _rng.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _rng.GetHashCode();
        }
    }

    public sealed class Dice : Random
    {

        private readonly RandomNumberGenerator _rng;

        public Dice()
        {
            _rng = new RNGCryptoServiceProvider();
        }

        public Dice(int seed)
        {
            var rgb = BitConverter.GetBytes(seed);
            _rng = new RNGCryptoServiceProvider(rgb);
        }

        public override int Next()
        {
            var data = new byte[sizeof(int)];
            _rng.GetBytes(data);
            return BitConverter.ToInt32(data, 0) & (int.MaxValue - 1);
        }

        public override int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue", minValue, "minValue must be less than or equals to maxValue");
            }
            return (int)Math.Floor(minValue + (maxValue - minValue) * NextDouble());
        }

        public int[] roll()
        {

            int[] dice = { 1, 6 };

            dice[0] = Next(1, 7);
            dice[1] = Next(1, 7);

            return dice;
        }

        public override double NextDouble()
        {
            var data = new byte[sizeof(uint)];
            _rng.GetBytes(data);
            var randUint = BitConverter.ToUInt32(data, 0);
            return randUint / (uint.MaxValue + 1.0);
        }

        public override void NextBytes(byte[] data)
        {
            _rng.GetBytes(data);
        }

        public override string ToString()
        {
            return _rng.ToString();
        }

        public override bool Equals(object obj)
        {
            return _rng.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _rng.GetHashCode();
        }
    }

}
