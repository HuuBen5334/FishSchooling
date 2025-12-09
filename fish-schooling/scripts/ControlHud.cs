using Godot;
using System;
using System.Collections.Generic;

public partial class ControlHud : Control
{
	private string selectedFish = "nemo";
	private int fishCount = 1;
	private FishManager fishManager;

	//Fish configuration to make it easy to add new fish types
	private Dictionary<string, FishConfig> fishConfigs = new Dictionary<string, FishConfig>();

	//UI references
	private HSlider parameterSlider;
	private Label parameterLabel;
	private HSlider separationSlider;
	private Label separationLabel;
	private HSlider cohesionSlider;
	private Label cohesionLabel;
	private HSlider alignmentSlider;
	private Label alignmentLabel;
	private Control separationContainer;
	private Control cohesionContainer;
	private Control alignmentContainer;
	private Dictionary<string, Label> censusLabels = new Dictionary<string, Label>();

	//Fish configuration class - holds all settings for one fish type
	private class FishConfig
	{
		public SliderConfig MainSlider;
		public SliderConfig SeparationSlider;
		public SliderConfig CohesionSlider;
		public SliderConfig AlignmentSlider;
		public bool ShowBehaviorContainers;
		public Action<BaseFish, float> ApplyMainParam;
		public Action<BaseFish, float> ApplySeparation;
		public Action<BaseFish, float> ApplyCohesion;
		public Action<BaseFish, float> ApplyAlignment;

		public FishConfig(
			SliderConfig mainSlider,
			SliderConfig separationSlider,
			SliderConfig cohesionSlider,
			SliderConfig alignmentSlider,
			bool showBehaviors,
			Action<BaseFish, float> applyMain,
			Action<BaseFish, float> applySeparation,
			Action<BaseFish, float> applyCohesion,
			Action<BaseFish, float> applyAlignment)
		{
			MainSlider = mainSlider;
			SeparationSlider = separationSlider;
			CohesionSlider = cohesionSlider;
			AlignmentSlider = alignmentSlider;
			ShowBehaviorContainers = showBehaviors;
			ApplyMainParam = applyMain;
			ApplySeparation = applySeparation;
			ApplyCohesion = applyCohesion;
			ApplyAlignment = applyAlignment;
		}
	}

	private class SliderConfig
	{
		public string LabelText;
		public float Min;
		public float Max;
		public float DefaultValue;

		public SliderConfig(string label, float min, float max, float defaultValue)
		{
			LabelText = label;
			Min = min;
			Max = max;
			DefaultValue = defaultValue;
		}
	}

	public override void _Ready()
	{
		SetupFishConfigs();
		SetupUI();
		ConfigureForFish("nemo");
	}

	private void SetupFishConfigs()
	{
		//Nemo configuration
		fishConfigs["nemo"] = new FishConfig(
			mainSlider: new SliderConfig("Speed", 50, 200, 100),
			separationSlider: new SliderConfig("Separation", 10, 100, 30),
			cohesionSlider: new SliderConfig("Cohesion", 50, 300, 150),
			alignmentSlider: new SliderConfig("Alignment", 50, 300, 150),
			showBehaviors: true,
			applyMain: (fish, val) => fish.MaxSpeed = val,
			applySeparation: (fish, val) => { if (fish is NemoFish nemo) nemo.SetSeparation(val); },
			applyCohesion: (fish, val) => { if (fish is NemoFish nemo) nemo.SetCohesion(val); },
			applyAlignment: (fish, val) => { if (fish is NemoFish nemo) nemo.SetAlignment(val); }
		);

		//Shark configuration
		fishConfigs["shark"] = new FishConfig(
			mainSlider: new SliderConfig("Speed", 80, 250, 120),
			separationSlider: new SliderConfig("Pursuit Radius", 100, 400, 200),
			cohesionSlider: null,
			alignmentSlider: null,
			showBehaviors: false,
			applyMain: (fish, val) => fish.MaxSpeed = val,
			applySeparation: (fish, val) => { if (fish is SharkFish shark) shark.SetPursuitRadius(val); },
			applyCohesion: null,
			applyAlignment: null
		);

		//Starfish configuration
		fishConfigs["starfish"] = new FishConfig(
			mainSlider: new SliderConfig("Speed", 5, 50, 15),
			separationSlider: new SliderConfig("Separation", 10, 80, 20),
			cohesionSlider: new SliderConfig("Cohesion", 30, 200, 100),
			alignmentSlider: new SliderConfig("Alignment", 30, 200, 100),
			showBehaviors: true,
			applyMain: (fish, val) => fish.MaxSpeed = val,
			applySeparation: null,
			applyCohesion: null,
			applyAlignment: null
		);

		//Eel configuration
		fishConfigs["eel"] = new FishConfig(
			mainSlider: new SliderConfig("Speed", 40, 150, 80),
			separationSlider: new SliderConfig("Separation", 10, 70, 25),
			cohesionSlider: new SliderConfig("Cohesion", 40, 180, 120),
			alignmentSlider: new SliderConfig("Alignment", 40, 180, 120),
			showBehaviors: true,
			applyMain: (fish, val) => fish.MaxSpeed = val,
			applySeparation: null,
			applyCohesion: null,
			applyAlignment: null
		);

		//Orca configuration
		fishConfigs["orca"] = new FishConfig(
			mainSlider: new SliderConfig("Speed", 100, 300, 150),
			separationSlider: null,
			cohesionSlider: null,
			alignmentSlider: null,
			showBehaviors: false,
			applyMain: (fish, val) => fish.MaxSpeed = val,
			applySeparation: null,
			applyCohesion: null,
			applyAlignment: null
		);
	}

