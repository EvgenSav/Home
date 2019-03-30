

namespace Driver.Mtrf64
{
    public enum CmdByteIdx
    {
        St, Mode, Ctr, Res, Ch, Cmd, Fmt,
        D0, D1, D2, D3, Id0, Id1, Id2, Id3, Crc, Sp
    };

    public class Buf
    {
        byte[] buf = new byte[17];
        public int Id => AddrF != 0 ? AddrF : Ch;
        public int AddrF {
            get => (buf[11] << 24 | buf[12] << 16 | buf[13] << 8 | buf[14]);

            set {
                buf[11] = (byte)(value >> 24);
                buf[12] = (byte)(value >> 16);
                buf[13] = (byte)(value >> 8);
                buf[14] = (byte)value;
            }
        }
        public int St {
            get => buf[0];
            set => buf[0] = (byte)value;
        }
        public int Sp {
            get => buf[16];
            set => buf[16] = (byte)value;
        }
        public int Mode {
            get => buf[1];
            set => buf[1] = (byte)value;
        }
        public int Ctr {
            get => buf[2];
            set => buf[2] = (byte)value;
        }
        public int Ch {
            get => buf[4];
            set => buf[4] = (byte)value;
        }
        public int Cmd {
            get => buf[5];
            set => buf[5] = (byte)value;
        }
        public int Fmt {
            get => buf[6];
            set => buf[6] = (byte)value;
        }
        public int D0 {
            get => buf[7];
            set => buf[7] = (byte)value;
        }
        public int D1 {
            get => buf[8];
            set => buf[8] = (byte)value;
        }
        public int D2 {
            get => buf[9];
            set => buf[9] = (byte)value;
        }
        public int D3 {
            get => buf[10];
            set => buf[10] = (byte)value;
        }
        public int Crc {
            get => buf[15];
            set => buf[15] = (byte)value;
        }
        public int GetCrc {
            get {
                byte crc = 0x00;
                for (CmdByteIdx i = CmdByteIdx.St; i < CmdByteIdx.Crc; i++)
                {
                    crc += buf[(int)i];
                }
                return crc;
            }
        }
        public int Length => buf.Length;
        public void LoadData(byte[] data)
        {
            buf = data;
        }
        public byte this[CmdByteIdx idx] {
            get => buf[(int)idx];
            set => buf[(int)idx] = value;
        }

        public static int Round(float val)
        {
            if ((val - (int)val) > 0.5) return (int)val + 1;
            else return (int)val;
        }

        public int ExtDevType() => D0;
        public int FirmwareVer() => D1;
        public int State() => D2;
        public int Bright() => Round(((float)D3 / 255) * 100);

        public byte[] GetBufData() => buf;
    }
}
