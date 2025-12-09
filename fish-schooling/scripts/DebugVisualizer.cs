using Godot;
using System.Collections.Generic;
using FishBehaviors;

public partial class DebugVisualizer : Node2D
{
    private bool debugEnabled = false;
    private Dictionary<BaseFish, DebugArrow> debugArrows = new Dictionary<BaseFish, DebugArrow>();
    private Dictionary<BaseFish, Dictionary<string, DebugArrow>> behaviorArrows = new Dictionary<BaseFish, Dictionary<string, DebugArrow>>();
    
    // Debug display options
    public bool ShowCombinedVector { get; set; } = true;
    public bool ShowIndividualBehaviors { get; set; } = false;
    public bool ShowPerceptionRadius { get; set; } = false;
    
    // Performance options
    public float UpdateInterval { get; set; } = 0.01f; // Update every 10ms instead of every frame
    public int MaxVisualizedFish { get; set; } = 50;   // Limit how many fish show debug arrows
    
    private float timeSinceLastUpdate = 0f;
    private List<BaseFish> visualizedFish = new List<BaseFish>();
    
    private partial class DebugArrow : Node2D
    {
        private Line2D line;
        private Polygon2D arrowHead;
        public Color ArrowColor { get; set; }
        
        public DebugArrow(Color color, string behaviorName = "")
        {
            ArrowColor = color;
            
            // Create line for the arrow shaft
            line = new Line2D();
            line.Width = 2.0f;
            line.DefaultColor = color;
            line.AddPoint(Vector2.Zero);
            line.AddPoint(Vector2.Zero);
            AddChild(line);
            
            // Create triangle for arrow head
            arrowHead = new Polygon2D();
            arrowHead.Color = color;
            Vector2[] triangle =
            [
                new Vector2(0, -8),
                new Vector2(-4, 0),
                new Vector2(4, 0)
            ];
            arrowHead.Polygon = triangle;
            AddChild(arrowHead);
        }
        
        public void UpdateArrow(Vector2 direction, float magnitude)
        {
            // Scale the arrow based on magnitude (cap at reasonable length)
            float maxLength = 100.0f;
            float length = Mathf.Min(magnitude * 30.0f, maxLength);
            
            if (length < 1.0f)
            {
                Visible = false;
                return;
            }
            
            Visible = true;
            
            // Update line
            line.SetPointPosition(1, direction * length);
            
            // Update arrowhead position and rotation
            arrowHead.Position = direction * length;
            arrowHead.Rotation = direction.Angle() + Mathf.Pi / 2;
        }
    }
    
    public void ToggleDebug()
    {
        debugEnabled = !debugEnabled;
        Visible = debugEnabled;
        
        if (!debugEnabled)
        {
            ClearDebugVisuals();
            visualizedFish.Clear();
        }
        
        GD.Print($"Debug mode: {(debugEnabled ? "ON" : "OFF")}");
    }
    
    public void UpdateDebugVisualization(List<BaseFish> allFish, float delta)
    {
        if (!debugEnabled) return;
        
        // Throttle updates
        timeSinceLastUpdate += delta;
        if (timeSinceLastUpdate < UpdateInterval)
            return;
        timeSinceLastUpdate = 0f;
        
        // Select which fish to visualize (limit count for performance)
        UpdateVisualizedFishList(allFish);
        
        foreach (var fish in visualizedFish)
        {
            if (fish == null || !IsInstanceValid(fish)) continue;
            if (fish.FishType == "death_effect") continue;
            
            // Update combined steering vector (only if not showing individual behaviors)
            if (ShowCombinedVector && !ShowIndividualBehaviors)
            {
                UpdateCombinedVector(fish);
            }
            else if (debugArrows.ContainsKey(fish))
            {
                // Hide the combined arrow when showing individual behaviors
                debugArrows[fish].Visible = false;
            }
            
            // Update individual behavior vectors
            if (ShowIndividualBehaviors)
            {
                UpdateBehaviorVectors(fish);
            }
            else if (behaviorArrows.ContainsKey(fish))
            {
                foreach (var arrow in behaviorArrows[fish].Values)
                {
                    arrow.Visible = false;
                }
            }
            
            // Update perception radius
            if (ShowPerceptionRadius)
            {
                UpdatePerceptionRadius(fish);
            }
        }
        
        // Hide arrows for fish no longer being visualized
        CleanupNonVisualizedFish();
    }
    
    // Overload for backward compatibility (without delta)
    public void UpdateDebugVisualization(List<BaseFish> allFish)
    {
        UpdateDebugVisualization(allFish, UpdateInterval); // Force update
    }
    
    private void UpdateVisualizedFishList(List<BaseFish> allFish)
    {
        visualizedFish.Clear();
        
        int count = 0;
        foreach (var fish in allFish)
        {
            if (count >= MaxVisualizedFish) break;
            if (fish.FishType == "death_effect") continue;
            
            visualizedFish.Add(fish);
            count++;
        }
    }
    
