# PGN Merger — Merge Chess PGN Files via Command Line

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![F#](https://img.shields.io/badge/F%23-7.0-blueviolet)](https://fsharp.org/)

A fast, lightweight F# command-line tool to **merge multiple PGN (Portable Game Notation) chess game files into a single file**.

PGN Merger takes a directory of chess game files and combines them into one consolidated `merge.pgn`. Perfect for building chess databases from Lichess Elite games, tournament archives, or personal game collections.

---

## What is a PGN file?

[PGN](https://en.wikipedia.org/wiki/Portable_Game_Notation) (Portable Game Notation) is the standard file format for recording chess games. Each `.pgn` file contains one or more chess games with metadata headers and move history. Chess players, coaches, and analysts frequently need to **merge PGN files** into a single database for training, opening preparation, or statistical analysis.

---

## Features

- **Merge all PGN files** in a directory into one `merge.pgn`
- **Custom output path** via `--output`
- **Recursive search** through subdirectories via `--recursive`
- **Preserves original file encoding** (UTF-8)
- **Zero dependencies** — standalone CLI binary
- **Cross-platform** — runs on Windows, macOS, and Linux via .NET
- **Progress feedback** with throughput stats in verbose mode
- **Quiet mode** for silent operation
- **Error handling** for missing directories, empty folders, and permission issues

---

## Installation

### Option 1: Download Pre-built Binary (Recommended)

Grab the latest release for your platform from the [Releases page](https://github.com/CorentinGS/pgn-merger/releases):

| Platform | Asset |
|----------|-------|
| Linux (x64) | `PgnMerger-linux-x64` |
| Windows (x64) | `PgnMerger-win-x64.exe` |
| macOS (Intel) | `PgnMerger-osx-x64` |
| macOS (Apple Silicon) | `PgnMerger-osx-arm64` |

No runtime required — self-contained executables.

### Option 2: .NET Global Tool

Requires [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later:

```bash
dotnet tool install -g PgnMerger
```

### Option 3: Build from Source

```bash
git clone https://github.com/CorentinGS/pgn-merger.git
cd pgn-merger
dotnet build
dotnet run -- <folder_path> [options]
```

### Option 3: Build from Source

```bash
git clone https://github.com/CorentinGS/pgn-merger.git
cd pgn-merger
dotnet build
dotnet run -- <folder_path> [options]
```

### As a Global .NET Tool (coming soon)

```bash
dotnet tool install -g PgnMerger
```

---

## Usage

### Basic

```bash
PgnMerger <folder_path> [options]
```

### Options

| Option | Description |
|--------|-------------|
| `--output <path>` | Output file path (default: `<folder>/merge.pgn`) |
| `--recursive` | Search subdirectories for PGN files |
| `--verbose` | Show detailed progress with throughput |
| `--quiet` | Only show errors |
| `-h`, `--help` | Show usage information |

### Examples

**Merge all PGN files in a directory:**

```bash
PgnMerger ./lichess_games
```

**Merge with custom output file:**

```bash
PgnMerger ./lichess_games --output ./combined_games.pgn
```

**Recursive merge with verbose output:**

```bash
PgnMerger ./chess_databases --recursive --verbose --output ./all_games.pgn
```

**Quiet mode (errors only):**

```bash
PgnMerger ./games --quiet --output ./merged.pgn
```

---

## Comparison: PGN Merger vs. Other Tools

| Tool | Language | Standalone | Preserves Headers | Preserves Comments | Merge Type |
|------|----------|------------|-------------------|--------------------|------------|
| **PGN Merger** (this) | F# / .NET | ✅ Yes | ✅ Yes | ✅ Yes | Concatenation |
| [merge-pgn](https://github.com/permutationlock/merge-pgn) | Python | ❌ Requires python-chess | ❌ No | ❌ No | Variation tree |
| [pgn-tools](https://github.com/olleeriksson/pgn-tools) | Python | ❌ Requires python-chess | ✅ Yes | ✅ Yes | Variation tree |

Choose **PGN Merger** when you want a simple, fast tool to combine PGN databases without extra dependencies or complex merging logic.

---

## FAQ

### How do I merge Lichess PGN files?

Download your Lichess games in PGN format, place them in one folder, and run:

```bash
PgnMerger ./my_lichess_games
```

The tool outputs a single `merge.pgn` with all games combined.

### Can I merge PGN files from Chess.com?

Yes. Export your Chess.com games as PGN, place them in a directory, and run the merger.

### What is the difference between merging and combining PGN files?

**Merging** (what this tool does) concatenates multiple games into one file. **Combining** into a single game tree is a different operation; for that, use tools like [pgn-tools](https://github.com/olleeriksson/pgn-tools).

### Does this tool modify game data?

No. PGN Merger concatenates files as-is, preserving all headers, comments, and move text.

---

## Input / Output

- **Input**: A directory containing one or more `.pgn` files
- **Output**: A single `merge.pgn` file in the same directory with all games concatenated

---

## Error Handling

The application handles common issues:

- Directory doesn't exist
- No PGN files found in the directory
- File access permission issues
- I/O errors during file operations

---

## See Also

- [awesome-chess](https://github.com/hkirat/awesome-chess) — Curated list of chess resources
- [awesome-fsharp](https://github.com/fsprojects/awesome-fsharp) — F# ecosystem tools
- [python-chess](https://github.com/niklasf/python-chess) — Python chess library
- [Lichess](https://lichess.org/) — Free online chess platform
- [PGN Standard](https://github.com/fsmosca/PGN-Standard) — PGN specification reference

---

## License

This project is licensed under the [MIT License](LICENSE).

## Author

[Corentin Giaufer Saubert](https://github.com/CorentinGS)
