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
    private Control separationContainer;
    private Control cohesionContainer;
    private Control alignmentContainer;
    private Dictionary<string, Label> censusLabels = new Dictionary<string, Label>();

    //Fish configuration class - holds all settings for one fish type
    private class FishConfig
    {
        public SliderConfig MainSlider;
        public SliderConfig SecondSlider;
        public bool ShowBehaviorContainers;
        public Action<BaseFish, float> ApplyMainParam;
        public Action<BaseFish, float> ApplySecondParam;

        public FishConfig(
            SliderConfig mainSlider,
            SliderConfig secondSlider,
            bool showBehaviors,
            Action<BaseFish, float> applyMain,
            Action<BaseFish, float> applySecond)
        {
            MainSlider = mainSlider;
            SecondSlider = secondSlider;
            ShowBehaviorContainers = showBehaviors;
            ApplyMainParam = applyMain;
            ApplySecondParam = applySecond;
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
            secondSlider: new SliderConfig("Separation", 10, 100, 30),
            showBehaviors: true,
            applyMain: (fish, val) => fish.MaxSpeed = val,
            applySecond: (fish, val) => { if (fish is NemoFish nemo) nemo.SetSeparation(val); }
        );

        //Shark configuration
        fishConfigs["shark"] = new FishConfig(
            mainSlider: new SliderConfig("Speed", 80, 250, 120),
            secondSlider: new SliderConfig("Pursuit Radius", 100, 400, 200),
            showBehaviors: false,
            applyMain: (fish, val) => fish.MaxSpeed = val,
            applySecond: (fish, val) => { if (fish is SharkFish shark) shark.SetPursuitRadius(val); }
        );

        //Starfish configuration
        fishConfigs["starfish"] = new FishConfig(
            mainSlider: new SliderConfig("Speed", 5, 50, 15),
            secondSlider: new SliderConfig("Separation", 10, 80, 20),
            showBehaviors: true,
            applyMain: (fish, val) => fish.MaxSpeed = val,
            applySecond: (fish, val) => { if (fish is StarfishFish star) star.SetSeparation(val); }
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
    }

    private void SetupSliders()
    {
        parameterSlider = GetNodeOrNull<HSlider>("Panel_Spawner/VBoxContainer/velocity_slider");
        parameterLabel = GetNodeOrNull<Label>("Panel_Spawner/VBoxContainer/velocity_label");
        separationSlider = GetNodeOrNull<HSlider>("Panel_Spawner/SeparationContainer/separation_slider");
        separationLabel = GetNodeOrNull<Label>("Panel_Spawner/SeparationContainer/separation_label");

        if (parameterSlider != null)
            parameterSlider.ValueChanged += OnMainSliderChanged;
        if (separationSlider != null)
            separationSlider.ValueChanged += OnSecondSliderChanged;
    }

    private void SetupBehaviorContainers()
    {
        separationContainer = GetNodeOrNull<Control>("Panel_Spawner/SeparationContainer");
        cohesionContainer = GetNodeOrNull<Control>("Panel_Spawner/CohesionContainer");
        alignmentContainer = GetNodeOrNull<Control>("Panel_Spawner/AlignmentContainer");
    }

    private void SetupCensusLabels()
    {
        string basePath = "Panel _Census/VBoxContainer/";
        censusLabels["nemo"] = GetNodeOrNull<Label>($"{basePath}nemo_fish_count_label");
        censusLabels["shark"] = GetNodeOrNull<Label>($"{basePath}shark_fish_count_label");
        censusLabels["starfish"] = GetNodeOrNull<Label>($"{basePath}starfish_fish_count_label");
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

        //Configure main slider
        if (parameterSlider != null)
        {
            parameterSlider.MinValue = config.MainSlider.Min;
            parameterSlider.MaxValue = config.MainSlider.Max;
            parameterSlider.Value = config.MainSlider.DefaultValue;
        }
        if (parameterLabel != null)
            parameterLabel.Text = $"{config.MainSlider.LabelText}: {config.MainSlider.DefaultValue:F0}";

        //Configure second slider
        if (separationSlider != null)
        {
            separationSlider.MinValue = config.SecondSlider.Min;
            separationSlider.MaxValue = config.SecondSlider.Max;
            separationSlider.Value = config.SecondSlider.DefaultValue;
        }
        if (separationLabel != null)
            separationLabel.Text = $"{config.SecondSlider.LabelText}: {config.SecondSlider.DefaultValue:F0}";

        //Update container visibility
        if (cohesionContainer != null)
            cohesionContainer.Visible = config.ShowBehaviorContainers;
        if (alignmentContainer != null)
            alignmentContainer.Visible = config.ShowBehaviorContainers;
        //Separation container always visible (reused for pursuit radius)
    }

    private void OnMainSliderChanged(double value)
    {
        if (!fishConfigs.TryGetValue(selectedFish, out var config))
            return;

        if (parameterLabel != null)
            parameterLabel.Text = $"{config.MainSlider.LabelText}: {value:F0}";

        ApplyToExistingFish(config.ApplyMainParam, (float)value);
    }

    private void OnSecondSliderChanged(double value)
    {
        if (!fishConfigs.TryGetValue(selectedFish, out var config))
            return;

        if (separationLabel != null)
            separationLabel.Text = $"{config.SecondSlider.LabelText}: {value:F0}";

        ApplyToExistingFish(config.ApplySecondParam, (float)value);
    }

    private void ApplyToExistingFish(Action<BaseFish, float> action, float value)
    {
        if (fishManager == null)
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

    public void UpdateFishCount(string type, int count)
    {
        if (censusLabels.TryGetValue(type, out var label) && label != null)
            label.Text = $"{char.ToUpper(type[0]) + type.Substring(1)}: {count}";
    }
}
