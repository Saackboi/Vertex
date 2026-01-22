using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Vertex.Application.DTOs;
using Vertex.Application.Interfaces;
using Vertex.Domain.Entities;

namespace Vertex.Application.Services;

/// <summary>
/// Servicio de autenticación que maneja registro, login y generación de tokens JWT.
/// Encapsula toda la lógica de negocio relacionada con autenticación.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator tokenGenerator,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema
    /// </summary>
    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            // 1. VALIDACIÓN DE ENTRADA
            if (string.IsNullOrWhiteSpace(registerDto.Email) || 
                string.IsNullOrWhiteSpace(registerDto.Password) ||
                string.IsNullOrWhiteSpace(registerDto.FullName))
            {
                _logger.LogWarning("Intento de registro con datos incompletos");
                return ApiResponse<AuthResponseDto>.ErrorResponse(
                    "Todos los campos son requeridos", 
                    400);
            }

            // 2. VERIFICAR SI EL USUARIO YA EXISTE
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Intento de registro con email duplicado: {Email}", registerDto.Email);
                return ApiResponse<AuthResponseDto>.ErrorResponse(
                    "El email ya está registrado", 
                    400);
            }

            // 3. CREAR NUEVO USUARIO
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
                _logger.LogWarning("Error al crear usuario {Email}: {Errors}", 
                    registerDto.Email, string.Join(", ", errors));
                
                return ApiResponse<AuthResponseDto>.ErrorResponse(
                    "Error al crear el usuario", 
                    400, 
                    errors);
            }

            _logger.LogInformation("Usuario {Email} registrado exitosamente", registerDto.Email);

            // 4. GENERAR TOKEN JWT
            var tokenResponse = _tokenGenerator.GenerateToken(user);

            // 5. CONSTRUIR RESPUESTA
            var authResponse = new AuthResponseDto
            {
                Token = tokenResponse.Token,
                Email = user.Email!,
                FullName = user.FullName,
                ExpiresAt = tokenResponse.ExpiresAt
            };

            return ApiResponse<AuthResponseDto>.SuccessResponse(
                authResponse, 
                "Usuario registrado exitosamente", 
                201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante el registro");
            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "Error interno del servidor", 
                500, 
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Inicia sesión en el sistema
    /// </summary>
    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        try
        {
            // 1. VALIDACIÓN DE ENTRADA
            if (string.IsNullOrWhiteSpace(loginDto.Email) || 
                string.IsNullOrWhiteSpace(loginDto.Password))
            {
                _logger.LogWarning("Intento de login con credenciales vacías");
                return ApiResponse<AuthResponseDto>.ErrorResponse(
                    "Email y contraseña son requeridos", 
                    400);
            }

            // 2. BUSCAR USUARIO POR EMAIL
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Intento de login con email no registrado: {Email}", loginDto.Email);
                return ApiResponse<AuthResponseDto>.ErrorResponse(
                    "Credenciales inválidas", 
                    401);
            }

            // 3. VERIFICAR CONTRASEÑA
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Intento de login fallido para {Email}: contraseña incorrecta", loginDto.Email);
                return ApiResponse<AuthResponseDto>.ErrorResponse(
                    "Credenciales inválidas", 
                    401);
            }

            _logger.LogInformation("Usuario {Email} inició sesión exitosamente", loginDto.Email);

            // 4. GENERAR TOKEN JWT
            var tokenResponse = _tokenGenerator.GenerateToken(user);

            // 5. CONSTRUIR RESPUESTA
            var authResponse = new AuthResponseDto
            {
                Token = tokenResponse.Token,
                Email = user.Email!,
                FullName = user.FullName,
                ExpiresAt = tokenResponse.ExpiresAt
            };

            return ApiResponse<AuthResponseDto>.SuccessResponse(
                authResponse, 
                "Login exitoso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante el login");
            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "Error interno del servidor", 
                500, 
                new List<string> { ex.Message });
        }
    }
}
