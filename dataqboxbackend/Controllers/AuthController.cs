using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using dataqboxbackend.Models;
using dataqboxbackend.Data;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;


[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public AuthController(IConfiguration configuration, AppDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    // Método para registrar un nuevo usuario
    [HttpPost("register")]
    [HideInProduction]
    public IActionResult Register([FromBody] UserRegisterDto userRegister)
    {
        // Verificar si el correo electrónico ya existe
        var existingUser = _context.Usuarios.SingleOrDefault(u => u.Email == userRegister.Email);
        if (existingUser != null)
            return BadRequest("El correo electrónico ya está en uso.");

        // Verificar si el código de usuario ya existe
        var existingCodUsuario = _context.Usuarios.SingleOrDefault(u => u.Cod_Usuario == userRegister.Cod_Usuario);
        if (existingCodUsuario != null)
            return BadRequest("El código de usuario ya está en uso.");

        // Crear un nuevo usuario
        var usuario = new Usuario
        {
            Cod_Usuario = userRegister.Cod_Usuario,
            Nombre = userRegister.Nombre,
            Email = userRegister.Email
        };

        // Encriptar la contraseña
        var passwordHasher = new PasswordHasher<Usuario>();
        usuario.Password = passwordHasher.HashPassword(usuario, userRegister.Password);

        // Guardar el usuario en la base de datos
        _context.Usuarios.Add(usuario);
        _context.SaveChanges();

        // Enviar correo de confirmación
        //SendConfirmationEmail(usuario.Email);

        return Ok(new { Message = "Usuario creado exitosamente. Se ha enviado un correo de confirmación." });
    }

    // Método para iniciar sesión
    [HttpPost("login")]
    [HideInProduction]
    public IActionResult Login([FromBody] UserLoginDto userLogin)
    {
        var user = _context.Usuarios.SingleOrDefault(u => u.Email == userLogin.Email);
        if (user == null)
            return Unauthorized();

        var passwordHasher = new PasswordHasher<Usuario>();
        var result = passwordHasher.VerifyHashedPassword(user, user.Password, userLogin.Password);

        if (result == PasswordVerificationResult.Failed)
            return Unauthorized();

        // Almacenar el usuario en la sesión
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserName", user.Nombre);

        // Generar el token JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = _configuration["Jwt:Key"] ?? string.Empty;

        if (string.IsNullOrEmpty(key) || key.Length < 32)
        {
            return StatusCode(500, new { Message = "Error en la configuración: la clave JWT es inválida." });
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.Name, user.Nombre),
        new Claim(ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Ok(new { Token = tokenHandler.WriteToken(token) });

    }

    // Método para enviar correo de confirmación
    private void SendConfirmationEmail(string email)
    {
        var fromAddress = new MailAddress("tuemail@dominio.com", "Tu Nombre");
        var toAddress = new MailAddress(email);
        const string fromPassword = "tu_contraseña"; // Asegúrate de usar una contraseña segura
        const string subject = "Confirmación de registro";
        string body = "Gracias por registrarte. Por favor, confirma tu correo haciendo clic en el siguiente enlace: [enlace de confirmación]";

        var smtp = new SmtpClient
        {
            Host = "smtp.dominio.com", // Cambia esto por tu servidor SMTP
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        };
        using (var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body
        })
        {
            smtp.Send(message);
        }
    }

    // Método para restablecer la contraseña
    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordDto model)
    {
        // Validar el token
        var resetToken = _context.PasswordResetTokens
            .SingleOrDefault(t => t.Token == model.Token && t.UserEmail == model.Email);

        if (resetToken == null || resetToken.Expiration < DateTime.UtcNow)
            return BadRequest("El token es inválido o ha expirado.");

        var user = _context.Usuarios.SingleOrDefault(u => u.Email == model.Email);
        if (user == null)
            return BadRequest("El correo electrónico no está registrado.");

        // Encriptar la nueva contraseña
        var passwordHasher = new PasswordHasher<Usuario>();
        user.Password = passwordHasher.HashPassword(user, model.NewPassword);

        // Guardar los cambios en la base de datos
        _context.SaveChanges();

        return Ok(new { Message = "La contraseña ha sido restablecida exitosamente." });
    }

    // Método para iniciar sesión con Google
    [HttpGet("login/google")]
    public IActionResult LoginWithGoogle()
    {
        var redirectUrl = Url.Action("GoogleResponse", "Auth");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    // Método para manejar la respuesta de Google
    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded || result.Principal == null)
            return BadRequest("Error en la autenticación de Google.");

        var email = result.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var name = result.Principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        // Verificar si el usuario ya existe en la base de datos
        var user = _context.Usuarios.SingleOrDefault(u => u.Email == email);
        if (user == null)
        {
            // Crear un nuevo usuario
            user = new Usuario
            {
                Cod_Usuario = Guid.NewGuid().ToString(), // Generar un nuevo código de usuario
                Nombre = name,
                Email = email,
                Password = string.Empty, // O asigna una contraseña predeterminada si es necesario
                Tipo = "Google" // Puedes usar este campo para indicar que el usuario se registró con Google
            };
            _context.Usuarios.Add(user);
            _context.SaveChanges();
        }

        // Generar el token JWT
        var token = GenerateJwtToken(user);
        return Ok(new { Message = "Usuario autenticado exitosamente.", Token = token });
    }

    // Método para generar el token JWT
    private string GenerateJwtToken(Usuario user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // DTOs
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.", MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código de usuario es obligatorio.")]
        public string Cod_Usuario { get; set; } = string.Empty;
    }

    public class UserLoginDto
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "El token es obligatorio.")]
        public string Token { get; set; } = string.Empty;
    }
}
