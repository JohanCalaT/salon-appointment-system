using FluentAssertions;
using FluentValidation.TestHelper;
using SalonAppointmentSystem.ApiService.Application.Validators.Users;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.Shared.DTOs.Users;

namespace SalonAppointmentSystem.ApiService.Tests.Application.Tests.Validators;

/// <summary>
/// Tests para CreateUserValidator
/// </summary>
public class CreateUserValidatorTests
{
    private readonly CreateUserValidator _validator;

    public CreateUserValidatorTests()
    {
        _validator = new CreateUserValidator();
    }

    #region Email Tests

    [Fact]
    public void Email_ShouldFail_WhenEmpty()
    {
        var model = CreateValidRequest() with { Email = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_ShouldFail_WhenInvalidFormat()
    {
        var model = CreateValidRequest() with { Email = "invalid-email" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_ShouldPass_WhenValid()
    {
        var model = CreateValidRequest() with { Email = "valid@email.com" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Password Tests

    [Fact]
    public void Password_ShouldFail_WhenEmpty()
    {
        var model = CreateValidRequest() with { Password = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_ShouldFail_WhenTooShort()
    {
        var model = CreateValidRequest() with { Password = "Ab1", ConfirmPassword = "Ab1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_ShouldFail_WhenNoUppercase()
    {
        var model = CreateValidRequest() with { Password = "password1", ConfirmPassword = "password1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_ShouldFail_WhenNoNumber()
    {
        var model = CreateValidRequest() with { Password = "Password", ConfirmPassword = "Password" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_ShouldPass_WhenValid()
    {
        var model = CreateValidRequest() with { Password = "Password1", ConfirmPassword = "Password1" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region ConfirmPassword Tests

    [Fact]
    public void ConfirmPassword_ShouldFail_WhenDoesNotMatch()
    {
        var model = CreateValidRequest() with { Password = "Password1", ConfirmPassword = "DifferentPassword1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    #endregion

    #region NombreCompleto Tests

    [Fact]
    public void NombreCompleto_ShouldFail_WhenEmpty()
    {
        var model = CreateValidRequest() with { NombreCompleto = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NombreCompleto);
    }

    [Fact]
    public void NombreCompleto_ShouldFail_WhenTooShort()
    {
        var model = CreateValidRequest() with { NombreCompleto = "A" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NombreCompleto);
    }

    #endregion

    #region Rol Tests

    [Fact]
    public void Rol_ShouldFail_WhenEmpty()
    {
        var model = CreateValidRequest() with { Rol = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Rol);
    }

    [Fact]
    public void Rol_ShouldFail_WhenInvalid()
    {
        var model = CreateValidRequest() with { Rol = "InvalidRole" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Rol);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Barbero")]
    [InlineData("Cliente")]
    public void Rol_ShouldPass_WhenValid(string rol)
    {
        var model = CreateValidRequest() with { Rol = rol };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Rol);
    }

    #endregion

    #region EstacionId Tests (For Barbero)

    [Fact]
    public void EstacionId_ShouldFail_WhenBarberoWithoutEstacion()
    {
        var model = CreateValidRequest() with { Rol = ApplicationRoles.Barbero, EstacionId = null };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.EstacionId);
    }

    [Fact]
    public void EstacionId_ShouldPass_WhenBarberoWithEstacion()
    {
        var model = CreateValidRequest() with { Rol = ApplicationRoles.Barbero, EstacionId = 1 };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.EstacionId);
    }

    #endregion

    #region Full Validation

    [Fact]
    public void FullValidation_ShouldPass_WhenAllFieldsValid()
    {
        var model = CreateValidRequest();
        var result = _validator.TestValidate(model);
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static CreateUserRequest CreateValidRequest()
    {
        return new CreateUserRequest
        {
            Email = "test@example.com",
            Password = "Password1",
            ConfirmPassword = "Password1",
            NombreCompleto = "Test User",
            Rol = ApplicationRoles.Cliente,
            Activo = true
        };
    }

    #endregion
}

