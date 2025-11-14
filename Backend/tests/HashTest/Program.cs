using System.Security.Cryptography;
using System.Text;

Console.WriteLine("Verificando hashes de contraseñas...");

var password = "Admin@123456";

// Método del test
string testHash = HashPassword(password);
Console.WriteLine($"Hash generado por test: {testHash}");

// Simulando el método AuthService VerifyPassword
bool isValid = VerifyPassword(password, testHash);
Console.WriteLine($"Verificación: {isValid}");

static string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var saltBytes = Encoding.UTF8.GetBytes("AudiSoft_School_Salt_2024");
    var passwordBytes = Encoding.UTF8.GetBytes(password);
    var combinedBytes = new byte[saltBytes.Length + passwordBytes.Length];
    
    Array.Copy(saltBytes, 0, combinedBytes, 0, saltBytes.Length);
    Array.Copy(passwordBytes, 0, combinedBytes, saltBytes.Length, passwordBytes.Length);
    
    var hashBytes = sha256.ComputeHash(combinedBytes);
    return Convert.ToBase64String(hashBytes);
}

static bool VerifyPassword(string password, string hash)
{
    using var sha256 = SHA256.Create();
    var saltBytes = Encoding.UTF8.GetBytes("AudiSoft_School_Salt_2024");
    var passwordBytes = Encoding.UTF8.GetBytes(password);
    var combinedBytes = new byte[saltBytes.Length + passwordBytes.Length];
    
    Array.Copy(saltBytes, 0, combinedBytes, 0, saltBytes.Length);
    Array.Copy(passwordBytes, 0, combinedBytes, saltBytes.Length, passwordBytes.Length);
    
    var hashBytes = sha256.ComputeHash(combinedBytes);
    var passwordHash = Convert.ToBase64String(hashBytes);
    
    return passwordHash == hash;
}
