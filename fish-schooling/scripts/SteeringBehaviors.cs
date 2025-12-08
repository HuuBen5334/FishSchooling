using Godot;
using System.Collections.Generic;
using System.Linq;

namespace FishBehaviors
{
	// Helper extension methods
	public static class Vector2Extensions
	{
		public static Vector2 LimitLength(this Vector2 vector, float maxLength)
		{
			if (vector.Length() > maxLength)
				return vector.Normalized() * maxLength;
			return vector;
		}
	}

	// Base interface for all steering behaviors
	public interface ISteeringBehavior
	{
		Vector2 Calculate(BaseFish fish, List<BaseFish> allFish);
		float Weight { get; set; }
		bool IsActive { get; set; }
	}

	// Alignment: Match velocity of nearby fish (FIXED to match original)
	public class AlignmentBehavior : ISteeringBehavior
	{
		public float Weight { get; set; } = 1.0f;
		public bool IsActive { get; set; } = true;
		public float PerceptionRadius { get; set; } = 150.0f;

		public Vector2 Calculate(BaseFish fish, List<BaseFish> allFish)
		{
			if (!IsActive) return Vector2.Zero;

			// Get neighbors of same type
			var neighbors = fish.GetNeighbors(allFish, PerceptionRadius);
			if (neighbors.Count == 0) return Vector2.Zero;

			// Calculate average velocity (matching original logic)
			Vector2 avgVelocity = Vector2.Zero;
			foreach (var neighbor in neighbors)
			{
				avgVelocity += neighbor.Velocity;
			}
			avgVelocity /= neighbors.Count;

			// Return normalized direction scaled by weight (matching original)
			Vector2 vector = avgVelocity / fish.MaxSpeed;
			if (vector.LengthSquared() > 1)
			{
				vector = vector.Normalized();
			}
			return vector * Weight;
		}
	}

	// Cohesion: Move toward center of nearby fish (FIXED to match original)
	public class CohesionBehavior : ISteeringBehavior
	{
		public float Weight { get; set; } = 1.0f;
		public bool IsActive { get; set; } = true;
		public float PerceptionRadius { get; set; } = 150.0f;

		public Vector2 Calculate(BaseFish fish, List<BaseFish> allFish)
		{
			if (!IsActive) return Vector2.Zero;

			var neighbors = fish.GetNeighbors(allFish, PerceptionRadius);
			if (neighbors.Count == 0) return Vector2.Zero;

			// Calculate center of mass
			Vector2 avgPosition = Vector2.Zero;
			foreach (var neighbor in neighbors)
			{
				avgPosition += neighbor.Position;
			}
			avgPosition /= neighbors.Count;

			// Match original cohesion calculation
			Vector2 vector = avgPosition - fish.Position;
			float distance = vector.Length();
			
			if (distance > 0)
			{
				vector = vector.Normalized();
				if (distance < PerceptionRadius)
				{
					vector *= distance / PerceptionRadius;
				}
			}
			
			return vector * Weight;
		}
	}

	// Separation: Avoid crowding neighbors (FIXED to match original)
	public class SeparationBehavior : ISteeringBehavior
	{
		public float Weight { get; set; } = 2.0f;
		public bool IsActive { get; set; } = true;
		public float SafeRadius { get; set; } = 30.0f;

		public Vector2 Calculate(BaseFish fish, List<BaseFish> allFish)
		{
			if (!IsActive) return Vector2.Zero;

			Vector2 sum = Vector2.Zero;
			
			foreach (var other in allFish)
			{
				if (other == fish) continue;
				
				// Check all fish for separation (not just same type)
				Vector2 vector = fish.Position - other.Position;
				float distance = vector.Length();
				float safeDistance = SafeRadius + SafeRadius; // Using consistent safe radius

				if (distance < safeDistance && distance > 0)
				{
					vector = vector.Normalized();
					vector *= (safeDistance - distance) / safeDistance;
					sum += vector;
				}
			}

			// Cap the separation force
			if (sum.Length() > 1.0f)
			{
				sum = sum.Normalized();
			}
			
			return sum * Weight;
		}
	}

