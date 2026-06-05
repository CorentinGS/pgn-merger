open System
open System.IO
open System.Text
open System.Diagnostics

type Options =
    { FolderPath: string
      OutputPath: string
      Recursive: bool
      Verbose: bool
      Quiet: bool }

type FileResult =
    | Success of string * int64
    | Failure of string * string

let showUsage () =
    printfn "Usage: PgnMerger <folder_path> [options]"
    printfn ""
    printfn "Options:"
    printfn "  --output <path>    Output file path (default: <folder>/merge.pgn)"
    printfn "  --recursive        Search subdirectories for PGN files"
    printfn "  --verbose          Show detailed progress with throughput"
    printfn "  --quiet            Only show errors"
    printfn ""
    printfn "Example:"
    printfn "  PgnMerger ./chess_games --output ./combined.pgn --verbose"

let parseArgs (args: string[]) =
    if args.Length = 0 then
        None
    else
        let rec loop i opts =
            if i >= args.Length then
                Some opts
            else
                match args.[i] with
                | "--output" when i + 1 < args.Length -> loop (i + 2) { opts with OutputPath = args.[i + 1] }
                | "--recursive" -> loop (i + 1) { opts with Recursive = true }
                | "--verbose" -> loop (i + 1) { opts with Verbose = true }
                | "--quiet" -> loop (i + 1) { opts with Quiet = true }
                | "--help"
                | "-h"
                | "-?" -> None
                | arg when arg.StartsWith("--") ->
                    printfn $"Error: Unknown option '%s{arg}'"
                    None
                | _ when i = 0 -> loop (i + 1) { opts with FolderPath = args.[0] }
                | _ ->
                    printfn $"Error: Unexpected argument '%s{args.[i]}'"
                    None

        loop
            0
            { FolderPath = ""
              OutputPath = ""
              Recursive = false
              Verbose = false
              Quiet = false }

let resolveOutputPath folderPath outputPath =
    if String.IsNullOrWhiteSpace(outputPath) then
        Path.Combine(folderPath, "merge.pgn")
    elif Path.IsPathRooted(outputPath) then
        outputPath
    else
        Path.Combine(Directory.GetCurrentDirectory(), outputPath)

let findPgnFiles folderPath recursive =
    let searchOption =
        if recursive then
            SearchOption.AllDirectories
        else
            SearchOption.TopDirectoryOnly

    Directory.GetFiles(folderPath, "*.pgn", searchOption)

let streamFileToWriter (reader: StreamReader) (writer: StreamWriter) bufferSize =
    let buffer = Array.zeroCreate<char> bufferSize
    let mutable totalBytes = 0L
    let mutable charsRead = 0

    while (charsRead <- reader.Read(buffer, 0, buffer.Length)
           charsRead > 0) do
        writer.Write(buffer, 0, charsRead)
        totalBytes <- totalBytes + int64 charsRead

    totalBytes

let formatBytes (bytes: int64) =
    if bytes > 1073741824L then
        $"%.2f{float bytes / 1073741824.0} GB"
    elif bytes > 1048576L then
        $"%.2f{float bytes / 1048576.0} MB"
    elif bytes > 1024L then
        $"%.2f{float bytes / 1024.0} KB"
    else
        $"%d{bytes} B"

let formatThroughput (bytes: int64) (elapsed: TimeSpan) =
    let seconds = elapsed.TotalSeconds

    if seconds > 0.0 then
        let bytesPerSec = float bytes / seconds

        if bytesPerSec > 1048576.0 then
            $"%.2f{bytesPerSec / 1048576.0} MB/s"
        elif bytesPerSec > 1024.0 then
            $"%.2f{bytesPerSec / 1024.0} KB/s"
        else
            $"%.0f{bytesPerSec} B/s"
    else
        "N/A"

