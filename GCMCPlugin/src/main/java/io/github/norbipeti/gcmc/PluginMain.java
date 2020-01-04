package io.github.norbipeti.gcmc;

import com.google.common.io.Files;
import com.google.gson.Gson;
import lombok.val;
import org.bukkit.Bukkit;
import org.bukkit.Location;
import org.bukkit.Material;
import org.bukkit.World;
import org.bukkit.block.Block;
import org.bukkit.command.Command;
import org.bukkit.command.CommandSender;
import org.bukkit.entity.Player;
import org.bukkit.plugin.java.JavaPlugin;

import java.io.File;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;

public class PluginMain extends JavaPlugin {
	@Override
	public void onEnable() {

	}

	@Override
	public boolean onCommand(CommandSender sender, Command command, String label, String[] args) {
		if (args.length < 6) {
			sender.sendMessage("§cUsage: /export <x1> <y1> <z1> <x2> <y2> <z2>");
			return true;
		}
		int[] xyz = new int[6];
		for (int i = 0; i < args.length; i++)
			xyz[i] = Integer.parseInt(args[i]);
		for (int i = 0; i < 3; i++) {
			if (xyz[i] >= xyz[i + 3]) {
				int tmp = xyz[i];
				xyz[i] = xyz[i + 3];
				xyz[i + 3] = tmp;
			}
		}
		World world = sender instanceof Player ? ((Player) sender).getWorld() : Bukkit.getWorlds().get(0);
		val list = new ArrayList<Blocks>();
		for (int y = xyz[1]; y < xyz[4]; y++) {
			Blocks blocks = new Blocks(null, null, null);
			for (int x = xyz[0]; x < xyz[3]; x++) {
				for (int z = xyz[2]; z < xyz[5]; z++) {
					Block block = world.getBlockAt(x, y, z);
					Material mat = block.getType();
					if (blocks.getMaterial() != mat) {
						if (blocks.getStart() != null)
							list.add(blocks);
						blocks.setMaterial(mat);
						blocks.setStart(new Location(null, x, y, z));
						blocks.setEnd(blocks.getStart());
					} else
						blocks.setEnd(new Location(null, x, y, z));
				}
			}
			list.add(blocks);
		}
		Gson gson = new Gson();
		try {
			Files.write(gson.toJson(list), new File("result.txt"), StandardCharsets.UTF_8);
			sender.sendMessage("§bSuccess!");
		} catch (IOException e) {
			e.printStackTrace();
			sender.sendMessage("§cAn error occurred.");
		}
		return true;
	}
}
