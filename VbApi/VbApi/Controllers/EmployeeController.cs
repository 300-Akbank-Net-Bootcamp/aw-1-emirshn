using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace VbApi.Controllers;

public class Employee : IValidatableObject
{
    public string Name { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public double HourlySalary { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var minAllowedBirthDate = DateTime.Today.AddYears(-65);
        if (minAllowedBirthDate > DateOfBirth)
        {
            yield return new ValidationResult("Birthdate is not valid.");
        }
    }
}

public class MinLegalSalaryRequiredAttribute : ValidationAttribute
{
    public MinLegalSalaryRequiredAttribute(double minJuniorSalary, double minSeniorSalary)
    {
        MinJuniorSalary = minJuniorSalary;
        MinSeniorSalary = minSeniorSalary;
    }

    public double MinJuniorSalary { get; }
    public double MinSeniorSalary { get; }
    public string GetErrorMessage() => $"Minimum hourly salary is not valid.";

    protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
    {
        var employee = (Employee)validationContext.ObjectInstance;
        var dateBeforeThirtyYears = DateTime.Today.AddYears(-30);
        var isOlderThanThirdyYears = employee.DateOfBirth <= dateBeforeThirtyYears;
        var hourlySalary = (double)value;

        var isValidSalary = isOlderThanThirdyYears ? hourlySalary >= MinSeniorSalary : hourlySalary >= MinJuniorSalary;

        return isValidSalary ? ValidationResult.Success : new ValidationResult(GetErrorMessage());
    }
}

[Route("api/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    private readonly IValidator<Employee> _validator;

    public EmployeeController(IValidator<Employee> validator)
    {
        _validator = validator;
    }

    [HttpPost]
    public IActionResult Post([FromBody] Employee value)
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
public class EmployeeValidator : AbstractValidator<Employee>
{
    public EmployeeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Please enter a name");
        RuleFor(x => x.DateOfBirth).Must(BeValidBirthDate).WithMessage("Birthdate is not valid");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Please enter a valid email address");
        RuleFor(x => x.Phone).Must(BeAValidPhoneNumber).WithMessage("Please enter a valid phone number");
        RuleFor(x => x.HourlySalary).Must(BeValidHourlySalary).WithMessage("Minimum hourly salary is not valid");
    }

    private bool BeValidBirthDate(DateTime birthDate)
    {
        var minAllowedBirthDate = DateTime.Today.AddYears(-65);
        return minAllowedBirthDate <= birthDate;
    }

    private bool BeValidHourlySalary(Employee employee, double hourlySalary)
    {
        var dateBeforeThirtyYears = DateTime.Today.AddYears(-30);
        var isOlderThanThirtyYears = employee.DateOfBirth <= dateBeforeThirtyYears;

        return isOlderThanThirtyYears ? hourlySalary >= 200 : hourlySalary >= 50;
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
