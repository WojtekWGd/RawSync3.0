using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawSync.Models
{
    public class DeletionProgressReportModel
    {
        private int _forDeleteNo = 0;

        public FileDescriptorModel CurrentFd = null;

        public int DeletedFilesNo { get; set; } = 0;
        public int DeletionProgress
        {
            get
            {
                return DeletedFilesNo * 100 / _forDeleteNo;

            }

        }

        public DeletionProgressReportModel(int forDeleteNo)
        {
            _forDeleteNo = forDeleteNo;
        }
    }
}
