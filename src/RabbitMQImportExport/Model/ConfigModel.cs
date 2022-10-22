using static RabbitMQImportExport.General.EnumList;

namespace RabbitMQImportExport.Model;

public class ConfigModel
{
    public string HostName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public int Port { get; set; }
    public string VirtualHost { get; set; }
    public string Queue { get; set; }
    public int MinMessageCount { get; set; }
    public bool AutoAck { get; set; }
    public EnmType Type { get; set; }
    public int SplitCount { get; set; }
}