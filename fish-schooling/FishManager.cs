using Godot;
using System.Collections.Generic;

public partial class FishManager : Node2D
{
	private List<BaseFish> allFish = new List<BaseFish>();

	public override void _Ready()
	{
		// Spawn some initial fish
		SpawnFish("nemo", 5);
	}

	public void SpawnFish(string type, int count)
	{
		for (int i = 0; i < count; i++)
		{
			BaseFish newFish = null;

			switch (type.ToLower())
			{
				case "nemo":
					newFish = new NemoFish();
					break;
				case "shark":
					newFish = new SharkFish();
					break;
				case "starfish":
					newFish = new StarfishFish();
					break;
				default:
					GD.PrintErr($"Unknown fish type: {type}");
					return;
			}

			if (newFish != null)
			{
				// Random position
				var screenSize = GetViewportRect().Size;
				newFish.Position = new Vector2(
					GD.Randf() * screenSize.X,
					GD.Randf() * screenSize.Y
				);

				AddChild(newFish);
				allFish.Add(newFish);

				GD.Print($"Spawned {type} at {newFish.Position}");
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		foreach (var fish in allFish)
		{
			fish.UpdateFish(allFish, dt);
			WrapPosition(fish);
		}
	}

	private void WrapPosition(BaseFish fish)
	{
		var screenSize = GetViewportRect().Size;
		var pos = fish.Position;

		if (pos.X < 0) pos.X = screenSize.X;
		else if (pos.X > screenSize.X) pos.X = 0;

		if (pos.Y < 0) pos.Y = screenSize.Y;
		else if (pos.Y > screenSize.Y) pos.Y = 0;

		fish.Position = pos;
	}
}
