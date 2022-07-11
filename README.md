# RabbitMQImportExport
This app will help you export and import messages in RabbitMQ, Just set the config.json and execute the app.

# Version Log
ver 1401.04.21.1 :
- First release of this app

# Dependencies
.Net 6

# How to use?
Set your config.json, Like this :
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
For export messages Set Type to 0 and for import messages set Type to 1.

If you want delete the messages after exporting them set AutoAck to true.
