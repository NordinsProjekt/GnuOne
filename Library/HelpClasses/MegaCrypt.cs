using System.Security.Cryptography;
using System.Text;

namespace GnuOne.Data
{
    public class MegaCrypt
    {
        /// <summary>
        /// Start of an asymmetric enkryption method.
        /// </summary>


        public string body { get; set; }
        public byte[] aesKey;
        public byte[] signature;

        public MegaCrypt()
        {

        }

        //Används vid Encrypt, tillsammans med RSADecryptIt
        public MegaCrypt(string json)
        {
            body = json;
        }

        //Används vid decrypt
        public MegaCrypt(string[] wholemail)
        {
            body = wholemail[0];
            signature = Encoding.UTF8.GetBytes(wholemail[1]);
            aesKey = Encoding.UTF8.GetBytes(wholemail[2]);
        }
        public string GetSignature()
        {
            return Encoding.UTF8.GetString(signature);
        }

        public string GetAESKey()
        {
            return Encoding.UTF8.GetString(aesKey);
        }
        public bool RSADecryptIt(string senderPublicKey, string receiverPrivateKeyLocation)
        {
            try
            {
                string[] aesSecret;
                aesSecret = DecryptAESKeyTo(receiverPrivateKeyLocation);
                byte[] orginalData = Encoding.UTF8.GetBytes(aesSecret[0] + ";;;" + aesSecret[1]);
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();
                RSAalg.ImportFromPem(senderPublicKey);
                RSAParameters Key = RSAalg.ExportParameters(false);
                //If signature is true, decode message.
                //Return true.
                if (Verify.VerifySignedHash(orginalData, signature, Key))
                {
                    Console.WriteLine("The data was verified.");
                    Console.WriteLine("Decrypt it");
                    //Password, salt
                    DecryptBodyAES(aesSecret[0], aesSecret[1]);
                    return true;
                }
                else
                {
                    Console.WriteLine("The data does not match the signature.");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        public bool RSAEncryptIt(string senderPrivateKeyLocation, string receiverPublicKey)
        {
            try
            {
                string secret = RandomText(32);
                string salt = RandomText(8);
                EncryptBodyAES(secret, salt);
                EncryptAESKeyWithRSA(receiverPublicKey, secret, salt);
                MakeSignature(senderPrivateKeyLocation, secret, salt);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(senderPrivateKeyLocation);
                return false;
            }
            return true;
        }
        private string RandomText(int antalTecken)
        {
            StringBuilder sb = new StringBuilder();
            Random rnd = new Random();
            string letters = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!""#%&/()=?[]@$*-+";
            for (int i = 0; i < antalTecken; i++)
            {
                sb.Append(letters[rnd.Next(0, letters.Length)]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Kodar meddelandet med password och salt.
        /// </summary>
        /// <param name="secretkey"></param>
        /// <param name="salt"></param>
        private void EncryptBodyAES(string secretkey, string salt)
        {
            string EncryptionKey = secretkey;
            byte[] userSalt = Encoding.UTF8.GetBytes(salt);
            byte[] clearBytes = Encoding.Unicode.GetBytes(body);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, userSalt);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    body = Convert.ToBase64String(ms.ToArray());
                }
            }
        }
        private void DecryptBodyAES(string secretkey, string salt)
        {
            string DecryptionKey = secretkey;
            byte[] userSalt = Encoding.UTF8.GetBytes(salt);
            byte[] cipherBytes = Convert.FromBase64String(body);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(DecryptionKey, userSalt);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    body = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
        }

        /// <summary> krypterar meddelande body baserat på receiverPublicKey</summary>
        private void EncryptAESKeyWithRSA(string receiverPublicKey, string secret, string salt)
        {
            Cryptor cryptor = new Cryptor();
            //var rsa = RSA.Create();
            //byte[] rsaPublic = Convert.FromBase64String(receiverPublicKey);
            //rsa.ImportSubjectPublicKeyInfo(new ReadOnlySpan<byte>(rsaPublic), out _);
            cryptor.SetPublicKey(receiverPublicKey);
            byte[] data = Encoding.UTF8.GetBytes(secret + ";;;" + salt);
            byte[] encryptedData = cryptor.AsymmetricEncrypt(data);
            aesKey = encryptedData;
        }

        private string[] DecryptAESKeyTo(string receiverPrivateKeyLocation)
        {
            string[] RSASecret;
            Cryptor cryptor = new Cryptor();
            cryptor.SetPrivateKey(receiverPrivateKeyLocation);
            byte[] encryptedData = aesKey;
            byte[] decryptedData = cryptor.AsymmetricDecrypt(encryptedData);
            ASCIIEncoding ByteConverter = new ASCIIEncoding();

            string temp = ByteConverter.GetString(decryptedData);
            RSASecret = temp.Split(";;;");
            return RSASecret;
        }

        /// <summary>
        /// Används för att verifiera källan
        /// </summary>
        /// <param name="senderPrivateKeyLocation"></param>
        /// <param name="secret"></param>
        /// <param name="salt"></param>
        private void MakeSignature(string senderPrivateKey, string secret, string salt)
        {
            Cryptor cryptor = new Cryptor();
            //Används för att använda den privata nyckeln för att generera ett hash som bara
            //den riktiga innehavaren av privatekey kan generera.
            //Stämmer inte detta så är det ett fejkmail.
            //var privateKey = File.ReadAllText(senderPrivateKeyLocation);
            var privateKey = senderPrivateKey;
            RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();
            RSAalg.ImportFromPem("" + privateKey + "");
            RSAParameters Key = RSAalg.ExportParameters(true);
            //Genererar hashet för den krypterade AES nyckeln + saltet som vi skickar.
            byte[] data = Encoding.UTF8.GetBytes(secret + ";;;" + salt);
            signature = Verify.HashAndSignBytes(data, Key);
        }
    }
    public class Cryptor
    {
        // new
        private readonly RSACryptoServiceProvider _rsa = new();
        //...
        public void SetPublicKey(string publicKey)
        {
            _rsa.ImportFromPem(publicKey);
        }
        public byte[] AsymmetricEncrypt(byte[] data)
        {
            return _rsa.Encrypt(data, false);
        }
        public byte[] AsymmetricDecrypt(byte[] data)
        {
            return _rsa.Decrypt(data, false);
        }

        public void SetPrivateKey(string privateKeyFile)
        {
            //var privateKey = File.ReadAllText(privateKeyFile);
            var privateKey = privateKeyFile;
            _rsa.ImportFromPem(privateKey);
        }
    }

    public class Verify
    {
        public static byte[] HashAndSignBytes(byte[] DataToSign, RSAParameters Key)
        {
            try
            {
                // Create a new instance of RSACryptoServiceProvider using the
                // key from RSAParameters.
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();
                RSAalg.ImportParameters(Key);
                // Hash and sign the data. Pass a new instance of SHA256
                // to specify the hashing algorithm.
                return RSAalg.SignData(DataToSign, SHA256.Create());
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

        public static bool VerifySignedHash(byte[] DataToVerify, byte[] SignedData, RSAParameters Key)
        {
            try
            {
                // Create a new instance of RSACryptoServiceProvider using the
                // key from RSAParameters.
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();
                RSAalg.ImportParameters(Key);
                // Verify the data using the signature.  Pass a new instance of SHA256
                // to specify the hashing algorithm.
                return RSAalg.VerifyData(DataToVerify, SHA256.Create(), SignedData);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

    }
}
