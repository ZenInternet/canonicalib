using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Moq;
using System.Reflection;
using Xunit;
using Zen.CanonicaLib.UI.Services;
using Zen.CanonicaLib.UI.Services.Interfaces;
using Zen.CanonicaLib.UI.Tests.TestModels;

namespace Zen.CanonicaLib.UI.Tests.Services
{
    public class DefaultSchemaGeneratorTests
    {
        private readonly Mock<IDiscoveryService> _mockDiscoveryService;
        private readonly Mock<ILogger<DefaultSchemaGenerator>> _mockLogger;
        private readonly DefaultSchemaGenerator _schemaGenerator;
        private readonly Assembly _testAssembly;

        public DefaultSchemaGeneratorTests()
        {
            _mockDiscoveryService = new Mock<IDiscoveryService>();
            _mockLogger = new Mock<ILogger<DefaultSchemaGenerator>>();
            _schemaGenerator = new DefaultSchemaGenerator(_mockDiscoveryService.Object, _mockLogger.Object);
            _testAssembly = typeof(SelfReferencingEntity).Assembly;

            // Setup default behavior for discovery service
            _mockDiscoveryService
                .Setup(x => x.GetAssemblyReferenceType(It.IsAny<Assembly>(), It.IsAny<Type>()))
                .Returns(AssemblyReferenceType.Internal);
        }

        [Fact]
        public void GenerateSchema_ShouldHandleSelfReferencingEntity_WithoutInfiniteRecursion()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var entityType = typeof(SelfReferencingEntity);

            // Act
            var schema = _schemaGenerator.GenerateSchema(entityType, context);

            // Assert
            schema.Should().NotBeNull();
            schema.Should().BeOfType<OpenApiSchemaReference>();
            
            // Verify the schema was added to context
            var schemaKey = entityType.FullName ?? entityType.Name;
            context.Document.Components!.Schemas.Should().ContainKey(schemaKey);
            
            // Verify the actual schema has the self-referencing property
            var actualSchema = context.Document.Components.Schemas[schemaKey];
            actualSchema.Should().BeOfType<OpenApiSchema>();
            var openApiSchema = (OpenApiSchema)actualSchema;
            openApiSchema.Properties.Should().ContainKey("children");
            openApiSchema.Properties.Should().ContainKey("parent");
        }

        [Fact]
        public void GenerateSchema_ShouldHandleArrayOfSelfReferencingEntities()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var entityType = typeof(SelfReferencingEntity);

            // Act
            var schema = _schemaGenerator.GenerateSchema(entityType, context);

            // Assert
            var schemaKey = entityType.FullName ?? entityType.Name;
            var actualSchema = (OpenApiSchema)context.Document.Components.Schemas[schemaKey];
            
            // Check that the Children property exists and is an array
            actualSchema.Properties.Should().ContainKey("children");
            var childrenProperty = actualSchema.Properties["children"];
            childrenProperty.Should().BeOfType<OpenApiSchema>();
            var childrenSchema = (OpenApiSchema)childrenProperty;
            childrenSchema.Type.Should().Be(JsonSchemaType.Array);
            
