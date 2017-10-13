using System;
using System.IO;
using System.Text;
using SafeVault.Security;
using SafeVault.Security.Ciphers;
using SafeVault.Transport.Exceptions;
using Random = SafeVault.Security.Random;

namespace SafeVault.Transport
{
    public class SecureChannel :IDisposable
    {
        protected virtual EncryptedStream ReadStream { get; set; }
        protected virtual EncryptedStream WriteStream { get; set; }

        public CipherCollection CipherLib { get; private set; }

        private ICipher _writeCipher;
        public ICipher WriteCipher
        {
            get { return _writeCipher; }
            set
            {
                _writeCipher?.Dispose();
                _writeCipher = value;
            }
        }

        private ICipher _readCipher;
        public ICipher ReadCipher
        {
            get { return _readCipher; }
            set
            {
                _readCipher?.Dispose();
                _readCipher = value;
            }
        }

        public SecureChannel()
        {
            CipherLib = new CipherCollection();
        }

        public void WriteObject<T>(T obj)
        {
            #if NETFX
            var js = new System.Web.Script.Serialization.JavaScriptSerializer();
            string  json = js.Serialize(obj);
            #endif

            #if NETSTANDARD2_0
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            #endif

            var data = Encoding.UTF8.GetBytes(json);
            Write(data);
        }

        public T ReadObject<T>()
        {
            byte[] data = Read();
            #if NETSTANDARD2_0
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
            #endif

            #if NETFX
            var js = new System.Web.Script.Serialization.JavaScriptSerializer();
            return js.Deserialize<T>(Encoding.UTF8.GetString(data));
            #endif
        }

        public byte[] Read()
        {
            while (true)
            {
                try
                {
                    var sc = (EncryptedStreamControl) ReadUInt16();
                    switch (sc)
                    {
                        case EncryptedStreamControl.StreamData:
                        {
                            var dataLen = ReadUInt32();
                            return ReadData(dataLen);
                        }
                        default:
                        {
                            ProcessStreamControl(sc);
                            break;
                        }
                    }
                }
                catch (SecureChannelException e)
                {
                    throw new ReadSecureChannelException(e, e.Message);
                }
            }
        }

        private void ProcessStreamControl(EncryptedStreamControl sc)
        {
            switch (sc)
            {
                case EncryptedStreamControl.EncryptRSA:
                {
                    if (!CipherLib.ContainsKey("rsa-private"))
                        throw new SecureChannelException("RSA certificate is required.");
                    
                    ReadCipher = CipherLib["rsa-private"].Clone();
                    break;
                }

                case EncryptedStreamControl.EncryptXOR:
                {
                    var passwLen = ReadUInt16();
                    if (passwLen != 0)
                    {
                        var passw = ReadData(passwLen);
                        CipherLib["xor"] = new XorCipher(passw);
                    }

                    ReadStream.Cipher = CipherLib["xor"].Clone();
                    break;
                }
                case EncryptedStreamControl.EncryptAES:
                {
                    var passwLen = ReadUInt16();
                    if (passwLen != 0)
                    {
                        var passw = ReadData(passwLen);
                        CipherLib["aes"] = new Aes256Cipher(passw);
                    }
                    ReadCipher = CipherLib["aes"].Clone();
                    break;
                }

                default:
                    throw new StreamControlException("Unknown StreamControl command");
            }
        }

