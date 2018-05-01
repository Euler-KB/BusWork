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
using BookingSystem.API.Models;
using BookingSystem.API.Models.DTO;
using System.IO;

namespace BookingSystem.Android.API.Endpoints
{
    public static class AccountEndpoints
    {
        public static readonly string BaseUri = "api/account";

        public static readonly string UserInfoUri = "api/account/myself";

        #region Internals

        static string ToApiAccountType(AccountType accountType)
        {
            switch (accountType)
            {
                case AccountType.Administrator:
                    return "Admin";
                case AccountType.User:
                    return "User";
            }

            return null;
        }

        #endregion


        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="userId">The username or email</param>
        /// <param name="password">The password for the account</param>
        public static IEndpoint Login(string userId, string password) => new ApiEndpoint($"{BaseUri}/login", HttpMethod.Post, new LoginModel()
        {
            UserId = userId,
            Password = password
        }, requireAuthentication: false);

        /// <summary>
        /// Refresh user token
        /// </summary>
        /// <param name="refreshToken">The refresh token issued earlier alongside login response</param>
        public static IEndpoint RefreshAccessToken(string refreshToken) => new ApiEndpoint($"{BaseUri}/refresh/token", HttpMethod.Post, new RefreshTokenModel()
        {
            RefreshToken = refreshToken
        }, false);

        public static IEndpoint UserDashboard => new ApiEndpoint($"{BaseUri}/dashboard/user", HttpMethod.Get);

        public static IEndpoint AdminDashboard => new ApiEndpoint($"{BaseUri}/dashboard/admin", HttpMethod.Get);

        public static IEndpoint BeginChangePhone(string phone) => new ApiEndpoint($"{BaseUri}/confirm/phone", HttpMethod.Post).AddQuery("Phone", phone);

        public static IEndpoint GetMySelf() => new ApiEndpoint($"{BaseUri}/myself", HttpMethod.Get);


        public static IEndpoint CompletePhoneChange(string confirmCode, string phone) => new ApiEndpoint($"{BaseUri}/change/phone", HttpMethod.Put, new ChangePhoneModel()
        {
            Phone = phone,
            Token = confirmCode
        });

        public static IEndpoint BeginChangeEmail(string email) => new ApiEndpoint($"{BaseUri}/confirm/email", HttpMethod.Post).AddQuery("Email", email);

        public static IEndpoint CompleteChangeEmail(string confirmCode, string email) => new ApiEndpoint($"{BaseUri}/change/email", HttpMethod.Put, new ChangeEmailModel()
        {
            Email = email,
            Token = confirmCode
        });

        public static IEndpoint CreateFeedback(string feedbackMessage) => new ApiEndpoint($"{BaseUri}/feedback", HttpMethod.Post, new CreateFeedback()
        {
            Message = feedbackMessage
        });

        public static IEndpoint GetAllFeedbacks() => new ApiEndpoint($"{BaseUri}/feedback", HttpMethod.Get);

        public static IEndpoint GetFeedback(int id) => new ApiEndpoint($"{BaseUri}/feedback/{id}", HttpMethod.Get);

        public static IEndpoint GetMyself() => new ApiEndpoint($"{BaseUri}/myself", HttpMethod.Get);

        public static IEndpoint Update(UpdateUserInfo userInfo) => new ApiEndpoint(BaseUri, HttpMethod.Put, userInfo);

        public static IEndpoint UploadProfileImage(Stream stream, string mimeType) => new ApiEndpoint($"{BaseUri}/photo", HttpMethod.Post)
                                                                                                    .SetContent(stream, mimeType);

        public static IEndpoint RemoveProfileImage() => new ApiEndpoint($"{BaseUri}/photo", HttpMethod.Delete);

        public static IEndpoint DeleteAccount() => new ApiEndpoint(BaseUri, HttpMethod.Delete);

        public static IEndpoint BeginActivation(string userToken) => new ApiEndpoint($"{BaseUri}/beginactivate", HttpMethod.Post, requireAuthentication: false)
                                                                            .AddQuery("Token", userToken);

        public static IEndpoint ActivateAccount(string activationCode, string userToken) => new ApiEndpoint($"{BaseUri}/activate", HttpMethod.Put, new ActivateAccountModel()
        {
            ActivationCode = activationCode,
            Token = userToken
        }, false);

        public static IEndpoint ForgotPassword(string email) => new ApiEndpoint($"{BaseUri}/forgotpassword", HttpMethod.Post, new ForgotPasswordModel()
        {
            Email = email
        }, requireAuthentication: false);

        public static IEndpoint ResetPassword(string token, string identity, string newPassword) => new ApiEndpoint($"{BaseUri}/resetpassword", HttpMethod.Post, new ResetPasswordModel()
        {
            NewPassword = newPassword,
            Token = token,
            UserIdentity = identity 
        }, requireAuthentication: false);


        public static IEndpoint GetPreferences() => new ApiEndpoint($"{BaseUri}/preferences", HttpMethod.Get);

        public static IEndpoint SetPreference(string key, string value) => new ApiEndpoint($"{BaseUri}/preferences", HttpMethod.Post)
                                                                                .AddQuery("Key", key)
                                                                                .SetContent(new StringContent(value,Encoding.UTF8,"application/json"));

        public static IEndpoint RemoveEndpoint(string key) => new ApiEndpoint($"{BaseUri}/preferences", HttpMethod.Delete)
                                                                        .AddQuery("Key", key);

        public static IEndpoint Register(AccountType accountType, RegisterModel accountInfo) => new ApiEndpoint($"{BaseUri}/register", HttpMethod.Post, accountInfo, requireAuthentication: false)
                                                                                                        .AddQuery("accountType", ToApiAccountType(accountType));

        public static IEndpoint ChangePassword(string oldpasword, string newPassword) => new ApiEndpoint($"{BaseUri}/change/password", HttpMethod.Put, new ChangePasswordModel()
        {
            NewPassword = newPassword,
            OldPassword = oldpasword
        });
    }
}