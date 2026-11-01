using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Emprendedoras.Web.Pages.Solicitudes;

// Exige que una casilla esté marcada, en el servidor y en el cliente. [Required] no sirve en
// el cliente porque jquery.validate.unobtrusive lo ignora en las casillas; ver validacion.js.
[AttributeUsage(AttributeTargets.Property)]
public sealed class DebeSerVerdaderoAttribute : ValidationAttribute, IClientModelValidator
{
    public override bool IsValid(object? value) => value is true;

    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes.TryAdd("data-val", "true");
        context.Attributes.TryAdd("data-val-debeserverdadero", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
    }
}
