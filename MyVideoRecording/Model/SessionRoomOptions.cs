namespace LBScreenRecording.Model
{
    public class SessionRoomOptions
    {
        public string roomId { get; set; }
        public Connection connection { get; set; }
    }
    public class Connection
    {
        public string role { get; set; }
        public object data { get; set; }
    }
}
