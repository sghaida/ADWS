using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml.Serialization;

namespace ADWS
{
    [Serializable]
    public struct SessionData
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime SessionStart { get; set; }

    }

    public class SessionHandler
    {
       
       
        private static string SerializeObject<T>(SessionData sessionData)
        {
            string serialized = string.Empty;
            XmlSerializer serializer = new XmlSerializer( typeof( T ) );

            using ( StringWriter writer = new StringWriter() )
            {
                serializer.Serialize( writer , sessionData );

                byte[] data = Encoding.UTF8.GetBytes( writer.ToString() );

                return Convert.ToBase64String( data );
            }

            
        }

        private static T DesrializeObject<T>( string data )
        {
            return (T)DesrializeObject( data , typeof( T ) );
        }

        private static object DesrializeObject( string objectData , Type type )
        {
            var serializer = new XmlSerializer( type );
            object result;

            using ( TextReader reader = new StringReader( objectData ) )
            {
                result = serializer.Deserialize( reader );
            }

            return result;
        }

        private static string Encrypt(string session)
        {
           string sessionCypher = Cryptor.Encrypt( session );

           return sessionCypher;
        }

      

        private static string Decrypt( string session ) 
        {
            string decrptedSession = Cryptor.Decrypt( session );
            return decrptedSession;
        }

        public static string EncryptSession(SessionData session)
        {
            string serializedSession = SerializeObject<SessionData>( session );
            string encryptedSession = Encrypt( serializedSession );

            return encryptedSession;
            
        }

        public static SessionData DecryptSession( string session ) 
        {
            string decryptedSession = Decrypt( session );
            SessionData desrialized = DesrializeObject<SessionData>( decryptedSession );
            return desrialized; 
        }


    }
}