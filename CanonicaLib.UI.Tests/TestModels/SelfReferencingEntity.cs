namespace Zen.CanonicaLib.UI.Tests.TestModels
{
    /// <summary>
    /// Test model that references itself through a collection property
    /// </summary>
    public class SelfReferencingEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SelfReferencingEntity[]? Children { get; set; }
        public SelfReferencingEntity? Parent { get; set; }
    }

    /// <summary>
    /// Test model with circular reference through another type
    /// </summary>
    public class Node
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Edge>? Edges { get; set; }
    }

    public class Edge
    {
        public int Id { get; set; }
        public Node? Source { get; set; }
        public Node? Target { get; set; }
    }

    /// <summary>
    /// Tree structure with self-reference
    /// </summary>
    public class TreeNode
    {
        public int Value { get; set; }
        public TreeNode? Left { get; set; }
        public TreeNode? Right { get; set; }
    }

    /// <summary>
    /// Simple non-self-referencing model for comparison
    /// </summary>
    public class SimpleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
