using System.ComponentModel.DataAnnotations;
using Archiver.API.DTO;
namespace Archiver.API.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private string[] _extensions;

        public AllowedExtensionsAttribute(string extensions)
        {
            _extensions = extensions.Split();
        }
        public override bool IsValid(object? value)
        {
            if (value is List<MyFileOptions> list)
            {
                
                foreach(var file in list.Select(x => x.File))
                {
                    if (file != null && file.Length > 0) 
                    {
                        var ext = Path.GetExtension(file.FileName);
                        if (String.IsNullOrEmpty(ext))
                        {
                            ErrorMessage = "File has no extension";
                            return false;
                        }
                        if (!_extensions.Any(x => ext.Substring(1) == x))
                        {
                            ErrorMessage = $"Invalid extension {ext}";
                            return false;
                        }
                    }
                }
                return true;
                
            }
            return false;
        }


    }
}
