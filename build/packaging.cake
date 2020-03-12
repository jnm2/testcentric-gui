//////////////////////////////////////////////////////////////////////
// PACKAGING METHODS AND CLASSES
//////////////////////////////////////////////////////////////////////

var RootFiles = new string[]
{
    "LICENSE.txt",
    "NOTICES.txt",
    "CHANGES.txt"
};

var baseFiles = new string[]
{
    "testcentric.exe",
    "testcentric.exe.config",
    "tc-next.exe",
    "tc-next.exe.config",
    "TestCentric.Common.dll",
    "TestCentric.Gui.Components.dll",
    "TestCentric.Gui.Runner.dll",
    "Experimental.Gui.Runner.dll",
    "nunit.uiexception.dll",
    "TestCentric.Gui.Model.dll",
    "testcentric.engine.api.dll",
    "testcentric.engine.metadata.dll",
    "testcentric.engine.core.dll",
    "testcentric.engine.dll",
    "Mono.Cecil.dll"
};

var PdbFiles = new string[]
{
    "testcentric.pdb",
    "tc-next.pdb",
    "TestCentric.Common.pdb",
    "TestCentric.Gui.Components.pdb",
    "TestCentric.Gui.Runner.pdb",
    "Experimental.Gui.Runner.pdb",
    "nunit.uiexception.pdb",
    "TestCentric.Gui.Model.pdb",
    "testcentric.engine.api.pdb",
    "testcentric.engine.metadata.pdb",
    "testcentric.engine.core.pdb",
    "testcentric.engine.pdb",
};

private void CreateImage(BuildParameters parameters)
{
	string imageDir = parameters.ImageDirectory;
	string imageBinDir = imageDir + "bin/";

    CreateDirectory(imageDir);
    CleanDirectory(imageDir);

	CopyFiles(RootFiles, imageDir);

	var copyFiles = new List<string>(baseFiles);
	if (!parameters.UsingXBuild)
		copyFiles.AddRange(PdbFiles);

	CreateDirectory(imageBinDir);

	foreach (string file in copyFiles)
		CopyFileToDirectory(parameters.OutputDirectory + file, imageBinDir);

	CopyDirectory(parameters.OutputDirectory + "Images", imageBinDir + "Images");

	foreach (var runtime in parameters.SupportedAgentRuntimes)
    {
        var targetDir = imageBinDir + "agents/" + Directory(runtime);
        var sourceDir = parameters.OutputDirectory + "agents/" + Directory(runtime);
        CopyDirectory(sourceDir, targetDir);
	}

	// NOTE: Files specific to a particular package are not copied
	// into the image directory but are added separately.
}

string MYGET_API_KEY = EnvironmentVariable("MYGET_API_KEY");
string MYGET_PUSH_URL = "https://www.myget.org/F/testcentric/api/v2";
string NUGET_API_KEY = EnvironmentVariable("NUGET_API_KEY");
string NUGET_PUSH_URL = "https://api.nuget.org/v3/index.json";
string CHOCO_API_KEY = EnvironmentVariable("CHOCO_API_KEY");
string CHOCO_PUSH_URL = "https://push.chocolatey.org/";

private void PublishToMyGet(FilePath packageName)
{
	EnsurePackageExists(packageName);

	Information($"Publishing {packageName} to myget.org.");
	NuGetPush(packageName, new NuGetPushSettings() { ApiKey=MYGET_API_KEY, Source=MYGET_PUSH_URL });
}

private void PublishToNuGet(FilePath packageName)
{
	EnsurePackageExists(packageName);

	Information($"Publishing {packageName} to nuget.org.");
	NuGetPush(packageName, new NuGetPushSettings() { ApiKey=NUGET_API_KEY, Source=NUGET_PUSH_URL });
}

private void PublishToChocolatey(FilePath packageName)
{
	EnsurePackageExists(packageName);
	EnsureKeyIsSet(CHOCO_API_KEY);

	Information($"Publishing {packageName} to chocolatey.");
	ChocolateyPush(packageName, new ChocolateyPushSettings() { ApiKey=CHOCO_API_KEY, Source=CHOCO_PUSH_URL });
}

