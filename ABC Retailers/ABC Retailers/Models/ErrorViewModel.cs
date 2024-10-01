using System;


namespace ABC_Retailers.Models
{
    //error model will store the error messages in the RequestId variable
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
