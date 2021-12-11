// See https://aka.ms/new-console-template for more information
using Confluent.Kafka;

string addr = "192.168.3.12:9092";
string consumerGroupId = "st_consumer_group";
Console.WriteLine($"kafka消费端，消费组：{consumerGroupId}");

var config = new ConsumerConfig
{
    //消费分组，同一个消费分组可以有多个实例（不大于topic的分区数），每个实例消费一个分区（其实当分区数大于实例数时，部分实例会消费多个分区）
    //多个消费分组订阅同一个topic可以实现重复消费
    //如果要实现两个消费实例消费同一个分区，用两个消费组即可实现
    GroupId = consumerGroupId, 
    BootstrapServers = addr,
    AutoOffsetReset = AutoOffsetReset.Earliest,
    
};

//kafka的一致性算法通过key来保证相同key的msg会被分配到同一个分区，这样来保证消息局部有序
//消费端可在同一个消费组里面创建多个消费实例（不大于分区数）来消费主题，这样可以增加消费端并发能力
//kafka该算法对生产端和消费段均透明，无需修改代码，只需要增加对应的消费实例即可（比如通过容器动态扩容）
using (var builder = new ConsumerBuilder<int, string>(config).Build())
{
    builder.Subscribe("test-topic");
    var cancelToken = new CancellationTokenSource();
    try
    {
        while (true)
        {

            var consumer = builder.Consume(cancelToken.Token);
            Console.WriteLine($"Message: {consumer.Message.Value} with Key:{consumer.Message.Key} received from {consumer.TopicPartitionOffset}");
        }
    }
    catch (ConsumeException e)
    {
        builder.Close();
        Console.WriteLine($"Receive failed: {e.Error.Reason}");
    }
}