        public void Write(byte[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        public void Write(byte[] buf, int offset, int count)
        {
            EnsureObjectNotDisposed();

            WriteUInt16((ushort) EncryptedStreamControl.StreamData);
            WriteUInt32(buf.Length);
            WriteData(buf);
        }

        public void Encrypt(bool reset = false)
        {
            if (reset)
            {
                WriteStream.Cipher = new XorCipher();
                WriteCipher = null;
            }

            if (CipherLib.ContainsKey("rsa-public"))
            {
                WriteUInt16((ushort) EncryptedStreamControl.EncryptRSA);
                WriteCipher = CipherLib["rsa-public"].Clone();
            }

            byte[] salt = Random.Get(32);
            WriteUInt16((ushort) EncryptedStreamControl.EncryptAES);
            WriteUInt16((ushort) salt.Length);
            WriteData(salt);
            CipherLib["aes"] = new Aes256Cipher(salt);
            WriteCipher = CipherLib["aes"].Clone();


            if (!CipherLib.ContainsKey("xor"))
            {
                byte[] passw = Random.Get(2048/8);
                WriteUInt16((ushort) EncryptedStreamControl.EncryptXOR);
                WriteUInt16((ushort) passw.Length);
                WriteData(passw);
                CipherLib["xor"] = new XorCipher(passw);
            }
            else
            {
                WriteUInt16((ushort) EncryptedStreamControl.EncryptXOR);
                WriteUInt16((ushort) 0);
            }
            WriteStream.Cipher = CipherLib["xor"].Clone();
        }

        private byte[] ReadData(int count)
        {
            EnsureObjectNotDisposed();
            using (var memory = new MemoryStream())
            {
                while (count > 0)
                {
                    var sc = (EncryptedStreamControl) ReadUInt16();
                    if (sc != EncryptedStreamControl.Data)
                        throw new ReadSecureChannelException("Stream read wrong stream control command received");

                    var dataLen = ReadUInt16();
                    var encData = new byte[dataLen];
                    int nread = ReadStream.Read(encData, 0, encData.Length);
                    if (nread != dataLen)
                        throw new ReadSecureChannelException("Stream readed unexpected block length");

                    byte[] data;
                    try
                    {
                        data = (ReadCipher != null)
                            ? ReadCipher.Decrypt(encData)
                            : encData;
                    }
                    catch (Exception e)
                    {
                        throw new ReadSecureChannelException(e, "Stream decrypt failed");
                    }

                    memory.Write(data, 0, data.Length);
                    count -= data.Length;
                }
                if (count != 0)
                    throw new ReadSecureChannelException("Stream total readed bytes is incorrect");

                return memory.ToArray();
            }
        }

        private void WriteData(byte[] buf)
        {
            EnsureObjectNotDisposed();

            int count = buf.Length;
            int offset = 0;
            while (count > 0)
            {
                int nbytes = (WriteCipher == null || count < WriteCipher.BlockSize) ? count : WriteCipher.BlockSize;
                if (nbytes > ushort.MaxValue)
                    nbytes = ushort.MaxValue;

                var data = new byte[nbytes];
                for (int i = 0; i < nbytes; i++)
                    data[i] = buf[offset++];

                var enc = (WriteCipher != null)
                    ? WriteCipher.Encrypt(data)
                    : data;

                if (enc.Length > ushort.MaxValue)
                    throw new SecureChannelException("Encrypted BlockSize exceed Max allowed value"); 

                WriteUInt16((ushort)EncryptedStreamControl.Data);
                WriteUInt16((ushort)enc.Length);
                WriteBinary(enc);
                count -= nbytes;
            }
        }

        public ushort ReadUInt16()
        {
            EnsureObjectNotDisposed();
            var data = new byte[2];
            ReadStream.Read(data, 0, 2);
            return BitConverter.ToUInt16(data, 0 );
        }

        public int ReadUInt32()
        {
            EnsureObjectNotDisposed();
            var data = new byte[4];
            ReadStream.Read(data, 0, 4);
            return BitConverter.ToInt32(data, 0 );
        }

        private void WriteBinary(byte[] data) 
        {
            EnsureObjectNotDisposed();
            WriteStream.Write(data, 0, data.Length);
        }

        private void WriteUInt32(int value)
        {
            EnsureObjectNotDisposed();

            var data = BitConverter.GetBytes(value);
            if (data.Length != 4)
                throw new StreamControlException("Invalid UInt32 Block Size");

            WriteStream.Write(data, 0, data.Length);
        }
        private void WriteUInt16(ushort value)
        {
            EnsureObjectNotDisposed();

            var data = BitConverter.GetBytes(value);
            if (data.Length != 2)
                throw new StreamControlException("Write Invalid Block Size");

            WriteStream.Write(data, 0, data.Length);
        }

        private bool _disposed = false;
        private void EnsureObjectNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Secure Channel is Disposed");
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) 
        {
            if (disposing)
            {
                _disposed = true;
                _readCipher?.Dispose();
                _readCipher = null;

                CipherLib?.Dispose();
                CipherLib = null;
            }
        }

        public void Flush() 
        {
            EnsureObjectNotDisposed();
            WriteStream.Flush();
        }
    }
}