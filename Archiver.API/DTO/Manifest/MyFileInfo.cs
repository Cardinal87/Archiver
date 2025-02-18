namespace Archiver.API.DTO.Manifest
{
    public class MyFileInfo
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public int Size { get; set; }
        public string Crc { get; set; } = "";

    }
}
