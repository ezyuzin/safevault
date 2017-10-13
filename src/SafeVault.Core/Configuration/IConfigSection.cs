namespace SafeVault.Configuration
{
    public interface IConfigSectionBase
    {
        string Get(string key);
        string Get(string key, bool throwIfNotFound);

        void Set(string key, string value);
        void Set(string key, int index, string value);

        IConfigSection GetSection(string key);
        IConfigSection GetSection(string key, bool throwIfNotFound);
        IConfigSection[] GetSections(string key);


        bool IsExist(string xpath);
        string GetSectionName();
        string[] GetValues(string xpath);
    }

    public interface IConfigSection : IConfigSectionBase
    {
        IConfig Config { get; }
    }
}