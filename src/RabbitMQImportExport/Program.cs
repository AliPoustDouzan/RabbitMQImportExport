using Newtonsoft.Json;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using RabbitMQImportExport.Service;

using RabbitToJSON.Model;

using System.Text;

using static RabbitMQImportExport.General.EnumList;
const string exportFileName = "ExportData";
Timer _timer = null;
int _counter = 0;
int _counterTotal = 0;
int _splitCount = 0;
bool _exit = false;
var connectionFactory = new ConnectionFactory()
{
    HostName = ServiceConfig.Config.HostName,
    UserName = ServiceConfig.Config.UserName,
    Password = ServiceConfig.Config.Password,
    Port = ServiceConfig.Config.Port,
    VirtualHost = ServiceConfig.Config.VirtualHost,
};
using (var connection = connectionFactory.CreateConnection())
{
    using (var channel = connection.CreateModel())
    {
        channel.QueueDeclare(queue: ServiceConfig.Config.Queue,
                 durable: true,
                 exclusive: false,
                 autoDelete: false,
                 arguments: null);
        if (ServiceConfig.Config.Type == EnmType.Import)
        {
            while (true)
            {
                _splitCount++;
                var fileName = exportFileName + _splitCount.ToString() + ".json";
                var fileExists = File.Exists(fileName);
                if (fileExists == false)
                {
                    if (_exit == false)
                    {
                        _exit = true;
                        _timer = null;
                        TimerHandler(null);
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss")} | Press a key to exit");
                    }
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss")} | Read data from {fileName}");
                    var jsonString = File.ReadAllText(fileName);
                    //Start timer
                    _timer = new Timer(TimerHandler, null, 0, 1000);
                    var ExportModel = JsonConvert.DeserializeObject<ExportModel>(jsonString);
                    foreach (var messageString in ExportModel.MessageList)
                    {
                        var bodyByte = Encoding.UTF8.GetBytes(messageString);
                        channel.BasicPublish(exchange: "",
                                             routingKey: ServiceConfig.Config.Queue,
                                             basicProperties: null,
                                             body: bodyByte);
                        _counter++;
                        _counterTotal++;
                    }
                }

            }
        }
        else
        {
            var consumer = new EventingBasicConsumer(channel);
            List<string> messageList = new List<string>();
            //Start timer
            _timer = new Timer(TimerHandler, null, 0, 1000);
            consumer.Received += (model, ea) =>
            {
                if (_counterTotal < ServiceConfig.Config.MinMessageCount)
                {
                    var bodyByte = ea.Body.ToArray();
                    var messageString = Encoding.UTF8.GetString(bodyByte);
                    messageList.Add(messageString);
                    _counter++;
                    _counterTotal++;

                    if (_counterTotal >= ServiceConfig.Config.MinMessageCount || messageList.Count >= ServiceConfig.Config.SplitCount)
                    {
                        _splitCount++;
                        _timer = null;
                        TimerHandler(null);
                        var fileName = exportFileName + _splitCount.ToString() + ".json";
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss")} | Saving to {fileName} start");
                        var exportModel = new ExportModel()
                        {
                            MessageList = messageList
                        };
                        string exportModelJSON = JsonConvert.SerializeObject(exportModel);
                        File.WriteAllText(fileName, exportModelJSON);
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss")} | {messageList.Count} Messages exported to {fileName}");
                        if (_counterTotal >= ServiceConfig.Config.MinMessageCount)
                        {
                            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss")} | Press a key to exit");
                        }
                        else
                        {
                            messageList = new List<string>();
                        }
                    }
                }
            };
            channel.BasicConsume(queue: ServiceConfig.Config.Queue,
                     autoAck: ServiceConfig.Config.AutoAck,
                     consumer: consumer);
        }
        Console.ReadKey();
    }
}
void TimerHandler(object state)
{
    if (_exit == false)
    {
        if (ServiceConfig.Config.Type == EnmType.Import)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss")} | Push {_counter} message per/second, Total : {_counterTotal}");
        }
        else
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss")} | Read {_counter} message per/second, Total : {_counterTotal}");
        }
        _counter = 0;
    }
}
