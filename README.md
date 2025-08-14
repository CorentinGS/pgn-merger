# PgnMerger

A simple F# command-line tool for merging multiple PGN (Portable Game Notation) chess game files into a single file.

## Description

PgnMerger takes a directory containing multiple PGN files and combines them into a single `merge.pgn` file in the same directory. This is useful for consolidating chess game databases from sources like Lichess Elite games spanning multiple months or years.

## Features

- Merges all PGN files in a specified directory
- Preserves the original file encoding (UTF-8)
- Creates a consolidated output file named `merge.pgn`
- Includes error handling for common file system issues
- Provides progress feedback during the merge process

## Requirements

- .NET 9.0 or later
- F# compiler

## Building

```bash
dotnet build
```

## Usage

```bash
dotnet run <folder_path>
```

### Example

```bash
dotnet run ./test_pgn
```

This will merge all PGN files in the `test_pgn` directory into `test_pgn/merge.pgn`.

## Input/Output

- **Input**: A directory containing one or more `.pgn` files
- **Output**: A single `merge.pgn` file in the same directory containing all merged games

## Error Handling

The application handles various error scenarios:
- Directory doesn't exist
- No PGN files found in the directory  
- File access permission issues
- I/O errors during file operations

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

Corentin Giaufer Saubert