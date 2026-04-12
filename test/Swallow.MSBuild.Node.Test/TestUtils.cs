using System.Diagnostics;

namespace Swallow.MSBuild.Node.Test;

internal static class TestUtils
{
    private static string? dotnetExecutablePath;

    public static string FindSolutionRoot(string startingDirectory)
    {
        const string solutionName = "Swallow.MSBuild.Node.slnx";

        var currentPath = startingDirectory;
        while (!File.Exists(Path.Combine(currentPath, solutionName)))
        {
            currentPath = Directory.GetParent(currentPath)?.FullName;
            if (currentPath is null)
            {
                throw new InvalidOperationException($"Could not find solution {solutionName} in {startingDirectory} or above.");
            }
        }

        return currentPath;
    }

    public static string CreateTempPath(string testName)
    {
        var tempPath = Path.GetTempPath();
        var testRunPath = Path.Combine(tempPath, $"swallow-msbuild-node-test-{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}", testName);
        Directory.CreateDirectory(testRunPath);

        return testRunPath;
    }

    public static void CopyProject(string fromPath, string toPath, string[] files)
    {
        foreach (var file in files)
        {
            var fromFile = Path.Combine(fromPath, file);
            var toFile = Path.Combine(toPath, file);

            var targetDirectory = Path.GetDirectoryName(toFile);
            if (targetDirectory is not null)
            {
                Directory.CreateDirectory(targetDirectory);
            }

            File.Copy(fromFile, toFile, overwrite: true);
        }
    }

    public static void PublishPackage(string projectPath, string outputPath)
    {
        dotnetExecutablePath ??= FindDotnetExecutable();
        Directory.CreateDirectory(outputPath);

        Execute(dotnetExecutablePath, "pack", "--output", outputPath, projectPath);
    }

    public static void AddPackageReference(string projectPath)
    {
        dotnetExecutablePath ??= FindDotnetExecutable();

        Execute(dotnetExecutablePath, "add", projectPath, "package", "Swallow.MSBuild.Node");
    }

    public static void PublishProject(string projectPath, string targetDirectory)
    {
        dotnetExecutablePath ??= FindDotnetExecutable();
        Directory.CreateDirectory(targetDirectory);

        Execute(dotnetExecutablePath, "restore", projectPath);
        Execute(dotnetExecutablePath, "build", "--no-restore", "--configuration", "Release", projectPath);
        Execute(dotnetExecutablePath, "publish", "--no-restore", "--no-build", "--configuration", "Release", "--output", targetDirectory, projectPath);
    }

    public static void PackProject(string projectPath, string targetDirectory)
    {
        dotnetExecutablePath ??= FindDotnetExecutable();
        Directory.CreateDirectory(targetDirectory);

        Execute(dotnetExecutablePath, "restore", projectPath);
        Execute(dotnetExecutablePath, "build", "--no-restore", "--configuration", "Release", projectPath);
        Execute(dotnetExecutablePath, "pack", "--no-restore", "--no-build", "--configuration", "Release", "--output", targetDirectory, projectPath);
    }

    private static string FindDotnetExecutable()
    {
        if (Environment.GetEnvironmentVariable("DOTNET_ROOT") is { } dotnetRoot)
        {
            return Path.Combine(dotnetRoot, "dotnet");
        }

        if (Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") is { } dotnetHostPath)
        {
            return dotnetHostPath;
        }

        throw new InvalidOperationException("Could not find dotnet executable; ensure that either DOTNET_ROOT or DOTNET_HOST_PATH is set.");
    }

    private static void Execute(string executable, params string[] arguments)
    {
        var process = new Process { StartInfo = new ProcessStartInfo(executable, arguments) { RedirectStandardOutput = true } };

        process.Start();
        process.WaitForExit();

        var output = process.StandardOutput.ReadToEnd();
        if (process.ExitCode is not 0)
        {
            throw new InvalidOperationException($"'{executable} {string.Join(" ", arguments)}' failed with exit code {process.ExitCode}.\n\n{output}");
        }
    }
}
