﻿using System;
using System.Linq;
using System.Reflection;
using InEngine.Core;
using InEngine.Core.Exceptions;
using NLog;

namespace InEngineCli
{
    public class ArgumentInterpreter
    {
        public Logger Logger { get; set; }

        public ArgumentInterpreter()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }

        public void Interpret(string[] args)
        {
            var parser = new CommandLine.Parser(with => with.IgnoreUnknownArguments = true);
            var options = new Options();

            if (parser.ParseArguments(args, options))
            {
                if (options == null)
                    Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
                var targetAssembly = Assembly.LoadFrom(options.PlugInName + ".dll");
                var types = targetAssembly.GetTypes();
                var optionType = types.FirstOrDefault(x => typeof(IOptions).IsAssignableFrom(x));
                if (optionType == null)
                    Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
                var pluginOptions = targetAssembly.CreateInstance(optionType.FullName) as IOptions;
                var isSuccessful = CommandLine
                    .Parser
                    .Default
                    .ParseArguments(args.Skip(1).ToArray(), pluginOptions, (verb, subOptions) => 
                {
                    try
                    {
                        if (subOptions == null)
                            ExitWithFailure(new CommandFailedException("Could not parse plugin options"));

                        var command = subOptions as ICommand;

                        if (command is AbstractCommand)
                            (command as AbstractCommand).Name = verb.Normalize();

                        var commandResult = command.Run();

                        if (commandResult.IsSuccessful)
                            ExitWithSuccess(commandResult.Message);
                        else
                            ExitWithFailure(new CommandFailedException(commandResult.Message));
                    }
                    catch (Exception exception)
                    {
                        ExitWithFailure(exception);
                    }
                });

                if (!isSuccessful)
                    ExitWithFailure();
            }
        }

        public void ExitWithSuccess(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "success";
            Logger.Debug($"✔ {message}");
            Environment.Exit(0);
        }

        public void ExitWithFailure(Exception exception = null)
        {
            Logger.Error(exception != null ? exception : new CommandFailedException(), "✘ fail");
            Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
        }
    }
}
