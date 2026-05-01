# KLAD - Agent Instructions

This project is a 2-player competitive arcade game written in C# 12, WPF (.NET 10.0-windows), and OpenTK.

## Build and Run
- **Commands**: Run standard `dotnet build` and `dotnet run` inside the `KLAD/` subdirectory.
- **WPF Designer Quirk**: If you encounter `CS0103 The name "OpenTkControl" does not exist in the current context` or XAML designer errors (XLS0414, XDG0008), the XAML designer failed because NuGet dependencies aren't compiled yet. Run a Clean and Rebuild.

## Framework Quirks & Gotchas
- **OpenTK.GLWpfControl XAML Namespace**: You MUST use `assembly=GLWpfControl` in the XAML namespace, NOT `OpenTK.GLWpfControl` or it will fail to resolve.
  - **Correct**: `xmlns:glWpf="clr-namespace:OpenTK.Wpf;assembly=GLWpfControl"`
- **Map Parsing**: `KLAD.Logic.MazeLoader` uses `System.Drawing.Common` to parse BMP pixel data from `Maps/set.bmp`. The path assumes a standard VS output directory `Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Maps", "set.bmp")`.
- **Coordinate System**: The game logic uses an Orthographic projection (`Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1.0f, 1.0f)`). The grid is `X` (left to right) and `Y` (top to bottom).

## Architecture Boundaries
- **`KLAD/Models/GameModels.cs`**: Contains the state (`GameState`, `Player`, `Maze`) and domain logic. Implements specific requested patterns:
  - **Decorator Pattern**: Used for modifying wall behavior (`TemporaryWallDecorator`, `DestroyedWallDecorator`).
  - **Factory Method**: Used for spawning randomized items (`PrizeFactory`).
- **`KLAD/MainWindow.xaml.cs`**: Handles keyboard input polling (`HashSet<Key>`), calls `UpdateGameLogic()` based on delta time, and triggers OpenGL rendering.
- **`KLAD/Rendering/GameRenderer.cs`**: OpenTK logic. Do not put domain logic here; this purely maps `GameState` to `GL.DrawElements`.

## Player Input Logic
Player 1 uses WASD + Space (Wall Actions), Player 2 uses Arrows + Enter. `Window_KeyDown` adds to a `HashSet<Key>`, and continuous movement calculation `dx * p.Speed * dt` happens on every frame inside `UpdateGameLogic()`. Single-press actions (like building walls) are checked directly in `KeyDown` to prevent continuous spamming.