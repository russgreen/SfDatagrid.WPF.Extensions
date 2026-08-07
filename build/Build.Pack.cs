using Fallout.Common;
using Fallout.Common.Git;
using Fallout.Common.Tools.DotNet;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    Target Pack => _ => _
        .TriggeredBy(Sign)
        .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch())
        .Executes(() =>
        {
            var packConfigurations = Solution.GetModel().BuildTypes;

            foreach (var configuration in packConfigurations)
            {
                if (configuration.StartsWith("Release"))
                {
                    DotNetPack(settings => settings
                        .SetConfiguration(configuration)
                        .SetOutputDirectory(OutputDirectory)
                        .SetVerbosity(DotNetVerbosity.minimal));
                }

            }
        });


}