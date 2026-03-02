using NpgsqlTypes; 

namespace ISTTP_lab_1.Models
{
    public enum OsEnum
    {
        [PgName("windows")]
        Windows,

        [PgName("linux")]
        Linux,

        [PgName("macos")]
        MacOS
    }

    public enum RequirementType
    {
        [PgName("minimal")]
        Minimal,

        [PgName("recommended")]
        Recommended,

        [PgName("ultra")]
        Ultra
    }
}
