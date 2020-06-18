# GCMC

Minecraft world importer for Gamecraft.

## Usage
You'll need a Bukkit/Spigot/Paper server first. Download the Bukkit plugin from [releases](https://git.exmods.org/NorbiPeti/GCMC/releases) and put it in the server's plugins directory.

Then run `/export <x1> <y1> <z1> <x2> <y2> <z2>` on the server with the starting and ending coordinates of the area you want to export.
Make sure to not select a large area as it can make Gamecraft lag.

After that, copy the `result.json` from the server folder into Gamecraft's folder.
Then, using the Gamecraft mod, do `importWorld "result.json"` in the GC console (opened by pressing the button near right shift by default) to import the Minecraft blocks.
It can take a while to load the file and place the blocks, depending on how many are there to place.

Open the game log (and scroll to the end) to see the blocks that the mod doesn't know how to place in GC. You can add block mappings to the BlockTypes.ini file in Gamecraft's folder following the format (Minecraft block goes in brackets, the rest are Gamecraft block settings, color darkness goes 0-9).
If you're up to contributing to this mod, please send this file to me (NorbiPeti) after adding more block types.

I have big plans for this project. One day...
