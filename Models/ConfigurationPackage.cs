using System;
using System.Text;
using System.Threading.Tasks;

namespace RawSync.Models
{
    // This class is only for backup configuration storage (in View Model) or
    // passing configuration parameters from the View Model to the Model for saving
    public class ConfigurationPackage
    {
        public string _RawExtension;
        public string _ProcessedExtension;
        public bool   _DeletePrompt;
        public bool   _RecycleBin;
    }
}
