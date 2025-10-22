using Godot;
using System;

public partial class ControlHud : Control
{
	private string selectedFish = "nemo";
	private int fishCount = 1;
	
	public override void _Ready()
	{
		var dropdown = GetNode<OptionButton>("Panel_Spawner/VBoxContainer/fish_choice_dropdown");
		dropdown.Connect("item_selected", new Callable(this, nameof(OnFishSelected)));
		
		var spinBox = GetNode<SpinBox>("Panel_Spawner/VBoxContainer/fish_amount_spinbox");
		spinBox.ValueChanged += OnQuantityChanged;
		
		var spawnButton = GetNode<Button>("Panel_Spawner/VBoxContainer/fish_spawn_button");
		spawnButton.Pressed += OnSpawnPressed;
	}
	
	private void OnFishSelected(int index)
	{
		var dropdown = GetNode<OptionButton>("Panel_Spawner/VBoxContainer/fish_choice_dropdown");
		switch (index)
		{
			case 0:
				selectedFish = "nemo";
				break;
			case 1:
				selectedFish = "shark";
				break;
			case 2:
				selectedFish = "starfish";
				break;
		}
		GD.Print($"Selected fish: {selectedFish}");
	}
	
	private void OnQuantityChanged(double value)
	{
		fishCount = (int)value;
	}
	
	private void OnSpawnPressed()
	{
		// Update the path to find FishManager instead of Flock
		var fishManager = GetNode<FishManager>("../FishManager");
		if (fishManager != null)
		{
			fishManager.SpawnFish(selectedFish, fishCount);
			GD.Print($"Spawning {fishCount} {selectedFish}");
		}
		else
		{
			GD.PrintErr("FishManager node not found!");
		}
	}
}