	// Wander: Random movement for sharks and starfish
	public class WanderBehavior : ISteeringBehavior
	{
		public float Weight { get; set; } = 1.0f;
		public bool IsActive { get; set; } = true;
		private float wanderAngle = 0.0f;

		public Vector2 Calculate(BaseFish fish, List<BaseFish> allFish)
		{
			if (!IsActive) return Vector2.Zero;

			// Simple wander by adding random turn
			wanderAngle += (GD.Randf() - 0.5f) * 2.0f;
			
			Vector2 wanderDirection = Vector2.FromAngle(wanderAngle);
			return wanderDirection.Normalized() * Weight;
		}
	}

	// Pursuit: Chase target fish (for sharks)
	public class PursuitBehavior : ISteeringBehavior
	{
		public float Weight { get; set; } = 1.5f;
		public bool IsActive { get; set; } = true;
		public float HuntRadius { get; set; } = 300.0f;

		public Vector2 Calculate(BaseFish fish, List<BaseFish> allFish)
		{
			if (!IsActive) return Vector2.Zero;

			// Find closest prey
			BaseFish closestPrey = null;
			float closestDistance = HuntRadius;

			foreach (var other in allFish)
			{
				if (other.FishType == "nemo")
				{
					float distance = fish.Position.DistanceTo(other.Position);
					if (distance < closestDistance)
					{
						closestDistance = distance;
						closestPrey = other;
					}
				}
			}

			if (closestPrey == null) return Vector2.Zero;

			// Simple pursuit - move toward prey
			Vector2 toPrey = closestPrey.Position - fish.Position;
			if (toPrey.Length() > 0)
			{
				return toPrey.Normalized() * Weight;
			}
			
			return Vector2.Zero;
		}
	}

	// Flee: Run from predators (for nemos)
	public class FleeBehavior : ISteeringBehavior
	{
		public float Weight { get; set; } = 3.0f;
		public bool IsActive { get; set; } = true;
		public float PanicDistance { get; set; } = 200.0f;

		public Vector2 Calculate(BaseFish fish, List<BaseFish> allFish)
		{
			if (!IsActive) return Vector2.Zero;

			Vector2 fleeForce = Vector2.Zero;
			int threatCount = 0;

			foreach (var other in allFish)
			{
				if (other.FishType == "shark" || other.FishType == "orca" || other.FishType == "eel")
				{
					float distance = fish.Position.DistanceTo(other.Position);
					if (distance < PanicDistance && distance > 0)
					{
						Vector2 awayFromThreat = (fish.Position - other.Position).Normalized();
						float urgency = 1.0f - (distance / PanicDistance);
						fleeForce += awayFromThreat * urgency;
						threatCount++;
					}
				}
			}

			if (threatCount > 0)
			{
				fleeForce /= threatCount;
				return fleeForce.Normalized() * Weight;
			}
			
			return Vector2.Zero;
		}
	}

	// Path Following: Move along ocean floor (for starfish)
	public class PathFollowBehavior : ISteeringBehavior
	{
		public float Weight { get; set; } = 1.0f;
		public bool IsActive { get; set; } = true;
		public float PathLength { get; set; } = 400.0f;

		private static Dictionary<BaseFish, float> fishTargetX = new Dictionary<BaseFish, float>();
		private static Dictionary<BaseFish, int> fishDirection = new Dictionary<BaseFish, int>();

		public Vector2 Calculate(BaseFish fish, List<BaseFish> allFish)
		{
			if (!IsActive) return Vector2.Zero;

			// Initialize if needed
			if (!fishTargetX.ContainsKey(fish))
			{
				// Random starting direction (1 = right, -1 = left)
				fishDirection[fish] = GD.Randf() > 0.5f ? 1 : -1;
				fishTargetX[fish] = fish.Position.X + PathLength * fishDirection[fish];
			}

			// Check if reached target
			float distanceToTarget = Mathf.Abs(fishTargetX[fish] - fish.Position.X);
			if (distanceToTarget < 10.0f)
			{
				// Reverse direction
				fishDirection[fish] *= -1;
				fishTargetX[fish] = fish.Position.X + PathLength * fishDirection[fish];
			}

			// Simple horizontal movement
			float xDirection = Mathf.Sign(fishTargetX[fish] - fish.Position.X);

			// Small vertical drift for organic feel
			float verticalDrift = (float)(Mathf.Sin(Time.GetUnixTimeFromSystem() * 0.001) * 0.2f);

			return new Vector2(xDirection, verticalDrift) * Weight;
		}
	}

