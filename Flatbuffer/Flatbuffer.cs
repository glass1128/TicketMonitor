using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketmasterMonitor.Flatbuffer
{
    class Flatbuffer
    {
        ByteBuffer bb;
        int bb_pos;

        public Flatbuffer()
        {
            this.bb = null;
            this.bb_pos = 0;
        }

        public Flatbuffer(ByteBuffer t)
        {
            this.bb = t;
            this.bb_pos = 0;
        }

        public Flatbuffer __init(ByteBuffer t, int pos)
        {
            this.bb = t;
            this.bb_pos = pos;
            return this;
        }

        public Flatbuffer getRootAsAvailability(ByteBuffer e)
        {
            Flatbuffer fb = new Flatbuffer().__init(e, e.readInt32(e.position()) + e.position());
            return fb;
        }

        public int numSeats()
        {
            int t = this.bb.__offset(this.bb_pos, 4);
            return t != 0 ? this.bb.readInt32(this.bb_pos + t) : 0;
        }

        public int numGASeats()
        {
            int t = this.bb.__offset(this.bb_pos, 6);
            return t != 0 ? this.bb.readInt32(this.bb_pos + t) : 0;
        }

        public Int64 manifestTimestamp()
        {
            int t = this.bb.__offset(this.bb_pos, 8);
            return t != 0 ? this.bb.readInt64(this.bb_pos + t) : this.bb.createLong(0, 0);
        }

        public Int64 pricingTimestamp()
        {
            int t = this.bb.__offset(this.bb_pos, 10);
            return t != 0 ? this.bb.readInt64(this.bb_pos + t) : this.bb.createLong(0, 0);
        }

        public Int64 processedTimestamp()
        {
            int t = this.bb.__offset(this.bb_pos, 12);
            return t != 0 ? this.bb.readInt64(this.bb_pos + t) : this.bb.createLong(0, 0);
        }

        public Int64 revision()
        {
            int t = this.bb.__offset(this.bb_pos, 14);
            return t != 0 ? this.bb.readInt64(this.bb_pos + t) : this.bb.createLong(0, 0);
        }

        public string eventId()
        {
            int t = this.bb.__offset(this.bb_pos, 16);
            return t != 0 ? this.bb.__string(this.bb_pos + t, 0) : null;
        }

        public string version()
        {
            int t = this.bb.__offset(this.bb_pos, 18);
            return t != 0 ? this.bb.__string(this.bb_pos + t, 0) : null;
        }

        public string manifestVersion()
        {
            int t = this.bb.__offset(this.bb_pos, 20);
            return t != 0 ? this.bb.__string(this.bb_pos + t, 0) : null;
        }
        public string pricingVersion()
        {
            int t = this.bb.__offset(this.bb_pos, 22);
            return t != 0 ? this.bb.__string(this.bb_pos + t, 0) : null;
        }

        public int statusLength()
        {
            int t = this.bb.__offset(this.bb_pos, 24);
            return t != 0 ? this.bb.__vector_len(this.bb_pos + t) : 0;
        }

        public RoaringBitmapPair statuses(int t)
        {
            int n = this.bb.__offset(this.bb_pos, 24);
            if (n != 0)
            {
                RoaringBitmapPair SRBP = new RoaringBitmapPair().__init(this.bb.__indirect(this.bb.__vector(this.bb_pos + n) + 4 * t), this.bb);
                return SRBP;
            }
            else return null;

        }
    }
}
