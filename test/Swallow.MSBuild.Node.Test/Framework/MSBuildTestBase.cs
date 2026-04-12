using TUnit.Core.Interfaces;

namespace Swallow.MSBuild.Node.Test.Framework;

[NotInParallel]
public abstract class MSBuildTestBase(string identifier) : IAsyncInitializer
{
    protected string CurrentDirectory { get; } = CreateTempPath(identifier);
    protected string SolutionPath { get; } = FindSolutionRoot(Environment.CurrentDirectory);
    protected string ProjectPath => Path.Combine(CurrentDirectory, "Project");

    protected abstract string GetSourceProjectPath(string solutionRoot);
    protected abstract IEnumerable<string> GetTargetFileNames();

    Task IAsyncInitializer.InitializeAsync()
    {
        CopySourceProject();
        CompilePackageUnderTest();
        AddReferenceToPackage();

        return Task.CompletedTask;
    }

    private void CopySourceProject()
    {
        var projectToCopy = GetSourceProjectPath(SolutionPath);
        var filesToCopy = GetTargetFileNames();

        foreach (var file in filesToCopy)
        {
            var fromFile = Path.Combine(projectToCopy, file);
            var toFile = Path.Combine(ProjectPath, file);

            var targetDirectory = Path.GetDirectoryName(toFile);
            if (targetDirectory is not null)
            {
                Directory.CreateDirectory(targetDirectory);
            }

            File.Copy(fromFile, toFile, overwrite: true);
        }
    }

    private void CompilePackageUnderTest()
    {
        var dotnet = new Dotnet();

        dotnet.Pack(
            outputDirectory: Path.Combine(CurrentDirectory, "packages"),
            project: Path.Combine(SolutionPath, "src", "Swallow.MSBuild.Node"));
    }

    private void AddReferenceToPackage()
    {
        var dotnet = new Dotnet();

        dotnet.AddPackage("Swallow.MSBuild.Node", ProjectPath);
    }

    private static string FindSolutionRoot(string startingDirectory)
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

    private static string CreateTempPath(string testName)
    {
        var tempPath = Path.GetTempPath();
        var testRunPath = Path.Combine(tempPath, $"swallow-msbuild-node-test-{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}-{Guid.NewGuid():N}", testName);
        Directory.CreateDirectory(testRunPath);

        return testRunPath;
    }
}
