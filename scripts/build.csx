using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.IO;

const string DefaultNugetFeed = "https://www.nuget.org";
const string ArtifactsLocation = "artifacts";

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

public void BuildProject(string project, string suffix = null)
{
    Console.WriteLine("Building " + project);
    
    var output = Path.Combine(ArtifactsLocation, Path.GetFileName(project));
    
    var pack = string.Format("pack \"{0}\" -c Release -o \"{1}\"", project, ArtifactsLocation);
    
    if (!string.IsNullOrEmpty(suffix))
    {
        pack += " --version-suffix " + suffix;
    }

    Exec("dotnet", pack);
}

public void TestProject(string project, string suffix = null)
{
    Console.WriteLine("Testing " + project);
    
    var output = Path.Combine(ArtifactsLocation, Path.GetFileName(project));
    
    var publish = string.Format("publish \"{0}\" -c Release -o \"{1}\"", project, output);
    
    if (!string.IsNullOrEmpty(suffix))
    {
        publish += " --version-suffix " + suffix;
    }
    
    Exec("dotnet", publish);
    Exec("xunit.console.exe", Path.Combine(output, Path.GetFileName(project) + ".dll"));
}

public void PublishNuget(string package, string apiKey, string feed = null)
{
    feed = feed ?? DefaultNugetFeed;
    Console.WriteLine("Publishing nuget packge: '" + package + "' to " + feed);

    // Don't push symbols
    var symbolPackage = Path.ChangeExtension(package, "symbols.nupkg");
    if (File.Exists(symbolPackage)) File.Delete(symbolPackage);

    Exec("nuget", string.Format("push \"{0}\" -Source {1} -ApiKey {2}", package, feed, apiKey));
}

public void BuildTestPublishPreRelease(
    string[] projects = null, string[] testProjects = null, string[] additionalProjects = null,
    string apiKey = null, string suffix = null, string feed = null, bool parallel = false)
{
    projects = projects ?? Directory.GetDirectories("src");
    testProjects = testProjects ?? Directory.GetDirectories("test");
    
    if (additionalProjects != null)
    {
        projects = projects.Concat(additionalProjects).ToArray();
    }
    
    while (string.IsNullOrEmpty(suffix))
    {
        Console.WriteLine("Enter Build suffix (e.g. alpha1):");
        suffix = Console.ReadLine().Trim();
    }

    if (Directory.Exists(ArtifactsLocation)) Directory.Delete(ArtifactsLocation, recursive: true);

    var loop = parallel
        ? new Action<IEnumerable<string>, Action<string>>((a, b) => Parallel.ForEach(a, b))
        : new Action<IEnumerable<string>, Action<string>>((a, b) => { foreach (var i in a) b(i); });

    loop(projects, p => BuildProject(p, suffix));
    loop(testProjects, p => TestProject(p, suffix));

    while (string.IsNullOrEmpty(apiKey))
    {
        Console.WriteLine("Enter ApiKey for '" + (feed ?? DefaultNugetFeed) + "':");
        apiKey = Console.ReadLine().Trim();
    }
    
    loop(Directory.GetFiles(ArtifactsLocation, "*.nupkg", SearchOption.AllDirectories)
                  .Where(file => file.EndsWith("-" + suffix + ".nupkg")), package =>
    {
        PublishNuget(package, apiKey, feed);
    });
}