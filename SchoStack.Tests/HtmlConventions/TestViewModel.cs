using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;

namespace SchoStack.Tests.HtmlConventions
{
    public class TestViewModel
    {
        [StringLength(20)]
        public string Name { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string PasswordConfirm { get; set; }

        [DataType(DataType.Text)]
        public string Text { get; set; }

        [HiddenInput]
        public string Hidden { get; set; }

        [DisplayName("Display")]
        public string DisplayName { get; set; }

        public bool IsCorrect { get; set; }

        public List<SelectListItem> Dropdown { get; set; }
        public MultiSelectList MultiSelect { get; set; }
        public int Int { get; set; }
        public decimal Decimal { get; set; }
        public double Double { get; set; }
        public string CreditCard { get; set; }
    }

    public class TestInputModel
    {
        [StringLength(20)]
        public string Name { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string PasswordConfirm { get; set; }

        [DataType(DataType.Text)]
        public string Text { get; set; }

        [HiddenInput]
        public string Hidden { get; set; }

        [DisplayName("Display")]
        public string DisplayName { get; set; }

        public bool IsCorrect { get; set; }

        public List<SelectListItem> Dropdown { get; set; }
        public MultiSelectList MultiSelect { get; set; }
        public int Int { get; set; }
        public decimal Decimal { get; set; }
        public double Double { get; set; }
        public string CreditCard { get; set; }
    }

    public class TestInputValidator : AbstractValidator<TestInputModel>
    {
        public const int NAME_MAXLENGTH = 50;
        public const int NAME_MINLENGTH = 10;

        public TestInputValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .Length(NAME_MINLENGTH, NAME_MAXLENGTH);

            RuleFor(x => x.CreditCard)
                .CreditCard();

            RuleFor(x => x.PasswordConfirm)
                .Equal(x => x.Password);
        }
    }
}