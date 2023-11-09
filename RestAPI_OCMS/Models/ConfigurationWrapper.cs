namespace RestAPI_OCMS.Models
{
    public interface IConfigurationWrapper
    {
        string GetConnectionString(string name);
    }

    public class ConfigurationWrapper : IConfigurationWrapper
    {
        private readonly IConfiguration _configuration;

        public ConfigurationWrapper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString(string name)
        {
            return _configuration.GetConnectionString(name);
        }
    }

}
