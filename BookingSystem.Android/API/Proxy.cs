using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net.Http;
using System.Threading.Tasks;
using BookingSystem.Android.API.Endpoints;
using System.Threading;
using BookingSystem.API.Models.DTO;
using System.IO;
using BookingSystem.API.Models;
using Newtonsoft.Json;
using BookingSystem.Android.Helpers;

namespace BookingSystem.Android.API
{
    public class AuthenticationInfo
    {
        public LoginResponse LoginInfo { get; set; }

        [JsonIgnore]
        public string AccessToken
        {
            get
            {
                return LoginInfo?.AccessToken;
            }
        }

        [JsonIgnore]
        public string RefreshToken
        {
            get
            {
                return LoginInfo?.RefreshToken;
            }
        }

        public UserInfo User { get; set; }

        public AuthenticationInfo() { }

        public AuthenticationInfo(LoginResponse loginInfo, UserInfo user)
        {
            LoginInfo = loginInfo;
            User = user;
        }

    }


    public class LoginStatusResponse
    {
        public bool Successful { get; set; }

        public string Message { get; set; }

        public bool RequireActivation { get; set; }

        public string ActivationTicket { get; set; }

        public AuthenticationInfo AuthInfo { get; set; }
    }

    public class ServiceProxy : HttpClientHandler
    {
        private string _baseUrl;
        private string _apiKey;
        private Task _restoreTask;
        private bool _isRestoring = false;

        private HttpClient httpClient;

        #region Authentication Info

        private AuthenticationInfo authInfo;

        #endregion

        public delegate void ResponseDelegate(object sender, HttpResponseMessage response, IEndpoint endpoint);

        public delegate void SendDelegate(object sender, HttpRequestMessage request, IEndpoint endpoint);

        /// <summary>
        /// Called before any message is sent to the server through the proxy
        /// </summary>
        public event SendDelegate OnSend;

        /// <summary>
        /// Called after any response is received from server
        /// </summary>
        public event ResponseDelegate OnResponse;

        /// <summary>
        /// Invoked any time the user is logged in
        /// </summary>
        public event EventHandler<AuthenticationInfo> OnLogin;

        /// <summary>
        /// Invoked any time the user is signed out
        /// </summary>
        public event EventHandler<AuthenticationInfo> OnSignOut;

        /// <summary>
        /// Invoked for token to be refresh
        /// </summary>
        public event EventHandler<AuthenticationInfo> OnRefreshToken;

        /// <summary>
        /// Called when the user is updated
        /// </summary>
        public event EventHandler<AuthenticationInfo> OnUserUpdated;

        /// <summary>
        /// Determines whether to automatically refresh to token
        /// </summary>
        public bool AutoRefreshToken { get; set; } = true;

        /// <summary>
        /// Gets the authentication info for the proxy
        /// </summary>
        public AuthenticationInfo AuthInfo
        {
            get
            {
                if (!IsAuthenticated)
                {
                    throw new InvalidOperationException();
                }

                return authInfo;
            }
        }

        /// <summary>
        /// Gets the current user
        /// </summary>
        public UserInfo User
        {
            get
            {
                return authInfo.User;
            }
        }

        public ServiceProxy(string serverAddress, string apiKey)
        {
            this._baseUrl = serverAddress;
            this._apiKey = apiKey;

            //  
            httpClient = new HttpClient(this)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this._apiKey != null)
            {
                request.Headers.Add("X-Api-Key", this._apiKey);
            }

            IEndpoint endpoint = request.Properties.ContainsKey("Endpoint") ? (IEndpoint)request.Properties["Endpoint"] : null;

            if (endpoint?.RequireAuth == true || (authInfo != null))
            {
                if (!request.Headers.Contains("Authorization"))
                {
                    request.Headers.Add("Authorization", $"Bearer {authInfo.AccessToken}");
                }
            }