	// Interception: Predict and intercept moving targets
	public class InterceptionBehavior : ISteeringBehavior
	{
		public float Weight { get; set; } = 3.0f;
		public bool IsActive { get; set; } = true;
		public float DetectionRadius { get; set; } = 300.0f;
		public float PredictionTime { get; set; } = 0.5f;
		public string[] TargetTypes { get; set; } = new string[] { "nemo", "starfish" }; // Which fish types to intercept

		public Vector2 Calculate(BaseFish fish, List<BaseFish> allFish)
		{
			if (!IsActive) return Vector2.Zero;

			// Find closest target to intercept
			BaseFish closestTarget = null;
			float closestDistance = DetectionRadius;

			foreach (var other in allFish)
			{
				// Check if this fish type is a target
				if (TargetTypes.Contains(other.FishType))
				{
					float distance = fish.Position.DistanceTo(other.Position);
					if (distance < closestDistance)
					{
						closestDistance = distance;
						closestTarget = other;
					}
				}
			}

			if (closestTarget == null) return Vector2.Zero;

			// Calculate interception point based on target's velocity
			Vector2 targetPosition = closestTarget.Position;
			Vector2 targetVelocity = closestTarget.Velocity;

			// Estimate time to intercept
			float timeToReach = closestDistance / fish.MaxSpeed;
			float lookAhead = Mathf.Min(timeToReach, PredictionTime);

			// Predict where target will be
			Vector2 predictedPosition = targetPosition + targetVelocity * lookAhead;

			// Seek the predicted position
			Vector2 toInterception = predictedPosition - fish.Position;

			if (toInterception.Length() > 0)
			{
				// More aggressive when closer to target
				float urgency = 1.0f - (closestDistance / DetectionRadius);
				return toInterception.Normalized() * Weight * (1.0f + urgency * 0.5f);
			}

			return Vector2.Zero;
		}
	}

	// Guard: Stay near home, return when straying
	public class HomeGuardBehavior : ISteeringBehavior
	{
		public float Weight { get; set; } = 2.0f;
		public bool IsActive { get; set; } = true;
		public float ComfortRadius { get; set; } = 50.0f;  // Comfortable distance from home
		public float MaxRadius { get; set; } = 300.0f;      // Maximum distance before forced return

		private static Dictionary<BaseFish, Vector2> fishHomes = new Dictionary<BaseFish, Vector2>();

		public Vector2 Calculate(BaseFish fish, List<BaseFish> allFish)
		{
			if (!IsActive) return Vector2.Zero;

			// Set home position on first call
			if (!fishHomes.ContainsKey(fish))
			{
				fishHomes[fish] = fish.Position;
				GD.Print($"{fish.FishType} home established at: {fish.Position}");
			}

			Vector2 home = fishHomes[fish];
			float distanceFromHome = fish.Position.DistanceTo(home);

			// If beyond max radius, strong pull home
			if (distanceFromHome > MaxRadius)
			{
				Vector2 toHome = home - fish.Position;
				return toHome.Normalized() * Weight * 2.0f; // Double weight for urgent return
			}

			// If beyond comfort zone but within max, gentle pull home
			if (distanceFromHome > ComfortRadius)
			{
				Vector2 toHome = home - fish.Position;
				float pullStrength = (distanceFromHome - ComfortRadius) / (MaxRadius - ComfortRadius);
				return toHome.Normalized() * Weight * pullStrength;
			}

			// Within comfort zone - no correction needed
			return Vector2.Zero;
		}

		public static Vector2 GetHome(BaseFish fish)
		{
			return fishHomes.ContainsKey(fish) ? fishHomes[fish] : fish.Position;
		}

