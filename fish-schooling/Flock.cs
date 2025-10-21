using Godot;
using System.Collections.Generic;

public partial class Flock : Node2D
{
    //Fish class
    //Represents a single fish in the flock with position, velocity, and physical properties
    public class Fish
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float MaxSpeed { get; set; } = 100.0f;
        public float SafeRadius { get; set; } = 30.0f;
        public Node2D Node { get; set; }
    }
    //Collection of all fish in the flock
    private List<Fish> fishList = new List<Fish>();
    private float flockRadius = 150.0f;
    private float alignmentStrength = 1.0f;
    private float cohesionStrength = 1.0f;
    private float separationStrength = 2.0f;

    //initializes the fish school
    public override void _Ready()
    {
        // Create 15 fish
        for (int i = 0; i < 15; i++)
        {
            CreateFish();
        }
    }
    //Creates a single fish with random position/velocity and adds it to the scene
    private void CreateFish()
    {
        var fish = new Fish();

        //Create visual as a Sprite2D using an imported PNG
        var fishNode = new Node2D();
        var sprite = new Sprite2D();
        // Attempt to load a fish texture; fall back to any available sprite
        Texture2D tex = GD.Load<Texture2D>("res://sprites/pixil-frame-0_3.png");
        if (tex == null)
        {
            tex = GD.Load<Texture2D>("res://sprites/pixil-frame-0_3.png");
        }
        if (tex == null)
        {
            tex = GD.Load<Texture2D>("res://sprites/pixil-frame-0_3.png");
        }
        if (tex != null)
        {
            sprite.Texture = tex;
            // Make sure the sprite's center is at the fish's origin so rotation works as expected
            sprite.Centered = true;
            // Optionally scale down if the sprite is large
            sprite.Scale = new Vector2(0.5f, 0.5f);
        }
        fishNode.AddChild(sprite);
        AddChild(fishNode);

        //Set random position and velocity
        var screenSize = GetViewportRect().Size;
        fish.Position = new Vector2(
            (float)GD.RandRange(0, screenSize.X),
            (float)GD.RandRange(0, screenSize.Y)
        );
        
        float angle = (float)GD.RandRange(0, 2 * Mathf.Pi);
        fish.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * fish.MaxSpeed * 0.5f;
        fish.Node = fishNode;

        fishList.Add(fish);
    }

    public override void _PhysicsProcess(double delta)
    {
        //Calculate average position and velocity
        Vector2 avgPosition = Vector2.Zero;
        Vector2 avgVelocity = Vector2.Zero;

        foreach (var fish in fishList)
        {
            avgPosition += fish.Position;
            avgVelocity += fish.Velocity;
        }

        avgPosition /= fishList.Count;
        avgVelocity /= fishList.Count;

        // Update each fish
        foreach (var fish in fishList)
        {
            Vector2 accel = Vector2.Zero;
            accel += CalcAlignment(fish, avgVelocity);
            accel += CalcCohesion(fish, avgPosition);
            accel += CalcSeparation(fish);

            // Apply acceleration
            fish.Velocity += accel * fish.MaxSpeed * (float)delta;

            // Limit speed
            if (fish.Velocity.Length() > fish.MaxSpeed)
            {
                fish.Velocity = fish.Velocity.Normalized() * fish.MaxSpeed;
            }

            // Update position
            fish.Position += fish.Velocity * (float)delta;

            // Screen wrapping
            var screenSize = GetViewportRect().Size;
            if (fish.Position.X < 0) fish.Position = new Vector2(screenSize.X, fish.Position.Y);
            else if (fish.Position.X > screenSize.X) fish.Position = new Vector2(0, fish.Position.Y);
            if (fish.Position.Y < 0) fish.Position = new Vector2(fish.Position.X, screenSize.Y);
            else if (fish.Position.Y > screenSize.Y) fish.Position = new Vector2(fish.Position.X, 0);

            // Update visual
            fish.Node.Position = fish.Position;
            if (fish.Velocity.Length() > 0)
            {
                fish.Node.Rotation = fish.Velocity.Angle();
            }
        }
    }
    //The three main flocking behaviors
    private Vector2 CalcAlignment(Fish fish, Vector2 avgVelocity)
    {
        Vector2 vector = avgVelocity / fish.MaxSpeed;
        if (vector.LengthSquared() > 1)
        {
            vector = vector.Normalized();
        }
        return vector * alignmentStrength;
    }

    private Vector2 CalcCohesion(Fish fish, Vector2 avgPosition)
    {
        Vector2 vector = avgPosition - fish.Position;
        float distance = vector.Length();
        vector = vector.Normalized();
        if (distance < flockRadius)
        {
            vector *= distance / flockRadius;
        }
        return vector * cohesionStrength;
    }

    private Vector2 CalcSeparation(Fish fish)
    {
        Vector2 sum = Vector2.Zero;
        foreach (var other in fishList)
        {
            if (other == fish) continue;

            Vector2 vector = fish.Position - other.Position;
            float distance = vector.Length();
            float safeDistance = fish.SafeRadius + other.SafeRadius;

            if (distance < safeDistance && distance > 0)
            {
                vector = vector.Normalized();
                vector *= (safeDistance - distance) / safeDistance;
                sum += vector;
            }
        }
        //Cap the separation force to avoid extreme reactions
        if (sum.Length() > 1.0f)
        {
            sum = sum.Normalized();
        }
        return sum * separationStrength;
    }
}