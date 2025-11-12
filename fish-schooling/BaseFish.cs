using Godot;
using System.Collections.Generic;
using System.Linq;
using FishBehaviors;

public abstract partial class BaseFish : Node2D
{
	public Vector2 Velocity { get; set; }
	public float MaxSpeed { get; set; } = 100.0f;
	public string FishType { get; protected set; }
	
	protected AnimatedSprite2D sprite;
	protected List<ISteeringBehavior> behaviors = new List<ISteeringBehavior>();

	public override void _Ready()
	{
		SetupVisual();
		SetupBehaviors();
		
		// Random initial velocity
		float angle = GD.Randf() * Mathf.Pi * 2;
		Velocity = Vector2.FromAngle(angle) * MaxSpeed * 0.5f;
	}

	protected virtual void SetupVisual()
	{
		// Override in derived classes
	}

	protected abstract void SetupBehaviors();

	public void UpdateFish(List<BaseFish> allFish, float delta)
	{
		Vector2 accel = Vector2.Zero;
		
		// Accumulate all active behaviors (matching original acceleration pattern)
		foreach (var behavior in behaviors)
		{
			if (behavior.IsActive)
			{
				accel += behavior.Calculate(this, allFish);
			}
		}

		// Apply acceleration (matching original physics)
		Velocity += accel * MaxSpeed * delta;
		
		// Limit speed
		if (Velocity.Length() > MaxSpeed)
		{
			Velocity = Velocity.Normalized() * MaxSpeed;
		}
		
		// Update position
		Position += Velocity * delta;
		
		// Update rotation
		if (Velocity.Length() > 0)
		{
			Rotation = Velocity.Angle();
		}
	}

	public List<BaseFish> GetNeighbors(List<BaseFish> allFish, float radius)
	{
		return allFish.Where(f => 
			f != this && 
			f.FishType == this.FishType && // Only flock with same type
			Position.DistanceTo(f.Position) < radius
		).ToList();
	}
}
