using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FileMutator.Tools.Interfaces;

namespace FileMutator.Tools
{
    public class MutatorService : IMutatorService
    {
        public string MutateText(string text)
        {
            var result = new StringBuilder(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
            result.AppendLine();
            result.AppendLine(text);
            result.Append(GetSecureRandomString(RandomNumberGenerator.GetInt32(1, 100)));

            return result.ToString();
        }

        public byte[] MutateText(byte[] data)
        {
            var text = Encoding.UTF8.GetString(data);
            return Encoding.UTF8.GetBytes(MutateText(text));
        }

        private string GetSecureRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var result = new char[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                var byteBuffer = new byte[length];

                rng.GetBytes(byteBuffer);

                for (var i = 0; i < length; i++)
                {
                    result[i] = chars[byteBuffer[i] % chars.Length];
                }
            }

            return new string(result);
        }
    }
}
