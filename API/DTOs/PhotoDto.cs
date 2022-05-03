namespace API.DTOs
{
    public class PhotoDto
    {
        // Summary:
        // Creates data transfer object for photos. 
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
        public bool IsApproved { get; set; }
    }
}