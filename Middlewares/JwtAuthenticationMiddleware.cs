using LibraryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryApi.Middlewares
{
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtService _jwtService;
        private readonly IAuthorizationPolicyProvider _policyProvider;

        public JwtAuthenticationMiddleware(RequestDelegate next, JwtService jwtService, IAuthorizationPolicyProvider policyProvider)
        {
            _next = next;
            _jwtService = jwtService;
            _policyProvider = policyProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split("AuthToken=").LastOrDefault();
            // Distinguir entre usuarios autenticados y visitantes
            if (string.IsNullOrEmpty(token))
            {
                // Si no hay token, verificar si es un visitante
                if (IsGuest(context))
                {
                    // Permitir acceso a secciones públicas
                    await _next(context);
                    return;
                }

                // Si no es un visitante, devolver error 401
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            var claimsPrincipal = _jwtService.ValidateToken(token);
            if (claimsPrincipal == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            context.User = claimsPrincipal;
            await _next(context);
        }
        private bool IsGuest(HttpContext context)
            {
                // Obtener la ruta actual
                var currentPath = context.Request.Path.Value;

                // Ruta raíz
                var rootPath = "/";

                // Ruta de vista login
                var loginPath = "/api/Auth/Login";

                // Si la ruta actual es la raíz o la de login, es un visitante
                return currentPath == rootPath || currentPath == loginPath;
            }
    }
}