    private void CleanupNonVisualizedFish()
    {
        // Hide arrows for fish not in the visualized list
        var fishToClean = new List<BaseFish>();
        
        foreach (var fish in debugArrows.Keys)
        {
            if (!visualizedFish.Contains(fish))
                fishToClean.Add(fish);
        }
        
        foreach (var fish in fishToClean)
        {
            if (debugArrows.ContainsKey(fish))
                debugArrows[fish].Visible = false;
            
            if (behaviorArrows.ContainsKey(fish))
            {
                foreach (var arrow in behaviorArrows[fish].Values)
                    arrow.Visible = false;
            }
        }
    }
    
    private void UpdateCombinedVector(BaseFish fish)
    {
        if (!debugArrows.ContainsKey(fish))
        {
            var arrow = new DebugArrow(new Color(1, 1, 0, 0.8f), "");
            fish.AddChild(arrow);
            debugArrows[fish] = arrow;
        }
        
        debugArrows[fish].Rotation = -fish.Rotation;
        
        Vector2 totalSteering = fish.GetLastSteeringForce();
        float magnitude = totalSteering.Length();
        
        if (magnitude > 0)
        {
            debugArrows[fish].UpdateArrow(totalSteering.Normalized(), magnitude);
        }
    }
    
    private void UpdateBehaviorVectors(BaseFish fish)
    {
        if (!behaviorArrows.ContainsKey(fish))
        {
            behaviorArrows[fish] = new Dictionary<string, DebugArrow>();
        }
        
        var behaviors = fish.GetBehaviorForces();
        
        foreach (var kvp in behaviors)
        {
            string behaviorName = kvp.Key;
            Vector2 force = kvp.Value;
            
            if (!behaviorArrows[fish].ContainsKey(behaviorName))
            {
                Color color = GetColorForBehavior(behaviorName);
                var arrow = new DebugArrow(color, behaviorName);
                fish.AddChild(arrow);
                behaviorArrows[fish][behaviorName] = arrow;
            }
            
            behaviorArrows[fish][behaviorName].Rotation = -fish.Rotation;
            
            float magnitude = force.Length();
            if (magnitude > 0)
            {
                Vector2 direction = force.Normalized();
                behaviorArrows[fish][behaviorName].Position = Vector2.Zero;
                behaviorArrows[fish][behaviorName].UpdateArrow(direction, magnitude);
            }
            else
            {
                behaviorArrows[fish][behaviorName].Visible = false;
            }
        }
    }
    
    private void UpdatePerceptionRadius(BaseFish fish)
    {
        // This could draw a circle showing perception radius
        // Implementation depends on your needs
    }
    
    private Color GetColorForBehavior(string behaviorName)
    {
        return behaviorName switch
        {
            "AlignmentBehavior" => new Color(0, 1, 0, 0.6f),      // Green
            "CohesionBehavior" => new Color(0, 0, 1, 0.6f),       // Blue
            "SeparationBehavior" => new Color(1, 0, 0, 0.6f),     // Red
            "FleeBehavior" => new Color(1, 0, 1, 0.6f),           // Magenta
            "PursuitBehavior" => new Color(1, 0.5f, 0, 0.6f),     // Orange
            "InterceptionBehavior" => new Color(1, 0.5f, 0, 0.6f), // Orange
            "WanderBehavior" => new Color(0.68f, 0.78f, 0.81f, 0.6f),   // Light Blue
            "PathFollowBehavior" => new Color(0.5f, 1, 0.5f, 0.6f), // Light Green
            "HomeGuardBehavior" => new Color(1, 1, 0.5f, 0.6f),   // Light Yellow
            "TerritoryDefenseBehavior" => new Color(0.8f, 0.2f, 0.2f, 0.6f), // Dark Red
            "LurkingBehavior" => new Color(0.5f, 0, 0.5f, 0.6f),  // Purple
            "ObstacleAvoidanceBehavior" => new Color(1, 1, 1, 0.6f), // White
            _ => new Color(0.5f, 0.5f, 0.5f, 0.6f)                // Gray
        };
    }
    
    private void ClearDebugVisuals()
    {
        foreach (var arrow in debugArrows.Values)
        {
            arrow.QueueFree();
        }
        debugArrows.Clear();
        
        foreach (var fishArrows in behaviorArrows.Values)
        {
            foreach (var arrow in fishArrows.Values)
            {
                arrow.QueueFree();
            }
        }
        behaviorArrows.Clear();
    }
    
    public void RemoveFishDebugVisuals(BaseFish fish)
    {
        visualizedFish.Remove(fish);
        
        if (debugArrows.ContainsKey(fish))
        {
            debugArrows[fish].QueueFree();
            debugArrows.Remove(fish);
        }
        
        if (behaviorArrows.ContainsKey(fish))
        {
            foreach (var arrow in behaviorArrows[fish].Values)
            {
                arrow.QueueFree();
            }
            behaviorArrows.Remove(fish);
        }
    }
}
