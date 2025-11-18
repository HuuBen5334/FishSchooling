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

        var fishManager = GetNodeOrNull<FishManager>("../FishManager");
        if (fishManager != null)
            fishManager.Connect("FishCountChanged", new Callable(this, nameof(UpdateFishCount)));
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
		if (string.IsNullOrEmpty(selectedFish))
		{
			GD.Print("Please select a fish!");
			return;
		}

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
	
	public void UpdateFishCount(string type, int count)
	{
		var nemoLabel = GetNode<Label>("Panel_Census/VBoxContainer/nemo_fish_count_label");
		var sharkLabel = GetNode<Label>("Panel_Census/VBoxContainer/nemo_fish_count_label");
		var starfishLabel = GetNode<Label>("Panel_Census/VBoxContainer/nemo_fish_count_label");

		GD.Print("Updating fish count display");

		if (type == "nemo") {
			nemoLabel.Text = $"Nemo: {count}";
		} else if (type == "shark") {
			sharkLabel.Text = $"Shark: {count}";
		} else if (type == "starfish") {
			starfishLabel.Text = $"Starfish: {count}";
		}
	}
}
