This is a tool for visualizing assembly code that takes an input file  containing assembly instructions and outputs an SVG.

Installations
dotnet
dot

Arguments
1. <input_file> the file with the assembly method(s) the user wish to visualize
2. <keep_hash_code> boolean to represent whether to show the hash code for each instruction
3. <output_folder_path> the folder for the SVG outputs
3. <weight_perfScore> boolean on whether to show the weight and perfscore of each group in the instruction

How To Run
1. dotnet run <input_file> <keep_hash_code> <output_folder_path> <weight_perfScore>

Understanding SVG Output(s)
The darker block colors signify the weight and perfscore. The darker the block the highter the weight of running that block of code. The darker/redder the borders the higher the perf score. The arrows signify the different jumps in the code. Red arrows - signify loops, Green arrows - backwards jump to a previous block, blue - forward jumps