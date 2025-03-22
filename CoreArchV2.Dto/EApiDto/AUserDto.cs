using System.ComponentModel.DataAnnotations;

namespace CoreArchV2.Dto.EApiDto
{
    public class AUserDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} alanı gereklidir")]
        public string Name { get; set; }

        [Required(ErrorMessage = "{0} alanı gereklidir")]
        public string Surname { get; set; }
    }
}