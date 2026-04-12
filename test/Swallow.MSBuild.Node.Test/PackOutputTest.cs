using System.IO.Compression;

namespace Swallow.MSBuild.Node.Test;

public sealed class PackOutputTest
{
    private static readonly string[] ProjectFiles = ["TestPackage.csproj", "Client/index.js", "build.js", "package.json", "package-lock.json", "NuGet.config"];

    [Test]
    public async Task BuiltAsset_IsInNugetPackage()
    {
        var testRunPath = TestUtils.CreateTempPath("packaged");
        var solutionPath = TestUtils.FindSolutionRoot(Environment.CurrentDirectory);

        var testProject = Path.Combine(testRunPath, "TestPackage");
        TestUtils.CopyProject(fromPath: Path.Combine(solutionPath, "test", "TestPackage"), toPath: testProject, files: ProjectFiles);

        var packageOutput = Path.Combine(testRunPath, "packages");
        TestUtils.PublishPackage(Path.Combine(solutionPath, "src", "Swallow.MSBuild.Node"), packageOutput);

        TestUtils.AddPackageReference(testProject);

        var publishOutput = Path.Combine(testRunPath, "publish");
        TestUtils.PackProject(testProject, publishOutput);

        await ZipFile.ExtractToDirectoryAsync(Path.Combine(publishOutput, "TestPackage.1.0.0.nupkg"), publishOutput);

        var expectedFile = Path.Combine(publishOutput, "staticwebassets", "index.min.js");
        await Assert.That(File.Exists(expectedFile)).IsTrue();
    }
}
