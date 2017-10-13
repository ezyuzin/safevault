namespace SafeVault.Transport
{
    public enum EncryptedStreamControl
    {
        Data = 0x0000,
        StreamData = 0x0100,
        EncryptXOR = 0x1000,
        EncryptRSA = 0x1100,
        EncryptAES = 0x1500,
    }
}