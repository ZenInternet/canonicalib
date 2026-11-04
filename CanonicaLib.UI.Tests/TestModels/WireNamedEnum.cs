using System.Text.Json.Serialization;

namespace Zen.CanonicaLib.UI.Tests.TestModels
{
    /// <summary>
    /// An enum whose wire values differ from its member names, as contract enums commonly do.
    /// </summary>
    public enum WireNamedEnum
    {
        [JsonStringEnumMemberName("data-collection")]
        DataCollection = 0,

        [JsonStringEnumMemberName("version_conflict")]
        VersionConflict = 1,

        Plain = 2,

        Alias = 2,

        [JsonStringEnumMemberName("unknown")]
        Unknown = int.MaxValue,
    }
}