            // The items should reference the same entity type
            childrenSchema.Items.Should().NotBeNull();
            childrenSchema.Items.Should().BeOfType<OpenApiSchemaReference>();
        }

        [Fact]
        public void GenerateSchema_ShouldHandleCircularReference_ThroughMultipleTypes()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var nodeType = typeof(Node);

            // Act
            var schema = _schemaGenerator.GenerateSchema(nodeType, context);

            // Assert
            schema.Should().NotBeNull();
            
            // Both Node and Edge should be in the schema components
            var nodeKey = nodeType.FullName ?? nodeType.Name;
            var edgeKey = typeof(Edge).FullName ?? typeof(Edge).Name;
            
            context.Document.Components!.Schemas.Should().ContainKey(nodeKey);
            context.Document.Components.Schemas.Should().ContainKey(edgeKey);
            
            // Verify Node has edges property
            var nodeSchema = (OpenApiSchema)context.Document.Components.Schemas[nodeKey];
            nodeSchema.Properties.Should().ContainKey("edges");
            
            // Verify Edge has source and target properties referencing Node
            var edgeSchema = (OpenApiSchema)context.Document.Components.Schemas[edgeKey];
            edgeSchema.Properties.Should().ContainKey("source");
            edgeSchema.Properties.Should().ContainKey("target");
        }

        [Fact]
        public void GenerateSchema_ShouldHandleTreeStructure_WithMultipleSelfReferences()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var treeNodeType = typeof(TreeNode);

            // Act
            var schema = _schemaGenerator.GenerateSchema(treeNodeType, context);

            // Assert
            schema.Should().NotBeNull();
            
            var schemaKey = treeNodeType.FullName ?? treeNodeType.Name;
            var actualSchema = (OpenApiSchema)context.Document.Components.Schemas[schemaKey];
            
            // Verify both left and right properties exist
            actualSchema.Properties.Should().ContainKey("left");
            actualSchema.Properties.Should().ContainKey("right");
            
            // Both should reference the same TreeNode type
            actualSchema.Properties["left"].Should().BeOfType<OpenApiSchemaReference>();
            actualSchema.Properties["right"].Should().BeOfType<OpenApiSchemaReference>();
        }

        [Fact]
        public void GenerateSchema_ShouldHandleSimpleEntity_WithoutRecursion()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var entityType = typeof(SimpleEntity);

            // Act
            var schema = _schemaGenerator.GenerateSchema(entityType, context);

            // Assert
            schema.Should().NotBeNull();
            
            var schemaKey = entityType.FullName ?? entityType.Name;
            context.Document.Components!.Schemas.Should().ContainKey(schemaKey);
            
            var actualSchema = (OpenApiSchema)context.Document.Components.Schemas[schemaKey];
            actualSchema.Properties.Should().ContainKey("id");
            actualSchema.Properties.Should().ContainKey("name");
            actualSchema.Properties.Should().ContainKey("createdAt");
            actualSchema.Properties.Should().HaveCount(3);
        }

        [Fact]
        public void GenerateSchema_ShouldReuseExistingSchema_WhenCalledMultipleTimes()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var entityType = typeof(SelfReferencingEntity);

            // Act
            var schema1 = _schemaGenerator.GenerateSchema(entityType, context);
            var schema2 = _schemaGenerator.GenerateSchema(entityType, context);

            // Assert
            schema1.Should().NotBeNull();
            schema2.Should().NotBeNull();
            
            // Both should return references
            schema1.Should().BeOfType<OpenApiSchemaReference>();
            schema2.Should().BeOfType<OpenApiSchemaReference>();
            
            // Schema should only be added once
            var schemaKey = entityType.FullName ?? entityType.Name;
            context.Document.Components!.Schemas.Should().ContainKey(schemaKey);
            context.Document.Components.Schemas.Should().HaveCount(1);
        }

        [Fact]
        public void GenerateSchema_ShouldThrowArgumentNullException_WhenSchemaDefinitionIsNull()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                _schemaGenerator.GenerateSchema(null!, context));
        }

        [Fact]
        public void GenerateSchema_ShouldThrowArgumentNullException_WhenContextIsNull()
        {
            // Arrange
            var entityType = typeof(SimpleEntity);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                _schemaGenerator.GenerateSchema(entityType, null!));
        }

        [Fact]
        public void GenerateSchema_ShouldSetCorrectSchemaProperties_ForSelfReferencingEntity()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var entityType = typeof(SelfReferencingEntity);

            // Act
            var schema = _schemaGenerator.GenerateSchema(entityType, context);

            // Assert
            var schemaKey = entityType.FullName ?? entityType.Name;
            var actualSchema = (OpenApiSchema)context.Document.Components.Schemas[schemaKey];
            
            actualSchema.Type.Should().Be(JsonSchemaType.Object);
            actualSchema.Title.Should().Be(entityType.Name);
            actualSchema.Properties.Should().NotBeEmpty();
        }

        [Fact]
        public void GenerateSchema_ShouldHandlePrimitiveTypes_WithoutAddingToComponents()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            
            // Setup discovery service to return Excluded for primitive types
            _mockDiscoveryService
                .Setup(x => x.GetAssemblyReferenceType(It.IsAny<Assembly>(), typeof(string)))
                .Returns(AssemblyReferenceType.Excluded);

            // Act
            var schema = _schemaGenerator.GenerateSchema(typeof(string), context);

            // Assert
            schema.Should().NotBeNull();
            schema.Should().BeOfType<OpenApiSchema>();
            
            // Primitive types should not be added to components
            context.Document.Components!.Schemas.Should().BeEmpty();
        }

        [Fact]
        public void GenerateSchema_ShouldMarkRequiredProperties_ForValueTypes()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var entityType = typeof(SimpleEntity);

            // Act
            var schema = _schemaGenerator.GenerateSchema(entityType, context);

            // Assert
            var schemaKey = entityType.FullName ?? entityType.Name;
            var actualSchema = (OpenApiSchema)context.Document.Components.Schemas[schemaKey];
            
            // Id and CreatedAt are value types, so they should be required
            actualSchema.Required.Should().Contain("id");
            actualSchema.Required.Should().Contain("createdAt");
            // Name is a reference type (string), so it should not be required by default
            actualSchema.Required.Should().NotContain("name");
        }
    }
}
