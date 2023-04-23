using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bookstore.ViewModels
{
    public class ForgottenPasswordViewModel
    {
        [Required(ErrorMessage = "* Email obligatorio")]
        [RegularExpression(@"^.*@.*\..*$",ErrorMessage ="Formato de Email invalido")]
        public String Email { get; set; }
    }
}
