using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloBench
{
    /// <summary>
    /// A class to hold information about a hash mismatch for a single object.
    /// </summary>
    public class MismatchResult
    {
        public string FilePath { get; set; } = "";
        public string ObjectName { get; set; } = "";
        public string ObjectType { get; set; } = "";
        public string BeforeHash { get; set; } = "";
        public string AfterHash { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public bool IsError => !string.IsNullOrEmpty(ErrorMessage);
        public bool IsUnsupported { get; set; } = false;
    }
}