private void EnsurePackageExists(FilePath path)
{
	if (!FileExists(path))
	{
		var packageName = path.GetFilename();
		throw new InvalidOperationException(
			$"Package not found: {packageName}.\nCode may have changed since package was last built.");
	}
}

private void EnsureKeyIsSet(string apiKey)
{
	if (string.IsNullOrEmpty(apiKey))
		throw new InvalidOperationException("The Api Key has not been set.");
}

string[] ENGINE_FILES = { 
    "testcentric.engine.dll", "testcentric.engine.core.dll", "testcentric.engine.api.dll", "testcentric.engine.metadata.dll", "Mono.Cecil.dll"};
string[] AGENT_FILES = { 
    "testcentric-agent.exe", "testcentric-agent.exe.config", "testcentric-agent-x86.exe", "testcentric-agent-x86.exe.config",
    "testcentric.engine.core.dll", "testcentric.engine.api.dll", "testcentric.engine.metadata.dll" };
string[] GUI_FILES = {
	"testcentric.exe", "testcentric.exe.config", "tc-next.exe", "tc-next.exe.config", "nunit.uiexception.dll",
	"TestCentric.Gui.Runner.dll", "Experimental.Gui.Runner.dll", "TestCentric.Gui.Model.dll", "TestCentric.Common.dll" };
string[] TREE_ICONS_JPG = {
    "Success.jpg", "Failure.jpg", "Ignored.jpg", "Inconclusive.jpg", "Skipped.jpg" };
string[] TREE_ICONS_PNG = {
    "Success.png", "Failure.png", "Ignored.png", "Inconclusive.png", "Skipped.png" };

private class PackageChecker
{
    protected string _packageName;
    protected string _packageDir;

    public PackageChecker(string packageName, string packageDir)
    {
        _packageName = packageName;
        _packageDir = packageDir;
    }

    public bool RunChecks(params Check[] checks)
    {
        bool allPassed = true;

        if (checks.Length == 0)
        {
            Console.WriteLine("  Package found but no checks were specified.");
        }
        else
        {
            foreach (var check in checks)
                allPassed &= check.Apply(_packageDir);

            if (allPassed)
                Console.WriteLine("  All checks passed!");
        }

        return allPassed;
    }
}

private abstract class Check
{
    public abstract bool Apply(string dir);

    protected static void RecordError(string msg)
    {
        Console.WriteLine("  ERROR: " + msg);
    }
}

private class FileCheck : Check
{
    string[] _paths;

    public FileCheck(string[] paths)
    {
        _paths = paths;
    }

    public override bool Apply(string dir)
    {
        var isOK = true;

        foreach (string path in _paths)
        {
            if (!System.IO.File.Exists(dir + path))
            {
                RecordError($"File {path} was not found.");
                isOK = false;
            }
        }

        return isOK;
    }
}

private class DirectoryCheck : Check
{
    private string _path;
    private List<string> _files = new List<string>();

    public DirectoryCheck(string path)
    {
        _path = path;
    }

    public DirectoryCheck WithFiles(params string[] files)
    {
        _files.AddRange(files);
        return this;
    }

    public DirectoryCheck AndFiles(params string[] files)
    {
        return WithFiles(files);
    }

    public DirectoryCheck WithFile(string file)
    {
        _files.Add(file);
        return this;
    }

    public override bool Apply(string dir)
    {
        if (!System.IO.Directory.Exists(dir + _path))
        {
            RecordError($"Directory {_path} was not found.");
            return false;
        }

        bool isOK = true;

        if (_files != null)
        {
            foreach (var file in _files)
            {
                if (!System.IO.File.Exists(System.IO.Path.Combine(dir + _path, file)))
                {
                    RecordError($"File {file} was not found in directory {_path}.");
                    isOK = false;
                }
            }
        }

        return isOK;
    }
}

private FileCheck HasFile(string file) => HasFiles(new [] { file });
private FileCheck HasFiles(params string[] files) => new FileCheck(files);  

private DirectoryCheck HasDirectory(string dir) => new DirectoryCheck(dir);