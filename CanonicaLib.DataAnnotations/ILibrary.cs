using System.Collections.Generic;

namespace Zen.CanonicaLib.DataAnnotations
{
  public interface ILibrary
  {
    public string FriendlyName { get; }

    public IList<OpenApiTagGroup>? TagGroups { get; }
  }
}