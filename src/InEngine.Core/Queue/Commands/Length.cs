﻿using System;
using CommandLine;

namespace InEngine.Core.Queue.Commands
{
    public class Length : AbstractCommand
    {
        public override void Run()
        {
            PrintUsage(Broker.Make());
            PrintUsage(Broker.Make(true));
        }

        public void PrintUsage(Broker broker)
        {
            var leftPadding = 15;
            Warning($"{broker.QueueName} Queue:");
            broker = Broker.Make(true);
            InfoText("Pending".PadLeft(leftPadding));
            Line(broker.GetPendingQueueLength().ToString().PadLeft(10));
            InfoText("In-progress".PadLeft(leftPadding));
            Line(broker.GetInProgressQueueLength().ToString().PadLeft(10));
            ErrorText("Failed".PadLeft(leftPadding));
            Line(broker.GetFailedQueueLength().ToString().PadLeft(10));
            Newline();
        }
    }
}
