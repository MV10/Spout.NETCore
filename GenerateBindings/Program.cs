
/*
    How to generate updated bindings:

    1. Clone the Spout2 repository from https://github.com/leadedge/Spout2
    2. Copy the latest Spout.DLL to the Spout.NETCore project directory
    3. In the code below:
    4.   Set SpoutNetCorePath to the Spout.NETCore project directory
    5.   Set SpoutHeaderPath to the SpoutGL directory in the local Spout2 clone
    6. Run this program to update Spout.Interop.cs and Std.cs
    7. The generated *.cpp files are in gitignore and will not be checked in
    8. Update the Spout.NETCore.csproj version to match the Spout.DLL version
*/

// Currently CppSharp is not compatible with Clang 19 shipping with
// Visual Studio 2022. When CppSharp is updated and re-packaged,
// comment out the version mismatch option in the Setup method below.

// Issue:
// https://github.com/mono/CppSharp/issues/1940

// Fixed by:
// https://github.com/mono/CppSharp/pull/1942
// https://github.com/mono/CppSharp/pull/1946

using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;

namespace GenerateBindings;

public class CppSharpLibrary : ILibrary
{
    // update to match your Spout2 repo clone path
    const string SpoutHeaderPath = @"C:\Source\Repos\Spout2\SPOUTSDK\SpoutGL\";
    
    // update to match your Spout.NETCore path (the Spout.NETCore.csproj location)
    const string SpoutNETCorePath = @"C:\Source\Repos\Spout.NETCore\Spout.NETCore\";

    public void Preprocess(Driver driver, ASTContext ctx)
    { }

    public void Postprocess(Driver driver, ASTContext ctx)
    { }

    public void Setup(Driver driver)
    {
        // COMMENT THIS OUT IF CPPSHARP PACKAGE MATCHES THE SHIPPING CLANG VERSION
        driver.ParserOptions.AddDefines("_ALLOW_COMPILER_AND_STL_VERSION_MISMATCH");

        var options = driver.Options;
        options.GeneratorKind = GeneratorKind.CSharp;
        options.OutputDir = SpoutNETCorePath;
        options.Compilation.Platform = TargetPlatform.Windows;
        options.Compilation.VsVersion = VisualStudioVersion.Latest;
        options.GenerateFinalizers = true;
        options.GenerationOutputMode = GenerationOutputMode.FilePerModule;

        var module = options.AddModule("Spout.Interop");
        module.IncludeDirs.Add(SpoutHeaderPath);
        foreach (string file in Directory.GetFiles(SpoutHeaderPath))
        {
            if (file.EndsWith(".h")) module.Headers.Add(file);
        }
        module.LibraryDirs.Add(SpoutNETCorePath);
        module.Libraries.Add("Spout.dll"); // in Spout2 repo's SpoutGL directory
    }

    public void SetupPasses(Driver driver)
    { }
}

internal class Program
{
    static void Main(string[] args)
    {
        ConsoleDriver.Run(new CppSharpLibrary());
    }
}
