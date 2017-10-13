namespace SafeVault.Security.Ciphers
{
    public class XorCipherKey : CipherKey
    {
        private readonly byte[] _data;
        private readonly byte[] _key;
        private int _pos;

        public XorCipherKey(byte[] key)
        {
            _data = new byte[key.Length];
            key.CopyTo(_data, 0);
            _key = key;
        }

        public void Shuffle(byte @byte)
        {
            for (int i = 0; i < _data.Length; i++)
                _data[i] = (byte) (_data[i] ^ @byte);
        }

        public byte GetNext()
        {
            if (_pos == _data.Length)
                _pos = 0;

            return _data[_pos++];
        }

        public XorCipherKey Clone()
        {
            return new XorCipherKey(this._key);
        }
    }
}