using System.ComponentModel.DataAnnotations;

namespace LeadSoft.Common.GlobalDomain.Entities.Infos.Documents
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class CNPJAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is null)
                return true;

            string str = value.ToString()!;

            if (str.IsCnpj())
                return true;

            ErrorMessage ??= "CNPJ inválido.";
            return false;
        }
    }
}
