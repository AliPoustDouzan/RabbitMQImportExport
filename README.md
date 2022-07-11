# RabbitMQImportExport
This app will help you export and import messages in RabbitMQ, Just set the config.json and execute the app.

# Version Log
ver 1401.04.21.1 :
- First release of this app

# Dependencies
.Net 6

# How to use?
1. Set your config.json, Like this :
```json
{
  "HostName": "localhost",
  "UserName": "guest",
  "Password": "guest",
  "Port": 5672,
  "VirtualHost": "/",
  "Queue": "testqueue",
  "MinMessageCount": 6,
  "AutoAck": false,
  "Type": 1 //export=0/import=1
}
```
2. For export messages Set Type to 0
3. If you want delete the messages after exporting them set AutoAck to true
4. Run RabbitMQImportExport.exe
5. Change Type to 1
6. Set different queue (for test of course)
7. Run RabbitMQImportExport.exe
