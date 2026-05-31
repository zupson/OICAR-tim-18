using SpendlyWebAPI.Enums;
using SpendlyWebAPI.Security;

namespace SpendlyWebAPI.Tests
{
    public class UnitTest1
    {

        public class PasswordHashProviderTests
        {
            // TEST 1:
            // GetSalt() treba vraæati valid Base64 string koji se može decodirati u 16 bytova
            [Fact]
            public void GetSalt_ShouldReturnValidBase64Of16Bytes()
            {
                var salt = PasswordHashProvider.GetSalt();

                var bytes = Convert.FromBase64String(salt); // throws ako nije valid Base64
                Assert.Equal(16, bytes.Length);             // 128 bits / 8 = 16 bytes
            }

            // TEST 2:
            // GetHash() s istom lozinkom i istim saltom UVIJEK mora dati isti hash
            // (determinizam — bitno za login provjeru)
            [Fact]
            public void GetHash_SamePasswordAndSalt_ShouldAlwaysReturnSameHash()
            {
                var salt = PasswordHashProvider.GetSalt();
                var password = "TestLozinka123!";

                var hash1 = PasswordHashProvider.GetHash(password, salt);
                var hash2 = PasswordHashProvider.GetHash(password, salt);

                Assert.Equal(hash1, hash2);
            }
        }

    
        public class JwtProviderTests
        {
            private const string SecureKey = "ovo-je-test-kljuc-koji-mora-biti-dovoljno-dugacak-32+chars!";
            private const int ExpirationMinutes = 60;

            // TEST 3:
            // CreateToken() treba vraæati string u formatu header.payload.signature (3 dijela)
            [Fact]
            public void CreateToken_ShouldReturnWellFormedJwt()
            {
                var token = JwtProvider.CreateToken(SecureKey, ExpirationMinutes, Role.User, userId: 1);

                var parts = token.Split('.');
                Assert.Equal(3, parts.Length);
            }

            // TEST 4:
            // Isti password s razlièitim saltovima mora dati razlièite hasheve
            // (ako bi bili isti, salt ne bi imao svrhu)
            [Fact]
            public void GetHash_SamePassword_DifferentSalts_ShouldReturnDifferentHashes()
            {
                var password = "TestLozinka123!";
                var salt1 = PasswordHashProvider.GetSalt();
                var salt2 = PasswordHashProvider.GetSalt();

                var hash1 = PasswordHashProvider.GetHash(password, salt1);
                var hash2 = PasswordHashProvider.GetHash(password, salt2);

                Assert.NotEqual(hash1, hash2);
            }

            
            public class PasswordHashSecurityTests
            {
                // TEST 5:
                // Dva razlièita poziva GetSalt() NE smiju vratiti isti salt
                // (svaki korisnik mora imati unique salt — obrana od rainbow table napada)
                [Fact]
                public void GetSalt_TwoCallsShouldReturnDifferentSalts()
                {
                    var salt1 = PasswordHashProvider.GetSalt();
                    var salt2 = PasswordHashProvider.GetSalt();

                    Assert.NotEqual(salt1, salt2);
                }
            }
        }

    }
}