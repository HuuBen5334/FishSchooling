using Godot;
using System.Collections.Generic;
using System.Collections.Specialized;

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
	}


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

			// Update fish count
			if (fishCount.ContainsKey(type))
			{
				var ControlHud = GetNode<ControlHud>("../Control_HUD");
				fishCount[type] += 1;
				//EmitSignal(nameof(FishCountChangedEventHandler), type, fishCount[type]);

				ControlHud.UpdateFishCount( type, fishCount[type]);
			}
		}
	}
	//for removing fish from list
	public void RemoveFish(BaseFish fish)
	{
		// can remove loop after testing/ printingit g debgging 

		if (fishCount.ContainsKey(fish.FishType))
			{
				fishCount[fish.FishType] = Mathf.Max(0, fishCount[fish.FishType] - 1);
				GD.Print($"Removed one {fish.FishType}, new count: {fishCount[fish.FishType]}");
				var ControlHud = GetNode<ControlHud>("../Control_HUD");
				//EmitSignal(nameof(FishCountChangedEventHandler), fish.FishType, fishCount[fish.FishType]);
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
}
