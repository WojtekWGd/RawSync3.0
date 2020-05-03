using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawSync.Models
{
    public class FileDescriptorModel
    {
		public string FilePath { get; set; }

		//This property has dirrent source (not from FilePath but is given while building the list)
		public int Id { get; set; }

		public string FullName { get { return (Path.GetFileName(FilePath)); } }
		public string Name { get { return (Path.GetFileNameWithoutExtension(FilePath)); } }
		public string Extension { get { return (Path.GetExtension(FilePath)); } }
		public string TimeStamp { get { return (File.GetLastWriteTime(FilePath).ToString()); } }
		
		// This property has different source (not from FilePath, but from Program Settings...
		public bool IsQualified { get; set; }

		//This property has dirrent source (not from FilePath but is given while building the list)
		public bool BelongsToDelta { get; set; }

		public FileDescriptorModel()
		{
			FilePath = "";
		}

    }

}