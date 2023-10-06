using System.ComponentModel.DataAnnotations;

namespace FreeCourse.Web.Models
{
    public class SignInInput
    {
        //Display label etiketinde gözüksün.
        [Required]
        [Display(Name = "Email adresiniz")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Şifreniz")]
        public string Password { get; set; }
        [Required]
        [Display(Name = "Beni hatırla")]
        public bool IsRemember { get; set; }
    }
}
