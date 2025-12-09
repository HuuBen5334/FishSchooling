using Godot;
using FishBehaviors;

public partial class NemoFish : BaseFish
{
	public NemoFish()
	{
		FishType = "nemo";
		MaxSpeed = 100.0f;
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
		// You can tweak weights and parameters for different schooling effects
		behaviors.Add(new AlignmentBehavior { Weight = 0.5f, PerceptionRadius = 150.0f });
		behaviors.Add(new CohesionBehavior { Weight = 0.8f, PerceptionRadius = 150.0f });
		behaviors.Add(new SeparationBehavior { Weight = 1.6f, SafeRadius = 30.0f });
		behaviors.Add(new WanderBehavior { Weight = 0.6f }); // Gentle wandering to make movement less rigid
		behaviors.Add(new FleeBehavior { Weight = 3.0f, PanicDistance = 200.0f });
		behaviors.Add(new ObstacleAvoidanceBehavior {
			Weight = 5.0f,
			DetectionRadius = 30.0f,
			ObstacleGroup = "obstacles"
		});
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
		behaviors.Add(new ObstacleAvoidanceBehavior {
			Weight = 5.0f,
			DetectionRadius = 30.0f,
			ObstacleGroup = "obstacles"
		});
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
			var deathEffect = new DeathEffect();
			deathEffect.Position = fish.Position;
			GetParent().AddChild(deathEffect);
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

public partial class DeathEffect : BaseFish
{
	private Tween tween;
	public DeathEffect()
	{
		FishType = "death_effect";
		MaxSpeed = 0.0f; // Static effect
	}

	protected override void SetupVisual()
	{
		var effectScene = GD.Load<PackedScene>("res://death_effect.tscn");
		var instance = effectScene.Instantiate();
		AddChild(instance);
		var sprite = instance.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		sprite.Play("death_animation");
		sprite.AnimationFinished += () => StartFadeOut(sprite);

	}
	private void StartFadeOut(AnimatedSprite2D sprite)
	{
		var fadeTween = CreateTween();
		fadeTween.TweenProperty(sprite, "modulate:a", 0.0f, 1.0f);
		fadeTween.TweenCallback(Callable.From(() => QueueFree()));
	}

	protected override void SetupBehaviors()
	{
		// No behaviors needed for death effect
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
		behaviors.Add(new PathFollowBehavior { Weight = 2.0f, PathLength = 500.0f });
		behaviors.Add(new WanderBehavior { Weight = 0.3f });
		behaviors.Add(new SeparationBehavior { Weight = 1.0f, SafeRadius = 20.0f });
		behaviors.Add(new ObstacleAvoidanceBehavior {
			Weight = 5.0f,
			DetectionRadius = 30.0f,
			ObstacleGroup = "obstacles"
		});
	}
}

public partial class EelFish : BaseFish
{
	public EelFish()
	{
		FishType = "eel";
		MaxSpeed = 140.0f; // Fast when chasing
	}
	
	protected override void SetupVisual()
	{
		var fishScene = GD.Load<PackedScene>("res://gold_fish.tscn");
		var instance = fishScene.Instantiate();
		AddChild(instance);
		sprite = instance.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		sprite.Play("default");
		
		// Green elongated appearance
		sprite.Modulate = new Color(0.3f, 0.8f, 0.4f);
		Scale = new Vector2(1.8f, 0.8f);
		SetupCollision(instance);
	}
	
	protected override void SetupBehaviors()
	{
		// Home guard keeps eel near its spot
		behaviors.Add(new HomeGuardBehavior { 
			Weight = 2.0f, 
			ComfortRadius = 50.0f,
			MaxRadius = 300.0f 
		});
		
		// Interception for smart pursuit
		behaviors.Add(new InterceptionBehavior { 
			Weight = 3.0f, 
			DetectionRadius = 250.0f,
			PredictionTime = 0.5f,
			TargetTypes = ["nemo", "starfish"]
		});
		
		// Territory defense activates when intruders present
		behaviors.Add(new TerritoryDefenseBehavior {
			Weight = 1.0f,
			TerritoryRadius = 250.0f,
			IntruderTypes = ["nemo", "starfish"]
		});
		
		// Lurking for idle animation
		behaviors.Add(new LurkingBehavior { 
			Weight = 0.5f,
			SwaySpeed = 0.05f,
			SwayAmount = 0.3f
		});
		
		// Basic separation and obstacle avoidance
		behaviors.Add(new SeparationBehavior { Weight = 1.5f, SafeRadius = 40.0f });
		behaviors.Add(new ObstacleAvoidanceBehavior {
			Weight = 5.0f,
			DetectionRadius = 30.0f,
			ObstacleGroup = "obstacles"
		});
	}

	private void SetupCollision(Node eelInstance)
	{
		if (eelInstance is Area2D area)
		{
			area.AreaEntered += OnAreaEntered;
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area.GetParent() is NemoFish fish)
		{
			GD.Print("Eel caught a fish!");
			CatchFish(fish);
			var deathEffect = new DeathEffect();
			deathEffect.Position = fish.Position;
			GetParent().AddChild(deathEffect);
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

public partial class OrcaFish : BaseFish
{
	public OrcaFish()
	{
		FishType = "orca";
		MaxSpeed = 130.0f;
	}
	
	protected override void SetupVisual()
	{
		var fishScene = GD.Load<PackedScene>("res://orca.tscn");
		var instance = fishScene.Instantiate();
		AddChild(instance);
		sprite = instance.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		sprite.Play("default");
		SetupCollision(instance);
	}
	
	protected override void SetupBehaviors()
	{
		// Smart shark uses interception instead of simple pursuit
		behaviors.Add(new InterceptionBehavior { 
			Weight = 3.0f,
			DetectionRadius = 400.0f, // Larger detection range
			PredictionTime = 0.8f,    // Better prediction
			TargetTypes = ["nemo", "eel"]
		});
		
		behaviors.Add(new WanderBehavior { Weight = 0.5f });
		behaviors.Add(new AlignmentBehavior { Weight = 0.5f, PerceptionRadius = 150.0f });
		behaviors.Add(new CohesionBehavior { Weight = 0.8f, PerceptionRadius = 150.0f });
		behaviors.Add(new SeparationBehavior { Weight = 1.6f, SafeRadius = 30.0f });
		behaviors.Add(new ObstacleAvoidanceBehavior {
			Weight = 5.0f,
			DetectionRadius = 30.0f,
			ObstacleGroup = "obstacles"
		});
	}

	private void SetupCollision(Node orcaInstance)
	{
		if (orcaInstance is Area2D area)
		{
			area.AreaEntered += OnAreaEntered;
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area.GetParent() is BaseFish fish && (fish.FishType == "nemo" || fish.FishType == "starfish" || fish.FishType == "eel"))
		{
			GD.Print("Orca caught a fish!");
			CatchFish(fish);
			var deathEffect = new DeathEffect();
			deathEffect.Position = fish.Position;
			GetParent().AddChild(deathEffect);
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
