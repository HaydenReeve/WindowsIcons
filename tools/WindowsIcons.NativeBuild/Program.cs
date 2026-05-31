#nullable enable

using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using AsmResolver;
using AsmResolver.PE;
using AsmResolver.PE.Builder;
using AsmResolver.PE.File;
using AsmResolver.PE.Win32Resources;
using AsmResolver.PE.Win32Resources.Icon;

return NativeIconDllBuilder.Run(args);

internal static class NativeIconDllBuilder
{
    private const uint NeutralLanguageId = 0;
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public static int Run(string[] args)
    {
        try
        {
            var options = BuildOptions.Parse(args);
            Build(options);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static void Build(BuildOptions options)
    {
        var projectDirectory = Path.GetFullPath(options.ProjectDirectory);
        if (!Directory.Exists(projectDirectory))
            throw new DirectoryNotFoundException($"Project directory not found: {projectDirectory}");

        var inputAssemblyPath = Path.GetFullPath(options.InputAssemblyPath);
        if (!File.Exists(inputAssemblyPath))
            throw new FileNotFoundException($"Input assembly not found: {inputAssemblyPath}", inputAssemblyPath);

        var machine = ResolveMachine(options.Machine);
        var iconsRoot = Path.Combine(projectDirectory, "Icons");
        if (!Directory.Exists(iconsRoot))
            throw new DirectoryNotFoundException($"Icons directory not found: {iconsRoot}");

        var iconSources = EnumerateIconSources(projectDirectory, iconsRoot);
        if (iconSources.Count == 0)
            throw new InvalidOperationException($"No .ico files found under {iconsRoot}");

        var intermediateRoot = options.IntermediateRootPath is { Length: > 0 }
            ? Path.GetFullPath(options.IntermediateRootPath)
            : Path.Combine(projectDirectory, "obj", "NativeIconDll", options.Configuration, machine.OutputSegment);

        var outputPath = options.OutputPath is { Length: > 0 }
            ? Path.GetFullPath(options.OutputPath)
            : Path.Combine(projectDirectory, "bin", options.Configuration, "native", machine.OutputSegment, Path.GetFileName(inputAssemblyPath));

        Directory.CreateDirectory(intermediateRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? throw new InvalidOperationException($"Invalid output path: {outputPath}"));

        var resourceScriptPath = Path.Combine(intermediateRoot, "icons.rc");
        var iconMapPath = Path.Combine(intermediateRoot, "icon-map.csv");

        WriteResourceScript(resourceScriptPath, iconSources);
        WriteIconMap(iconMapPath, iconSources);

        var image = PEImage.FromFile(inputAssemblyPath);
        image.Resources ??= new ResourceDirectory((uint) 0);

        var iconResource = new IconResource(IconType.Icon);
        var nextIconId = 1;

        foreach (var iconSource in iconSources)
        {
            var group = new IconGroup(iconSource.ResourceId, NeutralLanguageId)
            {
                Type = IconType.Icon,
            };

            foreach (var imageData in iconSource.Images)
            {
                group.Icons.Add(new IconEntry(checked((ushort) nextIconId++), NeutralLanguageId)
                {
                    Width = imageData.Width,
                    Height = imageData.Height,
                    ColorCount = imageData.ColorCount,
                    Reserved = imageData.Reserved,
                    Planes = imageData.Planes,
                    BitsPerPixel = imageData.BitsPerPixel,
                    PixelData = new DataSegment(imageData.PixelData),
                });
            }

            iconResource.Groups.Add(group);
        }

        iconResource.InsertIntoDirectory(image.Resources);
        image.MachineType = machine.MachineType;
        image.PEKind = machine.PEKind;
        image.ToPEFile(new ManagedPEFileBuilder()).Write(outputPath);

        Console.WriteLine($"Generated {iconSources.Count.ToString(CultureInfo.InvariantCulture)} icon groups.");
        Console.WriteLine($"RC file: {resourceScriptPath}");
        Console.WriteLine($"Icon map: {iconMapPath}");
        Console.WriteLine($"Built native icon DLL: {outputPath}");
    }

    private static IReadOnlyList<IconSource> EnumerateIconSources(string projectDirectory, string iconsRoot)
    {
        var files = Directory
            .EnumerateFiles(iconsRoot, "*.ico", SearchOption.AllDirectories)
            .Select(path =>
            {
                var fullPath = Path.GetFullPath(path);
                var relativePath = Path.GetRelativePath(projectDirectory, fullPath).Replace('/', '\\');

                return new
                {
                    FullPath = fullPath,
                    RelativePath = relativePath,
                };
            })
            .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => file.RelativePath, StringComparer.Ordinal)
            .ToList();

        var result = new List<IconSource>(files.Count);
        for (var index = 0; index < files.Count; index++)
        {
            var file = files[index];
            result.Add(new IconSource(
                checked((uint) index + 1),
                file.FullPath,
                file.RelativePath,
                ParseIconFile(file.FullPath)));
        }

        return result;
    }

    private static IReadOnlyList<IconImageData> ParseIconFile(string path)
    {
        byte[] fileBytes;
        try
        {
            fileBytes = File.ReadAllBytes(path);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Failed to read icon file '{path}': {exception.Message}", exception);
        }

        if (fileBytes.Length < 6)
            throw new InvalidOperationException($"Icon file '{path}' is too small to contain an ICO header.");

        var fileSpan = fileBytes.AsSpan();
        var reserved = BinaryPrimitives.ReadUInt16LittleEndian(fileSpan[..2]);
        var iconType = BinaryPrimitives.ReadUInt16LittleEndian(fileSpan.Slice(2, 2));
        var imageCount = BinaryPrimitives.ReadUInt16LittleEndian(fileSpan.Slice(4, 2));

        if (reserved != 0)
            throw new InvalidOperationException($"Icon file '{path}' has invalid ICO reserved field '{reserved}'.");

        if (iconType != 1)
            throw new InvalidOperationException($"Icon file '{path}' is not an ICO file. Type '{iconType}' is unsupported.");

        if (imageCount == 0)
            throw new InvalidOperationException($"Icon file '{path}' does not contain any icon images.");

        var directoryLength = 6 + (imageCount * 16);
        if (fileBytes.Length < directoryLength)
            throw new InvalidOperationException($"Icon file '{path}' is truncated before icon directory table.");

        var images = new List<IconImageData>(imageCount);
        for (var index = 0; index < imageCount; index++)
        {
            var entryOffset = 6 + (index * 16);
            var imageOffset = BinaryPrimitives.ReadUInt32LittleEndian(fileSpan.Slice(entryOffset + 12, 4));
            var imageLength = BinaryPrimitives.ReadUInt32LittleEndian(fileSpan.Slice(entryOffset + 8, 4));

            if (imageOffset > int.MaxValue || imageLength > int.MaxValue)
                throw new InvalidOperationException($"Icon file '{path}' contains an image entry larger than supported buffer limits.");

            var imageStart = checked((int) imageOffset);
            var bytesInResource = checked((int) imageLength);

            if (imageStart < directoryLength)
                throw new InvalidOperationException($"Icon file '{path}' contains an icon entry with invalid image offset '{imageOffset}'.");

            if (imageStart > fileBytes.Length || bytesInResource > fileBytes.Length - imageStart)
                throw new InvalidOperationException($"Icon file '{path}' contains an icon entry that extends beyond end of file.");

            var pixelData = new byte[bytesInResource];
            Buffer.BlockCopy(fileBytes, imageStart, pixelData, 0, bytesInResource);

            images.Add(new IconImageData(
                fileSpan[entryOffset],
                fileSpan[entryOffset + 1],
                fileSpan[entryOffset + 2],
                fileSpan[entryOffset + 3],
                BinaryPrimitives.ReadUInt16LittleEndian(fileSpan.Slice(entryOffset + 4, 2)),
                BinaryPrimitives.ReadUInt16LittleEndian(fileSpan.Slice(entryOffset + 6, 2)),
                pixelData));
        }

        return images;
    }

    private static void WriteResourceScript(string resourceScriptPath, IReadOnlyList<IconSource> iconSources)
    {
        var builder = new StringBuilder();
        builder.AppendLine("// Generated by tools\\WindowsIcons.NativeBuild. Do not edit.");
        builder.AppendLine("#pragma code_page(65001)");
        builder.AppendLine();

        foreach (var iconSource in iconSources)
        {
            builder
                .Append(iconSource.ResourceId.ToString(CultureInfo.InvariantCulture))
                .Append(" ICON \"")
                .Append(EscapeRcPath(iconSource.FullPath))
                .AppendLine("\"");
        }

        File.WriteAllText(resourceScriptPath, builder.ToString(), Utf8NoBom);
    }

    private static void WriteIconMap(string iconMapPath, IReadOnlyList<IconSource> iconSources)
    {
        var builder = new StringBuilder();
        builder.AppendLine("ResourceId,RelativePath");

        foreach (var iconSource in iconSources)
        {
            builder
                .Append(iconSource.ResourceId.ToString(CultureInfo.InvariantCulture))
                .Append(",\"")
                .Append(iconSource.RelativePath.Replace("\"", "\"\""))
                .AppendLine("\"");
        }

        File.WriteAllText(iconMapPath, builder.ToString(), Utf8NoBom);
    }

    private static string EscapeRcPath(string path) =>
        path.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);

