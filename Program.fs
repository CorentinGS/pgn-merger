open System
open System.IO
open System.Text

[<EntryPoint>]
let main argv =
    if argv.Length <> 1 then
        printfn "Usage: PgnMerger <folder_path>"
        printfn "Example: PgnMerger ./chess_games"
        1
    else
        let folderPath = argv.[0]
        
        if not (Directory.Exists folderPath) then
            printfn $"Error: Directory '%s{folderPath}' does not exist"
            1
        else
            try
                // Get all PGN files in the directory
                let pgnFiles = Directory.GetFiles(folderPath, "*.pgn")
                
                if pgnFiles.Length = 0 then
                    printfn $"No PGN files found in directory '%s{folderPath}'"
                    1
                else
                    printfn $"Found %d{pgnFiles.Length} PGN files to merge"
                    
                    // Create output file path
                    let outputPath = Path.Combine(folderPath, "merge.pgn")
                    
                    // Merge files using streams
                    use outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write)
                    use writer = new StreamWriter(outputStream, Encoding.UTF8)
                    
                    let mutable isFirstFile = true
                    
                    for pgnFile in pgnFiles do
                        printfn $"Processing: %s{Path.GetFileName pgnFile}"
                        
                        // Read and write the PGN file content
                        use inputStream = new FileStream(pgnFile, FileMode.Open, FileAccess.Read)
                        use reader = new StreamReader(inputStream, Encoding.UTF8)
                        
                        let content = reader.ReadToEnd()
                        
                        // Add space between files (except for the first one)
                        if not isFirstFile then
                            writer.Write(" ")
                        
                        writer.Write(content)
                        isFirstFile <- false
                    
                    writer.Flush()
                    printfn $"Successfully merged %d{pgnFiles.Length} PGN files into: %s{outputPath}"
                    0
                    
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