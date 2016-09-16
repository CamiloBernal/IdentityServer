/*
 * Copyright 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using IdentityModel;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.Default;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Constants = IdentityServer3.Core.Constants;

namespace IdentityServer.AspNetIdentityServices.App_Packages.IdentityServer3.AspNetIdentity
{
    public class AspNetIdentityUserService<TUser, TKey> : UserServiceBase
        where TUser : class, IUser<TKey>, new()
        where TKey : IEquatable<TKey>
    {
        protected readonly Func<string, TKey> ConvertSubjectToKey;
        protected readonly UserManager<TUser, TKey> UserManager;

        public AspNetIdentityUserService(UserManager<TUser, TKey> userManager, Func<string, TKey> parseSubject = null)
        {
            if (userManager == null) throw new ArgumentNullException(nameof(userManager));

            UserManager = userManager;

            if (parseSubject != null)
            {
                ConvertSubjectToKey = parseSubject;
            }
            else
            {
                var keyType = typeof(TKey);
                if (keyType == typeof(string)) ConvertSubjectToKey = subject => (TKey)ParseString(subject);
                else if (keyType == typeof(int)) ConvertSubjectToKey = subject => (TKey)ParseInt(subject);
                else if (keyType == typeof(uint)) ConvertSubjectToKey = subject => (TKey)ParseUInt32(subject);
                else if (keyType == typeof(long)) ConvertSubjectToKey = subject => (TKey)ParseLong(subject);
                else if (keyType == typeof(Guid)) ConvertSubjectToKey = subject => (TKey)ParseGuid(subject);
                else
                {
                    throw new InvalidOperationException("Key type not supported");
                }
            }

            EnableSecurityStamp = true;
        }

        public string DisplayNameClaimType { get; set; }
        public bool EnableSecurityStamp { get; set; }

        public override async Task AuthenticateExternalAsync(ExternalAuthenticationContext ctx)
        {
            var externalUser = ctx.ExternalIdentity;

            if (externalUser == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }

            var user = await UserManager.FindAsync(new UserLoginInfo(externalUser.Provider, externalUser.ProviderId)).ConfigureAwait(false);
            if (user == null)
            {
                ctx.AuthenticateResult = await ProcessNewExternalAccountAsync(externalUser.Provider, externalUser.ProviderId, externalUser.Claims).ConfigureAwait(false);
            }
            else
            {
                ctx.AuthenticateResult = await ProcessExistingExternalAccountAsync(user.Id, externalUser.Provider, externalUser.ProviderId, externalUser.Claims).ConfigureAwait(false);
            }
        }

        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext ctx)
        {
            var username = ctx.UserName;
            var password = ctx.Password;
            var message = ctx.SignInMessage;

            ctx.AuthenticateResult = null;

            if (UserManager.SupportsUserPassword)
            {
                var user = await FindUserAsync(username).ConfigureAwait(false);
                if (user != null)
                {
                    if (UserManager.SupportsUserLockout &&
                        await UserManager.IsLockedOutAsync(user.Id).ConfigureAwait(false))
                    {
                        return;
                    }

                    if (await UserManager.CheckPasswordAsync(user, password).ConfigureAwait(false))
                    {
                        if (UserManager.SupportsUserLockout)
                        {
                            await UserManager.ResetAccessFailedCountAsync(user.Id).ConfigureAwait(false);
                        }

                        var result = await PostAuthenticateLocalAsync(user, message).ConfigureAwait(false);
                        if (result == null)
                        {
                            var claims = await GetClaimsForAuthenticateResult(user).ConfigureAwait(false);
                            result = new AuthenticateResult(user.Id.ToString(), await GetDisplayNameForAccountAsync(user.Id).ConfigureAwait(false), claims);
                        }

                        ctx.AuthenticateResult = result;
                    }
                    else if (UserManager.SupportsUserLockout)
                    {
                        await UserManager.AccessFailedAsync(user.Id).ConfigureAwait(false);
                    }
                }
            }
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext ctx)
        {
            var subject = ctx.Subject;
            var requestedClaimTypes = ctx.RequestedClaimTypes;

            if (subject == null) throw new ArgumentNullException(nameof(ctx));

            var key = ConvertSubjectToKey(subject.GetSubjectId());
            var acct = await UserManager.FindByIdAsync(key).ConfigureAwait(false);
            if (acct == null)
            {
                throw new ArgumentException("Invalid subject identifier");
            }

            var claims = await GetClaimsFromAccount(acct).ConfigureAwait(false);
            var claimTypes = requestedClaimTypes as IList<string> ?? requestedClaimTypes.ToList();
            if (claimTypes.Any())
            {
                claims = claims.Where(x => claimTypes.Contains(x.Type));
            }

            ctx.IssuedClaims = claims;
        }

        public override async Task IsActiveAsync(IsActiveContext ctx)
        {
            var subject = ctx.Subject;

            if (subject == null) throw new ArgumentNullException(nameof(ctx));

            var id = subject.GetSubjectId();
            var key = ConvertSubjectToKey(id);
            var acct = await UserManager.FindByIdAsync(key).ConfigureAwait(false);

            ctx.IsActive = false;

            if (acct != null)
            {
                if (EnableSecurityStamp && UserManager.SupportsUserSecurityStamp)
                {
                    var securityStamp = subject.Claims.Where(x => x.Type == "security_stamp").Select(x => x.Value).SingleOrDefault();
                    if (securityStamp != null)
                    {
                        var dbSecurityStamp = await UserManager.GetSecurityStampAsync(key).ConfigureAwait(false);
                        if (dbSecurityStamp != securityStamp)
                        {
                            return;
                        }
                    }
                }

                ctx.IsActive = true;
            }
        }

        protected virtual async Task<AuthenticateResult> AccountCreatedFromExternalProviderAsync(TKey userId, string provider, string providerId, IEnumerable<Claim> claims)
        {
            claims = await SetAccountEmailAsync(userId, claims).ConfigureAwait(false);
            claims = await SetAccountPhoneAsync(userId, claims).ConfigureAwait(false);

            return await UpdateAccountFromExternalClaimsAsync(userId, provider, providerId, claims).ConfigureAwait(false);
        }

        protected virtual async Task<TUser> FindUserAsync(string username)
        {
            return await UserManager.FindByNameAsync(username).ConfigureAwait(false);
        }

        protected virtual async Task<IEnumerable<Claim>> GetClaimsForAuthenticateResult(TUser user)
        {
            var claims = new List<Claim>();
            if (!EnableSecurityStamp || !UserManager.SupportsUserSecurityStamp) return claims;
            var stamp = await UserManager.GetSecurityStampAsync(user.Id).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(stamp))
            {
                claims.Add(new Claim("security_stamp", stamp));
            }
            return claims;
        }

        protected virtual async Task<IEnumerable<Claim>> GetClaimsFromAccount(TUser user)
        {
            var claims = new List<Claim>{
                new Claim(Constants.ClaimTypes.Subject, user.Id.ToString()),
                new Claim(Constants.ClaimTypes.PreferredUserName, user.UserName),
            };

            if (UserManager.SupportsUserEmail)
            {
                var email = await UserManager.GetEmailAsync(user.Id).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(email))
                {
                    claims.Add(new Claim(Constants.ClaimTypes.Email, email));
                    var verified = await UserManager.IsEmailConfirmedAsync(user.Id).ConfigureAwait(false);
                    claims.Add(new Claim(Constants.ClaimTypes.EmailVerified, verified ? "true" : "false"));
                }
            }

            if (UserManager.SupportsUserPhoneNumber)
            {
                var phone = await UserManager.GetPhoneNumberAsync(user.Id).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    claims.Add(new Claim(Constants.ClaimTypes.PhoneNumber, phone));
                    var verified = await UserManager.IsPhoneNumberConfirmedAsync(user.Id).ConfigureAwait(false);
                    claims.Add(new Claim(Constants.ClaimTypes.PhoneNumberVerified, verified ? "true" : "false"));
                }
            }

            if (UserManager.SupportsUserClaim)
            {
                claims.AddRange(await UserManager.GetClaimsAsync(user.Id).ConfigureAwait(false));
            }

            if (!UserManager.SupportsUserRole) return claims;
            var roleClaims =
                from role in await UserManager.GetRolesAsync(user.Id).ConfigureAwait(false)
                select new Claim(Constants.ClaimTypes.Role, role);
            claims.AddRange(roleClaims);

            return claims;
        }

        protected virtual async Task<string> GetDisplayNameForAccountAsync(TKey userId)
        {
            var user = await UserManager.FindByIdAsync(userId).ConfigureAwait(false);
            var claims = await GetClaimsFromAccount(user).ConfigureAwait(false);

            Claim nameClaim = null;
            var claimList = claims as IList<Claim> ?? claims.ToList();
            if (DisplayNameClaimType != null)
            {
                nameClaim = claimList.FirstOrDefault(x => x.Type == DisplayNameClaimType);
            }
            if (nameClaim == null) nameClaim = claimList.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name);
            if (nameClaim == null) nameClaim = claimList.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            return nameClaim != null ? nameClaim.Value : user.UserName;
        }

        protected virtual Task<TUser> InstantiateNewUserFromExternalProviderAsync(string provider, string providerId, IEnumerable<Claim> claims)
        {
            var user = new TUser() { UserName = Guid.NewGuid().ToString("N") };
            return Task.FromResult(user);
        }

        protected virtual Task<AuthenticateResult> PostAuthenticateLocalAsync(TUser user, SignInMessage message)
        {
            return Task.FromResult<AuthenticateResult>(null);
        }

        protected virtual async Task<AuthenticateResult> ProcessExistingExternalAccountAsync(TKey userId, string provider, string providerId, IEnumerable<Claim> claims)
        {
            return await SignInFromExternalProviderAsync(userId, provider).ConfigureAwait(false);
        }

        protected virtual async Task<AuthenticateResult> ProcessNewExternalAccountAsync(string provider, string providerId, IEnumerable<Claim> claims)
        {
            var claimList = claims as IList<Claim> ?? claims.ToList();
            var user = await TryGetExistingUserFromExternalProviderClaimsAsync(provider, claimList).ConfigureAwait(false);
            if (user == null)
            {
                user = await InstantiateNewUserFromExternalProviderAsync(provider, providerId, claimList).ConfigureAwait(false);
                if (user == null)
                    throw new InvalidOperationException("CreateNewAccountFromExternalProvider returned null");

                var createResult = await UserManager.CreateAsync(user).ConfigureAwait(false);
                if (!createResult.Succeeded)
                {
                    return new AuthenticateResult(createResult.Errors.First());
                }
            }

            var externalLogin = new UserLoginInfo(provider, providerId);
            var addExternalResult = await UserManager.AddLoginAsync(user.Id, externalLogin).ConfigureAwait(false);
            if (!addExternalResult.Succeeded)
            {
                return new AuthenticateResult(addExternalResult.Errors.First());
            }

            var result = await AccountCreatedFromExternalProviderAsync(user.Id, provider, providerId, claimList).ConfigureAwait(false);
            if (result != null) return result;

            return await SignInFromExternalProviderAsync(user.Id, provider).ConfigureAwait(false);
        }

        protected virtual async Task<IEnumerable<Claim>> SetAccountEmailAsync(TKey userId, IEnumerable<Claim> claims)
        {
            var claimList = claims as IList<Claim> ?? claims.ToList();
            var email = claimList.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Email);
            if (email == null) return claimList;
            {
                var userEmail = await UserManager.GetEmailAsync(userId).ConfigureAwait(false);
                if (userEmail != null) return claimList;
                // if this fails, then presumably the email is already associated with another
                // account so ignore the error and let the claim pass thru
                var result = await UserManager.SetEmailAsync(userId, email.Value).ConfigureAwait(false);
                if (!result.Succeeded) return claimList;
                var emailVerified = claimList.FirstOrDefault(x => x.Type == Constants.ClaimTypes.EmailVerified);
                if (emailVerified != null && emailVerified.Value == "true")
                {
                    var token = await UserManager.GenerateEmailConfirmationTokenAsync(userId).ConfigureAwait(false);
                    await UserManager.ConfirmEmailAsync(userId, token).ConfigureAwait(false);
                }

                var emailClaims = new[] { Constants.ClaimTypes.Email, Constants.ClaimTypes.EmailVerified };
                return claimList.Where(x => !emailClaims.Contains(x.Type));
            }
        }

        protected virtual async Task<IEnumerable<Claim>> SetAccountPhoneAsync(TKey userId, IEnumerable<Claim> claims)
        {
            var claimList = claims as Claim[] ?? claims.ToArray();
            var phone = claimList.FirstOrDefault(x => x.Type == Constants.ClaimTypes.PhoneNumber);
            if (phone == null) return claimList;
            {
                var userPhone = await UserManager.GetPhoneNumberAsync(userId).ConfigureAwait(false);
                if (userPhone != null) return claimList;
                // if this fails, then presumably the phone is already associated with another
                // account so ignore the error and let the claim pass thru
                var result = await UserManager.SetPhoneNumberAsync(userId, phone.Value).ConfigureAwait(false);
                if (!result.Succeeded) return claimList;
                var phoneVerified = claimList.FirstOrDefault(x => x.Type == Constants.ClaimTypes.PhoneNumberVerified);
                if (phoneVerified != null && phoneVerified.Value == "true")
                {
                    var token = await UserManager.GenerateChangePhoneNumberTokenAsync(userId, phone.Value).ConfigureAwait(false);
                    await UserManager.ChangePhoneNumberAsync(userId, phone.Value, token).ConfigureAwait(false);
                }

                var phoneClaims = new[] { Constants.ClaimTypes.PhoneNumber, Constants.ClaimTypes.PhoneNumberVerified };
                return claimList.Where(x => !phoneClaims.Contains(x.Type));
            }
        }

        protected virtual async Task<AuthenticateResult> SignInFromExternalProviderAsync(TKey userId, string provider)
        {
            var user = await UserManager.FindByIdAsync(userId).ConfigureAwait(false);
            var claims = await GetClaimsForAuthenticateResult(user).ConfigureAwait(false);

            return new AuthenticateResult(
                userId.ToString(),
                await GetDisplayNameForAccountAsync(userId).ConfigureAwait(false),
                claims,
                authenticationMethod: Constants.AuthenticationMethods.External,
                identityProvider: provider);
        }

        protected virtual Task<TUser> TryGetExistingUserFromExternalProviderClaimsAsync(string provider, IEnumerable<Claim> claims)
        {
            return Task.FromResult<TUser>(null);
        }

        protected virtual async Task<AuthenticateResult> UpdateAccountFromExternalClaimsAsync(TKey userId, string provider, string providerId, IEnumerable<Claim> claims)
        {
            var existingClaims = await UserManager.GetClaimsAsync(userId).ConfigureAwait(false);
            var claimList = claims as IList<Claim> ?? claims.ToList();
            var intersection = existingClaims.Intersect(claimList, new ClaimComparer());
            var newClaims = claimList.Except(intersection, new ClaimComparer());

            foreach (var claim in newClaims)
            {
                var result = await UserManager.AddClaimAsync(userId, claim).ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    return new AuthenticateResult(result.Errors.First());
                }
            }

            return null;
        }

        private static object ParseGuid(string sub)
        {
            Guid key;
            return !Guid.TryParse(sub, out key) ? Guid.Empty : key;
        }

        private static object ParseInt(string sub)
        {
            int key;
            return !int.TryParse(sub, out key) ? 0 : key;
        }

        private static object ParseLong(string sub)
        {
            long key;
            return !long.TryParse(sub, out key) ? 0 : key;
        }

        private static object ParseString(string sub)
        {
            return sub;
        }

        private static object ParseUInt32(string sub)
        {
            uint key;
            return !uint.TryParse(sub, out key) ? 0 : key;
        }
    }
}