            //  Dispatch to on send listeners
            OnSend?.Invoke(this, request, endpoint);

            //
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            //  Dispatch to on response listeners
            OnResponse?.Invoke(this, response, endpoint);

            return response;
        }

        public ServiceProxy() :
#if DEBUG
            this(Resources.APIBaseAddress, Resources.APIKey)
#else
            this(Resources.APIBaseAddressRelease, Resources.APIKey)
#endif

        {

        }

        public async Task<LoginStatusResponse> LoginAsync(string emailOrUsername, string password)
        {
            // Wait for restore to complete
            await EnsureRestoreComplete();

            var result = new LoginStatusResponse();

            var response = await ExecuteAsync(AccountEndpoints.Login(emailOrUsername, password));
            if (response.Successful)
            {
                //  set authentication info
                authInfo = new AuthenticationInfo(await response.GetDataAsync<LoginResponse>(), null);

                //  update user info
                var userInfo = await UpdateUserInfoAsync();
                if (userInfo != null)
                    authInfo.User = userInfo;

                //
                OnLogin?.Invoke(this, authInfo);

                //  
                result.AuthInfo = authInfo;
                result.Message = "Login successfull";
                result.Successful = true;
            }
            else
            {
                var str = response.GetErrorDescription();
                if (str == MessageResources.MSG_LOGIN_INVALID_CRED)
                {
                    str = "Invalid username or password";
                }
                else if (str == MessageResources.MSG_LOGIN_LOCKED_ACC)
                {
                    str = "You cannot login into your account at this time. Wait for a while and try again!";
                }
                else if (str == MessageResources.MSG_LOGIN_REQUIRE_EMAIL_CONFIRM)
                {
                    result.ActivationTicket = response.GetHeader("UserToken");
                    result.RequireActivation = true;
                }

                result.Message = str;
            }

            return result;
        }

        public Task<bool> SignOut()
        {
            ///
            if (this.authInfo == null)
                return Task.FromResult(false);


            var _authInfo = authInfo;

            //  nullify authentication info
            this.authInfo = null;

            OnSignOut?.Invoke(this, _authInfo);


            return Task.FromResult(true);
        }

        private async Task EnsureRestoreComplete()
        {
            if (_isRestoring)
            {
                await _restoreTask;
            }
        }

        public bool RestoreAsync(AuthenticationInfo authInfo, bool updateUser = false)
        {
            if (_isRestoring)
                return true;

            if (authInfo == null || authInfo.AccessToken == null || authInfo.RefreshToken == null)
                return false;

            //  set authentication info
            this.authInfo = authInfo;

            //
            OnLogin?.Invoke(this, this.authInfo);
            
            //
            _restoreTask = Task.Run(async () =>
            {
                _isRestoring = true;

                //  validate auth info
                if (NeedTokenRefresh)
                {
                    //  Refresh token
                    await RefreshTokenAsync();

                    updateUser = true;
                }

                //  update current user
                if (updateUser || (authInfo != null && authInfo.User == null))
                    await UpdateUserInfoAsync();

                //
                _isRestoring = false;

            });


            return true;
        }

        public bool IsAuthenticated
        {
            get
            {
                return this.authInfo != null;
            }
        }

        public void EnsureEnthenticated()
        {
            if (!IsAuthenticated)
                throw new InvalidOperationException("Proxy not authenticated. Please login with valid credentials to begin communication");
        }

        /// <summary>
        /// Updates the current user info
        /// </summary>
        public async Task<UserInfo> UpdateUserInfoAsync()
        {

            try
            {
                var response = await InternalExecute(new HttpRequestMessage(HttpMethod.Get, $"{this._baseUrl}/{AccountEndpoints.UserInfoUri}"));
                if (response.IsSuccessStatusCode)
                {
                    //  get user from response body
                    var user = JsonConvert.DeserializeObject<UserInfo>((await response.Content.ReadAsStringAsync()));

                    //  update current authentication info
                    if (IsAuthenticated)
                    {
                        authInfo.User = user;

                        OnUserUpdated?.Invoke(this, authInfo);
                    }


                    return user;
                }
            }
            catch
            {

            }


            return null;
        }

        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                //
                var request = AccountEndpoints.RefreshAccessToken(authInfo.RefreshToken).GetRequstAsync(httpClient.BaseAddress).Result;
                var response = await InternalExecute(request);
                if (!response.IsSuccessStatusCode)
                    return false;

