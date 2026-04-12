namespace Swallow.MSBuild.Node.Test;

public sealed class PublishOutputTest
{
    private static readonly string[] ProjectFiles = ["TestHost.csproj", "Client/index.js", "build.js", "package.json", "package-lock.json", "Program.cs", "NuGet.config"];

    [Test]
    public async Task BuiltAsset_IsInPublishedOutput()
    {
        var testRunPath = TestUtils.CreateTempPath("published");
        var solutionPath = TestUtils.FindSolutionRoot(Environment.CurrentDirectory);

        var testProject = Path.Combine(testRunPath, "TestHost");
        TestUtils.CopyProject(fromPath: Path.Combine(solutionPath, "test", "TestHost"), toPath: testProject, files: ProjectFiles);

        var packageOutput = Path.Combine(testRunPath, "packages");
        TestUtils.PublishPackage(Path.Combine(solutionPath, "src", "Swallow.MSBuild.Node"), packageOutput);

        TestUtils.AddPackageReference(testProject);

        var publishOutput = Path.Combine(testRunPath, "publish");
        TestUtils.PublishProject(testProject, publishOutput);

        var expectedFile = Path.Combine(publishOutput, "wwwroot", "index.min.js");
        await Assert.That(File.Exists(expectedFile)).IsTrue();
    }
}
