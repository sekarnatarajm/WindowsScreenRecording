using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyVideoRecording.Model
{
    public class UserDetail
    {
        public int id { get; set; }
        public string employee_id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string date_of_birth { get; set; }
        public string qualification { get; set; }
        public string date_of_joining { get; set; }
        public int role_id { get; set; }
        public int reporting_role_id { get; set; }
        public int reporting_user_id { get; set; }
        public int is_auditor { get; set; }
        public int is_tutor { get; set; }
        public string skill_categories { get; set; }
        public string contact_no { get; set; }
        public object official_no { get; set; }
        public string locality { get; set; }
        public int status { get; set; }
        public int job_type { get; set; }
        public int students_per_hour { get; set; }
        public string languages { get; set; }
        public string accessible_branches { get; set; }
        public string profile_photo_path { get; set; }
        public object email_verified_at { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string job_type_label { get; set; }
        public string status_label { get; set; }
    }
}
