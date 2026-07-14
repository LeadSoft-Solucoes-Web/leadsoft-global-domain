using System.Text.RegularExpressions;

namespace LeadSoft.Common.GlobalDomain.Entities
{
    public static partial class PhoneExtensions
    {
        /// <summary>
        /// Validate Phone Contact. Pattern: (xx) xxxxx-xxxx
        /// </summary>
        public static bool IsValidPhone(this string phone)
        {
            Regex regex = new(@"^\(?[1-9]{2}\)? ?(?:[2-9]|9[1-9])[0-9]{3}\-?[0-9]{4}$");//(@"^\([1-9]{2}\) (?:[2-9]|9[1-9])[0-9]{3}\-[0-9]{4}$");

            try
            {
                return regex.IsMatch(phone.Trim());
            }
            catch
            {
                return false;
            }
        }
        public static bool IsValidWhatsAppPhone(this string phone)
        {
            Regex regex = new(@"^\+?[1-9][0-9]{6,14}$");

            try
            {
                return regex.IsMatch(phone.Trim());
            }
            catch
            {
                return false;
            }
        }


    }
}
