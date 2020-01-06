package io.github.norbipeti.gcmc;

import lombok.Data;
import org.bukkit.Location;

@Data
public class Blocks {
	private Location start;
	private Location end;
	private String material;
}
