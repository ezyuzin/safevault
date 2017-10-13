using System.IO;

namespace SafeVault.Transport
{
    public class ServiceChannel : SecureChannel
    {
        public void SetReadStream(Stream stream, bool canDispose=true)
        {
            ReadStream?.Dispose();
            ReadStream = new EncryptedStream(stream, canDispose);
        }
        public void SetWriteStream(Stream stream, bool canDispose=true)
        {
            WriteStream?.Dispose();
            WriteStream = new EncryptedStream(stream, canDispose);
        }
    }
}