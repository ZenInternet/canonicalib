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

    /// <summary>
    /// Model exercising the DataAnnotations validation attributes the schema generator honours.
    /// </summary>
    public class AnnotatedModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string? Name { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public NestedReference? Owner { get; set; }

        public string? Optional { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(50, MinimumLength = 3)]
        public string? Slug { get; set; }

        [System.ComponentModel.DataAnnotations.Range(1, 100)]
        public int Percentage { get; set; }

        [System.ComponentModel.DataAnnotations.RegularExpression("^[a-z]+$")]
        public string? LowerOnly { get; set; }

        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string? Email { get; set; }

        [System.ComponentModel.DataAnnotations.Url]
        public string? Website { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(5)]
        public List<string>? Tags { get; set; }

        [System.ComponentModel.ReadOnly(true)]
        public string? ComputedCode { get; set; }

        [System.ComponentModel.DefaultValue(10)]
        public int PageSize { get; set; }
    }

    /// <summary>
    /// A nested reference type used to verify that <c>[Required]</c> promotes a reference-typed property.
    /// </summary>
    public class NestedReference
    {
        public string? Id { get; set; }
    }
}
