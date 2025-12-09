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

	private Vector2 lastSteeringForce = Vector2.Zero;
    private Dictionary<string, Vector2> lastBehaviorForces = new Dictionary<string, Vector2>();

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
        lastBehaviorForces.Clear();
        
        // Calculate and store individual behavior forces for debug
        foreach (var behavior in behaviors)
        {
            if (behavior.IsActive)
            {
                Vector2 behaviorForce = behavior.Calculate(this, allFish);
                string behaviorName = behavior.GetType().Name;
                lastBehaviorForces[behaviorName] = behaviorForce;
                accel += behaviorForce;
            }
        }
        
        // Store combined force for debug visualization
        lastSteeringForce = accel;
        
        // Apply acceleration
        Velocity += accel * MaxSpeed * delta;
        
        if (Velocity.Length() > MaxSpeed)
        {
            Velocity = Velocity.Normalized() * MaxSpeed;
        }
        
        Position += Velocity * delta;
        
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

	public Vector2 GetLastSteeringForce()
    {
        return lastSteeringForce;
    }
    
    public Dictionary<string, Vector2> GetBehaviorForces()
    {
        return lastBehaviorForces;
    }
}
