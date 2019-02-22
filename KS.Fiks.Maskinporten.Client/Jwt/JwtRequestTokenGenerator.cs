using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;

namespace Ks.Fiks.Maskinporten.Client.Jwt
{
    public class JwtRequestTokenGenerator
    {
        private const int JwtExpireTimeInMinutes = 2;
        private const string DummyKey = ""; // Required by encoder, but not used with RS256Algorithm 

        private readonly JwtEncoder _encoder;
        private readonly X509Certificate2 _certificate;
        
        
        public JwtRequestTokenGenerator(X509Certificate2 certificate)
        {
            _certificate = certificate;
            _encoder = new JwtEncoder(
                new RS256Algorithm(_certificate),
                new JsonNetSerializer(),
                new JwtBase64UrlEncoder());
        }

        public string CreateEncodedJwt(string scope, MaskinportenClientProperties properties)
        {

            var payload = CreateJwtPayload(scope, properties);
            var header = CreateJwtHeader();
            var jwt = _encoder.Encode(header, payload, DummyKey);

            return jwt;
        }

        private IDictionary<string, object> CreateJwtHeader()
        {
            return new Dictionary<string, object>
            {
                {
                    "x5c",
                    new List<string>() {Convert.ToBase64String(_certificate.Export(X509ContentType.Cert))}
                }
            };
        }

        private IDictionary<string, object> CreateJwtPayload(string scope, MaskinportenClientProperties properties)
        {
            var jwtData = new JwtData();

            jwtData.Payload.Add("iss", properties.Issuer);
            jwtData.Payload.Add("aud", properties.Audience);
            jwtData.Payload.Add("iat", UnixEpoch.GetSecondsSince(DateTime.UtcNow));
            jwtData.Payload.Add("exp", UnixEpoch.GetSecondsSince(DateTime.UtcNow.AddMinutes(JwtExpireTimeInMinutes)));
            jwtData.Payload.Add("scope", scope);
            jwtData.Payload.Add("jti", Guid.NewGuid());

            return jwtData.Payload;
        }
    }
}