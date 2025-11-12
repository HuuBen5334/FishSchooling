using Godot;
using FishBehaviors;

public partial class NemoFish : BaseFish
{
	public NemoFish()
	{
		FishType = "nemo";
		MaxSpeed = 100.0f; // Match original speed
	}

	protected override void SetupVisual()
	{
		var fishScene = GD.Load<PackedScene>("res://gold_fish.tscn");
		var instance = fishScene.Instantiate();
		AddChild(instance);
		sprite = instance.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		sprite.Play("default");
	}

	protected override void SetupBehaviors()
	{
		// Use the same weights as original
		behaviors.Add(new AlignmentBehavior { Weight = 0.5f, PerceptionRadius = 150.0f });
		behaviors.Add(new CohesionBehavior { Weight = 0.8f, PerceptionRadius = 150.0f });
		behaviors.Add(new SeparationBehavior { Weight = 1.6f, SafeRadius = 30.0f });
		behaviors.Add(new WanderBehavior { Weight = 0.6f }); // Gentle wandering to make movement less rigid
		behaviors.Add(new FleeBehavior { Weight = 3.0f, PanicDistance = 200.0f });
	}
}

public partial class SharkFish : BaseFish
{
	[Signal]
	public delegate void SharkCaughtFishEventHandler();
	public SharkFish()
	{
		FishType = "shark";
		MaxSpeed = 120.0f;
	}

	protected override void SetupVisual()
	{
		var fishScene = GD.Load<PackedScene>("res://shark.tscn");
		var instance = fishScene.Instantiate();
		AddChild(instance);
		sprite = instance.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		sprite.Play("shark_swim");
		sprite.Modulate = new Color(0.5f, 0.5f, 0.5f); // Darker
		// Scale = new Vector2(1.5f, 1.5f); // Bigger
		SetupCollision(instance);
	}

	protected override void SetupBehaviors()
	{
		behaviors.Add(new PursuitBehavior { Weight = 2.0f });
		behaviors.Add(new WanderBehavior { Weight = 0.5f });
		behaviors.Add(new SeparationBehavior { Weight = 1.5f, SafeRadius = 50.0f });
	}
	private void SetupCollision(Node sharkInstance)
	{
		if (sharkInstance is Area2D area)
		{
			area.AreaEntered += OnAreaEntered;
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area.GetParent() is NemoFish fish)
		{
			GD.Print("Shark caught a fish!");
			EmitSignal(SignalName.SharkCaughtFish);
			CatchFish(fish);
		}
	}
	private void CatchFish(BaseFish fish)
	{
		//remove fish from list in FishManager
		var fishManager = GetParent() as FishManager;
		if (fishManager != null)
        {
            fishManager.RemoveFish(fish);
        }
		fish.QueueFree();
	}
}

public partial class StarfishFish : BaseFish
{
	public StarfishFish()
	{
		FishType = "starfish";
		MaxSpeed = 15.0f; // Very slow
	}

	protected override void SetupVisual()
	{
		var fishScene = GD.Load<PackedScene>("res://starfish.tscn");
		var instance = fishScene.Instantiate();
		AddChild(instance);
		sprite = instance.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		sprite.Play("starfish_animation");
	}

	protected override void SetupBehaviors()
	{
		behaviors.Add(new PathFollowBehavior { Weight = 2.0f, BottomY = 600.0f });
		behaviors.Add(new WanderBehavior { Weight = 0.3f });
		behaviors.Add(new SeparationBehavior { Weight = 1.0f, SafeRadius = 20.0f });
	}
}
