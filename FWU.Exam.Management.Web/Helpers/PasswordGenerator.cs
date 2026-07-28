using System.Security.Cryptography;

namespace FWU.Exam.Management.Web.Helpers;

public static class PasswordGenerator
{
    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Digits = "0123456789";
    private const string Special = "@#$!%*?&";
    private const int DefaultLength = 12;

    public static string Generate(int length = DefaultLength)
    {
        if (length < 8) length = 8;

        var allChars = Lowercase + Uppercase + Digits + Special;

        var password = new char[length];

        password[0] = GetRandomChar(Lowercase);
        password[1] = GetRandomChar(Uppercase);
        password[2] = GetRandomChar(Digits);
        password[3] = GetRandomChar(Special);

        for (int i = 4; i < length; i++)
        {
            password[i] = GetRandomChar(allChars);
        }

        Shuffle(password);

        return new string(password);
    }

    private static char GetRandomChar(string source)
    {
        return source[RandomNumberGenerator.GetInt32(source.Length)];
    }

    private static void Shuffle(char[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
