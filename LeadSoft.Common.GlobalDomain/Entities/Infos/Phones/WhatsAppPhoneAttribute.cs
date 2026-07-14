using System.ComponentModel.DataAnnotations;

namespace LeadSoft.Common.GlobalDomain.Entities.Infos.Phones
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class WhatsAppPhoneAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is null)
                return true;

            string str = value.ToString()!;

            if (str.IsValidWhatsAppPhone())
                return true;

            ErrorMessage ??= "Número de WhatsApp inválido.";
            return false;
        }
    }
}
