﻿using CommandLine;
using CommandLine.Text;
using InEngine.Commands.Sample;
using InEngine.Core;

namespace InEngine.Commands
{
    public class MoreOptions : IOptions
    {
        [VerbOption("sample:show-progress")]
        public ShowProgress ShowProgress { get; set; }

        [VerbOption("sample:say-hello")]
        public SayHello SayHello { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }
}