namespace SafeVault.Configuration
{
    public interface IConfig : IConfigSectionBase
    {
        T Get<T>() where T : IConfigData;
        void Save(string filename);
        void Save();
    }
}