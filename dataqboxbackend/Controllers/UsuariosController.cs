using System.Text.Json;
using Microsoft.Data.SqlClient;
using System.Data;
using StackExchange.Redis;
using dataqboxbackend.Data;
using dataqboxbackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace dataqboxbackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConnectionMultiplexer _redis;
        private readonly IConfiguration _configuration; // Configuración para obtener cadenas de conexión

        public UsuariosController(AppDbContext context, IConnectionMultiplexer redis, IConfiguration configuration)
        {
            _context = context;
            _redis = redis;
            _configuration = configuration;
        }

        // GET: api/usuarios
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var db = _redis.GetDatabase();
            string cacheKey = "usuarios_cache";

            try
            {
                // Intentar obtener los datos de Redis
                var cachedData = db.StringGet(cacheKey);
                if (cachedData.HasValue)
                {
                    var usuariosCache = JsonSerializer.Deserialize<List<Usuario>>(cachedData!) ?? new List<Usuario>();
                    return Ok(new { Data = usuariosCache, TotalCount = usuariosCache.Count });
                }

                // Si no hay datos en Redis, consultar la base de datos
                var connectionString = _configuration.GetConnectionString("DefaultConnection"); // Obtener la conexión
                List<Usuario> usuarios = new();
                int totalUsuarios = 0;

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("GetAllUsuarios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // Leer los datos en un DataTable
                            var dataTable = new DataTable();
                            dataTable.Load(reader);

                            // Mapear el DataTable a la lista de objetos Usuario
                            usuarios = dataTable.AsEnumerable().Select(row => new Usuario
                            {
                                Cod_Usuario = row["Cod_Usuario"]?.ToString() ?? string.Empty,
                                Nombre = row["Nombre"]?.ToString() ?? string.Empty,
                                Email = row["Email"]?.ToString() ?? string.Empty,
                                Password = row["Password"]?.ToString() ?? string.Empty
                            }).ToList();

                            // Leer el segundo conjunto de resultados (total de usuarios)
                            if (await reader.NextResultAsync() && reader.Read())
                            {
                                totalUsuarios = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                            }
                        }
                    }
                }

                if (!usuarios.Any())
                {
                    return NotFound(new
                    {
                        StatusCode = 404,
                        Message = "No se encontraron usuarios en la base de datos."
                    });
                }

                // Almacenar los datos en Redis
                db.StringSet(cacheKey, JsonSerializer.Serialize(usuarios), TimeSpan.FromMinutes(30));

                return Ok(new { Data = usuarios, TotalCount = totalUsuarios });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetUsuarios: {ex.Message}");

                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Error al procesar la solicitud.",
                    Detailed = ex.Message
                });
            }
        }

        // GET: api/usuarios/getall
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var db = _redis.GetDatabase();
            string cacheKey = $"usuarios_page_{pageNumber}_size_{pageSize}";

            try
            {
                // Intentar obtener los datos de Redis
                var cachedData = db.StringGet(cacheKey);
                if (cachedData.HasValue)
                {
                    var usuariosCache = JsonSerializer.Deserialize<List<Usuario>>(cachedData!) ?? new List<Usuario>();
                    int totalUsuariosCache = usuariosCache.Count;

                    return Ok(new
                    {
                        TotalCount = totalUsuariosCache,
                        TotalPages = (int)Math.Ceiling((double)totalUsuariosCache / pageSize),
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        Data = usuariosCache.Skip((pageNumber - 1) * pageSize).Take(pageSize)
                    });
                }

                // Si no hay datos en Redis, consultar la base de datos
                var connectionString = _configuration.GetConnectionString("DefaultConnection"); // Obtener la conexión
                List<Usuario> usuarios = new();
                int totalUsuarios = 0;

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("GetAllUsuarios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // Leer los datos en un DataTable
                            var dataTable = new DataTable();
                            dataTable.Load(reader);

                            // Mapear el DataTable a la lista de objetos Usuario
                            usuarios = dataTable.AsEnumerable().Select(row => new Usuario
                            {
                                Cod_Usuario = row["Cod_Usuario"]?.ToString() ?? string.Empty,
                                Nombre = row["Nombre"]?.ToString() ?? string.Empty,
                                Email = row["Email"]?.ToString() ?? string.Empty,
                                Password = row["Password"]?.ToString() ?? string.Empty
                            }).ToList();

                            // Leer el segundo conjunto de resultados (total de usuarios)
                            if (await reader.NextResultAsync() && reader.Read())
                            {
                                totalUsuarios = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                            }
                        }
                    }
                }

                if (!usuarios.Any())
                {
                    return NotFound(new
                    {
                        StatusCode = 404,
                        Message = "No se encontraron usuarios en la base de datos."
                    });
                }

                // Almacenar los datos en Redis
                db.StringSet(cacheKey, JsonSerializer.Serialize(usuarios), TimeSpan.FromMinutes(30));

                // Paginación
                var paginatedData = usuarios.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                return Ok(new
                {
                    TotalCount = totalUsuarios,
                    TotalPages = (int)Math.Ceiling((double)totalUsuarios / pageSize),
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    Data = paginatedData
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetAll: {ex.Message}");

                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Error al procesar la solicitud.",
                    Detailed = ex.Message
                });
            }
        }

        // POST: api/usuarios
        [HttpPost]
        public IActionResult CreateUsuario([FromBody] UserCreateDto userCreate)
        {
            try
            {
                // Verificar si el correo electrónico ya existe
                var existingUser = _context.Usuarios.SingleOrDefault(u => u.Email == userCreate.Email);
                if (existingUser != null)
                    return BadRequest("El correo electrónico ya está en uso.");

                // Crear un nuevo usuario
                var usuario = new Usuario
                {
                    Cod_Usuario = Guid.NewGuid().ToString(),
                    Nombre = userCreate.Nombre,
                    Email = userCreate.Email
                };

                // Encriptar la contraseña
                var passwordHasher = new PasswordHasher<Usuario>();
                usuario.Password = passwordHasher.HashPassword(usuario, userCreate.Password);

                // Guardar el usuario en la base de datos
                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                // Eliminar el cache general de usuarios
                var db = _redis.GetDatabase();
                db.KeyDelete("usuarios_cache");

                return Ok(new { Message = "Usuario creado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Error al procesar la solicitud.",
                    Detailed = ex.Message
                });
            }
        }

        // PUT: api/usuarios/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateUsuario(string id, [FromBody] UserUpdateDto userUpdate)
        {
            try
            {
                var usuario = _context.Usuarios.SingleOrDefault(u => u.Cod_Usuario == id);
                if (usuario == null)
                    return NotFound("Usuario no encontrado.");

                // Verificar si el correo electrónico ya existe (excluyendo el usuario actual)
                var existingUser = _context.Usuarios
                    .SingleOrDefault(u => u.Email == userUpdate.Email && u.Cod_Usuario != id);
                if (existingUser != null)
                    return BadRequest("El correo electrónico ya está en uso.");

                // Actualizar los campos del usuario
                usuario.Nombre = userUpdate.Nombre;
                usuario.Email = userUpdate.Email;

                // Encriptar la nueva contraseña si se proporciona
                if (!string.IsNullOrEmpty(userUpdate.Password))
                {
                    var passwordHasher = new PasswordHasher<Usuario>();
                    usuario.Password = passwordHasher.HashPassword(usuario, userUpdate.Password);
                }

                // Guardar los cambios en la base de datos
                _context.SaveChanges();

                // Eliminar el cache general de usuarios
                var db = _redis.GetDatabase();
                db.KeyDelete("usuarios_cache");

                return Ok(new { Message = "Usuario actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Error al procesar la solicitud.",
                    Detailed = ex.Message
                });
            }
        }

        // DELETE: api/usuarios/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(string id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                    return NotFound();

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                // Eliminar el cache general de usuarios
                var db = _redis.GetDatabase();
                db.KeyDelete("usuarios_cache");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Error al procesar la solicitud.",
                    Detailed = ex.Message
                });
            }
        }

        // DTOs
        public class UserCreateDto
        {
            public string Nombre { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class UserUpdateDto
        {
            public string Nombre { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty; // Opcional
        }
    }
}
