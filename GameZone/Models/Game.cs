namespace GameZone.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        // Add other properties as needed (e.g., ImageUrl, BranchId, etc.)
        public byte[] ImageData { get; set; } // Store image as byte array
        public string ImageContentType { get; set; }
    }
}