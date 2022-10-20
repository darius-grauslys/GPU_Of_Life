# GPU_Of_Life
A cellular automaton simulator written in C# using OpenTK. It accepts shaders for life rules, as well as grid visualizers and grid manipulation tools.
See GPU_Programs for examples. The shaders located in the base directory are the default shaders loaded at runtime.

The project is functional at the time being. I will return to wrap up the final features at a later date.

Features:
- Custom life rules.
- Custom grid visualizers.
- Custom tools.
- Can load any normal image with stbimagesharp.
- Can load RLE files ( https://copy.sh/life/examples/ )
- Control simulation speed.
- 2D Camera.

## Usage

Requires dotnet 6.0.0

The set up for this project isn't finished yet, so you need to do somethings manually.

First clone:
https://github.com/WearsomeKarma/Gwen.Net

Navigate to GPU_Of_Life in the command line and run the following:
- `dotnet add reference [folder_path_to_gwen.net.csproj]`
- `dotnet add package OpenTK`
- `dotnet add package StbImageSharp`
- `dotnet add package SharpYaml`

Then run:

- `dotnet run`
