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

        [Fact]
        public void GenerateSchema_ShouldMarkRequiredProperties_ForExplicitRequiredAttribute()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var type = typeof(AnnotatedModel);

            // Act
            _schemaGenerator.GenerateSchema(type, context);

            // Assert
            var schemaKey = type.FullName ?? type.Name;
            var actualSchema = (OpenApiSchema)context.Document.Components.Schemas[schemaKey];

            // [Required] promotes reference-typed properties (a string and a nested model) ...
            actualSchema.Required.Should().Contain("name");
            actualSchema.Required.Should().Contain("owner");
            // ... a non-nullable value type is required inherently ...
            actualSchema.Required.Should().Contain("percentage");
            // ... but an un-annotated nullable reference type is not.
            actualSchema.Required.Should().NotContain("optional");
        }

        [Fact]
        public void GenerateSchema_ShouldProjectValidationAttributes_OntoSchemaFacets()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var type = typeof(AnnotatedModel);

            // Act
            _schemaGenerator.GenerateSchema(type, context);

            // Assert
            var schemaKey = type.FullName ?? type.Name;
            var actualSchema = (OpenApiSchema)context.Document.Components.Schemas[schemaKey];
            OpenApiSchema Prop(string name) => (OpenApiSchema)actualSchema.Properties[name];

            // [StringLength(50, MinimumLength = 3)]
            Prop("slug").MaxLength.Should().Be(50);
            Prop("slug").MinLength.Should().Be(3);

            // [Range(1, 100)]
            Prop("percentage").Minimum.Should().Be("1");
            Prop("percentage").Maximum.Should().Be("100");

            // [RegularExpression]
            Prop("lowerOnly").Pattern.Should().Be("^[a-z]+$");

            // [EmailAddress] / [Url] -> format
            Prop("email").Format.Should().Be("email");
            Prop("website").Format.Should().Be("uri");

            // [MaxLength(5)] on a collection -> maxItems (not maxLength)
            Prop("tags").MaxItems.Should().Be(5);
            Prop("tags").MaxLength.Should().BeNull();

            // [ReadOnly(true)]
            Prop("computedCode").ReadOnly.Should().BeTrue();

            // [DefaultValue(10)]
            Prop("pageSize").Default!.GetValue<int>().Should().Be(10);
        }

        [Fact]
        public void GenerateSchema_ShouldSetDescription_OnScalarPropertyFromXmlSummary()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var type = typeof(DocumentedModel);

            // Act
            _schemaGenerator.GenerateSchema(type, context);

            // Assert
            var schemaKey = type.FullName ?? type.Name;
            var model = (OpenApiSchema)context.Document.Components!.Schemas[schemaKey];

            var accountNumber = (OpenApiSchema)model.Properties["accountNumber"];
            accountNumber.Description.Should().Be("The unique account number.");

            var lineCount = (OpenApiSchema)model.Properties["lineCount"];
            lineCount.Description.Should().Be("The number of active lines.");

            // Array (inline) properties should also carry the description.
            var roles = (OpenApiSchema)model.Properties["roles"];
            roles.Description.Should().Be("The roles held on the account.");
        }

        [Fact]
        public void GenerateSchema_ShouldFallBackToDescriptionAttribute_WhenNoXmlSummary()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var type = typeof(DocumentedModel);

            // Act
            _schemaGenerator.GenerateSchema(type, context);

            // Assert
            var schemaKey = type.FullName ?? type.Name;
            var model = (OpenApiSchema)context.Document.Components!.Schemas[schemaKey];

            var fallbackOnly = (OpenApiSchema)model.Properties["fallbackOnly"];
            fallbackOnly.Description.Should().Be("The description-attribute fallback value.");
        }

        [Fact]
        public void GenerateSchema_ShouldNotInventDescription_ForUndocumentedProperty()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var type = typeof(DocumentedModel);

            // Act
            _schemaGenerator.GenerateSchema(type, context);

            // Assert
            var schemaKey = type.FullName ?? type.Name;
            var model = (OpenApiSchema)context.Document.Components!.Schemas[schemaKey];

            var undocumented = (OpenApiSchema)model.Properties["undocumented"];
            undocumented.Description.Should().BeNullOrEmpty();
        }

        [Fact]
        public void GenerateSchema_ShouldSetReferenceDescription_OnComplexProperty()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var type = typeof(DocumentedModel);

            // Act
            _schemaGenerator.GenerateSchema(type, context);

            // Assert
            var schemaKey = type.FullName ?? type.Name;
            var model = (OpenApiSchema)context.Document.Components!.Schemas[schemaKey];

            // Complex-typed property references another schema; the property's own summary should take
            // precedence over the referenced type's description (OpenAPI 3.1 reference sibling).
            var billingAddress = model.Properties["billingAddress"];
            billingAddress.Should().BeOfType<OpenApiSchemaReference>();
            ((OpenApiSchemaReference)billingAddress).Description.Should().Be("The billing address for the account.");
        }

        [Fact]
        public void GetSchemaKey_ShouldReturnFullName_ForNonGenericType()
        {
            // Non-generic types keep their existing FullName-based key.
            var type = typeof(SimpleEntity);

            var key = GeneratorContext.GetSchemaKey(type);

            key.Should().Be(type.FullName);
        }

        [Fact]
        public void GetSchemaKey_ShouldProduceIdentifierSafeKey_ForConstructedGenericType()
        {
            // Type.FullName for a constructed generic is assembly-qualified and contains
            // backticks, brackets, commas and spaces - which break downstream code generators.
            var type = typeof(PagedResultModel<SimpleEntity>);

            // Guard: confirm the raw FullName really is unsafe, so this test stays meaningful.
            type.FullName.Should().MatchRegex("[`\\[\\], ]");

            var key = GeneratorContext.GetSchemaKey(type);

            key.Should().NotContainAny("`", "[", "]", ",", " ");
            key.Should().Be($"{typeof(PagedResultModel<>).FullName!.Split('`')[0]}Of{typeof(SimpleEntity).FullName}");
        }

        [Fact]
        public void GenerateSchema_ShouldRegisterGenericType_UnderIdentifierSafeKey()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var type = typeof(PagedResultModel<SimpleEntity>);

            // Act
            var schema = _schemaGenerator.GenerateSchema(type, context);

            // Assert
            schema.Should().BeOfType<OpenApiSchemaReference>();
            var expectedKey = GeneratorContext.GetSchemaKey(type);
            context.Document.Components!.Schemas.Should().ContainKey(expectedKey);
            // No registered schema key should contain identifier-breaking characters.
            context.Document.Components!.Schemas!.Keys.Should().OnlyContain(k => !k.Contains(' ') && !k.Contains('`'));
        }

        [Fact]
        public void GenerateSchema_ShouldSetFriendlyGenericTitle_ForConstructedGenericType()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var type = typeof(PagedResultModel<SimpleEntity>);

            // Act
            _schemaGenerator.GenerateSchema(type, context);

            // Assert: the schema Title drives the documentation nav label (e.g. Redocly). It should read
            // as angle-bracket generic syntax rather than the raw CLR name (PagedResultModel`1).
            var key = GeneratorContext.GetSchemaKey(type);
            var registered = (OpenApiSchema)context.Document.Components!.Schemas![key];
            registered.Title.Should().Be("PagedResultModel<SimpleEntity>");
            registered.Title.Should().NotContain("`");
        }

        [Fact]
        public void GenerateSchema_ShouldEmitUnconstrainedItems_ForOpenGenericParameter()
        {
            // Arrange: the OPEN generic (T unbound) is what a standalone canonical models
            // library discovers — there is no closed instantiation to bind T. The unbound T
            // previously fell through to the object handler and was emitted as a propertyless
            // object, which downstream renders as `object[]`.
            var context = new GeneratorContext(_testAssembly);
            var type = typeof(PagedResultModel<>);

            // Act
            _schemaGenerator.GenerateSchema(type, context);

            // Assert: the registered schema's Items array carries an unconstrained ("any")
            // element schema — no type, no properties, and not a component reference — so it
            // renders as `any[]` and lets consumers re-type via a thin generic wrapper.
            var key = GeneratorContext.GetSchemaKey(type);
            context.Document.Components!.Schemas.Should().ContainKey(key);
            var registered = (OpenApiSchema)context.Document.Components.Schemas[key];

            registered.Properties.Should().ContainKey("items");
            var items = (OpenApiSchema)registered.Properties["items"];
            items.Type.Should().Be(JsonSchemaType.Array);

            items.Items.Should().BeOfType<OpenApiSchema>();
            var element = (OpenApiSchema)items.Items!;
            element.Type.Should().BeNull();
            element.Properties.Should().BeNullOrEmpty();
        }

        [Fact]
        public void GenerateSchema_ShouldStampGenericVendorExtensions_ForOpenGeneric()
        {
            // Arrange
            var context = new GeneratorContext(_testAssembly);
            var type = typeof(PagedResultModel<>);

            // Act
            _schemaGenerator.GenerateSchema(type, context);

            // Assert
            var key = GeneratorContext.GetSchemaKey(type);
            var registered = (OpenApiSchema)context.Document.Components!.Schemas[key];

            // Type-level: the original type parameter names are recorded so a downstream
            // generator can rebuild `PagedResult<T>` from the erased schema.
            registered.Extensions.Should().NotBeNull();
            registered.Extensions.Should().ContainKey("x-generic-type-parameters");

            // Property-level: the collection-of-T property is marked so the generator restores
            // `Array<T>` in place of the erased element type.
            var items = (OpenApiSchema)registered.Properties["items"];
            items.Extensions.Should().NotBeNull();
            items.Extensions.Should().ContainKey("x-generic-item-ref");

            // The markers must survive serialization, since the downstream codemod reads the emitted
            // spec JSON (not the in-memory model). Serialize via the same path used to produce specs.
            var stringWriter = new System.IO.StringWriter();
            context.Document.SerializeAsV31(new OpenApiJsonWriter(stringWriter));
            var json = stringWriter.ToString();
            json.Should().Contain("x-generic-type-parameters");
            json.Should().Contain("x-generic-item-ref");
        }
    }
}
