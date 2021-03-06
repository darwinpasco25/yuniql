﻿using Yuniql.CLI;
using Yuniql.Core;
using CommandLine;

namespace Yuniql
{
    public class Program
    {
        //https://github.com/commandlineparser/commandline
        //https://github.com/dotnet/command-line-api

        public static int Main(string[] args)
        {
            var environmentService = new EnvironmentService();
            var traceService = new FileTraceService();
            var localVersionService = new LocalVersionService(traceService);
            var migrationServiceFactory = new CLI.MigrationServiceFactory(traceService);
            var commandLineService = new CommandLineService(migrationServiceFactory, localVersionService, environmentService, traceService);

            var resultCode = Parser.Default.ParseArguments<
                InitOption,
                RunOption,
                NextVersionOption,
                InfoOption,
                VerifyOption,
                EraseOption,
                BaselineOption,
                RebaseOption>(args)
              .MapResult(
                (InitOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunInitOption(opts);
                },
                (RunOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunMigration(opts);
                },
                (NextVersionOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.IncrementVersion(opts);
                },
                (InfoOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunInfoOption(opts);
                },
                (VerifyOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunVerify(opts);
                },
                (EraseOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunEraseOption(opts);
                },
                (BaselineOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunBaselineOption(opts);
                },
                (RebaseOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunRebaseOption(opts);
                },
                (ArchiveOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunArchiveOption(opts);
                },
                errs => 1);

            return resultCode;
        }
    }
}
