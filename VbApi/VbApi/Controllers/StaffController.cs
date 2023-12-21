using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace VbApi.Controllers;

public class Staff
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public decimal? HourlySalary { get; set; }
}

[Route("api/[controller]")]
[ApiController]
public class StaffController : ControllerBase
{
    private readonly IValidator<Staff> _validator;

    public StaffController(IValidator<Staff> validator)
    {
        _validator = validator;
    }

    [HttpPost]
    public IActionResult Post([FromBody] Staff value)
    {
        var validationResult = _validator.Validate(value);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));
        }

        return Ok(value);
    }
}

//hw1 code for validator
public class StaffValidator : AbstractValidator<Staff>
{
    public StaffValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Please enter a name");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Please enter a valid email address");
        RuleFor(x => x.Phone).Must(BeAValidPhoneNumber).WithMessage("Please enter a valid phone number");
        RuleFor(x => x.HourlySalary).NotNull().InclusiveBetween(30, 400).WithMessage("Hourly salary must be between 30 and 400");
    }
    
    private bool BeAValidPhoneNumber(string? phone)
    {
        if (string.IsNullOrEmpty(phone))
        {
            return false;
        }
        if (phone.Length < 7 || phone.Length > 15)
        {
            return false;
        }
        return phone.All(char.IsDigit);
    }
}
