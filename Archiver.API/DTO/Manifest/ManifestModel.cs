namespace Archiver.API.DTO.Manifest
{
    public class ManifestModel
    {
        public ArchiveInfo ArchiveInfo { get; set; } = new();
        public List<MyFileInfo> Content { get; set; } = new();
    }
}
