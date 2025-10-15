
/*
    How to generate updated bindings:

    1. Clone the Spout2 repository from https://github.com/leadedge/Spout2
    2. In the code below, set SpoutSourcePath to SpoutGL in the local Spout2 clone
    3. Copy the matching-version Spout.DLL to the cloned SpoutGL directory
    4. Create an "output" directory inside the cloned SpoutGL directory
    5. Run this program to generate the bindings
    6. Generated bindings will be in the "output" directory, copy to Spout.NETCore
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
    const string SpoutSourcePath = @"C:\Source\Repos\Spout2\SPOUTSDK\SpoutGL\";

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
        options.OutputDir = $"{SpoutSourcePath}output";
        options.Compilation.Platform = TargetPlatform.Windows;
        options.Compilation.VsVersion = VisualStudioVersion.Latest;
        options.GenerateFinalizers = true;
        options.GenerationOutputMode = GenerationOutputMode.FilePerModule;

        var module = options.AddModule("Spout.Interop");
        module.IncludeDirs.Add(SpoutSourcePath);
        foreach (string file in Directory.GetFiles(SpoutSourcePath))
        {
            if (file.EndsWith(".h")) module.Headers.Add(file);
        }
        module.LibraryDirs.Add(SpoutSourcePath);
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
