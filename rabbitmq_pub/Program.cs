// See https://aka.ms/new-console-template for more information
using RabbitMQ.Client;
using System.Text;


var connectionFactory = new ConnectionFactory
{
    HostName = "192.168.0.229",
};

var connection = connectionFactory.CreateConnection();
var channel = connection.CreateModel();
channel.QueueDeclare("to_do_list", false, false, false);
channel.BasicQos(1,1,true);
Console.WriteLine("rabbitmq链接成功，请输入消息， 输入exit退出!");
//消息发送失败回调函数
channel.BasicAcks += (sender, arg) =>
{
    Console.WriteLine(arg.DeliveryTag);
};

channel.BasicReturn += (sender, arg) =>
{
    //Console.WriteLine(arg.);
};

string input;
do
{
    input = Console.ReadLine();
    //转换成字节数组
    var bytes = Encoding.UTF8.GetBytes(input);
    channel.BasicPublish("", "to_do_list", null, bytes);
    

} while (input.Trim().ToLower() != "exit");

channel.Close();
connection.Close();