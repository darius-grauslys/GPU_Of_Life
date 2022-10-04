# GPU_Of_Life
Conway's Game of Life implemented on the GPU. Computation is done with the usage of Shaders. Compute Shaders could be used, but my version of OpenGL on this laptop doens't support it.

Shader_Compute -- The vertex and fragment shader
                  that modifies the two textures representing
                  the simulation grid.

Shader_Draw    -- The vertex/geometry/fragment shader for
                  drawing the grid to the screen.
                  
Shader_Tool    -- To be replaced.

Requires dotnet 6.0.0
