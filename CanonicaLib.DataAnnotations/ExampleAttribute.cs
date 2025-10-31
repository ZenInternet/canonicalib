using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    /// <summary>
    /// Specifies an example type to be used for documentation and schema generation.
    /// This attribute can be applied multiple times to provide different examples.
    /// </summary>
    /// <example>
    /// <code>
    /// public void ProcessData([Example(typeof(PersonExample))] Person person)
    /// {
    ///     // method implementation
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public class ExampleAttribute : Attribute
    {
        /// <summary>
        /// Gets the type that provides example data for the decorated parameter.
        /// </summary>
        /// <value>The type containing example data.</value>
        public Type ExampleType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleAttribute"/> class.
        /// </summary>
        /// <param name="exampleType">The type that provides example data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exampleType"/> is null.</exception>
        public ExampleAttribute(Type exampleType)
        {
            ExampleType = exampleType ?? throw new ArgumentNullException(nameof(exampleType));
        }
    }
}
