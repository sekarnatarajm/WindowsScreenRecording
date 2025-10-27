using Newtonsoft.Json;
using NLog.Layouts;

namespace MyVideoRecording.Model
{
    public class VideoStateChange
    {
        public bool Muted { get; set; }
        public string PeerId { get; set; }
        public string Role { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
