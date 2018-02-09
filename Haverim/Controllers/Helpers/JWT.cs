using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haverim.Controllers.Helpers
{
    public static class JWT
    {
        public static string GetToken(object payload)
        {     
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            string token = encoder.Encode(payload, GlobalVariables.JWTSecretKey);
            return token;
        }

        public static (TokenStatus,ApiClasses.Payload) VerifyToken(string token)
        {
            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);

                string json = decoder.Decode(token, GlobalVariables.JWTSecretKey, verify: true);
                return (TokenStatus.Valid, JsonConvert.DeserializeObject<ApiClasses.Payload>(json));
            }
            catch (TokenExpiredException)
            {
                return (TokenStatus.Expired, null);
            }
            catch (SignatureVerificationException)
            {
                return (TokenStatus.InvalidSignature,null);
            }
            catch (ArgumentOutOfRangeException)
            {
                return (TokenStatus.InvalidSignature, null);
            }
        }

        public enum TokenStatus
        {
            Valid,Expired,InvalidSignature
        }
    }
}
