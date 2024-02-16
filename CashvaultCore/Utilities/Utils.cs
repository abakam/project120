using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Security.Cryptography;
using System.Globalization;
using System.Reflection;

namespace CashvaultCore.Utilities
{
    public static class Utils
    {
        public static string GetMd5HashString(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }

        public static byte[] GetSHA1Hash(string inputString)
        {
            HashAlgorithm algorithm = SHA1.Create();  // SHA1.Create()
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetGetSHA1HashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetSHA1Hash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        private static string convertBigDecToHex(String decryptedSessionKeytring)
        {
            var s = decryptedSessionKeytring;
            var result = new List<byte>();
            result.Add(0);
            foreach (char c in s)
            {
                int val = (int)(c - '0');
                for (int i = 0; i < result.Count; i++)
                {
                    int digit = result[i] * 10 + val;
                    result[i] = (byte)(digit & 0x0F);
                    val = digit >> 4;
                }
                if (val != 0)
                    result.Add((byte)val);
            }

            var hex = "";
            foreach (byte b in result)
                hex = "0123456789ABCDEF"[b] + hex;
            return hex;
        }

        public static ICryptoTransform Create3DESCryptographicInstance(byte[] encryptionKey, byte[] encryptionIv = null, PaddingMode paddingMode = PaddingMode.None, CipherMode cipherMode = CipherMode.CBC, bool createAsEncryptor = true)
        {
            SymmetricAlgorithm provider = new TripleDESCryptoServiceProvider { Mode = cipherMode, Padding = paddingMode };
            return createAsEncryptor ? provider.CreateEncryptor(encryptionKey, encryptionIv) : provider.CreateDecryptor(encryptionKey, encryptionIv);
        }

        public static byte[] PerformCryptoGraphicAction(byte[] encodedText, ICryptoTransform cryptoProvider)
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream
                    (ms, cryptoProvider, CryptoStreamMode.Write))
                {
                    cs.Write(encodedText, 0, encodedText.Length);
                    cs.FlushFinalBlock();
                }

                //  return encoding.GetString(ms.ToArray());//Convert.ToBase64String(ms.ToArray());
                return ms.ToArray();
            }
        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            var hexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < hexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                hexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return hexAsBytes;
        }

        public static string convertByteToHexString(byte[] byteData)
        {
            string result = "";
            //Console.WriteLine("The ByteData Are..");
            foreach (Byte b in byteData)
            {
                //Console.Write(b.ToString());
                result += b.ToString("X2"); // convert byte to hex
            }
            //Console.WriteLine();

            return result;
        }

        public static string toHex(Int64 d)
        {
            Int64 r = d % 16;
            string result = null;
            if (d - r == 0)
                result += toChar(r);
            else
                result += toHex((d - r) / 16) + toChar(r);
            return result;
        }

        public static string toChar(Int64 n)
        {
            string alpha = "0123456789ABCDEF";
            int substring = (Int32)(alpha.Length - n);
            return alpha.Substring(0, substring);
        }

        public static string ConvertHexToString(string input)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            int len = input.Length;
            int i = 0;
            int sIx = 0;
            while (true)
            {
                if (input[i] == '%')
                {
                    sb.Append(input.Substring(sIx, i - sIx));
                    string hc = input.Substring(i + 1, 2);
                    int hi = int.Parse(hc, System.Globalization.NumberStyles.HexNumber);
                    char c = (char)hi;
                    sb.Append(c);

                    sIx = i + 3;
                    i = i + 2;
                }
                i++;

                if (i >= len)
                {
                    sb.Append(input.Substring(sIx));
                    break;
                }
            }
            return sb.ToString();
        }

        public static string GetAssemblyPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referencePrefix"></param>
        /// <param name="random"></param> E.g new Random(Guid.NewGuid().GetHashCode())
        /// <returns></returns>
        public static string getRandomReference(String referencePrefix, Random random)
        {

            StringBuilder builder = new StringBuilder();
            builder.Append(referencePrefix);
            builder.Append(RandomString(2, false, random));
            builder.Append(RandomNumber(001, 999, random));
            builder.Append(RandomString(2, false, random));
            builder.Append(DateTime.Now.ToString("ds"));
            if (builder.ToString().Trim().Length > 12)
            {
                return builder.ToString().Substring(0, 12);
            }
            else
            {
                return builder.ToString();
            }
        }

        public static string getRandomPassword(String referencePrefix, Random random)
        {

            StringBuilder builder = new StringBuilder();
            builder.Append(referencePrefix);
            builder.Append(RandomString(2, true, random));
            builder.Append(RandomNumber(001, 999, random));
            builder.Append(RandomString(1, false, random));
            builder.Append(DateTime.Now.ToString("ds"));
            if (builder.ToString().Trim().Length > 8)
            {
                return builder.ToString().Substring(0, 8);
            }
            else
            {
                return builder.ToString();
            }
        }

        /// <summary>
        /// Generates a random string with the given length
        /// </summary>
        /// <param name="size">Size of the string</param>
        /// <param name="lowerCase">If true, generate lowercase string</param>
        /// <returns>Random string</returns>
        public static string RandomString(int size, bool lowerCase, Random random)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        public static int RandomNumber(int min, int max, Random random)
        {
            return random.Next(min, max);
        }

        public static double FormatMoney(string dealerBalance)
        {
            return Convert.ToDouble(FormatStringToCurrency(dealerBalance));
        }

        internal static string FormatStringToCurrency(string moneyString)
        {
            string mString = moneyString.Replace(".","");
            if (mString.Length > 0)
            {
                if (mString.Length < 3)
                {
                    return moneyString + ".00";
                }
                else
                {
                    mString = mString.Insert((mString.Length-2), ".");
                    return mString;
                }
            }
            else {
                return "0.00";
            }
        }

        public static string formatInterswitchMoney(string value)
        {
            return value.Contains(".") ? value.Replace(".", "") : value += "00";
        }

        public static string formatInterswitchMoney(int value)
        {
            return value.ToString().Contains(".") ? value.ToString().Replace(".", "") : value.ToString() + "00";
        }

        public static bool TryParseXml(string xml)
        {
            try
            {
                System.Xml.Linq.XDocument.Parse(xml);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static String ToDecmialString(this double source)
        {
            if ((source % 1) == 0)
                return source.ToString("f1");
            else
                return source.ToString();

        }

        public static string RandomNumber(int Codelength, string PossibleChars)
        {
            CryptoRandom rngCsp = new CryptoRandom();

            string SecretCode = string.Empty;
            int i = 0;
            while (i < Codelength)
            {
                // Fill the array with a random value.
                int Value = rngCsp.Next(9);

                string SingleChar = PossibleChars.Substring(Value, 1);
                SecretCode += SingleChar;
                i += 1;
            }

            return SecretCode;
        }

        public static string RandomUniqueNumber(int Codelength, string PossibleChars)
        {
            CryptoRandom rngCsp = new CryptoRandom();

            string SecretCode = string.Empty;
            int i = 0;
            while (i < Codelength)
            {
                // Fill the array with a random value.
                int value = rngCsp.Next(9);

                string singleChar = PossibleChars.Substring(value, 1);
                if (!(SecretCode.Contains(singleChar)))
                {
                    SecretCode += singleChar;
                    i += 1;
                }
            }

            return SecretCode;
        }

        public static DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
        }
    }
}
