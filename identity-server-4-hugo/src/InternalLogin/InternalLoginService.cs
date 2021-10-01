using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IdentiyServer4Hugo.InternalLogin
{
    public interface IInternalLoginService
    {
        VerifyUserResult VerifyUser(string userName, string password);
    }

    public class InternalLoginService : IInternalLoginService
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public InternalLoginService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public VerifyUserResult VerifyUser(string userName, string password)
        {
            userName = userName.Trim();
            var enc = System.Text.Encoding.ASCII;
            var userNameParameter = new SqlParameter("@userName", userName);
            var passwordParameter = new SqlParameter("@password", enc.GetBytes(password));

            var passwordHashParameter = new SqlParameter("@passwordHash", GetPasswordHash(password));


            // run the statement to exec Stored Procedure inside the database
            // and capture the return values
            var users = _applicationDbContext.VerifyUser
                .FromSqlRaw("exec VerifyUser @userName, @password, @passwordHash",
                    userNameParameter, passwordParameter, passwordHashParameter)
                .ToList();
            if (users.Count == 1)
            {
                return users[0];
            }

            return null;
        }
        public static byte[] GetPasswordHash(string plainTextPassword)
        {
            SHA512 sha = new SHA512CryptoServiceProvider();
            var encoding = CodePagesEncodingProvider.Instance.GetEncoding(1252);
            byte[] dataBytes = encoding.GetBytes(plainTextPassword); //Encoding.Unicode.GetBytes(plainTextPassword)
            byte[] resultBytes = sha.ComputeHash(dataBytes);
            sha.Clear();
            return resultBytes;
        }
    }


}