	private void SetupUI()
	{
		SetupSpawnerControls();
		SetupSliders();
		SetupBehaviorContainers();
		SetupCensusLabels();
		SetupFishManager();
	}

	private void SetupSpawnerControls()
	{
		var dropdown = GetNode<OptionButton>("Panel_Spawner/VBoxContainer/fish_choice_dropdown");
		dropdown.ItemSelected += OnFishSelected;
		dropdown.Selected = 0;

		var spinBox = GetNode<SpinBox>("Panel_Spawner/VBoxContainer/fish_amount_spinbox");
		spinBox.ValueChanged += OnQuantityChanged;
		
		var spawnButton = GetNode<Button>("Panel_Spawner/VBoxContainer/fish_spawn_button");
		spawnButton.Pressed += OnSpawnPressed;

		//for delete button
		var deleteButton = GetNode<Button>("Panel_Census/delete_button");
		deleteButton.Pressed += OnDeletePressed;
	}

	private void SetupSliders()
	{
		//main speed/parameter slider
		parameterSlider = GetNodeOrNull<HSlider>("Panel_Spawner/VBoxContainer/velocity_slider");
		parameterLabel = GetNodeOrNull<Label>("Panel_Spawner/VBoxContainer/velocity_label");
		//separation/pursuit slider
		separationSlider = GetNodeOrNull<HSlider>("Panel_Spawner/SeparationContainer/separation_slider");
		separationLabel = GetNodeOrNull<Label>("Panel_Spawner/SeparationContainer/separation_label");
		//cohesion slider
		cohesionSlider = GetNodeOrNull<HSlider>("Panel_Spawner/CohesionContainer/cohesion_slider");
		cohesionLabel = GetNodeOrNull<Label>("Panel_Spawner/CohesionContainer/cohesion_label");
		//allignment slider
		alignmentSlider = GetNodeOrNull<HSlider>("Panel_Spawner/AlignmentContainer/alignment_slider");
		alignmentLabel = GetNodeOrNull<Label>("Panel_Spawner/AlignmentContainer/alignment_label");

		// Connect signals
		if (parameterSlider != null)
			parameterSlider.ValueChanged += OnMainSliderChanged;
		if (separationSlider != null)
			separationSlider.ValueChanged += OnSeparationSliderChanged;
		if (cohesionSlider != null)
			cohesionSlider.ValueChanged += OnCohesionSliderChanged;
		if (alignmentSlider != null)
			alignmentSlider.ValueChanged += OnAlignmentSliderChanged;
	}

	private void SetupBehaviorContainers()
	{
		separationContainer = GetNodeOrNull<Control>("Panel_Spawner/SeparationContainer");
		cohesionContainer = GetNodeOrNull<Control>("Panel_Spawner/CohesionContainer");
		alignmentContainer = GetNodeOrNull<Control>("Panel_Spawner/AlignmentContainer");
	}

	private void SetupCensusLabels()
	{
		string basePath = "Panel_Census/VBoxContainer/";
		censusLabels["nemo"] = GetNodeOrNull<Label>($"{basePath}nemo_fish_count_label");
		censusLabels["shark"] = GetNodeOrNull<Label>($"{basePath}shark_fish_count_label");
		censusLabels["starfish"] = GetNodeOrNull<Label>($"{basePath}starfish_fish_count_label");
		censusLabels["eel"] = GetNodeOrNull<Label>($"{basePath}eel_fish_count_label");
		censusLabels["orca"] = GetNodeOrNull<Label>($"{basePath}orca_fish_count_label");
	}

	private void SetupFishManager()
	{
		fishManager = GetNodeOrNull<FishManager>("../FishManager");
		if (fishManager != null)
			fishManager.FishCountChanged += UpdateFishCount;
	}

