# FallingPuzzle

A minimal .NET solution containing a core Tetris-like engine and unit tests.

## Prerequisites
- .NET 8 SDK
  - If not installed on the system, you can use the included script to install locally under `$HOME/.dotnet`.

## Quick start
```bash
# 1) Install .NET 8 SDK locally (if dotnet is not available)
bash ./dotnet-install.sh --channel 8.0 --install-dir $HOME/.dotnet --quality ga
export PATH="$HOME/.dotnet:$PATH"; export DOTNET_ROOT="$HOME/.dotnet"

# 2) Restore, build, and run tests
dotnet restore ./FallingPuzzle.sln
dotnet build   ./FallingPuzzle.sln -v minimal
dotnet test    ./FallingPuzzle.sln -v minimal --logger "trx;LogFileName=test-results.trx" --results-directory ./TestResults
```

## Projects
- `FallingPuzzle.Core` (netstandard2.1)
  - Contains board logic, SRS rotation with kick tables, scoring, and a 7-bag generator
  - Uses C# latest features via `LangVersion` to support target-typed `new()`
- `FallingPuzzle.Tests` (net8.0)
  - xUnit tests with FluentAssertions

## Notes
- Hard drop scoring is intentionally handled outside the core library. `Board.HardDrop()` returns drop distance but does not modify `Score`.
- Rotation uses SRS kick tables. When rotating near walls, non-zero kicks are preferred first to ensure a visible kick when possible.