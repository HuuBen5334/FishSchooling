using Godot;
using System;

public partial class TestFish : Node2D
{
	[Export]
	public float Speed = 200;

	public override void _Ready()
	{
		Position = new Vector2(0, 0);
	}

	public override void _Process(double delta)
	{
		var velocity = Vector2.Zero;
		
		if (Input.IsActionPressed("ui_right"))
			velocity.X += 1;
		if (Input.IsActionPressed("ui_left"))
			velocity.X -= 1;
		if (Input.IsActionPressed("ui_down"))
			velocity.Y += 1;
		if (Input.IsActionPressed("ui_up"))
			velocity.Y -= 1;
		
		Position += velocity.Normalized() * Speed * (float)delta;
	}
}
