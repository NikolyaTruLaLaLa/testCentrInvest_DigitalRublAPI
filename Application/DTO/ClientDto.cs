namespace Application.DTO
{
    public class ClientDto
    {
        // не private ?
        public Guid Id { get; set; }
        public string Mid { get; set; }
        public string FullName { get; set; }
        public string? ParticipantDRId { get; set; }

    }
}
