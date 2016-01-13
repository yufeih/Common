using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.IO;

public int Exec(string cmd, string args, bool shellExecute = false)
{
    var ps = new ProcessStartInfo(cmd, args) { UseShellExecute = shellExecute };
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

public void PublishNuget(IEnumerable<string> packages, string apiKey, string feed = null)
{
    Parallel.ForEach(packages, package =>
    {
        PublishNuget(package, apiKey, feed);
    });
}

public void PublishNuget(string package, string apiKey, string feed = null)
{
    feed = feed ?? "https://nuget.org/api/v2/";
    Console.WriteLine("Publishing nuget packge: '" + package + "' to " + feed);
    Exec("nuget", string.Format("push \"{0}\" -Source {1} -ApiKey {2}", package, feed, apiKey));
}

public void BuildAndPublishPreRelease(string project, string apiKey, string buildVersion = null, string feed = null)
{
    buildVersion = buildVersion ?? "build" + VersionTimestamp();
    BuildProject(project, buildVersion);
    PublishNuget(Directory.GetFiles("artifacts/Release").Where(file => file.EndsWith("-" + buildVersion + ".nupkg")), apiKey, feed);
}