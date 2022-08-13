using System;

namespace JavaAsm.Helpers {
    public static class ModifiedUtf8Helper {
        public static byte[] Encode(string value) {
            int offset = 0;
            byte[] buffer = new byte[GetBytesCount(value)];
            foreach (char c in value) {
                if (c != 0 && c <= 127)
                    buffer[offset++] = (byte) c;
                else if (c <= 2047) {
                    buffer[offset++] = (byte) (0xc0 | (0x1f & (c >> 6)));
                    buffer[offset++] = (byte) (0x80 | (0x3f & c));
                }
                else {
                    buffer[offset++] = (byte) (0xe0 | (0x0f & (c >> 12)));
                    buffer[offset++] = (byte) (0x80 | (0x3f & (c >> 6)));
                    buffer[offset++] = (byte) (0x80 | (0x3f & c));
                }
            }

            return buffer;
        }

        public static ushort GetBytesCount(string value) {
            int bytesCount = 0;
            foreach (char c in value) {
                if (c != 0 && c <= 127)
                    bytesCount++;
                else if (c <= 2047)
                    bytesCount += 2;
                else
                    bytesCount += 3;

                if (bytesCount > ushort.MaxValue)
                    throw new FormatException("String more than 65535 UTF bytes long");
            }

            return (ushort) bytesCount;
        }

        public static string Decode(byte[] data) {
            int length = data.Length;
            char[] result = new char[length];
            int count = 0;
            int numberOfChars = 0;
            while (count < length) {
                if ((result[numberOfChars] = (char) data[count++]) < '\u0080')
                    numberOfChars++;
                else {
                    int a;
                    if (((a = result[numberOfChars]) & 0xe0) == 0xc0) {
                        if (count >= length)
                            throw new FormatException($"Bad second byte at {count}");
                        int b = data[count++];
                        if ((b & 0xC0) != 0x80)
                            throw new FormatException($"Bad second byte at {count - 1}");
                        result[numberOfChars++] = (char) (((a & 0x1F) << 6) | (b & 0x3F));
                    }
                    else if ((a & 0xf0) == 0xe0) {
                        if (count + 1 >= length)
                            throw new FormatException($"Bad third byte at {count + 1}");
                        int b = data[count++];
                        int c = data[count++];
                        if ((b & 0xC0) != 0x80 || (c & 0xC0) != 0x80)
                            throw new FormatException($"Bad second or third byte at {count - 2}");
                        result[numberOfChars++] = (char) (((a & 0x0F) << 12) | ((b & 0x3F) << 6) | (c & 0x3F));
                    }
                    else
                        throw new FormatException($"Bad byte at {count - 1}");
                }
            }

            return new string(result, 0, numberOfChars);
        }
    }
}