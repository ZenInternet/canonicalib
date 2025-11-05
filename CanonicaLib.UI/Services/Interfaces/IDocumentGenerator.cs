using Microsoft.OpenApi;
using System.Reflection;

namespace Zen.CanonicaLib.UI.Services.Interfaces
{
    public interface IDocumentGenerator
    {
        GeneratorContext GenerateDocument(Assembly assembly);
    }
}