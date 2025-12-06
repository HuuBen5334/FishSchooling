using Godot;
using System;
using System.Collections.Generic;

public partial class ControlHud : Control
{
	private string selectedFish = "nemo";
	private int fishCount = 1;

	//store parameters for each fish type
	private Dictionary<string, float> fishParameters = new Dictionary<string, float>
	{
		{"nemo_speed", 100.0f},
		{"shark_pursuit", 120.0f},
		{"starfish_speed", 15.0f}
	};

	//slider config for each fish type
	private Dictionary<string, SliderConfig> sliderConfigs = new Dictionary<string, SliderConfig>();

	//census labels
	private Label nemoLabel;
	private Label sharkLabel;
	private Label starfishLabel;

	//parameter slider
	private HSlider parameterSlider;
	private Label parameterLabel;
	
	//behavior containers
	private Control separationContainer;
	private Control cohesionContainer;
	private Control alignmentContainer;
	private FishManager fishManager;

	private class SliderConfig
    {
        public string LabelText;
		public float Min;
		public float Max;
		public string ParameterKey;
		public SliderConfig(string label, float min, float max, string key)
        {
            LabelText = label;
            Min = min;
            Max = max;
            ParameterKey = key;
        }
    }
	
	public override void _Ready()
	{
		SetupSliderConfigs();
        SetupSpawnerControls();
        SetupCensusLabels();
        SetupFishManager();
        SetupParameterSlider();
        SetupBehaviorContainers();   
	}

	private void SetupSliderConfigs()
    {
        sliderConfigs["nemo"] = new SliderConfig("Speed", 50, 200, "nemo_speed");
        sliderConfigs["shark"] = new SliderConfig("Pursuit Radius", 100, 400, "shark_pursuit");
        sliderConfigs["starfish"] = new SliderConfig("Speed", 5, 50, "starfish_speed");
    }

	private void SetupSpawnerControls()
    {
        var dropdown = GetNode<OptionButton>("Panel_Spawner/VBoxContainer/fish_choice_dropdown");
        dropdown.Connect("item_selected", new Callable(this, nameof(OnFishSelected)));
        dropdown.Selected = 0;

        var spinBox = GetNode<SpinBox>("Panel_Spawner/VBoxContainer/fish_amount_spinbox");
        spinBox.ValueChanged += OnQuantityChanged;

        var spawnButton = GetNode<Button>("Panel_Spawner/VBoxContainer/fish_spawn_button");
        spawnButton.Pressed += OnSpawnPressed;
    }

	private void SetupCensusLabels()
    {
        nemoLabel = GetNodeOrNull<Label>("Panel _Census/VBoxContainer/nemo_fish_count_label");
        sharkLabel = GetNodeOrNull<Label>("Panel _Census/VBoxContainer/shark_fish_count_label");
        starfishLabel = GetNodeOrNull<Label>("Panel _Census/VBoxContainer/starfish_fish_count_label");

        if (nemoLabel == null || sharkLabel == null || starfishLabel == null)
            GD.PrintErr("One or more fish count labels not found.");
    }

	private void SetupFishManager()
    {
        fishManager = GetNodeOrNull<FishManager>("../FishManager");
        
        if (fishManager != null)
            fishManager.Connect("FishCountChanged", new Callable(this, nameof(UpdateFishCount)));
        else
            GD.PrintErr("FishManager not found!");
    }

	private void SetupParameterSlider()
    {
        parameterSlider = GetNodeOrNull<HSlider>("Panel_Spawner/VBoxContainer/velocity_slider");
        parameterLabel = GetNodeOrNull<Label>("Panel_Spawner/VBoxContainer/velocity_label");

        if (parameterSlider == null)
            return;

        parameterSlider.MinValue = 50;
        parameterSlider.MaxValue = 200;
        parameterSlider.Value = 100;
        parameterSlider.ValueChanged += OnParameterChanged;
        ConfigureSliderForFish("nemo");
    }

	private void SetupBehaviorContainers()
    {
        separationContainer = GetNodeOrNull<Control>("Panel_Spawner/SeparationContainer");
        cohesionContainer = GetNodeOrNull<Control>("Panel_Spawner/CohesionContainer");
        alignmentContainer = GetNodeOrNull<Control>("Panel_Spawner/AlignmentContainer");
    }


	private void ConfigureSliderForFish(string fishType)
    {
        if (!sliderConfigs.ContainsKey(fishType) || parameterSlider == null)
            return;
        
        var config = sliderConfigs[fishType];
        parameterSlider.MinValue = config.Min;
        parameterSlider.MaxValue = config.Max;
        parameterSlider.Value = fishParameters[config.ParameterKey];
        
        if (parameterLabel != null)
            parameterLabel.Text = $"{config.LabelText}: {fishParameters[config.ParameterKey]:F0}";
    }

	private void OnParameterChanged(double value)
    {
        if (!sliderConfigs.ContainsKey(selectedFish))
            return;
        
        var config = sliderConfigs[selectedFish];
        fishParameters[config.ParameterKey] = (float)value;
        
        if (parameterLabel != null)
            parameterLabel.Text = $"{config.LabelText}: {value:F0}";
        
        //Update existing fish
        ApplyParameterToExistingFish(selectedFish, (float)value);
    }


	private void ApplyParameterToExistingFish(string fishType, float value)
    {
        if (fishManager == null)
            return;
        
        foreach (var child in fishManager.GetChildren())
        {
            switch (fishType)
            {
                case "nemo":
                    if (child is NemoFish nemo)
                        nemo.MaxSpeed = value;
                    break;
                case "shark":
                    if (child is SharkFish shark)
                        shark.SetPursuitRadius(value);
                    break;
                case "starfish":
                    if (child is StarfishFish starfish)
                        starfish.MaxSpeed = value;
                    break;
            }
        }
    }



	private void OnFishSelected(int index)
	{
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
		ConfigureSliderForFish(selectedFish);
        
		// Hide behavior sliders for sharks
        bool showBehaviors = (selectedFish != "shark");
        
        if (separationContainer != null)
            separationContainer.Visible = showBehaviors;
        if (cohesionContainer != null)
            cohesionContainer.Visible = showBehaviors;
        if (alignmentContainer != null)
            alignmentContainer.Visible = showBehaviors;
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
		if (fishManager != null)
		{
			float param = fishParameters[sliderConfigs[selectedFish].ParameterKey];
            fishManager.SpawnFish(selectedFish, fishCount, param);
            GD.Print($"Spawning {fishCount} {selectedFish}");
		}
		else
		{
			GD.PrintErr("FishManager node not found!");
		}
	}
	
	public void UpdateFishCount(string type, int count)
	{
		if (type == "nemo") {
			nemoLabel.Text = $"Nemo: {count}";
		} else if (type == "shark") {
			sharkLabel.Text = $"Shark: {count}";
		} else if (type == "starfish") {
			starfishLabel.Text = $"Starfish: {count}";
		}

	}
}
