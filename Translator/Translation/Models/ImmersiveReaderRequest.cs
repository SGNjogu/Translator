using System;
using System.Collections.Generic;
using System.Text;

namespace Translation.Models
{
    public class ImmersiveReaderRequest
    {
        public string Content { get; set; } = "";
        public string LanguageCode { get; set; } = "en-us";
    }
}
