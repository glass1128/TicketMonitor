using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketmasterMonitor.Flatbuffer
{
    public class Base32Decoder
    {
        public static async Task<byte[]> Decode(string encoded)
        {
            // Custom Base32 character set
            string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567=";

            // Remove any padding characters (=) from the end of the string
            encoded = encoded.TrimEnd('=');

            int padding = (8 - encoded.Length % 8) % 8; // Calculate padding length

            int byteCount = encoded.Length * 5 / 8;
            byte[] result = new byte[byteCount];

            int resultIndex = 0;
            int buffer = 0;
            int bufferLength = 0;

            foreach (char c in encoded)
            {
                int value = base32Chars.IndexOf(c);
                if (value == -1)
                {
                    throw new FormatException("Invalid Base32 character: " + c);
                }

                buffer = (buffer << 5) | value;
                bufferLength += 5;

                if (bufferLength >= 8)
                {
                    result[resultIndex++] = (byte)(buffer >> (bufferLength - 8));
                    bufferLength -= 8;
                }
            }

            if (bufferLength > 0)
            {
                throw new FormatException("Invalid Base32 length");
            }

            return result;
        }
    }
}
