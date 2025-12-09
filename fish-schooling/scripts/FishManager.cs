using Godot;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

public partial class FishManager : Node2D
{
	[Signal] public delegate void FishCountChangedEventHandler(string type, int count);
	private List<BaseFish> allFish = new List<BaseFish>();
	Dictionary<string, int> fishCount = new Dictionary<string, int>();
	public FishManager()
	{
		fishCount["nemo"] = 0;
		fishCount["shark"] = 0;
		fishCount["starfish"] = 0;
		fishCount["orca"] = 0;
		fishCount["eel"] = 0;
	}


	private int maxNemoFish = 110;
	private int maxSharkFish = 150;
	private int maxStarfishFish = 200;
	private int maxEelFish = 30;
	private int maxOrcaFish = 20;
	public override void _Ready()
	{
		// Spawn some initial fish
		CallDeferred(nameof(SpawnFish), "nemo", 5, 100.0f);
	}

	public void SpawnFish(string type, int count, float nemoSpeed = 100.0f)
	{

		int currentNemo = allFish.Count(f => f.FishType == "nemo");
		int currentSharks = allFish.Count(f => f.FishType == "shark");
		int currentStarfish = allFish.Count(f => f.FishType == "starfish");
		int currentEels = allFish.Count(f => f.FishType == "eel");
		int currentOrcas = allFish.Count(f => f.FishType == "orca");

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
			case "eel":
				maxAllowed = maxEelFish;
				currentCount = currentEels;
				break;
			case "orca":
				maxAllowed = maxOrcaFish;
				currentCount = currentOrcas;
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
					newFish.MaxSpeed = nemoSpeed;
					break;
				case "shark":
					newFish = new SharkFish();
					break;
				case "starfish":
					newFish = new StarfishFish();
					break;
				case "eel":
					newFish = new EelFish();
					break;
				case "orca":
					newFish = new OrcaFish();
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
			}

			// Update fish count
			if (fishCount.ContainsKey(type))
			{
				var ControlHud = GetNode<ControlHud>("../Control_HUD");
				fishCount[type] += 1;
				ControlHud.UpdateFishCount(type, fishCount[type]);
			}
			
		}
	}
	//for removing fish from list
	public void RemoveFish(BaseFish fish)
	{
		if (fishCount.ContainsKey(fish.FishType))
			{
				fishCount[fish.FishType] = Mathf.Max(0, fishCount[fish.FishType] - 1);
				GD.Print($"Removed one {fish.FishType}, new count: {fishCount[fish.FishType]}");
				var ControlHud = GetNode<ControlHud>("../Control_HUD");
				ControlHud.UpdateFishCount( fish.FishType, fishCount[fish.FishType]);
			}
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
