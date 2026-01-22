using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Vertex.Application.DTOs;
using Vertex.Domain.Entities;

namespace Vertex.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema
    /// </summary>
    /// <param name="registerDto">Datos del nuevo usuario</param>
    /// <returns>Respuesta con el token JWT</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            // Validar entrada
            if (string.IsNullOrWhiteSpace(registerDto.Email) || 
                string.IsNullOrWhiteSpace(registerDto.Password) ||
                string.IsNullOrWhiteSpace(registerDto.FullName))
            {
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(
                    "Todos los campos son requeridos", 
                    400));
            }

            // Verificar si el usuario ya existe
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(
                    "El email ya está registrado", 
                    400));
            }

            // Crear nuevo usuario
            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FullName = registerDto.FullName
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(
                    "Error al crear el usuario", 
                    400, 
                    errors));
            }

            _logger.LogInformation("Usuario {Email} registrado exitosamente", registerDto.Email);

            // Generar token JWT
            var token = GenerateJwtToken(user);

            var authResponse = new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                FullName = user.FullName,
                ExpiresAt = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["JwtSettings:DurationInMinutes"]!))
            };

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(
                authResponse, 
                "Usuario registrado exitosamente", 
                201));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el registro");
            return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResponse(
                "Error interno del servidor", 
                500, 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Inicia sesión en el sistema
    /// </summary>
    /// <param name="loginDto">Credenciales del usuario</param>
    /// <returns>Respuesta con el token JWT</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            // Validar entrada
            if (string.IsNullOrWhiteSpace(loginDto.Email) || 
                string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(
                    "Email y contraseña son requeridos", 
                    400));
            }

            // Buscar usuario por email
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse(
                    "Credenciales inválidas", 
                    401));
            }

            // Verificar contraseña
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Intento de login fallido para {Email}", loginDto.Email);
                return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse(
                    "Credenciales inválidas", 
                    401));
            }

            _logger.LogInformation("Usuario {Email} inició sesión exitosamente", loginDto.Email);

            // Generar token JWT
            var token = GenerateJwtToken(user);

            var authResponse = new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                FullName = user.FullName,
                ExpiresAt = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["JwtSettings:DurationInMinutes"]!))
            };

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(
                authResponse, 
                "Login exitoso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el login");
            return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResponse(
                "Error interno del servidor", 
                500, 
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Genera un token JWT para el usuario autenticado
    /// </summary>
    /// <param name="user">Usuario autenticado</param>
    /// <returns>Token JWT como string</returns>
    private string GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Key"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var durationInMinutes = int.Parse(jwtSettings["DurationInMinutes"]!);

        // Claims críticos del token
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id), // ID del usuario (CRÍTICO)
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID único del token
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()) // Issued at
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(durationInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
