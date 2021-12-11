// See https://aka.ms/new-console-template for more information
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

Console.WriteLine("Hello, World!");
var factory = new ConnectionFactory
{
    HostName = "192.168.0.229"
};

var connection = factory.CreateConnection();

//创建通道
var channel = connection.CreateModel();

//事件基本消费者
EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

//接收到消息事件
consumer.Received += (ch, ea) => {
    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
    Console.WriteLine($"收到消息：{message}");
    //确认该消息已被消费
    channel.BasicAck(ea.DeliveryTag, false);
    //channel.BasicNack(ea.DeliveryTag, false, true);
};

//启动消费者
channel.BasicConsume("to_do_list", false, consumer);

Console.WriteLine("消费者已启动");
Console.ReadLine();

channel.Dispose();
connection.Close();