    private static MachineInfo ResolveMachine(string? requestedMachine)
    {
        var normalizedMachine = string.IsNullOrWhiteSpace(requestedMachine) || requestedMachine.Trim().Equals("Any CPU", StringComparison.OrdinalIgnoreCase) || requestedMachine.Trim().Equals("AnyCPU", StringComparison.OrdinalIgnoreCase)
            ? GetHostMachineName()
            : requestedMachine.Trim();

        return normalizedMachine.ToUpperInvariant() switch
        {
            "AMD64" or "X64" => new MachineInfo("x64", MachineType.Amd64, OptionalHeaderMagic.PE32Plus),
            "X86" or "WIN32" or "IA32" => new MachineInfo("x86", MachineType.I386, OptionalHeaderMagic.PE32),
            "ARM64" => new MachineInfo("arm64", MachineType.Arm64, OptionalHeaderMagic.PE32Plus),
            _ => throw new InvalidOperationException($"Unsupported machine '{requestedMachine}'. Use x64, x86, arm64, or AnyCPU."),
        };
    }

    private static string GetHostMachineName() =>
        RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            _ => throw new InvalidOperationException($"Unsupported host architecture '{RuntimeInformation.OSArchitecture}'."),
        };
}

internal sealed record BuildOptions(
    string ProjectDirectory,
    string Configuration,
    string InputAssemblyPath,
    string? Machine,
    string? IntermediateRootPath,
    string? OutputPath)
{
    public static BuildOptions Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0 || args.Any(argument => argument.Equals("--help", StringComparison.OrdinalIgnoreCase) || argument.Equals("-h", StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Usage: dotnet WindowsIcons.NativeBuild.dll --project-dir <path> --configuration <Configuration> --input-assembly <path> [--machine <x64|x86|arm64|AnyCPU>] [--intermediate-root <path>] [--output-path <path>]");
            Environment.Exit(0);
        }

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Count; index++)
        {
            var argument = args[index];
            if (!argument.StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException($"Unexpected argument '{argument}'.");

            if (index + 1 >= args.Count)
                throw new ArgumentException($"Missing value for argument '{argument}'.");

            values[argument] = args[++index];
        }

        return new BuildOptions(
            GetRequiredValue(values, "--project-dir"),
            GetRequiredValue(values, "--configuration"),
            GetRequiredValue(values, "--input-assembly"),
            GetOptionalValue(values, "--machine"),
            GetOptionalValue(values, "--intermediate-root"),
            GetOptionalValue(values, "--output-path"));
    }

    private static string GetRequiredValue(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException($"Missing required argument '{key}'.");

    private static string? GetOptionalValue(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value) ? value : null;
}

internal sealed record MachineInfo(string OutputSegment, MachineType MachineType, OptionalHeaderMagic PEKind);

internal sealed record IconSource(uint ResourceId, string FullPath, string RelativePath, IReadOnlyList<IconImageData> Images);

internal sealed record IconImageData(
    byte Width,
    byte Height,
    byte ColorCount,
    byte Reserved,
    ushort Planes,
    ushort BitsPerPixel,
    byte[] PixelData);
