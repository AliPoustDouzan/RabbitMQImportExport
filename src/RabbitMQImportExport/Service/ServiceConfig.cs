using Newtonsoft.Json;

using RabbitMQImportExport.Model;

namespace RabbitMQImportExport.Service
{
    internal static class ServiceConfig
    {
        //Read RabbitConfig.json
        private static ConfigModel? _config;
        internal static ConfigModel Config
        {
            get
            {
                if (_config == null)
                {
                    var jsonString = File.ReadAllText("Config.json");
                    _config = JsonConvert.DeserializeObject<ConfigModel>(jsonString);
                }
                return _config;
            }
        }
    }
}