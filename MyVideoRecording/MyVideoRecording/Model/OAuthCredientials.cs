using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyVideoRecording.Model
{
    public class OAuthCredientials
    {
        public string ClientId { get; set; } = "998935976350-3s0q3nlgp3efv7qmnhke12lvi5vjfhp7.apps.googleusercontent.com";
        public string ClientSecret { get; set; } = "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";
    }
    public class OAuthFileDetail
    {
        public OAuthFileDetail()
        {
            OAuthCredientials = new OAuthCredientials();
        }
        public string FilePath { get; set; }
        public string ParentFolderId { get; set; } = "1RjGyXLSPVMH5Pggq5a3D1cH0I5my8qsa";
        public string Description { get; set; }
        public OAuthCredientials OAuthCredientials { get; set; }
    }
}
