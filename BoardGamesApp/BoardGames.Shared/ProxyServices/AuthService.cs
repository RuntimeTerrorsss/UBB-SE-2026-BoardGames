using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;


namespace BoardGames.Shared.ProxyServices
{
    public sealed class AuthService : ApiServiceBase, IAuthService
    {
        public AuthService(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public Task<ServiceResult> RegisterAsync(RegisterDTO request, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.PostAsJsonAsync("api/auth/register", request, token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<AccountProfileDTO>> LoginAsync(LoginDTO request, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync<AccountProfileDTO>(
                token => client.PostAsJsonAsync("api/auth/login", request, token),
                async (response, token) =>
                {
                    var result = await ApiResponseReader.ReadJsonAsync<AccountProfileDTO>(response, token);
                    if (result.Success && result.Data is not null)
                    {
                        ApiUrlHelper.RebaseAvatarUrl(client.BaseAddress!, result.Data);
                    }

                    return result;
                },
                cancellationToken);
        }

        public Task<ServiceResult> LogoutAsync(CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.PostAsync("api/auth/logout", content: null, token),
                (response, token) => ApiResponseReader.EnsureSuccessAsync(response, token),
                cancellationToken);
        }

        public Task<ServiceResult<string>> ForgotPasswordAsync(CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            return ApiResponseReader.SendAsync(
                token => client.GetAsync("api/auth/forgot-password", token),
                (response, token) => ApiResponseReader.ReadStringAsync(response, token),
                cancellationToken);
        }
    }
}
