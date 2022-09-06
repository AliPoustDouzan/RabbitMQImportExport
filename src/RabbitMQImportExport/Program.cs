using Newtonsoft.Json;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using RabbitMQImportExport.Service;

using RabbitToJSON.Model;

using System.Text;

using static RabbitMQImportExport.General.EnumList;
const string exportFileName = "ExportData.json";
Timer _timer = null;
int _counter = 0;
int _counterTotal = 0;
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
            var jsonString = File.ReadAllText(exportFileName);
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
            _timer = null;
            TimerHandler(null);
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss")} | Press a key to exit");
        }
        else
        {
            var consumer = new EventingBasicConsumer(channel);
            List<string> messageList = new List<string>();
            //Start timer
            _timer = new Timer(TimerHandler, null, 0, 1000);
            consumer.Received += (model, ea) =>
            {
                if (messageList.Count < ServiceConfig.Config.MinMessageCount)
                {
                    var bodyByte = ea.Body.ToArray();
                    var messageString = Encoding.UTF8.GetString(bodyByte);
                    messageList.Add(messageString);
                    _counter++;
                    _counterTotal++;
                    if (messageList.Count >= ServiceConfig.Config.MinMessageCount)
                    {
                        _timer = null;
                        TimerHandler(null);
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss")} | Saving to file start");
                        var exportModel = new ExportModel()
                        {
                            MessageList = messageList
                        };
                        string exportModelJSON = JsonConvert.SerializeObject(exportModel);
                        File.WriteAllText(exportFileName, exportModelJSON);
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss")} | {messageList.Count} Messages exported to {exportFileName}");
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss")} | Press a key to exit");
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
