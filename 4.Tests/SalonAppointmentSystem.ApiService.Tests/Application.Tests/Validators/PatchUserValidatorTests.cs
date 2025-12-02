using FluentAssertions;
using FluentValidation.TestHelper;
using SalonAppointmentSystem.ApiService.Application.Validators.Users;
using SalonAppointmentSystem.Shared.DTOs.Users;

namespace SalonAppointmentSystem.ApiService.Tests.Application.Tests.Validators;

/// <summary>
/// Tests para PatchUserValidator
/// </summary>
public class PatchUserValidatorTests
{
    private readonly PatchUserValidator _validator;

    public PatchUserValidatorTests()
    {
        _validator = new PatchUserValidator();
    }

    #region Email Tests

    [Fact]
    public void Email_ShouldPass_WhenNull()
    {
        var model = new PatchUserRequest { Email = null };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_ShouldFail_WhenInvalidFormat()
    {
        var model = new PatchUserRequest { Email = "invalid-email" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_ShouldPass_WhenValid()
    {
        var model = new PatchUserRequest { Email = "valid@email.com" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Password Tests

    [Fact]
    public void Password_ShouldPass_WhenNull()
    {
        var model = new PatchUserRequest { Password = null };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_ShouldFail_WhenTooShort()
    {
        var model = new PatchUserRequest { Password = "Ab1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_ShouldPass_WhenValid()
    {
        var model = new PatchUserRequest { Password = "Password1" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region Rol Tests

    [Fact]
    public void Rol_ShouldPass_WhenNull()
    {
        var model = new PatchUserRequest { Rol = null };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Rol);
    }

    [Fact]
    public void Rol_ShouldFail_WhenInvalid()
    {
        var model = new PatchUserRequest { Rol = "InvalidRole" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Rol);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Barbero")]
    [InlineData("Cliente")]
    public void Rol_ShouldPass_WhenValid(string rol)
    {
        var model = new PatchUserRequest { Rol = rol };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Rol);
    }

    #endregion

    #region PuntosAcumulados Tests

    [Fact]
    public void PuntosAcumulados_ShouldPass_WhenNull()
    {
        var model = new PatchUserRequest { PuntosAcumulados = null };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.PuntosAcumulados);
    }

    [Fact]
    public void PuntosAcumulados_ShouldFail_WhenNegative()
    {
        var model = new PatchUserRequest { PuntosAcumulados = -1 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PuntosAcumulados);
    }

    [Fact]
    public void PuntosAcumulados_ShouldPass_WhenZeroOrPositive()
    {
        var model = new PatchUserRequest { PuntosAcumulados = 0 };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.PuntosAcumulados);
    }

    #endregion

    #region Full Validation

    [Fact]
    public void FullValidation_ShouldPass_WhenEmptyRequest()
    {
        var model = new PatchUserRequest();
        var result = _validator.TestValidate(model);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void FullValidation_ShouldPass_WhenAllFieldsValid()
    {
        var model = new PatchUserRequest
        {
            Email = "test@email.com",
            NombreCompleto = "Updated Name",
            Rol = "Admin",
            Activo = true,
            PuntosAcumulados = 100
        };
        var result = _validator.TestValidate(model);
        result.IsValid.Should().BeTrue();
    }

    #endregion
}

