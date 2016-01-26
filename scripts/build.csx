using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.IO;

const string DefaultNugetFeed = "https://www.nuget.org";
const string ArtifactsLocation = "artifacts/Release";

public int Exec(string cmd, string args, bool shellExecute = false, string workingDirectory = null)
{
    var ps = new ProcessStartInfo(cmd, args) { UseShellExecute = shellExecute, WorkingDirectory = workingDirectory };
    var process = Process.Start(ps);
    process.WaitForExit();
    if (process.ExitCode != 0)
    {
        throw new Exception("Error: " + cmd + " " + args);
    }
    return process.ExitCode;
}

public string ExecOutput(string cmd, string args)
{
    var ps = new ProcessStartInfo(cmd, args) { UseShellExecute = false, RedirectStandardOutput = true };
    var process = Process.Start(ps);
    process.WaitForExit();
    if (process.ExitCode != 0)
    {
        throw new Exception("Error: " + cmd + " " + args);
    }
    return process.StandardOutput.ReadToEnd();
}

public string VersionTimestamp()
{
    return DateTime.UtcNow.ToString("yyyyMMddhhmmss");
}

public string VersionGitCommit()
{
    return ExecOutput("git", "rev-list -1 HEAD");
}

public void BuildProject(string project, string buildVersion = null, string output = null)
{
    output = output ?? "artifacts";

    if (!string.IsNullOrEmpty(buildVersion))
    {
        Console.WriteLine("Building " + project + " pre-release " + buildVersion);
        Environment.SetEnvironmentVariable("DNX_BUILD_VERSION", buildVersion);
    }
    else
    {
        Console.WriteLine("Building " + project);
    }

    Exec("dnu", string.Format("pack \"{0}\" --configuration Release --out \"{1}\"", project, output), shellExecute: true);
}

public void TestProject(string project)
{
    Console.WriteLine("Testing " + project);
    Exec("dnx", string.Format("-p \"{0}\" test", project));
}

public void PublishNuget(string package, string apiKey, string feed = null)
{
    feed = feed ?? DefaultNugetFeed;
    Console.WriteLine("Publishing nuget packge: '" + package + "' to " + feed);
    Exec("nuget", string.Format("push \"{0}\" -Source {1} -ApiKey {2}", package, feed, apiKey));
}

public void BuildTestPublishPreRelease(string[] projects, string[] testProjects, string apiKey = null, string buildVersion = null, string feed = null, bool parallel = true)
{
    while (string.IsNullOrEmpty(buildVersion))
    {
        Console.WriteLine("Enter Build Version (e.g. alpha1):");
        buildVersion = Console.ReadLine().Trim();
    }

    while (string.IsNullOrEmpty(apiKey))
    {
        Console.WriteLine("Enter ApiKey for '" + (feed ?? DefaultNugetFeed) + "':");
        apiKey = Console.ReadLine().Trim();
    }

    if (Directory.Exists(ArtifactsLocation)) Directory.Delete(ArtifactsLocation, recursive: true);

    var loop = parallel
        ? new Action<IEnumerable<string>, Action<string>>((a, b) => Parallel.ForEach(a, b))
        : new Action<IEnumerable<string>, Action<string>>((a, b) => { foreach (var i in a) b(i); });

    loop(projects, p => BuildProject(p, buildVersion));
    loop(testProjects, p => TestProject(p));
    loop(Directory.GetFiles(ArtifactsLocation).Where(file => file.EndsWith("-" + buildVersion + ".nupkg")), package =>
    {
        PublishNuget(package, apiKey, feed);
    });
}