                var payload = JsonConvert.DeserializeObject<LoginResponse>(await response.Content.ReadAsStringAsync());
                if (IsAuthenticated)
                {
                    authInfo.LoginInfo = payload;

                    //
                    OnRefreshToken?.Invoke(this, authInfo);

                    return true;
                }

            }
            catch
            {

            }

            return false;
        }

        /// <summary>
        /// Indicates whether 
        /// </summary>
        public bool NeedTokenRefresh
        {
            get
            {
                if (!IsAuthenticated)
                    return false;

                return DateTime.UtcNow.AddSeconds(30) > this.authInfo.LoginInfo.ExpiresAt;
            }
        }

        private async Task<HttpResponseMessage> InternalExecute(HttpRequestMessage request, CancellationToken? cancellationToken = null)
        {
            if (cancellationToken == null)
                return await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            else
                return await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken.Value);
        }

        /// <summary>
        /// Resolves the absolute address for the given path
        /// </summary>
        /// <param name="path">The additional path for the image</param>
        /// <returns></returns>
        public string GetUri(string path)
        {
            return $"{_baseUrl}/{path}";
        }

        /// <summary>
        /// Executes an api endpoint
        /// Note: This method does not throw any exception. All required to inspect the response for the status
        /// </summary>
        /// <param name="endpoint">The defined api endpoint</param>
        /// <param name="cancellationToken">The cancellation token for the request</param>
        public async Task<ApiResponse> ExecuteAsync(IEndpoint endpoint, CancellationToken? cancellationToken = null)
        {
            //  Ensure restore is completed
            await EnsureRestoreComplete();

            try
            {
                if (endpoint.RequireAuth && !this.IsAuthenticated)
                {
                    return new ApiResponse(new HttpRequestException("Cannot execute request. Endpoint requires user to be authenticated!"), endpoint);
                }

                if (NeedTokenRefresh)
                {
                    if (!await this.RefreshTokenAsync())
                        return new ApiResponse(new HttpRequestException("Token refresh failed. Please try again!"), endpoint);
                }

                if (endpoint.RequireAuth && AuthInfo?.User == null)
                {
                    await UpdateUserInfoAsync();
                }

                //  Get request object
                var request = await endpoint.GetRequstAsync(httpClient.BaseAddress);

                //  Add endpoint to properties
                if (request.Properties.ContainsKey("Endpoint"))
                    request.Properties["Endpoint"] = endpoint;
                else
                    request.Properties.Add("Endpoint", endpoint);

                var response = await InternalExecute(request, cancellationToken);

#if DEBUG
                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
                    response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Endpoint: {endpoint.Uri}");

                    sb.AppendLine($"Method: {request.Method}");

                    if (request.Content is StringContent)
                        sb.AppendLine($"Body: {await request.Content.ReadAsStringAsync()}");


                    sb.AppendLine();
                    sb.AppendLine(await response.Content.ReadAsStringAsync());
                    Helpers.DialogHelpers.ShowDebugMessage("Server Error", sb.ToString());
                }

#endif
                //  Execute request
                return new ApiResponse(response, endpoint);
            }
            catch (TaskCanceledException)
            {
                return new ApiResponse(new HttpRequestException("Request timed out", 
                    new System.Net.WebException("Request timed out", System.Net.WebExceptionStatus.Timeout) ), endpoint);
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse(ex, endpoint);
            }
        }

    }
}