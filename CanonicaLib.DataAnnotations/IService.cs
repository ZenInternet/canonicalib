using Microsoft.OpenApi;
using System.Collections.Generic;

namespace Zen.CanonicaLib.DataAnnotations
{
    public interface IService
    {
        IList<OpenApiServer>? Servers { get; }
    }
}