	private void ConfigureForFish(string fishType)
	{
		if (!fishConfigs.TryGetValue(fishType, out var config))
			return;

		// Configure main slider
		ConfigureSlider(parameterSlider, parameterLabel, config.MainSlider);

		// Configure separation slider
		ConfigureSlider(separationSlider, separationLabel, config.SeparationSlider);

		// Configure cohesion slider
		ConfigureSlider(cohesionSlider, cohesionLabel, config.CohesionSlider);

		// Configure alignment slider
		ConfigureSlider(alignmentSlider, alignmentLabel, config.AlignmentSlider);

		//Update container visibility
		if (cohesionContainer != null)
			cohesionContainer.Visible = config.ShowBehaviorContainers;
		if (alignmentContainer != null)
			alignmentContainer.Visible = config.ShowBehaviorContainers;
	}

	private void ConfigureSlider(HSlider slider, Label label, SliderConfig config)
	{
		if (config == null || slider == null)
			return;

		slider.MinValue = config.Min;
		slider.MaxValue = config.Max;
		slider.Value = config.DefaultValue;

		if (label != null)
			label.Text = $"{config.LabelText}: {config.DefaultValue:F0}";
	}

	private void OnMainSliderChanged(double value)
	{
		if (!fishConfigs.TryGetValue(selectedFish, out var config))
			return;

		if (parameterLabel != null)
			parameterLabel.Text = $"{config.MainSlider.LabelText}: {value:F0}";

		ApplyToExistingFish(config.ApplyMainParam, (float)value);
	}

	private void OnSeparationSliderChanged(double value)
	{
		if (!fishConfigs.TryGetValue(selectedFish, out var config) || config.SeparationSlider == null)
			return;

		if (separationLabel != null)
			separationLabel.Text = $"{config.SeparationSlider.LabelText}: {value:F0}";

		ApplyToExistingFish(config.ApplySeparation, (float)value);
	}

	private void OnCohesionSliderChanged(double value)
	{
		if (!fishConfigs.TryGetValue(selectedFish, out var config) || config.CohesionSlider == null)
			return;

		if (cohesionLabel != null)
			cohesionLabel.Text = $"{config.CohesionSlider.LabelText}: {value:F0}";

		ApplyToExistingFish(config.ApplyCohesion, (float)value);
	}

	private void OnAlignmentSliderChanged(double value)
	{
		if (!fishConfigs.TryGetValue(selectedFish, out var config) || config.AlignmentSlider == null)
			return;

		if (alignmentLabel != null)
			alignmentLabel.Text = $"{config.AlignmentSlider.LabelText}: {value:F0}";

		ApplyToExistingFish(config.ApplyAlignment, (float)value);
	}

	private void ApplyToExistingFish(Action<BaseFish, float> action, float value)
	{
		if (fishManager == null || action == null)
			return;

		foreach (var child in fishManager.GetChildren())
		{
			if (child is BaseFish fish && fish.FishType == selectedFish)
				action(fish, value);
		}
	}

	private void OnFishSelected(long index)
	{
		selectedFish = index switch
		{
			0 => "nemo",
			1 => "shark",
			2 => "starfish",
			3 => "eel",
			4 => "orca",
			_ => selectedFish
		};

		ConfigureForFish(selectedFish);
		GD.Print($"Selected fish: {selectedFish}");
	}

	private void OnQuantityChanged(double value) => fishCount = (int)value;

	private void OnSpawnPressed()
	{
		if (fishManager == null || !fishConfigs.TryGetValue(selectedFish, out var config))
			return;

		float speed = (float)(parameterSlider?.Value ?? config.MainSlider.DefaultValue);
		fishManager.SpawnFish(selectedFish, fishCount, speed);
		GD.Print($"Spawning {fishCount} {selectedFish}");
	}

	private void OnPausePressed()
	{
		GetTree().Paused = !GetTree().Paused;
	}

	private void GoHome()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://menu.tscn");
	}

	private void OnDeletePressed()
	{
		if (fishManager == null)
			return;
		int deleted = 0;
		var children = fishManager.GetChildren();
		foreach (var child in children)
		{
			if (deleted >= fishCount)
				break;
			if (child is BaseFish fish && fish.FishType == selectedFish)
			{
				fishManager.RemoveFish(fish);
				fish.QueueFree();
				deleted++;
			}
		}
		// GD.Print($"Deleted {deleted} {selectedFish}");
	}

	public void UpdateFishCount(string type, int count)
	{
		if (censusLabels.TryGetValue(type, out var label) && label != null)
			label.Text = $"{char.ToUpper(type[0]) + type.Substring(1)}: {count}";
	}
}
