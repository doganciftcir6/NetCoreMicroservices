using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace FreeCourse.Web.Models.Catalog
{
    public class FeatureViewModel
    {
        [Display(Name = "Kurs Süresi")]
        public int Duration { get; set; }
    }
}