[<EntryPoint>]
let main argv =
    match parseArgs argv with
    | None ->
        showUsage ()
        1
    | Some opts ->
        let folderPath = opts.FolderPath

        if not (Directory.Exists folderPath) then
            printfn $"Error: Directory '%s{folderPath}' does not exist"
            1
        else
            try
                let outputPath = resolveOutputPath folderPath opts.OutputPath
                let outputFileName = Path.GetFileName(outputPath)

                let pgnFiles =
                    findPgnFiles folderPath opts.Recursive
                    |> Array.filter (fun f ->
                        let fileName = Path.GetFileName(f)
                        not (String.Equals(fileName, outputFileName, StringComparison.OrdinalIgnoreCase)))

                if pgnFiles.Length = 0 then
                    printfn $"No PGN files found in directory '%s{folderPath}'"
                    1
                else
                    if not opts.Quiet then
                        printfn $"Found %d{pgnFiles.Length} PGN files to merge"

                    let totalBytes = pgnFiles |> Array.map FileInfo |> Array.sumBy (fun fi -> fi.Length)

                    if opts.Verbose && not opts.Quiet then
                        printfn $"Total size: %s{formatBytes totalBytes}"
                        printfn $"Output: %s{outputPath}"
                        printfn ""

                    let stopwatch = Stopwatch.StartNew()
                    let mutable bytesProcessed = 0L
                    let mutable results: FileResult list = []

                    use outputStream =
                        new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 65536)

                    use writer = new StreamWriter(outputStream, Encoding.UTF8, 65536)

                    let mutable isFirstFile = true

                    for pgnFile in pgnFiles do
                        try
                            let fileName = Path.GetFileName(pgnFile)
                            let fileSize = FileInfo(pgnFile).Length
                            let fileStopwatch = Stopwatch.StartNew()

                            use inputStream =
                                new FileStream(pgnFile, FileMode.Open, FileAccess.Read, FileShare.Read, 65536)

                            use reader = new StreamReader(inputStream, Encoding.UTF8, true, 65536)

                            if not isFirstFile then
                                writer.Write(" ")
                                bytesProcessed <- bytesProcessed + 1L

                            let charsCopied = streamFileToWriter reader writer 65536
                            bytesProcessed <- bytesProcessed + int64 charsCopied
                            isFirstFile <- false

                            fileStopwatch.Stop()

                            if opts.Verbose && not opts.Quiet then
                                let throughput = formatThroughput (int64 charsCopied) fileStopwatch.Elapsed

                                let percent =
                                    if totalBytes > 0L then
                                        float bytesProcessed / float totalBytes * 100.0
                                    else
                                        0.0

                                printfn "[%5.1f%%] %s (%s) - %s" percent fileName (formatBytes fileSize) throughput
                            elif not opts.Quiet then
                                printfn $"Processing: %s{fileName}"

                            results <- Success(fileName, fileSize) :: results

                        with
                        | :? UnauthorizedAccessException ->
                            results <- Failure(pgnFile, "Access denied") :: results
                            printfn $"Error: Access denied to '%s{pgnFile}'"
                        | :? IOException as ex ->
                            results <- Failure(pgnFile, ex.Message) :: results
                            printfn $"Error: IO error processing '%s{pgnFile}': %s{ex.Message}"
                        | ex ->
                            results <- Failure(pgnFile, ex.Message) :: results
                            printfn $"Error: Unexpected error processing '%s{pgnFile}': %s{ex.Message}"

                    writer.Flush()
                    stopwatch.Stop()

                    let successCount =
                        results
                        |> List.filter (function
                            | Success _ -> true
                            | _ -> false)
                        |> List.length

                    let failureCount = results.Length - successCount

                    if not opts.Quiet then
                        printfn ""

                        printfn
                            $"Successfully merged %d{successCount} of %d{pgnFiles.Length} PGN files into: %s{outputPath}"

                        printfn "Total time: %s" (stopwatch.Elapsed.ToString("hh\\:mm\\:ss\\.fff"))

                        if stopwatch.Elapsed.TotalSeconds > 0.0 then
                            printfn $"Average throughput: %s{formatThroughput bytesProcessed stopwatch.Elapsed}"

                    if failureCount > 0 then
                        printfn ""
                        printfn $"Failed files (%d{failureCount}):"

                        results
                        |> List.filter (function
                            | Failure _ -> true
                            | _ -> false)
                        |> List.iter (function
                            | Failure(path, msg) -> printfn "  - %s: %s" (Path.GetFileName(path)) msg
                            | _ -> ())

                    if failureCount > 0 && successCount = 0 then 1
                    elif failureCount > 0 then 2
                    else 0

            with
            | :? UnauthorizedAccessException ->
                printfn "Error: Access denied to directory or files"
                1
            | :? IOException as ex ->
                printfn $"Error: IO error occurred: %s{ex.Message}"
                1
            | ex ->
                printfn $"Error: Unexpected error occurred: %s{ex.Message}"
                1
