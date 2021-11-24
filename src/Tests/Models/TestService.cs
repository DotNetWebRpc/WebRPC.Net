using System;

namespace WebRPC.Models
{
    public class TestService : ITestService
    {
        public void Simple()
        {
        }

        public void Simple(bool b)
        {
        }

        public void Simple(byte b)
        {
        }

        public void Simple(short s)
        {
        }

        public void Simple(int i)
        {
        }

        public void Simple(long l)
        {
        }

        public void Simple(decimal d)
        {
        }

        public void Simple(float f)
        {
        }

        public void Simple(double d)
        {
        }

        public void Simple(DateTime d)
        {
            
        }

        public void Simple(string s)
        {
            
        }

        public void SimpleOut(out bool b)
        {
            b = false;
        }

        public void SimpleOut(out byte b)
        {
            b = 10;
        }

        public void SimpleOut(out short s)
        {
            s = 10;
        }

        public void SimpleOut(out int i)
        {
            i = 10;
        }

        public void SimpleOut(out long l)
        {
            l = 10;
        }

        public void SimpleOut(out decimal d)
        {
            d = 10.10M;
        }

        public void SimpleOut(out float f)
        {
            f = 10.10f;
        }

        public void SimpleOut(out double d)
        {
            d = 10.10;
        }

        public void SimpleOut(out DateTime d)
        {
            d = new DateTime(2000, 10, 10);
        }

        public void SimpleOut(out string s)
        {
            s = "ten";
        }

        public void SimpleRef(ref bool b)
        {
            b = !b;
        }

        public void SimpleRef(ref byte b)
        {
            b++;
        }

        public void SimpleRef(ref short s)
        {
            s++;
        }

        public void SimpleRef(ref int i)
        {
            i++;
        }

        public void SimpleRef(ref long l)
        {
            l++;
        }

        public void SimpleRef(ref decimal d)
        {
            d++;
        }

        public void SimpleRef(ref float f)
        {
            f++;
        }

        public void SimpleRef(ref double d)
        {
            d++;
        }

        public void SimpleRef(ref DateTime d)
        {
            d = d.AddDays(1);
        }

        public void SimpleRef(ref string s)
        {
            s += " new";
        }

        public bool SimpleReturn(bool b)
        {
            return b;
        }

        public byte SimpleReturn(byte b)
        {
            return b;
        }

        public short SimpleReturn(short s)
        {
            return s;
        }

        public int SimpleReturn(int i)
        {
            return i;
        }

        public long SimpleReturn(long l)
        {
            return l;
        }

        public decimal SimpleReturn(decimal d)
        {
            return d;
        }

        public float SimpleReturn(float f)
        {
            return f;
        }

        public double SimpleReturn(double d)
        {
            return d;
        }

        public DateTime SimpleReturn(DateTime d)
        {
            return d;
        }

        public string SimpleReturn(string s)
        {
            return s;
        }
    }
}
