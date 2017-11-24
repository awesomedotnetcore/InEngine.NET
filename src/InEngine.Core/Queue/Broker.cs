﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using InEngine.Core.Exceptions;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace InEngine.Core.Queue
{
    public class Broker
    {
        public string QueueBaseName { get; set; } = "InEngine:Queue";
        public string QueueName { get; internal set; } = "Primary";
        public string PendingQueueName { get { return QueueBaseName + $":{QueueName}:Pending"; } }
        public string InProgressQueueName { get { return QueueBaseName + $":{QueueName}:InProgress"; } }
        public string FailedQueueName { get { return QueueBaseName + $":{QueueName}:Failed"; } }
        public static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() => { 
            var queueSettings = InEngineSettings.Make().Queue;
            var redisConfig = ConfigurationOptions.Parse($"{queueSettings.RedisHost}:{queueSettings.RedisPort}");
            redisConfig.Password = string.IsNullOrWhiteSpace(queueSettings.RedisPassword) ? 
                null : 
                queueSettings.RedisPassword;
            redisConfig.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(redisConfig); 
        });
        public static ConnectionMultiplexer Connection { get { return lazyConnection.Value; } } 
        public ConnectionMultiplexer _connectionMultiplexer;
        public IDatabase Redis
        {
            get
            {
                return Connection.GetDatabase(RedisDb);
            }
        }
        public static string RedisHost { get; set; }
        public static int RedisDb { get; set; }
        public static int RedisPort { get; set; }
        public static string RedisPassword { get; set; }

        public Broker()
        {}

        public Broker(bool useSecondaryQueue) : this()
        {
            if (useSecondaryQueue)
                QueueName = "Secondary";
        }

        public static Broker Make(bool useSecondaryQueue = false)
        {
            return new Broker(useSecondaryQueue) {
                QueueBaseName = InEngineSettings.Make().Queue.QueueName
            };
        }

        public void Publish(ICommand command)
        {
            Redis.ListLeftPush(
                PendingQueueName,
                new Message() {
                    CommandClassName = command.GetType().FullName,
                    CommandAssemblyName = command.GetType().Assembly.GetName().Name + ".dll",
                    SerializedCommand = JsonConvert.SerializeObject(command)
                }.SerializeToJson()
            );
        }

        public bool Consume()
        {
            var stageMessageTask = Redis.ListRightPopLeftPush(PendingQueueName, InProgressQueueName);
            var serializedMessage = stageMessageTask.ToString();
            if (serializedMessage == null)
                return false;
            var message = serializedMessage.DeserializeFromJson<Message>();
            if (message == null)
                return false;

            var commandType = Type.GetType($"{message.CommandClassName}, {message.CommandAssemblyName}");
            if (commandType == null)
                throw new CommandFailedException("Consumed command failed: could not locate command type.");
            var commandInstance = JsonConvert.DeserializeObject(message.SerializedCommand, commandType) as ICommand;

            try
            {
                commandInstance.Run();
            }
            catch (Exception exception)
            {
                Redis.ListRemove(InProgressQueueName, serializedMessage, 1);
                Redis.ListLeftPush(FailedQueueName, stageMessageTask);
                throw new CommandFailedException("Consumed command failed.", exception);
            }

            try
            {
                Redis.ListRemove(InProgressQueueName, serializedMessage, 1);
            }
            catch (Exception exception)
            {
                throw new CommandFailedException($"Failed to remove completed message from queue: {InProgressQueueName}", exception);
            }

            return true;
        }

        #region Queue Management Methods
        public long GetPendingQueueLength()
        {
            return Redis.ListLength(PendingQueueName);
        }

        public long GetInProgressQueueLength()
        {
            return Redis.ListLength(InProgressQueueName);
        }

        public long GetFailedQueueLength()
        {
            return Redis.ListLength(FailedQueueName);
        }

        public bool ClearPendingQueue()
        {
            return Redis.KeyDelete(PendingQueueName);
        }

        public bool ClearInProgressQueue()
        {
            return Redis.KeyDelete(InProgressQueueName);
        }

        public bool ClearFailedQueue()
        {
            return Redis.KeyDelete(FailedQueueName);
        }

        public void RepublishFailedMessages()
        {
            Redis.ListRightPopLeftPush(FailedQueueName, PendingQueueName);
        }
        #endregion
    }
}
