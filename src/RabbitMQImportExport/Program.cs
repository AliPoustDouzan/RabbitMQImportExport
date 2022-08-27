using Newtonsoft.Json;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using RabbitMQImportExport.Service;

using RabbitToJSON.Model;

using System.Text;

using static RabbitMQImportExport.General.EnumList;
const string exportFileName = "ExportData.json";
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
            var ExportModel = JsonConvert.DeserializeObject<ExportModel>(jsonString);
            var publishedMessageCount = 0;
            foreach (var messageString in ExportModel.MessageList)
            {
                var bodyByte = Encoding.UTF8.GetBytes(messageString);
                channel.BasicPublish(exchange: "",
                                     routingKey: ServiceConfig.Config.Queue,
                                     basicProperties: null,
                                     body: bodyByte);
                publishedMessageCount++;
                Console.WriteLine($"{publishedMessageCount} Messages published in {ServiceConfig.Config.Queue} Queue");
            }
            Console.WriteLine("Press a key to exit");
        }
        else
        {
            var consumer = new EventingBasicConsumer(channel);
            List<string> messageList = new List<string>();
            int messageCount = 0;
            consumer.Received += (model, ea) =>
            {
                if (messageList.Count < ServiceConfig.Config.MinMessageCount)
                {
                    var bodyByte = ea.Body.ToArray();
                    var messageString = Encoding.UTF8.GetString(bodyByte);
                    messageList.Add(messageString);
                    messageCount++;
                    Console.WriteLine($"Total readed message : {messageCount}");
                    if (messageList.Count >= ServiceConfig.Config.MinMessageCount)
                    {
                        var exportModel = new ExportModel()
                        {
                            MessageList = messageList
                        };
                        string exportModelJSON = JsonConvert.SerializeObject(exportModel);
                        File.WriteAllText(exportFileName, exportModelJSON);
                        Console.WriteLine($"{messageList.Count} Messages exported to {exportFileName}");
                        Console.WriteLine("Press a key to exit");
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
