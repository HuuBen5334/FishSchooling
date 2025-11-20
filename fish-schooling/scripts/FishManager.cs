using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class FishManager : Node2D
{
	private List<BaseFish> allFish = new List<BaseFish>();

	private int maxNemoFish = 110;
    private int maxSharkFish = 150;
    private int maxStarfishFish = 200;
	public override void _Ready()
	{
		// Spawn some initial fish
		SpawnFish("nemo", 5);
	}

	public void SpawnFish(string type, int count)
	{

		int currentNemo = allFish.Count(f => f.FishType == "nemo");
		int currentSharks = allFish.Count(f => f.FishType == "shark");
		int currentStarfish = allFish.Count(f => f.FishType == "starfish");

		int maxAllowed = 0;
		int currentCount = 0;

		switch (type.ToLower())
		{
			case "nemo":
				maxAllowed = maxNemoFish;
				currentCount = currentNemo;
				break;
			case "shark":
				maxAllowed = maxSharkFish;
				currentCount = currentSharks;
				break;
			case "starfish":
				maxAllowed = maxStarfishFish;
				currentCount = currentStarfish;
				break;
			default:
				GD.PrintErr($"Unknown fish type: {type}");
				return;
		}

		int availableSlots = maxAllowed - currentCount;
		int actualSpawnCount = Mathf.Min(count, availableSlots);

		if (actualSpawnCount <= 0)
		{
			GD.Print($"Cannot spawn {type}: limit reached ({currentCount}/{maxAllowed})");
			return;
		}

		for (int i = 0; i < actualSpawnCount; i++)
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
	//for removing fish from list
	public void RemoveFish(BaseFish fish)
	{
		allFish.Remove(fish);
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

	private void FishLimit(int nemoLimit, int sharkLimit, int starfishLimit)
	{
		maxNemoFish = nemoLimit;
		maxSharkFish = sharkLimit;
		maxStarfishFish = starfishLimit;
		GD.Print($"Fish limits updated - Nemo: {maxNemoFish}, Sharks: {maxSharkFish}, Starfish: {maxStarfishFish}");
		
	}
}


