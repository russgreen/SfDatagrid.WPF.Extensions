using Fallout.Common;
using Fallout.Common.Git;
using Fallout.Common.IO;
using Fallout.Solutions;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Utilities.Collections;
using System.IO;
using System.Linq;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

partial class Build : FalloutBuild
{

    readonly AbsolutePath OutputDirectory = RootDirectory / "output";
    readonly AbsolutePath SourceDirectory = RootDirectory / "source";

    readonly string[] CompiledAssemblies = { "SfDatagrid.WPF.Extensions.dll" };

    [GitRepository]
    [Required]
    readonly GitRepository GitRepository;

    [Solution]
    Solution Solution;

    public static int Main() => Execute<Build>(x => x.Clean);

}
