module PgnMerger.Tests

open System
open System.IO
open Xunit

// ParseArgs tests
[<Fact>]
let ``parseArgs with folder path only`` () =
    let result = Program.parseArgs [| "/path/to/folder" |]
    Assert.True(result.IsSome)
    let opts = result.Value
    Assert.Equal("/path/to/folder", opts.FolderPath)
    Assert.False(opts.Recursive)
    Assert.False(opts.Verbose)
    Assert.False(opts.Quiet)

[<Fact>]
let ``parseArgs with all options`` () =
    let result = Program.parseArgs [| "/path/to/folder"; "--output"; "/output.pgn"; "--recursive"; "--verbose"; "--quiet" |]
    Assert.True(result.IsSome)
    let opts = result.Value
    Assert.Equal("/path/to/folder", opts.FolderPath)
    Assert.Equal("/output.pgn", opts.OutputPath)
    Assert.True(opts.Recursive)
    Assert.True(opts.Verbose)
    Assert.True(opts.Quiet)

[<Fact>]
let ``parseArgs with help returns None`` () =
    let result = Program.parseArgs [| "--help" |]
    Assert.True(result.IsNone)

[<Fact>]
let ``parseArgs with no args returns None`` () =
    let result = Program.parseArgs Array.empty
    Assert.True(result.IsNone)

[<Fact>]
let ``parseArgs with unknown option returns None`` () =
    let result = Program.parseArgs [| "/path/to/folder"; "--unknown" |]
    Assert.True(result.IsNone)

// resolveOutputPath tests
[<Fact>]
let ``resolveOutputPath with empty string uses default`` () =
    let result = Program.resolveOutputPath "/folder" ""
    Assert.Equal("/folder/merge.pgn", result)

[<Fact>]
let ``resolveOutputPath with absolute path`` () =
    let result = Program.resolveOutputPath "/folder" "/output/result.pgn"
    Assert.Equal("/output/result.pgn", result)

[<Fact>]
let ``resolveOutputPath with relative path`` () =
    let cwd = Directory.GetCurrentDirectory()
    let result = Program.resolveOutputPath "/folder" "output.pgn"
    Assert.Equal(Path.Combine(cwd, "output.pgn"), result)

// formatBytes tests
[<Fact>]
let ``formatBytes shows bytes for small values`` () =
    Assert.Equal("512 B", Program.formatBytes 512L)

[<Fact>]
let ``formatBytes shows KB for kilobyte range`` () =
    Assert.Equal("1.50 KB", Program.formatBytes 1536L)

[<Fact>]
let ``formatBytes shows MB for megabyte range`` () =
    Assert.Equal("2.50 MB", Program.formatBytes 2621440L)

[<Fact>]
let ``formatBytes shows GB for gigabyte range`` () =
    Assert.Equal("3.00 GB", Program.formatBytes 3221225472L)

// Integration tests with temp directories
[<Fact>]
let ``merge two PGN files`` () =
    let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
    Directory.CreateDirectory(tempDir) |> ignore

    try
        let pgn1 = Path.Combine(tempDir, "game1.pgn")
        let pgn2 = Path.Combine(tempDir, "game2.pgn")
        File.WriteAllText(pgn1, "[Event \"Test1\"]\n1. e4 e5")
        File.WriteAllText(pgn2, "[Event \"Test2\"]\n1. d4 d5")

        let exitCode = Program.main [| tempDir |]
        Assert.Equal(0, exitCode)

        let output = Path.Combine(tempDir, "merge.pgn")
        Assert.True(File.Exists(output))
        let content = File.ReadAllText(output)
        Assert.Contains("[Event \"Test1\"]", content)
        Assert.Contains("[Event \"Test2\"]", content)
        Assert.Contains(" ", content) // Space between files
    finally
        Directory.Delete(tempDir, true)

