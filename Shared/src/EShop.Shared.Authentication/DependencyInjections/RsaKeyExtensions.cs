using System.Security.Cryptography;

namespace EShop.Shared.Authentication.DependencyInjections
{
    public static class RsaKeyExtensions
    {
        public static RSA GetPrivateKey(this RsaKeyPair keyPair)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(keyPair.PrivateKeyPem.ToCharArray());
            return rsa;
        }

        public static RSA GetPublicKey(this RsaKeyPair keyPair)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(keyPair.PublicKeyPem.ToCharArray());
            return rsa;
        }
    }
}
