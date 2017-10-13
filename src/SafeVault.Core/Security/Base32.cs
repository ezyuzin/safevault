using System;
using System.Diagnostics;
using System.Text;

namespace SafeVault.Security
{
    public sealed class Base32
    {
        const char PADDING_CHAR = '=';
        static readonly char[] _base32 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

        static int Min(int x, int y)
        {
	        return x < y ? x : y;
        }

        /// <summary>
        /// Pad the given buffer with len padding characters.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        static void Pad(byte[] buf, int offset, int count)
        {
	        while(count-- > 0)
		        buf[offset++] = (byte)PADDING_CHAR;
        }

        /// <summary>
        /// This convert a 5 bits value into a base32 character.
        /// Only the 5 least significant bits are used.
        /// </summary>
        static byte EncodeChar(byte c)
        {
	        return (byte)_base32[c & 0x1F];  // 0001 1111
        }

        /// <summary>
        ///  Decode given character into a 5 bits value. 
        ///  Returns -1 iff the argument given was an invalid base32 character
        ///  or a padding character.
        /// </summary>
        /// <param name=""></param>
        /// <param name="c"></param>
        /// <returns></returns>
        static int DecodeChar(byte c)
        {
	        int retval = -1;

	        if (c >= 'A' && c <= 'Z')
		        retval = c - 'A';
	        if (c >= '2' && c <= '7')
		        retval = c - '2' + 26;

	        if (retval == -1 || (retval <= 0x1F))
    	        return retval;

            throw new ArithmeticException("Invalid char");
        }



        /// <summary>
        /// Given a block id between 0 and 7 inclusive, this will return the index of
        /// the octet in which this block starts. For example, given 3 it will return 1
        /// because block 3 starts in octet 1:
        /// 
        /// +--------+--------+
        /// | ......<|.3 >....|
        /// +--------+--------+
        /// octet 1 | octet 2
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        static int GetOctet(int block)
        {
	        Debug.Assert(block >= 0 && block < 8);
	        return (block*5) / 8;
        }

        /// <summary>
        /// Given a block id between 0 and 7 inclusive, this will return how many bits
        /// we can drop at the end of the octet in which this block starts. 
        /// For example, given block 0 it will return 3 because there are 3 bits
        /// we don't care about at the end:
        /// 
        ///  +--------+-
        ///  |< 0 >...|
        ///  +--------+-
        /// 
        /// Given block 1, it will return -2 because there
        /// are actually two bits missing to have a complete block:
        /// 
        ///  +--------+-
        ///  |.....< 1|..
        ///  +--------+-
        /// </summary>
        static int GetOffset(int block)
        {
	        Debug.Assert(block >= 0 && block < 8);
	        return (8 - 5 - (5*block) % 8);
        }

        /// <summary>
        /// Like "b >> offset" but it will do the right thing with negative offset.
        /// We need this as bitwise shifting by a negative offset is undefined
        /// behavior.
        /// </summary>
        static byte ShiftRight(byte b, int offset)
        {
            return (offset > 0)
                ? (byte)(b >>  offset)
                : (byte)(b << -offset);
        }

        static byte ShiftLeft(byte b, int offset)
        {
	        return ShiftRight(b, - offset);
        }

        /// <summary>
        /// Encode a sequence. A sequence is no longer than 5 octets by definition.
        /// Thus passing a length greater than 5 to this function is an error. Encoding
        /// sequences shorter than 5 octets is supported and padding will be added to the
        /// output as per the specification.
        /// </summary>
        static void EncodeSequence(byte[] plain, int plainOffset, int len, ref byte[] coded, int codedOffset)
        {
	        Debug.Assert(len >= 0 && len <= 5);

	        for (int block = 0; block < 8; block++) 
            {
		        int octet = GetOctet(block);  // figure out which octet this block starts in
		        int junk = GetOffset(block);  // how many bits do we drop from this octet?

		        if (octet >= len) { // we hit the end of the buffer
			        Pad(coded, codedOffset + block, 8 - block);
			        return;
		        }

		        byte c = ShiftRight(plain[plainOffset + octet], junk);  // first part

		        if (junk < 0  // is there a second part?
		            &&  octet < len - 1)  // is there still something to read?
		        {
			        c |= ShiftRight(plain[plainOffset + octet+1], 8 + junk);
		        }
		        coded[codedOffset + block] = EncodeChar(c);
	        }
        }

        private static int DecodeSequence(byte[] coded, int codedOffset, byte[] plain, int plainOffset)
        {
	        plain[plainOffset + 0] = 0;
	        for (int block = 0; block < 8; block++) {
		        int offset = GetOffset(block);
		        int octet = GetOctet(block);

		        int c = DecodeChar(coded[codedOffset + block]);
		        if (c < 0)  // invalid char, stop here
			        return octet;

		        plain[plainOffset + octet] |= ShiftLeft((byte)c, offset);
		        if (offset < 0) {  // does this block overflows to next octet?
			        Debug.Assert(octet < 4);
			        plain[plainOffset + octet+1] = ShiftLeft((byte)c, 8 + offset);
		        }
	        }
	        return 5;
        }

        public static string Encode(byte[] data)
        {
            byte[] coded = new byte[data.Length * 2 + 16];
            int len = data.Length;

            for (int i = 0, j = 0; i < len; i += 5, j += 8) {
                EncodeSequence(data, i, Min(len - i, 5), ref coded, j);
            }

            int count = 0;
            while (count < coded.Length && coded[count] != 0)
                count++;

            return Encoding.ASCII.GetString(coded, 0, count);
        }

        public static byte[] Decode(string base32)
        {
            var coded = Encoding.UTF8.GetBytes(base32);
            Array.Resize(ref coded, coded.Length + 16);
            byte[] plain = new byte[coded.Length];

            int written = 0;
            for (int i = 0, j = 0; ; i += 8, j += 5) {
                int n = DecodeSequence(coded, i, plain, j);
                written += n;
                if (n < 5)
                    break;
            }

            Array.Resize(ref plain, written);
            return plain;
        }
    }
}