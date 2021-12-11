// See https://aka.ms/new-console-template for more information
using Confluent.Kafka;
string addr = "192.168.3.12:9092";

Console.WriteLine("kafka生产端");
var config = new ProducerConfig { BootstrapServers = addr };

// If serializers are not specified, default serializers from
// `Confluent.Kafka.Serializers` will be automatically used where
// available. Note: by default strings are encoded as UTF8.
using (var builder = new ProducerBuilder<int, string>(config).Build())
{
    try
    {
        var i = 0; 
        while (true)
        {
            //kafka的一致性算法通过key来保证相同key的msg会被分配到同一个分区，这样来保证消息局部有序
            //消费端可在同一个消费组里面创建多个消费实例（不大于分区数）来消费主题，这样可以增加消费端并发能力
            //kafka该算法对生产端和消费段均透明，无需修改代码，只需要增加对应的消费实例即可（比如通过容器动态扩容）
            var dr = await builder.ProduceAsync("test-topic", new Message<int, string> { Key= i % 6, Value = "test" + i });
            Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            i++;
            //Thread.Sleep(500);
        }
    }
    catch (ProduceException<Null, string> e)
    {
        
        Console.WriteLine($"Delivery failed: {e.Error.Reason}");
    }
}
