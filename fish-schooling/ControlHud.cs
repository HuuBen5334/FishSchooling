using Godot;
using System;

public partial class ControlHud : Control
{
	private string selectedFish = "";
	private int fishCount = 1;

	public override void _Ready()
	{
		// Example: assuming your OptionButton node is named "FishDropdown"
		var dropdown = GetNode<OptionButton>("Panel_Spawner/VBoxContainer/fish_choice_dropdown");
		dropdown.Connect("item_selected", new Callable(this, nameof(OnFishSelected)));
		if (dropdown == null){
			GD.Print("fish_choice_dropdown not found! Adjust the path or check the node names.");
		}

		// Assuming your SpinBox for quantity is named "QuantitySpinBox"
		var spinBox = GetNode<SpinBox>("Panel_Spawner/VBoxContainer/fish_amount_spinbox");
		 if (spinBox == null){
			GD.Print("fish_amount_spinbox Node not found! Adjust the path or check the node names.");
		}
		spinBox.ValueChanged += OnQuantityChanged;

		// And your spawn button is named "SpawnButton"
		var spawnButton = GetNode<Button>("Panel_Spawner/VBoxContainer/fish_spawn_button");
		 if (spawnButton == null){
			GD.Print("fish_spawn_button Node not found! Adjust the path or check the node names.");
		}
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

		var flock = GetNode<Flock>("../Flock");
		if (flock != null)
		{
			flock.SpawnFish(selectedFish, fishCount);
		}
		else
		{
			GD.Print("Flock node not found. Adjust the path in Control_hud.cs");
		}

		GD.Print($"Spawning {fishCount} {selectedFish}");

		int currentFishCount = flock != null ? flock.GetFishCount() : 0;
		UpdateFishCount(currentFishCount);
		// Call the spawn method in your main scene
		// var mainScene = GetNode<Node>("..").GetNode("Main");
		// mainScene.Call("spawn_fish", selectedFish, fishCount);
	}
	
	public void UpdateFishCount(int count)
	{
		var fishLabel = GetNode<Label>("Panel_Census/VBoxContainer/fish_count_label");
		GD.Print("Updating fish count display");
		fishLabel.Text = $"Active Fish: {count}";
	}
}
