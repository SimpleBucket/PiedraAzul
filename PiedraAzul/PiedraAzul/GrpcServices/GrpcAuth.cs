using Google.Protobuf.Collections;
using Grpc.Core;
using PiedraAzul.ApplicationServices.Services;
using PiedraAzul.Data;
using PiedraAzul.Shared.Grpc;
using Shared.Grpc;
using System.Data;

namespace PiedraAzul.GrcpServices
{
    public class GrpcAuth(IUserService userService, IJwtTokenService jwtTokenService, IRefreshTokenService refresh) :  AuthService.AuthServiceBase
    {
        public override async Task<AuthResponse> Login(LoginRequest request, ServerCallContext context)
        {
            var userAndRoles = await userService.Login(request.Email, request.Password);
            var user = userAndRoles.Item1;
            var roles = userAndRoles.Item2;

            if (user == null)  throw new RpcException(new Status(StatusCode.Unauthenticated, "Las credenciales no son correctas, intentalo de nuevo"));
            var accessToken = await jwtTokenService.CreateTokenAsync(user);
            var refreshToken = await refresh.GenerateRefreshTokenAsync(user.Id);

            var UserResponse = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                BirthDate = user.BirthDate.Value.ToShortDateString() ?? string.Empty,
                Gender = (int)user.Gender,
                IdentificationNumber = user.IdentificationNumber,
            };
            UserResponse.Roles.AddRange(roles);

            return new AuthResponse
            {
                User = UserResponse,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        public override async Task<AuthResponse> RefreshToken(RefreshTokenRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.RefreshToken)) throw new RpcException(new Status(StatusCode.InvalidArgument, "El token de refresco no puede estar vacio"));

            var storedToken = await refresh.ValidateRefreshTokenAsync(request.RefreshToken);

            if (storedToken == null) throw new RpcException(new Status(StatusCode.Unauthenticated, "El token de refresco no es valido"));
            
            var user = storedToken.User;
            var roles = await userService.GetRolesByUser(user);

            var newRefreshToken = await refresh.RotateRefreshTokenAsync(storedToken);
            var acessToken = await jwtTokenService.CreateTokenAsync(user);

            var UserResponse = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                BirthDate = user.BirthDate.Value.ToShortDateString() ?? string.Empty,
                Gender = (int)user.Gender,
                IdentificationNumber = user.IdentificationNumber,
            };
            UserResponse.Roles.AddRange(roles);

            return new AuthResponse
            {
                User = UserResponse,
                AccessToken = acessToken,
                RefreshToken = newRefreshToken
            };
        }
        public override async Task<UserResponse> GetCurrentUser(Empty request, ServerCallContext context)
        {
            var auth = context.RequestHeaders.FirstOrDefault(h => h.Key == "authorization")?.Value;
            var jwt = auth?.StartsWith("Bearer ") == true ? auth.Substring("Bearer ".Length) : null;
            if (string.IsNullOrEmpty(jwt)) throw new RpcException(new Status(StatusCode.Unauthenticated, "No se proporcionó un token de acceso"));

            var userId = await jwtTokenService.GetUserIdByToken(jwt);
            if (userId == null) throw new RpcException(new Status(StatusCode.Unauthenticated, "Token de acceso no válido"));

            var user = await userService.GetById(userId);
            if (user == null) throw new RpcException(new Status(StatusCode.Unauthenticated, "Usuario no encontrado"));
            var roles = await userService.GetRolesByUser(user);

            var UserResponse = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                BirthDate = user.BirthDate.Value.ToShortDateString() ?? string.Empty,
                Gender = (int)user.Gender,
                IdentificationNumber = user.IdentificationNumber,
            };
            UserResponse.Roles.AddRange(roles);

            return UserResponse;
        }
        public override Task<Empty> RevokeToken(RevokeTokenRequest request, ServerCallContext context)
        {
            return base.RevokeToken(request, context);
        }
        public override async Task<AuthResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            ApplicationUser userRequest = new ApplicationUser
            {
                Email = request.Email,
                IdentificationNumber = request.IdentificationNumber,
                PhoneNumber = request.Phone,
                Gender = (Shared.Enums.GenderType)request.Gender,
                BirthDate = request.BirthDate.ToDateTime(),
                Name = request.Name,

            };
            var roles = new List<string>() { request.Role };

            var user = await userService.Register(userRequest, request.Password, roles);

            if (user == null) throw new RpcException(new Status(StatusCode.Internal, "No se pudo registrar el usuario, intentalo de nuevo"));

            var accessToken = await jwtTokenService.CreateTokenAsync(user);
            var refreshToken = await refresh.GenerateRefreshTokenAsync(user.Id);

            var UserResponse = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                BirthDate = user.BirthDate.Value.ToShortDateString() ?? string.Empty,
                Gender = (int)user.Gender,
                IdentificationNumber = user.IdentificationNumber,
            };
            UserResponse.Roles.AddRange(roles);

            return new AuthResponse
            {
                User = UserResponse,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}
