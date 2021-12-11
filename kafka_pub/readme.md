## kafka 单机部署方案

### docker 部署

```shell
docker run -d --name zookeeper -p 2181:2181  wurstmeister/zookeeper
docker run -d --name kafka -p 9092:9092 -e KAFKA_BROKER_ID=0 -e KAFKA_ZOOKEEPER_CONNECT=zookeeper:2181 --link zookeeper -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://192.168.1.60(机器IP):9092 -e KAFKA_LISTENERS=PLAINTEXT://0.0.0.0:9092 -t wurstmeister/kafka
```

### docker-componse 部署
```yaml
version: '2'
services:
  zookeeper:
    image: wurstmeister/zookeeper
    ports:
      - "2181:2181"
  kafka:
    image: wurstmeister/kafka
    ports:
      - "9092:9092"
    environment:
      # client 要访问的 broker 地址
      KAFKA_ADVERTISED_HOST_NAME: 118.25.215.105
      # 通过端口连接 zookeeper
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      # 每个容器就是一个 broker，设置其对应的 ID
      KAFKA_BROKER_ID: 0
      # 外部网络只能获取到容器名称，在内外网络隔离情况下
      # 通过名称是无法成功访问 kafka 的
      # 因此需要通过绑定这个监听器能够让外部获取到的是 IP
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://118.25.215.105:9092
      #kafka启动后初始化一个有2个partition(分区)0个副本名叫kafeidou的topic
      KAFKA_CREATE_TOPICS: "kafeidou:2:0"   
      # kafka 监听器，告诉外部连接者要通过什么协议访问指定主机名和端口开放的 Kafka 服务。
      KAFKA_LISTENERS: PLAINTEXT://0.0.0.0:9092
      # Kafka默认使用-Xmx1G -Xms1G的JVM内存配置，由于服务器小，调整下启动配置
      # 这个看自己的现状做调整，如果资源充足，可以不用配置这个
      KAFKA_HEAP_OPTS: "-Xmx256M -Xms128M"
      # 设置 kafka 日志位置
      KAFKA_LOG_DIRS: "/kafka/logs"
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      # 挂载 kafka 日志
      - /data/kafka/logs:/kafka/logs
```

## 通过kafka自带工具生产消费消息测试
1. 首先,进入到kafka的docker容器中

```shell
docker exec -it kafka sh
```

2. 运行消费者,进行消息的监听
```shell
kafka-console-consumer.sh --bootstrap-server 192.168.1.60:9094 --topic kafeidou --from-beginning
```
3. 打开一个新的ssh窗口,同样进入kafka的容器中,执行下面这条命令生产消息
```shell
kafka-console-producer.sh --broker-list 192.168.1.60(机器IP):9092 --topic kafeidou
```

输入完这条命令后会进入到控制台，可以输入任何想发送的消息,这里发送一个hello
```shell
>>
>hello
>
>
>
```

## 为topic增加分区
```shell
#新增主题并设置分区
docker exec -it kafka容器id /bin/bash -c \
'/opt/kafka/bin/kafka-topics.sh --create --bootstrap-server localhost:9092 \
--replication-factor 1 --partitions 1 --topic test'
#为已存在主题扩展分区 , 和新增的区别是不能带 --replication-factor 参数
docker exec -it kafka容器id /bin/bash -c \
'/opt/kafka/bin/kafka-topics.sh --alter --bootstrap-server localhost:9092 \
--partitions 2 --topic test'
```

- –-partitions 1： 分区数量 1
- -–replication-factor 1： 副本因子数量 1
