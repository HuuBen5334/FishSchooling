using Godot;
using System;

public partial class Control_hud : Control{
	private string selectedFish = "";
	private int fishCount = 1;

	public override void _Ready()
	{
		// Example: assuming your OptionButton node is named "FishDropdown"
		var dropdown = GetNode<OptionButton>("/Panel_Spawner/VBoxContainer/fish_choice_dropdown");
		dropdown.Connect("item_selected", new Callable(this, nameof(OnFishSelected)));

		// Assuming your SpinBox for quantity is named "QuantitySpinBox"
		var spinBox = GetNode<SpinBox>("/Panel_Spawner/VBoxContainer/fish_amount_spinbox");
		spinBox.ValueChanged += OnQuantityChanged;

		// And your spawn button is named "SpawnButton"
		var spawnButton = GetNode<Button>("/Panel_Spawner/VBoxContainer/fish_spawn_button");
		spawnButton.Pressed += OnSpawnPressed;
	}

	private void OnFishSelected(uint index)
	{
		var dropdown = GetNode<OptionButton>("FishDropdown");
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
			default:
				selectedFish = "";
				break;
		}
		GD.Print("Selected fish: ", selectedFish);
	}

	private void OnQuantityChanged(double value)
	{
		fishCount = (int)value;
		GD.Print("Fish quantity:", fishCount);
	}

	private void OnSpawnPressed()
	{
		if (string.IsNullOrEmpty(selectedFish))
		{
			GD.Print("Please select a fish!");
			return;
		}
		GD.Print($"Spawning {fishCount} {selectedFish}");
		// Call the spawn method in your main scene
		var mainScene = GetNode<Node>("..").GetNode("Main");
		mainScene.Call("spawn_fish", selectedFish, fishCount);
	}
}
