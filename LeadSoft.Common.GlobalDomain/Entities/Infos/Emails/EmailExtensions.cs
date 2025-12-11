using LeadSoft.Common.Library.Extensions;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace LeadSoft.Common.GlobalDomain.Entities
{
    public static class EmailExtensions
    {
        /// <summary>
        /// Checks if string is an e-mail
        /// </summary>
        /// <param name="aEmail">E-mail Address</param>
        /// <returns>Boolean</returns>
        public static bool IsValidEmail(this string aEmail)
        {
            Regex regex = new(@"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$", RegexOptions.IgnoreCase);

            try
            {
                string email = new MailAddress(aEmail).Address;

                return email.Equals(aEmail) && regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Method available to transform a semicolon separated e-mail from single string to a string array
        /// </summary>
        /// <param name="aString">Self String</param>
        /// <returns>String array</returns>
        public static string[] SplitEmail(this string aString)
        {
            if (string.IsNullOrWhiteSpace(aString))
                return [];

            return aString.Replace(",", ";").Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Mascara um e-mail para exibição (ex.: "eu@lucasrtavares.com.br" -> "e******@***tavares.com.br").
        /// Regras aplicadas:
        ///  - mantém entre 1 e 3 caracteres iniciais do local-part (mínimo 1, máximo 3);
        ///  - substitui o restante do local-part por '*' garantindo pelo menos 6 '*' visíveis;
        ///  - no domínio: substitui o começo do primeiro label por '***' e revela os últimos N caracteres desse primeiro label
        ///    (N = min(7, comprimento do label)). Mantém os demais labels (ex.: ".com.br").
        ///  - se a string não for um e-mail, retorna a própria string (opção conservadora).
        /// </summary>
        public static string OvershadowEmail(this string email)
        {
            email = email.Trim();

            if (email.IsNothing())
                return email;

            int atIndex = email.IndexOf('@');
            if (atIndex <= 0 || atIndex != email.LastIndexOf('@') || atIndex == email.Length - 1)
                return email;

            string local = email[..atIndex];
            string domain = email[(atIndex + 1)..];

            int keepLocal = Math.Min(2, Math.Max(1, local.Length));
            int toMaskLocal = Math.Max(6, local.Length - keepLocal);
            string maskedLocal = local.Substring(0, keepLocal) + new string('*', toMaskLocal);

            string[] labels = domain.Split('.');
            if (labels.Length == 0)
                return maskedLocal + "@" + domain;

            string firstLabel = labels[0];
            int revealSuffix = Math.Min(2, firstLabel.Length);
            string revealedSuffix = firstLabel.Length <= revealSuffix
                ? firstLabel
                : firstLabel[^revealSuffix..];

            string maskedFirstPart = "*****" + revealedSuffix;

            string rest = labels.Length > 1 ? "." + string.Join(".", labels.Skip(1)) : string.Empty;
            string maskedDomain = maskedFirstPart + rest;

            return $"{maskedLocal[..8]}@{maskedDomain}".ToLower();
        }
    }
}
