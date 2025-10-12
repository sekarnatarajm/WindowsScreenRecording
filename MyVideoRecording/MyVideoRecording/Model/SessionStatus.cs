using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyVideoRecording.Model
{
    public class SessionStatus
    {
        public int Id { get; set; }
        public string TeacherEmail { get; set; }
        public string ClassStatus { get; set; }
        public string RecordingStatus { get; set; }
        public string ClassStartTime { get; set; }
        public string ClassEndTime { get; set; }
        public int ClassDuration { get; set; }
        public string RecordedFileName { get; set; }
        public string RecordedFolderName { get; set; }
        public string ErrorMessage { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
    }
}
