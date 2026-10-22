using System.Security.Cryptography;

namespace InventarioActivos.Data;

public static class PasswordHasher
{
    private const int Iteraciones = 100_000;

    public static string Hash(string contrasena)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, salt, Iteraciones, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToHexString(salt)}:{Convert.ToHexString(hash)}";
    }

    public static bool Verificar(string contrasena, string guardado)
    {
        var partes = guardado.Split(':');
        if (partes.Length != 2)
        {
            return false;
        }
        var salt = Convert.FromHexString(partes[0]);
        var esperado = Convert.FromHexString(partes[1]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(contrasena, salt, Iteraciones, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(esperado, actual);
    }
}
