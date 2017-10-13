using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using SafeVault.Exceptions;
using SafeVault.Logger;
using SafeVault.Security;
using SafeVault.Security.Ciphers;
using SafeVault.Transport.Exceptions;

namespace SafeVault.Transport
{
    public class EncryptedStream : Stream
    {
        private Stream _stream;
        private readonly bool _canDisposeStream;

        private ICipher _cipher;
        public ICipher Cipher
        {
            get { return _cipher; }
            set
            {
                _cipher?.Dispose();
                _cipher = value;
            }
        }
        
        public EncryptedStream(Stream stream, bool canDispose=true)
        {
            _cipher = new XorCipher();
            _stream = stream;
            _canDisposeStream = canDispose;
        }

        public Stream GetRawStream()
        {
            EnsureStreamNotDisposed();
            return _stream;
        }

        public override int Read(byte[] buf, int offset, int count)
        {
            EnsureStreamNotDisposed();
            int ncount = count;
            DateTime timeout1 = DateTime.Now.AddMilliseconds(500);
            while (count > 0)
            {
                var nread = _stream.Read(buf, offset, count);

                if (nread != 0 && Cipher != null)
                {
                    var enc = new byte[nread];
                    for (int i = 0; i < nread; i++)
                        enc[i] = buf[offset + i];

                    var data = (Cipher != null) ? Cipher.Decrypt(enc) : enc;
                    if (enc.Length != data.Length)
                        throw new StreamControlException("Invalid Decrypted Block Length");   

                    //if (Logger.IsDebugEnabled)
                    //    Logger.Debug("<< Data: {0}", string.Join(" ", enc.Select(m => $"{m:x2}")));

                    for (int i = 0; i < nread; i++)
                        buf[offset++] = data[i];
                }
                if (nread == 0)
                {
                    if (DateTime.Now > timeout1)
                        throw new SecureChannelException("Timeout");
                    Thread.Sleep(1);
                }
                else
                {
                    timeout1 = DateTime.Now.AddMilliseconds(500);
                    count -= nread;
                }
            }
            return ncount;
        }

        public override void Write(byte[] buf, int offset, int count)
        {
            EnsureStreamNotDisposed();
            while (count > 0)
            {
                var nbytes = (count < 65535) ? count : 65535;
                var data = new byte[nbytes];
                for (int i = 0; i < nbytes; i++)
                    data[i] = buf[offset++];

                var enc = (Cipher != null) ? Cipher.Encrypt(data) : data;
                if (enc.Length != data.Length)
                    throw new StreamControlException("Invalid Encrypted Block Length");               
            
                //if (Logger.IsDebugEnabled)
                //    Logger.Debug(">> Data: {0}", string.Join(" ", enc.Select(m => $"{m:x2}")));

                _stream.Write(enc, 0, enc.Length);
                count -= nbytes;
            }
        }

        public override void Flush()
        {
            EnsureStreamNotDisposed();
            _stream.Flush();
        }

        private void EnsureStreamNotDisposed()
        {
            if (_stream == null)
                throw new ObjectDisposedException("Stream Disposed");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_canDisposeStream)
                {
                    _stream?.Dispose();
                }
                _stream = null;

                _cipher?.Dispose();
                _cipher = null;
            }
            base.Dispose(disposing);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureStreamNotDisposed();
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            EnsureStreamNotDisposed();
            _stream.SetLength(value);
        }

        public override bool CanRead
        {
            get
            {
                EnsureStreamNotDisposed();
                return _stream.CanRead;
            }
        }
        public override bool CanSeek
        {
            get
            {
                EnsureStreamNotDisposed();
                return _stream.CanSeek;
            }
        }
        public override bool CanWrite
        {
            get
            {
                EnsureStreamNotDisposed();
                return _stream.CanWrite;
            }
        }
        public override long Length
        {
            get
            {
                EnsureStreamNotDisposed();
                return _stream.Length;
            }
        }
        public override long Position
        {
            get
            {
                EnsureStreamNotDisposed();
                return _stream.Position;
            }
            set
            {
                EnsureStreamNotDisposed();
                _stream.Position = value;
            }
        }
    }
}