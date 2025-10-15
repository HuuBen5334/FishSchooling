using Godot;
using System;

public partial class Fish : Node2D
{
    
    public float maxVelocity = 256.0f;
    public float acceleration = 512.0f;
    private Vector2 velocity = Vector2.Zero;

    public override void _Process(double delta)
    {
        Vector2 inputVector = Vector2.Zero;
        Vector2 accelerationVector = Vector2.Zero;

        //get input direction
        if (Input.IsActionPressed("ui_right"))
        {
            inputVector.X += 1.0f;
        }
        if (Input.IsActionPressed("ui_left"))
        {
            inputVector.X -= 1.0f;
        }
        if (Input.IsActionPressed("ui_down"))
        {
            inputVector.Y += 1.0f;
        }
        if (Input.IsActionPressed("ui_up"))
        {
            inputVector.Y -= 1.0f;
        }

        //normalize input vector to have consistent speed in all directions
        if (inputVector != Vector2.Zero)
        {
            inputVector = inputVector.Normalized();
            accelerationVector = inputVector * acceleration; // acc vector = (negative or positive) * acceleration
            velocity += accelerationVector * (float)delta; // v = a * t
        }
        if (velocity.Length() > maxVelocity)
        {
            velocity = velocity.Normalized() * maxVelocity; // limit velocity to maxVelocity
        }
        //when no input is pressed, decelerate to a stop
        if (inputVector == Vector2.Zero && velocity.Length() > 0)
        {
            accelerationVector = -velocity.Normalized() * acceleration; // deceleration vector
            //clamp to zero if velocity is very low to avoid jittering
            if(velocity.Length() < acceleration * (float)delta)
            {
                velocity = Vector2.Zero;
            }
            else
            {
                velocity += accelerationVector * (float)delta; // v = a * t
            }
        }
        Position += velocity * (float)delta; //update position
    }

}
