using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ADWS
{
    public static class Cryptor
    {   

        private static readonly byte[] key = Base64Decode( ConfigurationManager.AppSettings[ "AesKey" ] );
        private static readonly byte[] iv  = Base64Decode( ConfigurationManager.AppSettings[ "AesIV" ] );


        /// <summary>
        /// Encrypt Data and return it in Base64 Format
        /// </summary>
        /// <param name="data">Data to cyper</param>
        /// <returns>Base64 Encoded Cypher</returns>
        public static string Encrypt( string data ) 
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.Zeros;
           
            using (ICryptoTransform encryptor = aes.CreateEncryptor( key , iv ))
            {
                using ( MemoryStream ms = new MemoryStream() )
                {
                    ms.Seek( 0 , SeekOrigin.Begin );

                    using ( var cs = new CryptoStream( ms , encryptor , CryptoStreamMode.Write ) )
                    {
                        try
                        {
                            byte[] dataInBytes = Base64Decode( data );

                            cs.Write( dataInBytes , 0 , dataInBytes.Length );

                            cs.FlushFinalBlock();

                            byte[] cipherInBytes = ms.ToArray();

                            return Base64Encode( cipherInBytes );
                        }
                        catch ( Exception ex ) 
                        {
                            throw ex;
                        }
                    }
                }
            }
           
        }

        /// <summary>
        /// Decrypt sypher by passing Base64 Encoded Cypher
        /// </summary>
        /// <param name="cipher">base64 Encoded Cypher</param>
        /// <returns>Base64 Decrypted Cypher</returns>
        public static string Decrypt( string cipher ) 
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.Zeros;
            
            byte[] cipheredData = Base64Decode( cipher );

            ICryptoTransform decryptor = aes.CreateDecryptor( key , iv );

            using ( MemoryStream ms = new MemoryStream( cipheredData ) )
            {
                using ( CryptoStream cs = new CryptoStream( ms , decryptor , CryptoStreamMode.Read ) )
                {   
                    using ( StreamReader sr = new StreamReader( cs , Encoding.UTF8 ) )
                    {
                        String data = sr.ReadToEnd();

                        return data;
                    }
                        
                }
            }
        }

        private static string Base64Encode( byte[] data )
        {
            return System.Convert.ToBase64String( data );
        }

        private static byte[] Base64Decode( string data )
        {
            return Convert.FromBase64String( data );
        }

    }
}