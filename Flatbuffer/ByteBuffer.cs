using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketmasterMonitor.Flatbuffer
{
    class ByteBuffer
    {
        byte[] bytes_;
        int position_ = 0;

        static int SIZEOF_SHORT = 2;
        static int SIZEOF_INT = 4;
        static int FILE_IDENTIFIER_LENGTH = 4;
        static int SIZE_PREFIX_LENGTH = 4;
        static int UTF8_BYTES = 1;


        public ByteBuffer(byte[] t)
        {
            bytes_ = t;
            position_ = 0;
        }

        public byte[] bytes()
        {
            return this.bytes_;
        }

        public int readInt32(int pos)
        {
            return (this.bytes_[pos] | (this.bytes_[pos + 1] << 8) | (this.bytes_[pos + 2] << 16) | (this.bytes_[pos + 3] << 24));
        }

        public Int16 readUint16(int pos)
        {
            return (Int16)(this.bytes_[pos] | (this.bytes_[pos + 1] << 8));
        }

        public Int16 readInt16(int pos)
        {
            return (Int16)((this.readUint16(pos) << 16) >> 16);
        }

        public int __offset(int pos, int size)
        {
            var r = pos - this.readInt32(pos);
            return size < this.readInt16(r) ? this.readInt16(r + size) : 0;
        }

        public Int64 createLong(int l, int h)
        {
            return ((Int64)l << 32) | (uint)h;
        }

        public Int64 FromBits(int highBits, int lowBits)
        {
            long result = 0;
            byte[] highBytes = BitConverter.GetBytes(highBits);
            byte[] lowBytes = BitConverter.GetBytes(lowBits);

            for (int i = 0; i < 4; i++)
            {
                result |= ((long)highBytes[i] << (8 * (7 - i)));
                result |= ((long)lowBytes[i] << (8 * (3 - i)));
            }

            return result;
        }

        public Int64 readInt64(int pos)
        {
            int high = this.readInt32(pos);
            int low = this.readInt32(pos + 4);
            return ((long)low << 32) | (uint)high;
        }

        public byte readUint8(int pos)
        {
            return this.bytes_[pos];
        }

        public int position()
        {
            return this.position_;
        }

        public string __string(int pos, int e)
        {
            int t = 0;
            t = pos + this.readInt32(pos);
            int r = this.readInt32(t);

            string i = "";
            int o = 0;

            t += ByteBuffer.SIZEOF_INT;

            if (e == ByteBuffer.UTF8_BYTES)
            {
                byte[] subBytes = new byte[r];
                Array.Copy(bytes_, t, subBytes, 0, r);
                return Encoding.UTF8.GetString(subBytes);
            }

            for (; o < r;)
            {
                int s = 0;
                byte a = this.readUint8(t + o++);
                if (a < 192) s = a;
                else
                {
                    byte u = this.readUint8(t + o++);
                    if (a < 224) s = (((31 & a) << 6) | (63 & u));
                    else
                    {
                        byte c = this.readUint8(t + o++);
                        s = a < 240 ? (((15 & a) << 12) | ((63 & u) << 6) | (63 & c)) : (((7 & a) << 18) | ((63 & u) << 12) | ((63 & c) << 6) | (63 & this.readUint8(t + o++)));
                    }
                }

                if (s < 65536)
                {
                    i += Char.ConvertFromUtf32(s);
                }
                else
                {
                    s -= 65536;
                    i += Char.ConvertFromUtf32(55296 + (s >> 10));
                    i += Char.ConvertFromUtf32(56320 + (1023 & s));

                }
            }


            return i;

        }

        public int __indirect(int t)
        {
            return t + this.readInt32(t);
        }

        public int __vector(int t)
        {
            return t + this.readInt32(t) + ByteBuffer.SIZEOF_INT;
        }

        public int __vector_len(int t)
        {
            return this.readInt32(t + this.readInt32(t));
        }

    }
}
