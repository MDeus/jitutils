

# About #
This is a tool for visualizing assembly code that takes an input file  containing assembly instructions and outputs an SVG.

## Installations ##
- dotnet
- dot

## Arguments ##
--input-file               Required. Input file to be processed.

--output-folder            Required. The folder for the SVG/HTML graph output(s)

--show-hash-code           (Default: false) Displays the hash code for each instruction if true

--show-weight-perfscore    (Default: false) Displays weight and perfscore of each group in the instruction if true

--html                     (Default: false) outputs HTML file if true, SVG if false

## How To Run ##
 `dotnet run --input-file=<input_file> --output-folder=<output_folder>`

## Understanding SVG Output(s) ##
The darker block colors signify the weight and perfscore. The darker the block the highter the weight of running that block of code. The darker/redder the borders the higher the perf score. The arrows signify the different jumps in the code. Red arrows - signify loops, Green arrows - backwards jump to a previous block, blue - forward jumps