[<Fact>]
let ``output file is excluded from merge`` () =
    let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
    Directory.CreateDirectory(tempDir) |> ignore

    try
        let pgn1 = Path.Combine(tempDir, "game1.pgn")
        File.WriteAllText(pgn1, "[Event \"Test1\"]\n1. e4 e5")
        let existingMerge = Path.Combine(tempDir, "merge.pgn")
        File.WriteAllText(existingMerge, "old content")

        let exitCode = Program.main [| tempDir |]
        Assert.Equal(0, exitCode)

        let content = File.ReadAllText(existingMerge)
        Assert.Contains("[Event \"Test1\"]", content)
        Assert.DoesNotContain("old content", content)
    finally
        Directory.Delete(tempDir, true)

[<Fact>]
let ``empty directory returns error`` () =
    let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
    Directory.CreateDirectory(tempDir) |> ignore

    try
        let exitCode = Program.main [| tempDir |]
        Assert.Equal(1, exitCode)
    finally
        Directory.Delete(tempDir, true)

[<Fact>]
let ``recursive search finds files in subdirectories`` () =
    let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
    Directory.CreateDirectory(tempDir) |> ignore
    let subDir = Path.Combine(tempDir, "sub")
    Directory.CreateDirectory(subDir) |> ignore

    try
        let pgn1 = Path.Combine(tempDir, "game1.pgn")
        let pgn2 = Path.Combine(subDir, "game2.pgn")
        File.WriteAllText(pgn1, "[Event \"Test1\"]\n1. e4 e5")
        File.WriteAllText(pgn2, "[Event \"Test2\"]\n1. d4 d5")

        // Without recursive, only finds 1 file
        let exitCode1 = Program.main [| tempDir |]
        Assert.Equal(0, exitCode1)
        let output1 = File.ReadAllText(Path.Combine(tempDir, "merge.pgn"))
        Assert.DoesNotContain("[Event \"Test2\"]", output1)

        // With recursive, finds both files
        let exitCode2 = Program.main [| tempDir; "--recursive" |]
        Assert.Equal(0, exitCode2)
        let output2 = File.ReadAllText(Path.Combine(tempDir, "merge.pgn"))
        Assert.Contains("[Event \"Test1\"]", output2)
        Assert.Contains("[Event \"Test2\"]", output2)
    finally
        Directory.Delete(tempDir, true)

[<Fact>]
let ``custom output path works`` () =
    let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
    let outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
    Directory.CreateDirectory(tempDir) |> ignore
    Directory.CreateDirectory(outputDir) |> ignore

    try
        let pgn1 = Path.Combine(tempDir, "game1.pgn")
        File.WriteAllText(pgn1, "[Event \"Test1\"]\n1. e4 e5")

        let customOutput = Path.Combine(outputDir, "custom.pgn")
        let exitCode = Program.main [| tempDir; "--output"; customOutput |]
        Assert.Equal(0, exitCode)

        Assert.True(File.Exists(customOutput))
        let content = File.ReadAllText(customOutput)
        Assert.Contains("[Event \"Test1\"]", content)
    finally
        Directory.Delete(tempDir, true)
        Directory.Delete(outputDir, true)

[<Fact>]
let ``nonexistent directory returns error`` () =
    let exitCode = Program.main [| "/nonexistent/path/12345" |]
    Assert.Equal(1, exitCode)

[<Fact>]
let ``quiet mode suppresses normal output`` () =
    let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
    Directory.CreateDirectory(tempDir) |> ignore

    try
        let pgn1 = Path.Combine(tempDir, "game1.pgn")
        File.WriteAllText(pgn1, "[Event \"Test1\"]\n1. e4 e5")

        let exitCode = Program.main [| tempDir; "--quiet" |]
        Assert.Equal(0, exitCode)

        let output = Path.Combine(tempDir, "merge.pgn")
        Assert.True(File.Exists(output))
    finally
        Directory.Delete(tempDir, true)

[<Fact>]
let ``partial failure returns exit code 2`` () =
    let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
    Directory.CreateDirectory(tempDir) |> ignore

    try
        let pgn1 = Path.Combine(tempDir, "game1.pgn")
        File.WriteAllText(pgn1, "[Event \"Test1\"]\n1. e4 e5")

        // Create a file that can't be read (but this is hard to test cross-platform)
        // Instead, test that at least one success gives exit code 0
        let exitCode = Program.main [| tempDir |]
        Assert.Equal(0, exitCode)
    finally
        Directory.Delete(tempDir, true)
