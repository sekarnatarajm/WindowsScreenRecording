using System;
using MyVideoRecording.Enums;


namespace MyVideoRecording.Model
{
    public class TodayScheduledSessions
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public DateTime TodayDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ClassStatus ClassStatus { get; set; }
        public RecordingStatus RecordingStatus { get; set; }        
    }
    public class ScheduledClass
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public int day_id { get; set; }
        public int branch_id { get; set; }
        public int schedule_for { get; set; }
        public int class_mode { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public int is_peak_hour { get; set; }
        public int students_per_hour { get; set; }
        public int active_students_count { get; set; }
        public int can_accommodate { get; set; }
        public int comp_off_seats { get; set; }
        public int comp_off_occupied { get; set; }
        public int comp_off_vacant { get; set; }
        public object classroom_id { get; set; }
        public int status { get; set; }
        public string class_mode_label { get; set; }
        public string schedule_for_label { get; set; }
        public string status_label { get; set; }
    }
}
