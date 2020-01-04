package io.github.norbipeti.gcmc;

import lombok.AllArgsConstructor;
import lombok.Data;
import org.bukkit.Location;
import org.bukkit.Material;

@Data
@AllArgsConstructor
public class Blocks {
	private Location start;
	private Location end;
	private Material material;
}
