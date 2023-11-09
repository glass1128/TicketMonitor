using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketmasterMonitor.Flatbuffer
{
    class RoaringBitmapPair
    {
        ByteBuffer bb;
        int bb_pos;

        public RoaringBitmapPair()
        {
            this.bb = null;
            this.bb_pos = 0;
        }

        public RoaringBitmapPair __init(int pos, ByteBuffer t)
        {
            this.bb = t;
            bb_pos = pos;
            return this;
        }

        public int roaringBitmapLength()
        {
            int t = this.bb.__offset(this.bb_pos, 6);
            return t != 0 ? this.bb.__vector_len(this.bb_pos + t) : 0;
        }

        public byte[] roaringBitmapArray()
        {
            int t = this.bb.__offset(this.bb_pos, 6);
            if (t != 0)
            {
                int firstIndex = this.bb.__vector(this.bb_pos + t);
                int subByteArrlen = this.bb.__vector_len(this.bb_pos + t);
                byte[] subByteArr = new byte[subByteArrlen];
                for (int i = 0; i < subByteArrlen; i++)
                {
                    subByteArr[i] = this.bb.bytes()[firstIndex++];
                }
                return subByteArr;
            }
            else
            {
                return null;
            }

        }
    }
}
