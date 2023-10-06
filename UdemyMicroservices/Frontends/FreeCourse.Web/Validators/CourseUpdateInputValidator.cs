using FluentValidation;
using FreeCourse.Web.Models.Catalog;

namespace FreeCourse.Web.Validators
{
    public class CourseUpdateInputValidator : AbstractValidator<CourseUpdateInput>
    {
        public CourseUpdateInputValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("İsim alanı boş olamaz");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama alanı boş olamaz");
            //int alanı null olamaz yapabilmek için.
            RuleFor(x => x.Feature.Duration).InclusiveBetween(1, int.MaxValue).WithMessage("Süre alanı boş olamaz");
            //virgülden önce toplam 4 karakter virgülden sonra 2 karakter kullanabilsin ama toplamı 6 karakter olacak.
            //$$$$.$$
            RuleFor(x => x.Price).NotEmpty().WithMessage("Fiyat alanı boş olamaz").ScalePrecision(2, 6).WithMessage("Hatalı fiyat formatı");
        }
    }
}