		public static void SetHome(BaseFish fish, Vector2 position)
		{
			fishHomes[fish] = position;
		}
	}

	// Lurking: Idle animation when waiting/hiding
	public class LurkingBehavior : ISteeringBehavior
	{
		public float Weight { get; set; } = 0.5f;
		public bool IsActive { get; set; } = true;
		public float SwaySpeed { get; set; } = 0.05f;
		public float SwayAmount { get; set; } = 0.3f;

		private static Dictionary<BaseFish, float> fishSwayAngles = new Dictionary<BaseFish, float>();

		public Vector2 Calculate(BaseFish fish, List<BaseFish> allFish)
		{
			if (!IsActive) return Vector2.Zero;

			// Initialize or update sway angle for this fish
			if (!fishSwayAngles.ContainsKey(fish))
			{
				fishSwayAngles[fish] = GD.Randf() * Mathf.Pi * 2; // Random starting angle
			}

			fishSwayAngles[fish] += SwaySpeed;
			float angle = fishSwayAngles[fish];

			// Create swaying motion
			float swayX = Mathf.Sin(angle) * SwayAmount;
			float swayY = Mathf.Cos(angle * 0.7f) * SwayAmount * 0.5f; // Different frequency for Y

			return new Vector2(swayX, swayY) * Weight;
		}
	}
	
	// Territory Defense: Only active when intruders in territory
	public class TerritoryDefenseBehavior : ISteeringBehavior
	{
		public float Weight { get; set; } = 1.0f;
		public bool IsActive { get; set; } = true;
		public float TerritoryRadius { get; set; } = 250.0f;
		public string[] IntruderTypes { get; set; } = new string[] { "nemo", "starfish" };
		
		public Vector2 Calculate(BaseFish fish, List<BaseFish> allFish)
		{
			if (!IsActive) return Vector2.Zero;
			
			// Get home position from HomeGuardBehavior
			Vector2 home = HomeGuardBehavior.GetHome(fish);
			
			// Check for intruders in territory
			foreach (var other in allFish)
			{
				if (IntruderTypes.Contains(other.FishType))
				{
					float distanceFromHome = other.Position.DistanceTo(home);
					
					// If intruder is in territory, this behavior activates
					// (actual chasing is handled by InterceptionBehavior)
					if (distanceFromHome < TerritoryRadius)
					{
						// Just return a small force toward the intruder to "wake up" the eel
						Vector2 toIntruder = other.Position - fish.Position;
						return toIntruder.Normalized() * Weight * 0.1f; // Small activation force
					}
				}
			}
			
			return Vector2.Zero;
		}
	}

	public class ObstacleAvoidanceBehavior : ISteeringBehavior
	{
		public float Weight { get; set; } = 1.2f;
		public bool IsActive { get; set; } = true;
		public float DetectionRadius { get; set; } = 80.0f;
		public string ObstacleGroup { get; set; } = "obstacles";

		public Vector2 Calculate(BaseFish fish, List<BaseFish> allFish)
		{
			if (!IsActive) return Vector2.Zero;

			Vector2 avoidanceForce = Vector2.Zero;
			int obstacleCount = 0;

			//Get all obstacles in scene by group
			var obstacles = fish.GetTree().GetNodesInGroup(ObstacleGroup);

			foreach (var obstacle in obstacles)
			{
				if (obstacle is Node2D obstacleNode)
				{
					float distance = fish.Position.DistanceTo(obstacleNode.GlobalPosition);
					
					if (distance < DetectionRadius && distance > 0)
					{
						Vector2 awayFromObstacle = (fish.Position - obstacleNode.GlobalPosition).Normalized();
						float urgency = 1.0f - (distance / DetectionRadius);
						avoidanceForce += awayFromObstacle * urgency;
						obstacleCount++;
					}
				}
			}

			if (obstacleCount > 0)
			{
				avoidanceForce /= obstacleCount;
				return avoidanceForce.Normalized() * Weight;
			}

			return Vector2.Zero;
		}
	}
}
